using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class LoanProduct
{
    public int LoanProductId { get; set; }

    public int BusinessId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal MinLoanAmount { get; set; }

    public decimal MaxLoanAmount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    public int MinTenureMonths { get; set; }

    public int MaxTenureMonths { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BusinessProfile Business { get; set; } = null!;

    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}
