using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Middlename { get; set; }

    public string? Phone { get; set; }

    public int? PostId { get; set; }

    public int? CabinetId { get; set; }

    public int? UserId { get; set; }

    public virtual Cabinet? Cabinet { get; set; }

    public virtual Post? Post { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual User? User { get; set; }

    public string FullName => $"{Lastname} {Firstname}";
}
