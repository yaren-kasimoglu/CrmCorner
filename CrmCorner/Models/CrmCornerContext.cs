using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Models;

public partial class CrmcornerContext : DbContext
{
    public CrmcornerContext()
    {
    }

    public CrmcornerContext(DbContextOptions<CrmcornerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=92.204.221.160;database=crmcorner;user=yaren;password=yagmuryaren123", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.14-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.IdEmployee, "FK_Customer_Employee");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IdEmployee)
                .HasColumnType("int(11)")
                .HasColumnName("idEmployee");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Status)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.IdEmployee)
                .HasConstraintName("FK_Customer_Employee");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PRIMARY");

            entity.ToTable("Department");

            entity.Property(e => e.IdDepartment)
                .HasColumnType("int(11)")
                .HasColumnName("idDepartment");
            entity.Property(e => e.DepartmentDescription)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("PRIMARY");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.IdPositions, "FK_Employee_Position");

            entity.HasIndex(e => e.IdDepartment, "fk_Employee_Department");

            entity.Property(e => e.IdEmployee)
                .HasColumnType("int(11)")
                .HasColumnName("idEmployee");
            entity.Property(e => e.EmployeeEmail)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.EmployeeName).HasMaxLength(45);
            entity.Property(e => e.EmployeePhone)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.EmployeeSurname)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IdDepartment).HasColumnType("int(11)");
            entity.Property(e => e.IdPositions)
                .HasColumnType("int(11)")
                .HasColumnName("idPositions");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.IdDepartment)
                .HasConstraintName("fk_Employee_Department");

            entity.HasOne(d => d.IdPositionsNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.IdPositions)
                .HasConstraintName("FK_Employee_Position");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.IdPositions).HasName("PRIMARY");

            entity.Property(e => e.IdPositions)
                .HasColumnType("int(11)")
                .HasColumnName("idPositions");
            entity.Property(e => e.PositionName)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
