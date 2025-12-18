using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(string id);
    Task<Patient> AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(Patient patient);
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm);
    Task<int> GetCountAsync();
    Task SaveChangesAsync();
} 