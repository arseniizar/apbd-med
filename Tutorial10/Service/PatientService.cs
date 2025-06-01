using Tutorial10.DTOs;
using Tutorial10.Repository.Interfaces;
using Tutorial10.Service.Interfaces;

namespace Tutorial10.Service;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDataResponseDto?> GetPatientDataAsync(int idPatient)
    {
        var patient = await _patientRepository.GetPatientWithDetailsAsync(idPatient);
        if (patient == null)
        {
            return null;
        }

        return new PatientDataResponseDto
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions
                .OrderByDescending(p => p.DueDate)
                .Select(p => new PrescriptionDetailResponseDto
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorInPrescriptionResponseDto
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName,
                        LastName = p.Doctor.LastName,
                        Email = p.Doctor.Email
                    },
                    Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentInPrescriptionResponseDto
                    {
                        IdMedicament = pm.Medicament.IdMedicament,
                        Name = pm.Medicament.Name,
                        Dose = pm.Dose,
                        Description = pm.Details
                    }).ToList()
                }).ToList()
        };
    }
}