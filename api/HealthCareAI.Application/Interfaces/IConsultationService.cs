using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Interfaces;

public interface IConsultationService
{
    Task<IEnumerable<ConsultationDto>> GetAllConsultationsAsync(string? patientId = null);
    Task<ConsultationDto?> GetConsultationByIdAsync(string id);
    Task<ConsultationDto> CreateConsultationAsync(ConsultationDto createDto);
    Task<ConsultationDto> UpdateConsultationAsync(string id, ConsultationDto updateDto);
    Task DeleteConsultationAsync(string id);
} 