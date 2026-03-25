using System;
using System.Collections.Generic;

namespace BeautySalon.Model;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int RegistrationId { get; set; }

    public decimal Summ { get; set; }

    public virtual Registration Registration { get; set; } = null!;
}
