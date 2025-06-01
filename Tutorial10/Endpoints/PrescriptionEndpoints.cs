using Tutorial10.DTOs;
using Tutorial10.Service.Interfaces;

namespace Tutorial10.Endpoints;

public static class PrescriptionEndpoints
{
    public static void MapPrescriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapPost("", async (AddPrescriptionRequestDto requestDto, IPrescriptionService prescriptionService) =>
            {
                try
                {
                    var prescriptionId = await prescriptionService.AddPrescriptionAsync(requestDto);
                    return Results.Created($"/api/prescriptions/{prescriptionId}", new { id = prescriptionId });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    return Results.Problem("An unexpected error occurred.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("AddPrescription")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}