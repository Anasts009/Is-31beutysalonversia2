using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class User
{
    public int UserId { get; set; }
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int RoleId { get; set; }

    public bool Block { get; set; }
    public bool FirstAuth { get; set; }

    public virtual Client? Client { get; set; }
    public virtual Employee? Employee { get; set; }
    public virtual Role Role { get; set; } = null!;
}
