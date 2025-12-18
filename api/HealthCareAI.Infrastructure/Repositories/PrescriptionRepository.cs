using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public new async Task<IEnumerable<Prescription>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql);
    }

    public async Task<Prescription?> GetByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""Id"" = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<Prescription>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(string patientId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""PatientId"" = @PatientId
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql, new { PatientId = patientId });
    }

    public async Task<IEnumerable<Prescription>> GetByDoctorIdAsync(string doctorId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""DoctorId"" = @DoctorId
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql, new { DoctorId = doctorId });
    }

    public async Task<IEnumerable<Prescription>> GetByStatusAsync(string status)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""Status"" = @Status
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql, new { Status = status });
    }

    public async Task<IEnumerable<Prescription>> GetByPatientIdAndStatusAsync(string patientId, string status)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""PatientId"" = @PatientId AND pr.""Status"" = @Status
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql, new { PatientId = patientId, Status = status });
    }

    public async Task<IEnumerable<Prescription>> GetActiveAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT pr.*, 
                   p.""MedicalRecordNumber"", p.""DateOfBirth"", p.""Gender"",
                   u.""Username"", u.""Email"", u.""FirstName"", u.""LastName""
            FROM ""Prescriptions"" pr
            LEFT JOIN ""Patients"" p ON pr.""PatientId"" = p.""Id""
            LEFT JOIN ""Users"" u ON pr.""DoctorId"" = u.""Id""
            WHERE pr.""Status"" = 'Active'
            ORDER BY pr.""PrescriptionDate"" DESC";
        
        return await connection.QueryAsync<Prescription>(sql);
    }

    public new async Task<Prescription> AddAsync(Prescription prescription)
    {
        prescription.CreatedAt = DateTime.UtcNow;
        await base.AddAsync(prescription);
        return prescription;
    }

    public new async Task UpdateAsync(Prescription prescription)
    {
        prescription.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(prescription);
    }

    public new async Task DeleteAsync(Prescription prescription)
    {
        await base.DeleteAsync(prescription);
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"Prescriptions\" WHERE \"Status\" = 'Pending'";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"Prescriptions\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
} 