
namespace LoanManagement.Application.DTOs.LoanApplications;

public class LoanApplicationResponse
{
    public int ApplicationId { get; set; }
    public int LoanProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public int RequestedTenureMonths { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}
