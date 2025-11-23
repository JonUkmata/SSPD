using System;

namespace NotificationService.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
