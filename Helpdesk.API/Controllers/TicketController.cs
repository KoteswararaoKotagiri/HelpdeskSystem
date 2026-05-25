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

        public TicketController(
            HelpdeskDbContext context,
            ICurrentUserService currentUser, IFileStorageService fileStorageService,INotificationService notificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
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
                    Status = x.Status.Name,
                    Priority = x.Priority.Name,
                    Category = x.Category.Name,
                    CreatedOn = x.CreatedOn
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = tickets
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

                Comment = request.Comment,

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

                Changes = request.Comment,

                PerformedByUserId = _currentUser.UserId,

                CreatedOn = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

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

                    UserName =
                        x.User.FirstName + " " + x.User.LastName,

                    Comment = x.Comment,

                    IsInternal = x.IsInternal,

                    CreatedOn = x.CreatedOn
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

                    FileName = x.OriginalFileName,

                    ContentType = x.ContentType,

                    FileSize = x.FileSize,

                    CreatedOn = x.CreatedOn
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
