using Tutorial10.DTOs;

namespace Tutorial10.Service.Interfaces;

public interface IPrescriptionService
{
    Task<int> AddPrescriptionAsync(AddPrescriptionRequestDto requestDto);
}