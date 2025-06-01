using System.ComponentModel.DataAnnotations;

namespace Tutorial10.DTOs;

public class PatientDto
{
    public int IdPatient { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public DateTime Birthdate { get; set; }
}

public class MedicamentPrescriptionDto
{
    public int IdMedicament { get; set; }
    public int? Dose { get; set; }
    [Required] public string Description { get; set; }
}

public class AddPrescriptionRequestDto
{
    [Required] public PatientDto Patient { get; set; }
    [Required] public IEnumerable<MedicamentPrescriptionDto> Medicaments { get; set; }
    [Required] public DateTime Date { get; set; }
    [Required] public DateTime DueDate { get; set; }
    [Required] public int IdDoctor { get; set; }
}