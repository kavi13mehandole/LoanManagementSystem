
using System.ComponentModel.DataAnnotations;

namespace LoanManagement.Application.DTOs.LoanApplications;

public class CreateLoanApplicationRequest
{
    [Range(1, int.MaxValue)]
    public int LoanProductId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal RequestedAmount { get; set; }

    [Range(1, 600)]
    public int RequestedTenureMonths { get; set; }
}
