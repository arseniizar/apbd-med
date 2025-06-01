namespace Tutorial10.DTOs;

public class MedicamentInPrescriptionResponseDto
{
    public int IdMedicament { get; set; }
    public string Name { get; set; }
    public int? Dose { get; set; }
    public string Description { get; set; }
}

public class DoctorInPrescriptionResponseDto
{
    public int IdDoctor { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class PrescriptionDetailResponseDto
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public IEnumerable<MedicamentInPrescriptionResponseDto> Medicaments { get; set; }
    public DoctorInPrescriptionResponseDto Doctor { get; set; }
}

public class PatientDataResponseDto
{
    public int IdPatient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthdate { get; set; }
    public IEnumerable<PrescriptionDetailResponseDto> Prescriptions { get; set; }
}