using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IConsultationRepository
{
    Task<IEnumerable<Consultation>> GetAllAsync();
    Task<IEnumerable<Consultation>> GetByPatientIdAsync(string patientId);
    Task<Consultation?> GetByIdAsync(string id);
    Task<Consultation> AddAsync(Consultation consultation);
    Task DeleteAsync(Consultation consultation);
    Task<int> GetCountAsync();
    Task<int> GetTodayCountAsync();
    Task SaveChangesAsync();
} 