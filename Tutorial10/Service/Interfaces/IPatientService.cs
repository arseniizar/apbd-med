using Tutorial10.DTOs;

namespace Tutorial10.Service.Interfaces;

public interface IPatientService
{
    Task<PatientDataResponseDto?> GetPatientDataAsync(int idPatient);
}