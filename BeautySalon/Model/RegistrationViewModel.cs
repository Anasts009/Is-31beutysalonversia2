using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautySalon.Models
{
    public class RegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public string ClientName { get; set; }
        public string EmployeeName { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string DateDisplay => Date.ToString("dd.MM.yyyy HH:mm");
        public string Services { get; set; }
        public decimal TotalSum { get; set; }
    }
}
