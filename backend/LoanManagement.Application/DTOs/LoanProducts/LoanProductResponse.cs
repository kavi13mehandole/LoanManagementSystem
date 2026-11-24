namespace LoanManagement.Application.DTOs.LoanProducts;

public class LoanProductResponse
{
    public int LoanProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal MinLoanAmount { get; set; }

    public decimal MaxLoanAmount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    public int MinTenureMonths { get; set; }

    public int MaxTenureMonths { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}