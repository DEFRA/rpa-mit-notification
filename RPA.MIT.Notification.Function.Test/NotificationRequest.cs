using System.ComponentModel.DataAnnotations;

namespace RPA.MIT.Notification.Function.Test
{
    public class NotificationRequest
    {
        public string? Id { get; set; }
        public string? InvoiceId { get; set; }
        public string? Scheme { get; set; }
        public string? Action { get; set; }
        public string? EmailAddress { get; set; }
        public string? RequestedBy { get; set; }
        public Personalisation? Data { get; set; }
    }
}
