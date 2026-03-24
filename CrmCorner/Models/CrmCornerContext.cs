using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CrmCorner.Models;

namespace CrmCorner.Models
{
    public partial class CrmCornerContext : IdentityDbContext<
        AppUser, AppRole, string,
        IdentityUserClaim<string>, AppUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public CrmCornerContext(DbContextOptions<CrmCornerContext> options)
            : base(options)
        {
        }

        public CrmCornerContext()
        {
        }

        // ---- DbSet tanımları ----
        public DbSet<Calendar> Calendars { get; set; }

        public DbSet<ChatHistory> ChatHistories { get; set; }
        public DbSet<CustomerN> CustomerNs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TaskComp> TaskComps { get; set; }
        public DbSet<FileAttachment> FileAttachments { get; set; }
        public DbSet<TodoBoard> TodoBoards { get; set; }
        public DbSet<TodoEntry> TodoEntries { get; set; }

        public DbSet<TaskCompLog> TaskCompLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BigecSayfaDeneme> BigeçSayfaDenemes { get; set; }
        public DbSet<PostSaleInfo> PostSaleInfos { get; set; }
        public DbSet<TableHeader> TableHeaders { get; set; }
        public DbSet<TaskCompNote> TaskCompNotes { get; set; }
        public DbSet<EmailList> EmailList { get; set; }
        public DbSet<SocialMediaContent> SocialMediaContents { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ApolloSettings> ApolloSettings { get; set; }
        public DbSet<ApolloContactDbModel> ApolloContacts { get; set; }

        public DbSet<PersonalBrandingContent> PersonalBrandingContents { get; set; }

        public DbSet<PersonalBrandingFeedback> PersonalBrandingFeedbacks { get; set; }



        // 🔑 Sadece AppUserRole’i DbSet olarak bırakıyoruz
        public DbSet<AppUserRole> AppUserRoles { get; set; }

        public DbSet<PipelineTask> PipelineTasks { get; set; }
        public DbSet<PipelineTaskNote> PipelineTaskNotes { get; set; }
        public DbSet<PipelineTaskLog> PipelineTaskLogs { get; set; }
        public DbSet<PipelineTaskHistory> PipelineTaskHistories { get; set; }
        public DbSet<PipelineTaskFileAttachment> PipelineTaskFileAttachments { get; set; }

        public DbSet<ApolloLabelSync> ApolloLabelSyncs { get; set; }


        public DbSet<FinanceInvoice> FinanceInvoices { get; set; }
        public DbSet<FinanceInvoiceDocument> FinanceInvoiceDocuments { get; set; }

        public DbSet<FinanceContract> FinanceContracts { get; set; }

        public DbSet<FinanceContractDocument> FinanceContractDocuments { get; set; }

        public DbSet<UserModule> UserModules { get; set; }






        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#warning Connection string'i appsettings.json içine taşı!
            optionsBuilder.UseMySql(
                "server=94.73.148.165;database=u1613932_db877;user=u1613932_user877;password=@TOSf4:8c8@Wb6n:;Charset=utf8mb4;ConvertZeroDateTime=True;",
                Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.18-mariadb"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasCharSet("utf8mb4");

            // CustomerN ↔ AppUser
            modelBuilder.Entity<CustomerN>()
                .HasOne(c => c.AppUser)
                .WithMany(u => u.Customers)
                .HasForeignKey(c => c.AppUserId)
                .IsRequired(false);

            // TaskComp
            modelBuilder.Entity<TaskComp>().HasKey(t => t.TaskId);

            modelBuilder.Entity<TaskComp>()
                .HasOne(t => t.AppUser)
                .WithMany(u => u.TaskComps)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<TaskCompLog>().HasKey(t => t.LogId);

            // Calendar tablo ayarları
            modelBuilder.Entity<Calendar>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
                entity.ToTable("Calendar");
                entity.HasIndex(e => e.Id, "Id");
                entity.Property(e => e.Id).HasColumnType("int(11)");
                entity.Property(e => e.Date).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(500);
                entity.Property(e => e.StartDate).HasMaxLength(50);
                entity.Property(e => e.EndDate).HasMaxLength(50);
            });

            // TableHeader ↔ Company
            modelBuilder.Entity<TableHeader>()
                .HasOne(th => th.Company)
                .WithMany(c => c.TableHeaders)
                .HasForeignKey(th => th.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Identity tablolarını custom tablo adlarına map et
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("users");
                entity.Property(u => u.Id).HasMaxLength(255);
            });

            modelBuilder.Entity<AppRole>(entity =>
            {
                entity.ToTable("roles");
                entity.Property(r => r.Id).HasMaxLength(255);
            });

            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("userclaims");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("roleclaims");

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("userlogins");
                entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("usertokens");
                entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });

            modelBuilder.Entity<AppUserRole>(entity =>
            {
                entity.ToTable("appuserrole");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.Property(ur => ur.UserId).HasMaxLength(255);
                entity.Property(ur => ur.RoleId).HasMaxLength(255);

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            // PipelineTask ↔ AppUser
            modelBuilder.Entity<PipelineTask>()
                .HasOne(t => t.AppUser)
                .WithMany()
                .HasForeignKey(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PipelineTask>()
                .HasOne(t => t.ResponsibleUser)
                .WithMany()
                .HasForeignKey(t => t.ResponsibleUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Feedback ↔ SocialMediaContent
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.SocialMediaContent)
                .WithMany(s => s.Feedbacks)
                .HasForeignKey(f => f.SocialMediaContentId);

            modelBuilder.Entity<TodoEntry>()
    .HasOne(e => e.TodoBoard)
    .WithMany(b => b.Entries)
    .HasForeignKey(e => e.TodoBoardId);


            modelBuilder.Entity<TodoEntry>()
    .HasOne<AppUser>(t => t.Assignee)
    .WithMany()
    .HasForeignKey(t => t.AssigneeId)
    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FinanceInvoice>()
      .HasIndex(x => new { x.CompanyId, x.ContractId, x.PeriodYear, x.PeriodMonth })
      .IsUnique();


            OnModelCreatingPartial(modelBuilder);
        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
