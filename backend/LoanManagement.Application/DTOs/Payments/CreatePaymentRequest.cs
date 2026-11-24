
using System.ComponentModel.DataAnnotations;

namespace LoanManagement.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    [Range(1, int.MaxValue)]
    public int ScheduleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;
}
