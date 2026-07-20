using Hangfire.Dashboard;

namespace Helpdesk.Infrastructure.BackgroundJobs
{
    // Restricts the Hangfire dashboard to authenticated Admins. In Development it is opened up
    // (browser navigation carries no JWT bearer token), controlled by the caller.
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly bool _allowAnonymous;

        public HangfireDashboardAuthorizationFilter(bool allowAnonymous)
        {
            _allowAnonymous = allowAnonymous;
        }

        public bool Authorize(DashboardContext context)
        {
            if (_allowAnonymous)
            {
                return true;
            }

            var httpContext = context.GetHttpContext();
            return httpContext.User?.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("Admin");
        }
    }
}
