using System;
using System.Collections.Generic;
using LoanManagement.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BusinessProfile> BusinessProfiles { get; set; }

    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<LoanApplication> LoanApplications { get; set; }

    public virtual DbSet<LoanProduct> LoanProducts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessProfile>(entity =>
        {
            entity.HasKey(e => e.BusinessId).HasName("PK__Business__F1EAA36E03F8CFD9");

            entity.HasIndex(e => e.UserId, "UQ__Business__1788CC4D3A30E3B5").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.BusinessName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.BusinessProfile)
                .HasForeignKey<BusinessProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessProfiles_Users");
        });

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D87A855C06");

            entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4DC0FE4FB3").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.CustomerProfile)
                .HasForeignKey<CustomerProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerProfiles_Users");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("PK__Loans__4F5AD4574CFDB556");

            entity.HasIndex(e => e.ApplicationId, "UQ__Loans__C93A4C98B6933DB8").IsUnique();

            entity.HasIndex(e => e.LoanNumber, "UQ__Loans__EEC2662808FBB869").IsUnique();

            entity.Property(e => e.AnnualInterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LoanNumber).HasMaxLength(50);
            entity.Property(e => e.LoanStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Application).WithOne(p => p.Loan)
                .HasForeignKey<Loan>(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Loans_Application");
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__LoanAppl__C93A4C994EB8D3FC");

            entity.Property(e => e.ApplicationStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.AppliedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.RequestedAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanApplications_Customer");

            entity.HasOne(d => d.LoanProduct).WithMany(p => p.LoanApplications)
                .HasForeignKey(d => d.LoanProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanApplications_Product");
        });

        modelBuilder.Entity<LoanProduct>(entity =>
        {
            entity.HasKey(e => e.LoanProductId).HasName("PK__LoanProd__0D22CCC2D9FD16DA");

            entity.Property(e => e.AnnualInterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxLoanAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinLoanAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(150);

            entity.HasOne(d => d.Business).WithMany(p => p.LoanProducts)
                .HasForeignKey(d => d.BusinessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoanProducts_Business");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38B6838E55");

            entity.HasIndex(e => e.TransactionReference, "UQ__Payments__6783D2A129FD7FAC").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionReference).HasMaxLength(100);

            entity.HasOne(d => d.Schedule).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Schedule");
        });

        modelBuilder.Entity<RepaymentSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Repaymen__9C8A5B49B083A72B");

            entity.HasIndex(e => new { e.LoanId, e.InstallmentNumber }, "UQ_RepaymentSchedules_Installment").IsUnique();

            entity.Property(e => e.Emiamount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("EMIAmount");
            entity.Property(e => e.InterestAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PrincipalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Loan).WithMany(p => p.RepaymentSchedules)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepaymentSchedules_Loan");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0F8ACEAA");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534ADE5F81B").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
