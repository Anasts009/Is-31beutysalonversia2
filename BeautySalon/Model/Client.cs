using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Client
{
    public int ClientId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Middlename { get; set; }

    public string? Phone { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual User? User { get; set; }

    public string FullName => $"{Lastname} {Firstname}";
}
