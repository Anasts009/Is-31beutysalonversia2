using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Cabinet
{
    public int CabinetId { get; set; }

    public string NameCabinet { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
