using System;
using System.Collections.Generic;

namespace LoanManagement.Infrastructure.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int ScheduleId { get; set; }

    public string TransactionReference { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual RepaymentSchedule Schedule { get; set; } = null!;
}
