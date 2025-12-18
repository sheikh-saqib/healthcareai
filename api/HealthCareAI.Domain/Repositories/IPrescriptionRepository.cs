using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IPrescriptionRepository
{
    Task<IEnumerable<Prescription>> GetAllAsync();
    Task<IEnumerable<Prescription>> GetByPatientIdAsync(string patientId);
    Task<IEnumerable<Prescription>> GetByStatusAsync(string status);
    Task<IEnumerable<Prescription>> GetByPatientIdAndStatusAsync(string patientId, string status);
    Task<Prescription?> GetByIdAsync(string id);
    Task<Prescription> AddAsync(Prescription prescription);
    Task UpdateAsync(Prescription prescription);
    Task DeleteAsync(Prescription prescription);
    Task<int> GetPendingCountAsync();
    Task SaveChangesAsync();
} 