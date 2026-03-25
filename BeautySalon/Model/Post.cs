using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Post
{
    public int PostId { get; set; }

    public string PostName { get; set; } = null!;

    public decimal? Salary { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
