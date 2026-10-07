using CounsellingServices.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CounsellingServices.Api.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<CounsellorProfile> CounsellorProfiles => Set<CounsellorProfile>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<CounsellorProfile>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.RegistrationNumber).IsUnique();
            entity.Property(x => x.Specialty).HasMaxLength(120).IsRequired();
            entity.HasOne(x => x.User)
                .WithOne(x => x.CounsellorProfile)
                .HasForeignKey<CounsellorProfile>(x => x.UserId);
        });

        modelBuilder.Entity<AvailabilitySlot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.CounsellorProfileId, x.StartsAt, x.EndsAt }).IsUnique();
            entity.HasOne(x => x.CounsellorProfile)
                .WithMany(x => x.AvailabilitySlots)
                .HasForeignKey(x => x.CounsellorProfileId);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(400).IsRequired();
            entity.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CounsellorProfile)
                .WithMany()
                .HasForeignKey(x => x.CounsellorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AvailabilitySlot)
                .WithMany()
                .HasForeignKey(x => x.AvailabilitySlotId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
