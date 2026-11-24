
using System.ComponentModel.DataAnnotations;

namespace LoanManagement.Application.DTOs.LoanProducts;

public class UpdateLoanProductRequest
{
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal MinLoanAmount { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal MaxLoanAmount { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal AnnualInterestRate { get; set; }

    [Range(1, 600)]
    public int MinTenureMonths { get; set; }

    [Range(1, 600)]
    public int MaxTenureMonths { get; set; }
}
