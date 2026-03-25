using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BeautySalon.Model;

public partial class BeautySalonContext : DbContext
{
    public BeautySalonContext()
    {
    }

    public BeautySalonContext(DbContextOptions<BeautySalonContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cabinet> Cabinets { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public object Genders { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=beauty_salon;Username=postgres;Password=111");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cabinet>(entity =>
        {
            entity.HasKey(e => e.CabinetId).HasName("cabinets_pkey");

            entity.ToTable("cabinets");

            entity.Property(e => e.CabinetId).HasColumnName("cabinet_id");
            entity.Property(e => e.NameCabinet)
                .HasMaxLength(100)
                .HasColumnName("name_cabinet");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.HasIndex(e => e.UserId, "clients_user_id_key").IsUnique();

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Middlename)
                .HasMaxLength(100)
                .HasColumnName("middlename");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Client)
                .HasForeignKey<Client>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("clients_user_id_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.HasIndex(e => e.UserId, "employees_user_id_key").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.CabinetId).HasColumnName("cabinet_id");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .HasColumnName("firstname");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .HasColumnName("lastname");
            entity.Property(e => e.Middlename)
                .HasMaxLength(100)
                .HasColumnName("middlename");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cabinet).WithMany(p => p.Employees)
                .HasForeignKey(d => d.CabinetId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("employees_cabinet_id_fkey");

            entity.HasOne(d => d.Post).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("employees_post_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("employees_user_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.RegistrationId, "payments_registration_id_key").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.RegistrationId).HasColumnName("registration_id");
            entity.Property(e => e.Summ)
                .HasPrecision(10, 2)
                .HasColumnName("summ");

            entity.HasOne(d => d.Registration).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.RegistrationId)
                .HasConstraintName("payments_registration_id_fkey");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("post_pkey");

            entity.ToTable("post");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.PostName)
                .HasMaxLength(100)
                .HasColumnName("post_name");
            entity.Property(e => e.Salary)
                .HasPrecision(10, 2)
                .HasColumnName("salary");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("registrations_pkey");

            entity.ToTable("registrations");

            entity.HasIndex(e => e.Number, "registrations_number_key").IsUnique();

            entity.Property(e => e.RegistrationId).HasColumnName("registration_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Number)
                .HasMaxLength(50)
                .HasColumnName("number");

            entity.HasOne(d => d.Client).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("registrations_client_id_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("registrations_employee_id_fkey");

            entity.HasMany(d => d.Services).WithMany(p => p.Registrations)
                .UsingEntity<Dictionary<string, object>>(
                    "RegistrationsService",
                    r => r.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("registrations_services_service_id_fkey1"),
                    l => l.HasOne<Registration>().WithMany()
                        .HasForeignKey("RegistrationId")
                        .HasConstraintName("registrations_services_registration_id_fkey1"),
                    j =>
                    {
                        j.HasKey("RegistrationId", "ServiceId").HasName("registrations_services_pkey1");
                        j.ToTable("registrations_services");
                        j.IndexerProperty<int>("RegistrationId").HasColumnName("registration_id");
                        j.IndexerProperty<int>("ServiceId").HasColumnName("service_id");
                    });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("services_pkey");

            entity.ToTable("services");

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.DescriptionService).HasColumnName("description_service");
            entity.Property(e => e.NameServices)
                .HasMaxLength(200)
                .HasColumnName("name_services");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Time).HasColumnName("time");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");
            entity.ToTable("users");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Login).HasMaxLength(100).HasColumnName("login");
            entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            // ВАЖНО: маппим нормальные свойства на реальные колонки
            entity.Property(e => e.Block)
                .HasDefaultValue(false)
                .HasColumnName("block");

            entity.Property(e => e.FirstAuth)
                .HasDefaultValue(false)
                .HasColumnName("firstauth");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
