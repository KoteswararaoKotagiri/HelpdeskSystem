using System.Threading;
using System.Threading.Tasks;
using Helpdesk.Application.Features.Emails;

namespace Helpdesk.Application.Interfaces
{
    // Transport abstraction. Implemented per provider (SMTP, SendGrid). Reusable by any feature
    // that needs to send email, so new notification types never change existing code.
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
