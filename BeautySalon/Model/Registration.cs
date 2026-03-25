using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Registration
{
    public int RegistrationId { get; set; }
    public int ClientId { get; set; }
    public int EmployeeId { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }

    public virtual Client Client { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}