using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class CustomerProfile
{
    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();

    public virtual User User { get; set; } = null!;
}
