using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? searchTerm = null);
    Task<PatientDto?> GetPatientByIdAsync(string id);
    Task<PatientDto> CreatePatientAsync(PatientDto createDto);
    Task<PatientDto> UpdatePatientAsync(string id, PatientDto updateDto);
    Task DeletePatientAsync(string id);
} 