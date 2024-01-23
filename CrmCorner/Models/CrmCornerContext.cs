using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Models;

public partial class CrmCornerContext : DbContext
{
    public CrmCornerContext()
    {
    }

    public CrmCornerContext(DbContextOptions<CrmCornerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Calendar> Calendars { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<TaskComp> TaskComps { get; set; }

    public virtual DbSet<TaskCompLog> TaskCompLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=94.73.148.165;database=u1613932_db877;user=u1613932_user877;password=@TOSf4:8c8@Wb6n:", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.16-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Calendar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Calendar");

            entity.HasIndex(e => e.Id, "Id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Date).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("Company")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.IdEmployee, "IdEmployee");

            entity.HasIndex(e => e.StatusId, "StatusId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CompanyEmail).HasMaxLength(255);
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.IdEmployee).HasColumnType("int(11)");
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusId).HasColumnType("int(11)");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Companies)
                .HasForeignKey(d => d.IdEmployee)
                .HasConstraintName("Company_ibfk_2");

            entity.HasOne(d => d.Status).WithMany(p => p.Companies)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("Company_ibfk_1");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("Customer")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.CompanyId, "FK_Customer_Company");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CompanyId).HasColumnType("int(11)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PhoneNumber).HasMaxLength(45);
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Company).WithMany(p => p.Customers)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Customer_Company");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PRIMARY");

            entity
                .ToTable("Department")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.Property(e => e.IdDepartment)
                .HasColumnType("int(11)")
                .HasColumnName("idDepartment")
                 .IsRequired();
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentDescription)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("PRIMARY");

            entity
                .ToTable("Employee")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.IdPositions, "FK_Employee_Position");

            entity.HasIndex(e => e.IdDepartment, "fk_Employee_Department");

            entity.Property(e => e.IdEmployee)
                .HasColumnType("int(11)")
                .HasColumnName("idEmployee");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
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
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");

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

            entity
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.Property(e => e.IdPositions)
                .HasColumnType("int(11)")
                .HasColumnName("idPositions");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.PositionName)
                .HasMaxLength(45)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PRIMARY");

            entity.ToTable("Status");

            entity.Property(e => e.StatusId)
                .HasColumnType("int(11)")
                .HasColumnName("statusId");
            entity.Property(e => e.StatusName)
                .HasMaxLength(255)
                .HasColumnName("statusName");
        });

        modelBuilder.Entity<TaskComp>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PRIMARY");

            entity.ToTable("TaskComp");

            entity.HasIndex(e => e.CustomerId, "FK_Customer");

            entity.HasIndex(e => e.EmployeeId, "FK_Employee");

            entity.HasIndex(e => e.StatusId, "statusId");

            entity.Property(e => e.TaskId)
                .HasColumnType("int(11)")
                .HasColumnName("TaskID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId)
                .HasColumnType("int(11)")
                .HasColumnName("customerId");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EmployeeId)
                .HasColumnType("int(11)")
                .HasColumnName("employeeId");
            entity.Property(e => e.ModifiedDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusId)
                .HasColumnType("int(11)")
                .HasColumnName("statusId");
            entity.Property(e => e.TaskCompcol).HasMaxLength(45);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UploadedFile).HasColumnType("blob");
            entity.Property(e => e.UploadedFileName).HasMaxLength(255);
            entity.Property(e => e.ValueOrOffer).HasPrecision(10, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.TaskComps)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Customer");

            entity.HasOne(d => d.Employee).WithMany(p => p.TaskComps)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Employee");

            entity.HasOne(d => d.Status).WithMany(p => p.TaskComps)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("TaskComp_ibfk_1");
        });
        //modelBuilder.Entity<ChatHistory>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PRIMARY");

        //    entity.ToTable("ChatHistory");

        //    entity.HasIndex(e => e.Id, "Id");


        //    entity.Property(e => e.Id).HasColumnType("int(11)");
        //    //entity.Property(e => e.Date).HasMaxLength(50);
        //    //entity.Property(e => e.Description).HasMaxLength(300);
        //    //entity.Property(e => e.Title).HasMaxLength(200);
        //});

        modelBuilder.Entity<TaskCompLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PRIMARY");

            entity
                .ToTable("TaskCompLog")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.TaskId, "FK_TaskCompLog_TaskComp");

            entity.HasIndex(e => e.UpdatedBy, "UpdatedBy");

            entity.Property(e => e.LogId).HasColumnType("int(11)");
            entity.Property(e => e.NewValue)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.OldValue)
                .HasMaxLength(250)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TaskId).HasColumnType("int(11)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasColumnType("int(11)");
            entity.Property(e => e.UpdatedField)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskCompLogs)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TaskCompLog_TaskComp");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.TaskCompLogs)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("TaskCompLog_ibfk_2");
        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Username, "Username").IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.UserImage).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    public Task InsertOneAsync(ChatHistory chatHistory)
    {
        throw new NotImplementedException();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
