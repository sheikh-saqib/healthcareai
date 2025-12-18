using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class ConsultationRepository : Repository<Consultation>, IConsultationRepository
{
    public ConsultationRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public new async Task<IEnumerable<Consultation>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT c.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"", p.""BloodType"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Consultations"" c
            LEFT JOIN ""Patients"" p ON c.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON c.""DoctorId"" = u.""Id""
            ORDER BY c.""ConsultationDate"" DESC";
        
        return await connection.QueryAsync<Consultation>(sql);
    }

    public async Task<Consultation?> GetByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT c.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"", p.""BloodType"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Consultations"" c
            LEFT JOIN ""Patients"" p ON c.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON c.""DoctorId"" = u.""Id""
            WHERE c.""Id"" = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<Consultation>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Consultation>> GetByPatientIdAsync(string patientId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT c.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"", p.""BloodType"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Consultations"" c
            LEFT JOIN ""Patients"" p ON c.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON c.""DoctorId"" = u.""Id""
            WHERE c.""PatientId"" = @PatientId
            ORDER BY c.""ConsultationDate"" DESC";
        
        return await connection.QueryAsync<Consultation>(sql, new { PatientId = patientId });
    }

    public async Task<IEnumerable<Consultation>> GetByDoctorIdAsync(string doctorId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT c.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"", p.""BloodType"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Consultations"" c
            LEFT JOIN ""Patients"" p ON c.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON c.""DoctorId"" = u.""Id""
            WHERE c.""DoctorId"" = @DoctorId
            ORDER BY c.""ConsultationDate"" DESC";
        
        return await connection.QueryAsync<Consultation>(sql, new { DoctorId = doctorId });
    }

    public async Task<IEnumerable<Consultation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT c.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"", p.""BloodType"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Consultations"" c
            LEFT JOIN ""Patients"" p ON c.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON c.""DoctorId"" = u.""Id""
            WHERE c.""ConsultationDate"" BETWEEN @StartDate AND @EndDate
            ORDER BY c.""ConsultationDate""";
        
        return await connection.QueryAsync<Consultation>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<int> GetTodayCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var sql = "SELECT COUNT(*) FROM \"Consultations\" WHERE \"CreatedAt\" >= @Today AND \"CreatedAt\" < @Tomorrow";
        return await connection.ExecuteScalarAsync<int>(sql, new { Today = today, Tomorrow = tomorrow });
    }

    public new async Task<Consultation> AddAsync(Consultation consultation)
    {
        consultation.CreatedAt = DateTime.UtcNow;
        await base.AddAsync(consultation);
        return consultation;
    }

    public new async Task UpdateAsync(Consultation consultation)
    {
        consultation.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(consultation);
    }

    public new async Task DeleteAsync(Consultation consultation)
    {
        await base.DeleteAsync(consultation);
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"Consultations\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
} 