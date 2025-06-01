using Moq;
using Tutorial10.Models;
using Tutorial10.Repository.Interfaces;
using Tutorial10.Service;

namespace Tutorial10.Tests;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _mockPatientRepository = new Mock<IPatientRepository>();
        _service = new PatientService(_mockPatientRepository.Object);
    }

    private Patient CreateTestPatientWithPrescriptions(int patientId, int numPrescriptions, int medsPerPrescription)
    {
        var patient = new Patient
        {
            IdPatient = patientId,
            FirstName = "Test",
            LastName = "User",
            Birthdate = new DateTime(1988, 5, 15)
        };

        for (int i = 0; i < numPrescriptions; i++)
        {
            var prescription = new Prescription
            {
                IdPrescription = 100 + i,
                IdPatient = patientId,
                Patient = patient,
                Date = DateTime.UtcNow.AddDays(-(i * 10)),
                DueDate = DateTime.UtcNow.AddDays(-(i * 10) + 7),
                IdDoctor = 1,
                Doctor = new Doctor { IdDoctor = 1, FirstName = "Dr.", LastName = "Who", Email = "doc@who.com" }
            };
            for (int j = 0; j < medsPerPrescription; j++)
            {
                prescription.PrescriptionMedicaments.Add(new PrescriptionMedicament
                {
                    IdMedicament = 200 + j,
                    Medicament = new Medicament
                        { IdMedicament = 200 + j, Name = $"Med{j}", Type = "TestType", Description = "TestDesc" },
                    IdPrescription = prescription.IdPrescription,
                    Dose = j + 1,
                    Details = $"Details for Med{j}"
                });
            }

            patient.Prescriptions.Add(prescription);
        }

        return patient;
    }


    [Fact]
    public async Task GetPatientDataAsync_PatientExistsWithPrescriptions_ReturnsCorrectData()
    {
        var patientId = 1;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 2, 1);
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        Assert.Equal(patientId, result.IdPatient);
        Assert.Equal(testPatient.FirstName, result.FirstName);
        Assert.Equal(2, result.Prescriptions.Count());
        Assert.Equal(1, result.Prescriptions.First().Medicaments.Count());
        Assert.NotNull(result.Prescriptions.First().Doctor);
    }

    [Fact]
    public async Task GetPatientDataAsync_PatientExistsNoPrescriptions_ReturnsDataWithEmptyPrescriptions()
    {
        var patientId = 2;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 0, 0);
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        Assert.Equal(patientId, result.IdPatient);
        Assert.Empty(result.Prescriptions);
    }

    [Fact]
    public async Task GetPatientDataAsync_PatientNotFound_ReturnsNull()
    {
        var patientId = 99;
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync((Patient)null);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPatientDataAsync_PrescriptionsAreSortedByDueDateDescending()
    {
        var patientId = 3;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 3, 1);
        testPatient.Prescriptions.ElementAt(0).DueDate = DateTime.UtcNow.AddDays(1);
        testPatient.Prescriptions.ElementAt(1).DueDate = DateTime.UtcNow.AddDays(5);
        testPatient.Prescriptions.ElementAt(2).DueDate = DateTime.UtcNow.AddDays(3);
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        var dueDates = result.Prescriptions.Select(p => p.DueDate).ToList();
        Assert.Equal(dueDates.OrderByDescending(d => d), dueDates);
    }

    [Fact]
    public async Task GetPatientDataAsync_MapsMedicamentDetailsCorrectly()
    {
        var patientId = 4;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 1, 1);
        var sourceMed = testPatient.Prescriptions.First().PrescriptionMedicaments.First();
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        var resultMed = result.Prescriptions.First().Medicaments.First();
        Assert.Equal(sourceMed.Medicament.Name, resultMed.Name);
        Assert.Equal(sourceMed.Dose, resultMed.Dose);
        Assert.Equal(sourceMed.Details, resultMed.Description);
    }

    [Fact]
    public async Task GetPatientDataAsync_MapsDoctorDetailsCorrectly()
    {
        var patientId = 5;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 1, 0);
        var sourceDoctor = testPatient.Prescriptions.First().Doctor;
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        var resultDoctor = result.Prescriptions.First().Doctor;
        Assert.Equal(sourceDoctor.FirstName, resultDoctor.FirstName);
        Assert.Equal(sourceDoctor.LastName, resultDoctor.LastName);
        Assert.Equal(sourceDoctor.Email, resultDoctor.Email);
    }

    [Fact]
    public async Task GetPatientDataAsync_PatientWithMultiplePrescriptionsMultipleMeds_MapsAll()
    {
        var patientId = 6;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 2, 3);
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Prescriptions.Count());
        Assert.All(result.Prescriptions, p => Assert.Equal(3, p.Medicaments.Count()));
    }

    [Fact]
    public async Task GetPatientDataAsync_MedicamentDoseCanBeNull_MapsCorrectly()
    {
        var patientId = 7;
        var testPatient = CreateTestPatientWithPrescriptions(patientId, 1, 1);
        testPatient.Prescriptions.First().PrescriptionMedicaments.First().Dose = null;
        _mockPatientRepository.Setup(r => r.GetPatientWithDetailsAsync(patientId)).ReturnsAsync(testPatient);

        var result = await _service.GetPatientDataAsync(patientId);

        Assert.NotNull(result);
        Assert.Null(result.Prescriptions.First().Medicaments.First().Dose);
    }
}