using Helpdesk.Application.Features.Tickets.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Common;
using Helpdesk.Domain.Entities.Tickets;
using Helpdesk.Infrastructure.Notifications;
using Helpdesk.Infrastructure.Storage;
using Helpdesk.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly HelpdeskDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;
        private readonly IEmailNotificationService _emailNotificationService;

        public TicketController(
            HelpdeskDbContext context,
            ICurrentUserService currentUser, IFileStorageService fileStorageService,INotificationService notificationService,
            IEmailNotificationService emailNotificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
            _emailNotificationService = emailNotificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket(
            TicketRequestDto request)
        {
            var openStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(x => x.Code == "OPEN");

            if (openStatus == null)
            {
                return BadRequest("Open status not configured.");
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),

                TicketNumber =
                    $"TKT-{DateTime.UtcNow:yyyyMMddHHmmss}",

                Title = request.Title,

                Description = request.Description,

                StatusId = openStatus.Id,

                PriorityId = request.PriorityId,

                CategoryId = request.CategoryId,

                CreatedByUserId = _currentUser.UserId,

                CreatedOn = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

            await _emailNotificationService.NotifyTicketCreatedAsync(ticket.Id);

            return Ok(new
            {
                Message = "Ticket created successfully.",
                TicketId = ticket.Id
            });
        }



        [HttpGet]
        public async Task<IActionResult> GetTickets(
        [FromQuery] GetTicketsRequestDto request)
        {
            var query = _context.Tickets
                .Include(x => x.Status)
                .Include(x => x.Priority)
                .Include(x => x.Category)
                .AsQueryable();

            // ROLE-BASED FILTERING

            if (_currentUser.Role == "Employee")
            {
                query = query.Where(x =>
                    x.CreatedByUserId == _currentUser.UserId);
            }
            else if (_currentUser.Role == "Support Engineer")
            {
                query = query.Where(x =>
                    x.AssignedToUserId == _currentUser.UserId);
            }

            // SEARCH

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(x =>
                    x.Title.Contains(request.Search) ||
                    x.Description.Contains(request.Search) ||
                    x.TicketNumber.Contains(request.Search));
            }

            // STATUS FILTER

            if (request.StatusId.HasValue)
            {
                query = query.Where(x =>
                    x.StatusId == request.StatusId.Value);
            }

            // PRIORITY FILTER

            if (request.PriorityId.HasValue)
            {
                query = query.Where(x =>
                    x.PriorityId == request.PriorityId.Value);
            }

            // TOTAL COUNT

            var totalCount = await query.CountAsync();

            // PAGINATION

            var tickets = await query
                .OrderByDescending(x => x.CreatedOn)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TicketResponseDto
                {
                    Id = x.Id,
                    TicketNumber = x.TicketNumber,
                    Title = x.Title,
                    Description = x.Description,
                    StatusName = x.Status.Name,
                    PriorityName = x.Priority.Name,
                    CategoryName = x.Category.Name,
                    RequesterName = _context.Users
                        .Where(u => u.Id == x.CreatedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    AssigneeName = _context.Users
                        .Where(u => u.Id == x.AssignedToUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    CommentCount = _context.TicketComments
                        .Count(c => c.TicketId == x.Id),
                    AttachmentCount = _context.TicketAttachments
                        .Count(a => a.TicketId == x.Id),
                    CreatedAt = x.CreatedOn,
                    UpdatedAt = x.ModifiedOn
                })
                .ToListAsync();

            return Ok(new
            {
                Items = tickets,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            });

        }
        [HttpPut("{ticketId}/assign")]
        [Authorize(Roles = "Admin,Support Engineer")]
        public async Task<IActionResult> AssignTicket(
    Guid ticketId,
    AssignTicketRequestDto request)
        {
            var ticket = await _context.Tickets
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == request.AssignedToUserId);

            if (user == null)
            {
                return BadRequest("Assigned user not found.");
            }

            var assignedStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(x => x.Code == "ASSIGNED");

            if (assignedStatus == null)
            {
                return BadRequest("Assigned status not configured.");
            }

            ticket.AssignedToUserId = request.AssignedToUserId;

            ticket.StatusId = assignedStatus.Id;

            ticket.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _notificationService
    .SendTicketAssigned(
        request.AssignedToUserId,
        new
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Message = "New ticket assigned"
        });

            await _emailNotificationService.NotifyTicketAssignedAsync(ticket.Id);

            return Ok("Ticket assigned successfully.");
        }
        [HttpPut("{ticketId}/status")]
        public async Task<IActionResult> UpdateStatus(
    Guid ticketId,
    UpdateTicketStatusRequestDto request)
        {
            var ticket = await _context.Tickets
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // EMPLOYEE CAN ONLY UPDATE OWN TICKETS

            if (_currentUser.Role == "Employee" &&
                ticket.CreatedByUserId != _currentUser.UserId)
            {
                return Forbid();
            }

            var newStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(x => x.Id == request.StatusId);

            if (newStatus == null)
            {
                return BadRequest("Invalid status.");
            }

            // SIMPLE WORKFLOW VALIDATION

            if (ticket.Status.Code == "CLOSED")
            {
                return BadRequest(
                    "Closed tickets cannot be modified.");
            }

            ticket.StatusId = request.StatusId;

            ticket.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _emailNotificationService.NotifyStatusChangedAsync(ticket.Id);

            return Ok("Ticket status updated successfully.");
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<IActionResult> AddComment(
    Guid ticketId,
    AddCommentRequestDto request)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // EMPLOYEE CANNOT ADD INTERNAL COMMENTS

            if (_currentUser.Role == "Employee" &&
                request.IsInternal)
            {
                return BadRequest(
                    "Employees cannot add internal comments.");
            }

            var comment = new TicketComment
            {
                Id = Guid.NewGuid(),

                TicketId = ticketId,

                UserId = _currentUser.UserId,

                Comment = request.Body,

                IsInternal = request.IsInternal,

                CreatedOn = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);

            // AUDIT LOG

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),

                EntityName = "Ticket",

                EntityId = ticket.Id,

                Action = "Comment Added",

                Changes = request.Body,

                PerformedByUserId = _currentUser.UserId,

                CreatedOn = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _emailNotificationService.NotifyCommentAddedAsync(ticketId);

            return Ok("Comment added successfully.");
        }
        [HttpGet("{ticketId}/comments")]
        public async Task<IActionResult> GetComments(
    Guid ticketId)
        {
            var query = _context.TicketComments
                .Include(x => x.User)
                .Where(x => x.TicketId == ticketId);

            // EMPLOYEE CANNOT SEE INTERNAL COMMENTS

            if (_currentUser.Role == "Employee")
            {
                query = query.Where(x => !x.IsInternal);
            }

            var comments = await query
                .OrderBy(x => x.CreatedOn)
                .Select(x => new TicketCommentResponseDto
                {
                    Id = x.Id,

                    TicketId = x.TicketId,

                    AuthorName =
                        x.User.FirstName + " " + x.User.LastName,

                    AuthorRole = x.User.Role.Name,

                    Body = x.Comment,

                    IsInternal = x.IsInternal,

                    CreatedAt = x.CreatedOn
                })
                .ToListAsync();

            return Ok(comments);
        }
        [HttpPost("{ticketId}/attachments")]
        public async Task<IActionResult> UploadAttachment(
    Guid ticketId,
    IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            // MAX 10MB

            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(
                    "File size cannot exceed 10MB.");
            }

            var allowedExtensions = new[]
            {
        ".pdf",
        ".png",
        ".jpg",
        ".jpeg",
        ".docx",
        ".xlsx"
    };

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type.");
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            var result =
                await _fileStorageService.SaveFileAsync(file);

            var attachment = new TicketAttachment
            {
                Id = Guid.NewGuid(),

                TicketId = ticketId,

                OriginalFileName = file.FileName,

                StoredFileName = result.StoredFileName,

                FilePath = result.FilePath,

                ContentType = file.ContentType,

                FileSize = file.Length,

                UploadedByUserId = _currentUser.UserId,

                CreatedOn = DateTime.UtcNow
            };

            _context.TicketAttachments.Add(attachment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "File uploaded successfully.",

                attachment.Id,

                attachment.FilePath
            });
        }
        [HttpGet("{ticketId}/attachments")]
        public async Task<IActionResult> GetAttachments(
    Guid ticketId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // EMPLOYEE CAN ONLY VIEW OWN TICKETS

            if (_currentUser.Role == "Employee" &&
                ticket.CreatedByUserId != _currentUser.UserId)
            {
                return Forbid();
            }

            var attachments = await _context.TicketAttachments
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new TicketAttachmentResponseDto
                {
                    Id = x.Id,

                    TicketId = x.TicketId,

                    FileName = x.OriginalFileName,

                    ContentType = x.ContentType,

                    FileSize = x.FileSize,

                    UploadedByName = _context.Users
                        .Where(u => u.Id == x.UploadedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),

                    UploadedAt = x.CreatedOn
                })
                .ToListAsync();

            return Ok(attachments);
        }
        [HttpGet("{ticketId}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(
    Guid ticketId,
    Guid attachmentId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // EMPLOYEE CAN ONLY ACCESS OWN TICKETS

            if (_currentUser.Role == "Employee" &&
                ticket.CreatedByUserId != _currentUser.UserId)
            {
                return Forbid();
            }

            var attachment = await _context.TicketAttachments
                .FirstOrDefaultAsync(x =>
                    x.Id == attachmentId &&
                    x.TicketId == ticketId);

            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                attachment.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Physical file not found.");
            }

            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(fullPath);

            return File(
                fileBytes,
                attachment.ContentType,
                attachment.OriginalFileName);
        }
    }
}
