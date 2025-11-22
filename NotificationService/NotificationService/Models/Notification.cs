using System;

namespace NotificationService.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
