using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Models;

public partial class CrmCornerContext : IdentityDbContext<AppUser, AppRole, string>
{
    public CrmCornerContext()
    {
    }

    public CrmCornerContext(DbContextOptions<CrmCornerContext> options)
        : base(options)
    {
    }


    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<ChatHistory> ChatHistories { get; set; }
    public DbSet<CustomerN> CustomerNs { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<TaskComp> TaskComps { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }

    public DbSet<ToDo> ToDos { get; set; }
    public DbSet<ToDoList> ToDoList { get; set; }

    public DbSet<TaskCompLog> TaskCompLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<BigecSayfaDeneme>BigeçSayfaDenemes  { get; set; }

    public DbSet<PostSaleInfo> PostSaleInfos { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=94.73.148.165;database=u1613932_db877;port=3306;user=u1613932_user877;password=@TOSf4:8c8@Wb6n:;Charset=utf8mb4;ConvertZeroDateTime=True;", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.16-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserLogin<string>>().HasNoKey();
        modelBuilder.Entity<IdentityUserRole<string>>().HasNoKey();
        modelBuilder.Entity<IdentityUserToken<string>>().HasNoKey();
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CustomerN>()
        .HasOne(c => c.AppUser) // Her CustomerN bir AppUser ile ilişkilidir.
        .WithMany(u => u.Customers) // Bir AppUser birden fazla CustomerN ile ilişkilendirilebilir.
        .HasForeignKey(c => c.AppUserId) // Yabancı anahtar olarak CustomerN'deki AppUserId kullanılır.
        .IsRequired(false); // Eğer her müşteri için bir AppUser olması zorunlu değilse, IsRequired(false) kullanın.

        //modelBuilder.Entity<TaskComp>().

        modelBuilder.Entity<TaskComp>()
           .HasKey(t => t.TaskId); // TaskId, TaskComp için birincil anahtar olarak tanımlanır

        // TaskComp ve AppUser arasındaki ilişkiyi konfigure edin
        modelBuilder.Entity<TaskComp>()
            .HasOne(t => t.AppUser) // TaskComp, bir AppUser ile ilişkilidir.
            .WithMany(u => u.TaskComps) // Bir AppUser, birçok TaskComp ile ilişkilendirilebilir.
            .HasForeignKey(t => t.UserId); // UserId yabancı anahtar olarak kullanılır.

        modelBuilder.Entity<TaskCompLog>().HasKey(t => t.LogId);

        //modelBuilder.Entity<EmailProperty>().HasNoKey();


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

        modelBuilder.Entity<AppUser>(entity => {
            entity.ToTable(name: "users");
        });

        modelBuilder.Entity<AppRole>(entity => {
            entity.ToTable(name: "roles");
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity => {
            entity.ToTable("userroles");
        });

        modelBuilder.Entity<IdentityUserClaim<string>>(entity => {
            entity.ToTable("userclaims");
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity => {
            entity.ToTable("userlogins");
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => {
            entity.ToTable("roleclaims");
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity => {
            entity.ToTable("usertokens");
        });

        OnModelCreatingPartial(modelBuilder);
        modelBuilder.Entity<AppUserRole>()
       .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<AppUserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
    }

    public Task InsertOneAsync(ChatHistory chatHistory)
    {
        throw new NotImplementedException();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}