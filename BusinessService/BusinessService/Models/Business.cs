using System;

namespace BusinessService.Models
{
    public class Business
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
    }
}
