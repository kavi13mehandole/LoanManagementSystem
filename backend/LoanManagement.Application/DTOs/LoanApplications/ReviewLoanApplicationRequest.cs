
using System.ComponentModel.DataAnnotations;

namespace LoanManagement.Application.DTOs.LoanApplications;

public class ReviewLoanApplicationRequest
{
    [Required]
    [RegularExpression("^(Approved|Rejected)$")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Remarks { get; set; }
}
