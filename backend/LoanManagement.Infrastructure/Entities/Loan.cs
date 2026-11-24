using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class Loan
{
    public int LoanId { get; set; }

    public int ApplicationId { get; set; }

    public string LoanNumber { get; set; } = null!;

    public decimal ApprovedAmount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    public int TenureMonths { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string LoanStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual LoanApplication Application { get; set; } = null!;

    public virtual ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
}
