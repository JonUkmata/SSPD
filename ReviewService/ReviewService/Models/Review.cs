using System;

namespace ReviewService.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
