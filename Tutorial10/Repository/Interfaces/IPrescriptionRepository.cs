using Tutorial10.Models;

namespace Tutorial10.Repository.Interfaces;

public interface IPrescriptionRepository
{
    Task<Patient?> GetPatientByIdAsync(int idPatient);
    Task<Patient> AddPatientAsync(Patient patient);
    Task<Doctor?> GetDoctorByIdAsync(int idDoctor);
    Task<Medicament?> GetMedicamentByIdAsync(int idMedicament);
    Task<Prescription> AddPrescriptionAsync(Prescription prescription);
    Task AddPrescriptionMedicamentAsync(PrescriptionMedicament prescriptionMedicament);
    Task<int> SaveChangesAsync();
}