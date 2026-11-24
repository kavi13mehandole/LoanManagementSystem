using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class RepaymentSchedule
{
    public int ScheduleId { get; set; }

    public int LoanId { get; set; }

    public int InstallmentNumber { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal InterestAmount { get; set; }

    public decimal Emiamount { get; set; }

    public decimal PaidAmount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public virtual Loan Loan { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
