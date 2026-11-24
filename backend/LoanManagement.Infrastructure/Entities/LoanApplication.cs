using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class LoanApplication
{
    public int ApplicationId { get; set; }

    public int CustomerId { get; set; }

    public int LoanProductId { get; set; }

    public decimal RequestedAmount { get; set; }

    public int RequestedTenureMonths { get; set; }

    public string ApplicationStatus { get; set; } = null!;

    public DateTime AppliedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? Remarks { get; set; }

    public virtual CustomerProfile Customer { get; set; } = null!;

    public virtual Loan? Loan { get; set; }

    public virtual LoanProduct LoanProduct { get; set; } = null!;
}
