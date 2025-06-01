using Tutorial10.DTOs;
using Tutorial10.Models;
using Tutorial10.Repository.Interfaces;
using Tutorial10.Service.Interfaces;

namespace Tutorial10.Service;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<int> AddPrescriptionAsync(AddPrescriptionRequestDto requestDto)
    {
        if (requestDto.DueDate < requestDto.Date)
        {
            throw new ArgumentException("DueDate must be greater than or equal to Date.");
        }

        if (requestDto.Medicaments.Count() > 10)
        {
            throw new ArgumentException("A prescription can include a maximum of 10 medications.");
        }

        var doctor = await _prescriptionRepository.GetDoctorByIdAsync(requestDto.IdDoctor);
        if (doctor == null)
        {
            throw new KeyNotFoundException($"Doctor with ID {requestDto.IdDoctor} not found.");
        }

        var patient = await _prescriptionRepository.GetPatientByIdAsync(requestDto.Patient.IdPatient);
        if (patient == null)
        {
            patient = new Patient
            {
                IdPatient = requestDto.Patient
                    .IdPatient,
                FirstName = requestDto.Patient.FirstName,
                LastName = requestDto.Patient.LastName,
                Birthdate = requestDto.Patient.Birthdate
            };
            await _prescriptionRepository.AddPatientAsync(patient);
        }
        else
        {
            patient.FirstName = requestDto.Patient.FirstName;
            patient.LastName = requestDto.Patient.LastName;
            patient.Birthdate = requestDto.Patient.Birthdate;
        }


        var prescription = new Prescription
        {
            Date = requestDto.Date,
            DueDate = requestDto.DueDate,
            IdPatient = patient.IdPatient,
            IdDoctor = requestDto.IdDoctor,
            Patient = patient,
            Doctor = doctor
        };

        await _prescriptionRepository.AddPrescriptionAsync(prescription);
        await _prescriptionRepository
            .SaveChangesAsync();

        foreach (var medDto in requestDto.Medicaments)
        {
            var medicament = await _prescriptionRepository.GetMedicamentByIdAsync(medDto.IdMedicament);
            if (medicament == null)
            {
                // This would ideally be part of a transaction that rolls back previous saves
                throw new KeyNotFoundException($"Medicament with ID {medDto.IdMedicament} not found.");
            }

            var prescriptionMedicament = new PrescriptionMedicament
            {
                IdMedicament = medDto.IdMedicament,
                IdPrescription = prescription.IdPrescription,
                Dose = medDto.Dose,
                Details = medDto.Description
            };
            await _prescriptionRepository.AddPrescriptionMedicamentAsync(prescriptionMedicament);
        }

        await _prescriptionRepository.SaveChangesAsync();
        return prescription.IdPrescription;
    }
}