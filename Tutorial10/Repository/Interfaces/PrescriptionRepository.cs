using Tutorial10.Database;
using Tutorial10.Models;

namespace Tutorial10.Repository.Interfaces;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly PrescriptionDbContext _context;

    public PrescriptionRepository(PrescriptionDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetPatientByIdAsync(int idPatient)
    {
        return await _context.Patients.FindAsync(idPatient);
    }

    public async Task<Patient> AddPatientAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
        return patient;
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int idDoctor)
    {
        return await _context.Doctors.FindAsync(idDoctor);
    }

    public async Task<Medicament?> GetMedicamentByIdAsync(int idMedicament)
    {
        return await _context.Medicaments.FindAsync(idMedicament);
    }

    public async Task<Prescription> AddPrescriptionAsync(Prescription prescription)
    {
        await _context.Prescriptions.AddAsync(prescription);
        return prescription;
    }

    public async Task AddPrescriptionMedicamentAsync(PrescriptionMedicament prescriptionMedicament)
    {
        await _context.PrescriptionMedicaments.AddAsync(prescriptionMedicament);
    }


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}