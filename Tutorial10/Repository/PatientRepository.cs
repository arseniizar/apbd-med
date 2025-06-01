using Microsoft.EntityFrameworkCore;
using Tutorial10.Database;
using Tutorial10.Models;
using Tutorial10.Repository.Interfaces;

namespace Tutorial10.Repository;

public class PatientRepository : IPatientRepository
{
    private readonly PrescriptionDbContext _context;

    public PatientRepository(PrescriptionDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetPatientWithDetailsAsync(int idPatient)
    {
        return await _context.Patients
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.Doctor)
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.PrescriptionMedicaments)
            .ThenInclude(pm => pm.Medicament)
            .Where(p => p.IdPatient == idPatient)
            .FirstOrDefaultAsync();
    }
}