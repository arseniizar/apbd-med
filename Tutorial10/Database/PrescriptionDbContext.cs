using Microsoft.EntityFrameworkCore;
using Tutorial10.Models;

namespace Tutorial10.Database;

public class PrescriptionDbContext : DbContext
{
    public PrescriptionDbContext(DbContextOptions<PrescriptionDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Medicament> Medicaments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicament> PrescriptionMedicaments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasKey(pm => new { pm.IdMedicament, pm.IdPrescription });

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Medicament)
            .WithMany(m => m.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdMedicament);

        modelBuilder.Entity<PrescriptionMedicament>()
            .HasOne(pm => pm.Prescription)
            .WithMany(p => p.PrescriptionMedicaments)
            .HasForeignKey(pm => pm.IdPrescription);

        // Seed Data (optional, for testing)
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor { IdDoctor = 1, FirstName = "John", LastName = "Smith", Email = "j.smith@clinic.com" },
            new Doctor { IdDoctor = 2, FirstName = "Alice", LastName = "Wonderland", Email = "a.wonder@clinic.com" }
        );

        modelBuilder.Entity<Medicament>().HasData(
            new Medicament { IdMedicament = 1, Name = "MedA", Description = "For pain", Type = "Analgesic" },
            new Medicament { IdMedicament = 2, Name = "MedB", Description = "For infection", Type = "Antibiotic" }
        );
    }
}