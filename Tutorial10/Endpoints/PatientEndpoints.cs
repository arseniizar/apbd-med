using Tutorial10.DTOs;
using Tutorial10.Service.Interfaces;

namespace Tutorial10.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").WithTags("Patients");

        group.MapGet("/{idPatient}", async (int idPatient, IPatientService patientService) =>
            {
                var patientData = await patientService.GetPatientDataAsync(idPatient);
                if (patientData == null)
                {
                    return Results.NotFound($"Patient with ID {idPatient} not found.");
                }

                return Results.Ok(patientData);
            })
            .WithName("GetPatientData")
            .Produces<PatientDataResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}