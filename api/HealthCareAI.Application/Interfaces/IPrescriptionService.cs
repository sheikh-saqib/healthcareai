using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Interfaces;

public interface IPrescriptionService
{
    Task<IEnumerable<PrescriptionDto>> GetAllPrescriptionsAsync(string? patientId = null, string? status = null);
    Task<PrescriptionDto?> GetPrescriptionByIdAsync(string id);
    Task<PrescriptionDto> CreatePrescriptionAsync(PrescriptionDto createDto);
    Task<PrescriptionDto> UpdatePrescriptionAsync(string id, PrescriptionDto updateDto);
    Task DeletePrescriptionAsync(string id);
} 