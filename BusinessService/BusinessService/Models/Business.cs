using System;

namespace BusinessService.Models
{
    public class Business
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Industry { get; set; }
    }
}
