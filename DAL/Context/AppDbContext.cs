namespace DAL.Context;

public sealed class AppDbContext : DbContext
{
    private const int MaxRoleNameLength = 50;
    private const int MaxEmailLength    = 256;
    private const int MaxNameLength     = 100;
    private const int MaxTitleLength    = 200;
    private const int MaxCompanyLength  = 200;

    private const string SalaryColumnType = "decimal(18,2)";

    private const int AdminRoleId    = 1;
    private const int EmployerRoleId = 2;
    private const int EmployeeRoleId = 3;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        ArgumentNullException.ThrowIfNull(options);
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Role> Roles { get; set; } = null!;

    public DbSet<Resume> Resumes { get; set; } = null!;

    public DbSet<Vacancy> Vacancies { get; set; } = null!;

    public DbSet<Application> Applications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        ConfigureRole(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureResume(modelBuilder);
        ConfigureVacancy(modelBuilder);
        ConfigureApplication(modelBuilder);

        SeedRoles(modelBuilder);
    }

    private static void ConfigureRole(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name)
                  .IsRequired()
                  .HasMaxLength(MaxRoleNameLength);
        });

    private static void ConfigureUser(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email)
                  .IsRequired()
                  .HasMaxLength(MaxEmailLength);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName)
                  .IsRequired()
                  .HasMaxLength(MaxNameLength);
            entity.Property(u => u.LastName)
                  .IsRequired()
                  .HasMaxLength(MaxNameLength);
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

    private static void ConfigureResume(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title)
                  .IsRequired()
                  .HasMaxLength(MaxTitleLength);
            entity.Property(r => r.Description).IsRequired();
            entity.Property(r => r.Skills).IsRequired();
            entity.Property(r => r.ExpectedSalary)
                  .HasColumnType(SalaryColumnType);
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Resumes)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    private static void ConfigureVacancy(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Title)
                  .IsRequired()
                  .HasMaxLength(MaxTitleLength);
            entity.Property(v => v.Description).IsRequired();
            entity.Property(v => v.Company)
                  .IsRequired()
                  .HasMaxLength(MaxCompanyLength);
            entity.Property(v => v.RequiredSkills).IsRequired();
            entity.Property(v => v.Salary)
                  .HasColumnType(SalaryColumnType);
            entity.HasOne(v => v.User)
                  .WithMany(u => u.Vacancies)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    private static void ConfigureApplication(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Type).IsRequired();
            entity.Property(a => a.Status).IsRequired();
            entity.Property(a => a.AppliedAt).IsRequired();

            entity.HasIndex(a => new { a.ResumeId, a.VacancyId });

            entity.HasOne(a => a.Resume)
                  .WithMany(r => r.Applications)
                  .HasForeignKey(a => a.ResumeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Vacancy)
                  .WithMany(v => v.Applications)
                  .HasForeignKey(a => a.VacancyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

    private static void SeedRoles(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = AdminRoleId,    Name = "Administrator" },
            new Role { Id = EmployerRoleId, Name = "Employer" },
            new Role { Id = EmployeeRoleId, Name = "Employee" });
}
