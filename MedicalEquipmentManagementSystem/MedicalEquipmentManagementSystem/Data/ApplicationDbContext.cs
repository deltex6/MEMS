using MedicalEquipmentManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalEquipmentManagementSystem.Data
{
    /// <summary>
    /// Kontekst bazy danych aplikacji zarządzania sprzętem medycznym.
    /// </summary>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        /// <summary>
        /// Kolekcja sprzętu medycznego.
        /// </summary>
        public DbSet<MedicalEquipment> MedicalEquipments { get; set; }

        /// <summary>
        /// Konfiguruje model bazy danych.
        /// </summary>
        /// <param name="modelBuilder">Builder modelu.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MedicalEquipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SerialNumber).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Manufacturer).HasMaxLength(150);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });
        }
    }
}
