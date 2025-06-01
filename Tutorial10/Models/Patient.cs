using System.ComponentModel.DataAnnotations;

namespace Tutorial10.Models;

public class Patient
{
    [Key] public int IdPatient { get; set; }
    [Required] [MaxLength(100)] public string FirstName { get; set; }
    [Required] [MaxLength(100)] public string LastName { get; set; }
    public DateTime Birthdate { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}