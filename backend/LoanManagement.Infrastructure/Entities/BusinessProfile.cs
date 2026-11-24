using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class BusinessProfile
{
    public int BusinessId { get; set; }

    public int UserId { get; set; }

    public string BusinessName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<LoanProduct> LoanProducts { get; set; } = new List<LoanProduct>();

    public virtual User User { get; set; } = null!;
}
