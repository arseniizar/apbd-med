using Tutorial10.Models;

namespace Tutorial10.Repository.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetPatientWithDetailsAsync(int idPatient);
}