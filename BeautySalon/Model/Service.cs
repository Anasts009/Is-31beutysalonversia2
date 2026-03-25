using System;
using System.Collections.Generic;

namespace BeautySalon.Model
{
    public partial class Service
    {
        public int ServiceId { get; set; }
        public string NameServices { get; set; } = null!;
        public string? DescriptionService { get; set; }
        public decimal Price { get; set; }

        // В БД integer — минуты
        public int Time { get; set; }  // минуты

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}