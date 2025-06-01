using Moq;
using Tutorial10.DTOs;
using Tutorial10.Models;
using Tutorial10.Repository.Interfaces;
using Tutorial10.Service;

namespace Tutorial10.Tests;

public class PrescriptionServiceTests
{
    private readonly Mock<IPrescriptionRepository> _mockPrescriptionRepository;
    private readonly PrescriptionService _service;

    public PrescriptionServiceTests()
    {
        _mockPrescriptionRepository = new Mock<IPrescriptionRepository>();
        _service = new PrescriptionService(_mockPrescriptionRepository.Object);
    }

    private AddPrescriptionRequestDto CreateValidRequestDto(int patientId = 1, int doctorId = 1, int numMeds = 1,
        bool newPatient = false)
    {
        var patientDto = new PatientDto
        {
            IdPatient = newPatient ? 0 : patientId, FirstName = "Test", LastName = "Patient",
            Birthdate = new DateTime(1990, 1, 1)
        };
        if (newPatient && patientId != 0) patientDto.IdPatient = patientId; 

        return new AddPrescriptionRequestDto
        {
            Patient = patientDto,
            Medicaments = Enumerable.Range(1, numMeds).Select(i => new MedicamentPrescriptionDto
                { IdMedicament = i, Dose = 1, Description = $"Med {i}" }).ToList(),
            Date = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(7),
            IdDoctor = doctorId
        };
    }

    [Fact]
    public async Task AddPrescriptionAsync_ValidRequestNewPatient_AddsPatientAndPrescription()
    {
        var request = CreateValidRequestDto(patientId: 100, newPatient: true); 
        var doctor = new Doctor { IdDoctor = request.IdDoctor };
        var patientToAdd = new Patient(); 
        var prescriptionToAdd = new Prescription(); 

        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync(doctor);
        _mockPrescriptionRepository.Setup(r => r.GetPatientByIdAsync(request.Patient.IdPatient))
            .ReturnsAsync((Patient)null); 
        _mockPrescriptionRepository.Setup(r => r.AddPatientAsync(It.IsAny<Patient>()))
            .Callback<Patient>(p => patientToAdd = p)
            .ReturnsAsync((Patient p) =>
            {
                p.IdPatient = request.Patient.IdPatient;
                return p;
            }); 
        _mockPrescriptionRepository.Setup(r => r.GetMedicamentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Medicament { IdMedicament = id, Name = "TestMed" });
        _mockPrescriptionRepository.Setup(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()))
            .Callback<Prescription>(p => prescriptionToAdd = p)
            .ReturnsAsync((Prescription p) =>
            {
                p.IdPrescription = 1;
                return p;
            }); 
        _mockPrescriptionRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var resultId = await _service.AddPrescriptionAsync(request);

        Assert.Equal(1, resultId);
        _mockPrescriptionRepository.Verify(
            r => r.AddPatientAsync(It.Is<Patient>(p => p.FirstName == request.Patient.FirstName)), Times.Once);
        _mockPrescriptionRepository.Verify(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()), Times.Once);
        _mockPrescriptionRepository.Verify(r => r.AddPrescriptionMedicamentAsync(It.IsAny<PrescriptionMedicament>()),
            Times.Exactly(request.Medicaments.Count()));
        _mockPrescriptionRepository.Verify(r => r.SaveChangesAsync(),
            Times.Exactly(2)); 
        Assert.Equal(request.Patient.FirstName, patientToAdd.FirstName);
        Assert.Equal(request.Date, prescriptionToAdd.Date);
    }

    [Fact]
    public async Task AddPrescriptionAsync_ValidRequestExistingPatient_AddsPrescriptionUpdatesPatient()
    {
        var request = CreateValidRequestDto(patientId: 1);
        var existingPatient = new Patient
            { IdPatient = 1, FirstName = "OldFirst", LastName = "OldLast", Birthdate = new DateTime(1980, 1, 1) };
        var doctor = new Doctor { IdDoctor = request.IdDoctor };

        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync(doctor);
        _mockPrescriptionRepository.Setup(r => r.GetPatientByIdAsync(request.Patient.IdPatient))
            .ReturnsAsync(existingPatient);
        _mockPrescriptionRepository.Setup(r => r.GetMedicamentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Medicament { IdMedicament = id, Name = "TestMed" });
        _mockPrescriptionRepository.Setup(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()))
            .ReturnsAsync((Prescription p) =>
            {
                p.IdPrescription = 2;
                return p;
            });
        _mockPrescriptionRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        await _service.AddPrescriptionAsync(request);

        _mockPrescriptionRepository.Verify(r => r.AddPatientAsync(It.IsAny<Patient>()), Times.Never); 
        Assert.Equal(request.Patient.FirstName, existingPatient.FirstName); 
        Assert.Equal(request.Patient.LastName, existingPatient.LastName);
        _mockPrescriptionRepository.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task AddPrescriptionAsync_DueDateBeforeDate_ThrowsArgumentException()
    {
        var request = CreateValidRequestDto();
        request.DueDate = request.Date.AddDays(-1);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPrescriptionAsync(request));
    }

    [Fact]
    public async Task AddPrescriptionAsync_TooManyMedicaments_ThrowsArgumentException()
    {
        var request = CreateValidRequestDto(numMeds: 11);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPrescriptionAsync(request));
    }

    [Fact]
    public async Task AddPrescriptionAsync_DoctorNotFound_ThrowsKeyNotFoundException()
    {
        var request = CreateValidRequestDto();
        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync((Doctor)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddPrescriptionAsync(request));
    }

    [Fact]
    public async Task AddPrescriptionAsync_MedicamentNotFound_ThrowsKeyNotFoundException()
    {
        var request = CreateValidRequestDto();
        var doctor = new Doctor { IdDoctor = request.IdDoctor };
        var patient = new Patient { IdPatient = request.Patient.IdPatient };

        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync(doctor);
        _mockPrescriptionRepository.Setup(r => r.GetPatientByIdAsync(request.Patient.IdPatient)).ReturnsAsync(patient);
        _mockPrescriptionRepository.Setup(r => r.GetMedicamentByIdAsync(request.Medicaments.First().IdMedicament))
            .ReturnsAsync((Medicament)null);
        _mockPrescriptionRepository.Setup(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()))
            .ReturnsAsync((Prescription p) =>
            {
                p.IdPrescription = 3;
                return p;
            });


        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddPrescriptionAsync(request));
    }

    [Fact]
    public async Task AddPrescriptionAsync_OneMedicament_SavesCorrectly()
    {
        var request = CreateValidRequestDto(numMeds: 1);
        var doctor = new Doctor { IdDoctor = request.IdDoctor };
        var patient = new Patient { IdPatient = request.Patient.IdPatient };
        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync(doctor);
        _mockPrescriptionRepository.Setup(r => r.GetPatientByIdAsync(request.Patient.IdPatient)).ReturnsAsync(patient);
        _mockPrescriptionRepository.Setup(r => r.GetMedicamentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Medicament { IdMedicament = id, Name = "TestMed" });
        _mockPrescriptionRepository.Setup(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()))
            .ReturnsAsync((Prescription p) =>
            {
                p.IdPrescription = 4;
                return p;
            });
        _mockPrescriptionRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        await _service.AddPrescriptionAsync(request);
        _mockPrescriptionRepository.Verify(r => r.AddPrescriptionMedicamentAsync(It.IsAny<PrescriptionMedicament>()),
            Times.Once);
    }

    [Fact]
    public async Task AddPrescriptionAsync_TenMedicaments_SavesCorrectly()
    {
        var request = CreateValidRequestDto(numMeds: 10);
        var doctor = new Doctor { IdDoctor = request.IdDoctor };
        var patient = new Patient { IdPatient = request.Patient.IdPatient };
        _mockPrescriptionRepository.Setup(r => r.GetDoctorByIdAsync(request.IdDoctor)).ReturnsAsync(doctor);
        _mockPrescriptionRepository.Setup(r => r.GetPatientByIdAsync(request.Patient.IdPatient)).ReturnsAsync(patient);
        _mockPrescriptionRepository.Setup(r => r.GetMedicamentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Medicament { IdMedicament = id, Name = "TestMed" });
        _mockPrescriptionRepository.Setup(r => r.AddPrescriptionAsync(It.IsAny<Prescription>()))
            .ReturnsAsync((Prescription p) =>
            {
                p.IdPrescription = 5;
                return p;
            });
        _mockPrescriptionRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        await _service.AddPrescriptionAsync(request);
        _mockPrescriptionRepository.Verify(r => r.AddPrescriptionMedicamentAsync(It.IsAny<PrescriptionMedicament>()),
            Times.Exactly(10));
    }
}