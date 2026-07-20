using Helpdesk.Infrastructure.Hubs;
using Helpdesk.Application.Configuration;
using Helpdesk.Application.Interfaces;
using Helpdesk.Infrastructure.Authentication;
using Helpdesk.Infrastructure.BackgroundJobs;
using Helpdesk.Infrastructure.Email;
using Helpdesk.Infrastructure.Sla;
using Helpdesk.Persistence.Jobs;
using Helpdesk.Infrastructure.Notifications;
using Helpdesk.Infrastructure.Storage;
using Helpdesk.Persistence.Contexts;
using Helpdesk.Persistence.Seed;
using Helpdesk.Persistence.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog: simple console + rolling file logging (sinks ship with Serilog.AspNetCore).
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

#region Services

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// CORS: allow the local Angular dev server. AllowCredentials + explicit origin are required for SignalR.
const string AngularCorsPolicy = "AngularClient";
builder.Services.AddCors(options =>
    options.AddPolicy(AngularCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Helpdesk API",
            Version = "v1"
        });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Token"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INotificationService,NotificationService>();
builder.Services.AddScoped<IFileStorageService,LocalFileStorageService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Email notifications: bind settings and select the provider from configuration.
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

var emailProvider = builder.Configuration["EmailSettings:Provider"];
if (string.Equals(emailProvider, "SendGrid", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IEmailSender, SendGridEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}

builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// SLA engine: policies bound from configuration; calculator is stateless, service is scoped.
builder.Services.Configure<SlaSettings>(
    builder.Configuration.GetSection("SlaSettings"));
builder.Services.AddSingleton<ISlaCalculator, SlaCalculator>();
builder.Services.AddScoped<ISlaService, SlaService>();

// Background jobs: register the job services, then wire up Hangfire (storage/server/retry).
builder.Services.AddScoped<ISlaMonitoringJob, SlaMonitoringJob>();
builder.Services.AddScoped<IReminderEmailJob, ReminderEmailJob>();
builder.Services.AddScoped<IDailyReportJob, DailyReportJob>();
builder.Services.AddScoped<ICleanupJob, CleanupJob>();
builder.Services.AddScoped<IScheduledNotificationJob, ScheduledNotificationJob>();
builder.Services.AddHangfireBackgroundJobs(builder.Configuration);
builder.Services.AddDbContext<HelpdeskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

#endregion

var app = builder.Build();

#region Seed Data

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<HelpdeskDbContext>();

    // Create the database and apply any pending migrations automatically on startup.
    await context.Database.MigrateAsync();

    await DbInitializer.SeedAsync(context);
}

#endregion

#region Middleware

// Global exception handler: return a clean JSON 500 instead of leaking stack traces.
app.UseExceptionHandler(errorApp =>
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new { error = "An unexpected error occurred." });
    }));

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// Only force HTTPS outside development so the local HTTP profile (port 5220) works without a dev cert.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors(AngularCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

// Hangfire dashboard + recurring job registration. Dashboard is open in Development,
// Admin-only otherwise.
app.UseHangfireBackgroundJobs(app.Configuration, app.Environment.IsDevelopment());

app.MapControllers();

#endregion
app.MapHub<NotificationHub>("/hubs/notifications");
app.Run();