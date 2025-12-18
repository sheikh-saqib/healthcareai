using Dapper;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Infrastructure.Data;
using System.Text.Json;
using System.Transactions;

namespace HealthCareAI.Infrastructure.Repositories;

public class UserRepository : IRepository<User>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT * from vw_users
        WHERE ""UserId"" = @UserId";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId.ToString("N") });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT 
            user_id as UserId,
            email as Email,
            ""Username"" as Username,
            password_hash as PasswordHash,
            password_salt as PasswordSalt,
            ""HashAlgorithm"" as HashAlgorithm,
            ""FirstName"" as FirstName,
            ""LastName"" as LastName,
            ""MiddleName"" as MiddleName,
            ""PrimaryPhone"" as PrimaryPhone,
            ""SecondaryPhone"" as SecondaryPhone,
            ""AlternateEmail"" as AlternateEmail,
            ""DateOfBirth"" as DateOfBirth,
            ""Gender"" as Gender,
            ""Nationality"" as Nationality,
            ""PreferredLanguage"" as PreferredLanguage,
            ""TimeZone"" as TimeZone,
            ""AddressLine1"" as AddressLine1,
            ""AddressLine2"" as AddressLine2,
            ""City"" as City,
            ""StateProvince"" as StateProvince,
            ""PostalCode"" as PostalCode,
            ""Country"" as Country,
            ""IsEmailVerified"" as IsEmailVerified,
            ""IsPhoneVerified"" as IsPhoneVerified,
            ""RequirePasswordChange"" as RequirePasswordChange,
            last_password_change_at as LastPasswordChangeAt,
            last_login_at as LastLoginAt,
            ""LastActivityAt"" as LastActivityAt,
            ""FailedLoginAttempts"" as FailedLoginAttempts,
            ""LastFailedLoginAt"" as LastFailedLoginAt,
            ""AccountLockedUntil"" as AccountLockedUntil,
            ""LockoutReason"" as LockoutReason,
            ""IsTwoFactorEnabled"" as IsTwoFactorEnabled,
            two_factor_secret as TwoFactorSecret,
            ""TwoFactorBackupCodes"" as TwoFactorBackupCodes,
            ""TwoFactorEnabledAt"" as TwoFactorEnabledAt,
            ""AccountStatus"" as AccountStatus,
            ""AccountStatusChangedAt"" as AccountStatusChangedAt,
            ""AccountStatusReason"" as AccountStatusReason,
            ""SecurityPreferences"" as SecurityPreferences,
            ""ForceLogoutAllDevices"" as ForceLogoutAllDevices,
            ""ForceLogoutAfter"" as ForceLogoutAfter,
            ""ProfilePictureUrl"" as ProfilePictureUrl,
            ""Bio"" as Bio,
            ""Preferences"" as Preferences,
            ""NotificationSettings"" as NotificationSettings,
            ""EmergencyContactName"" as EmergencyContactName,
            ""EmergencyContactPhone"" as EmergencyContactPhone,
            ""EmergencyContactRelation"" as EmergencyContactRelation,
            ""Specialty"" as Specialty,
            ""LicenseNumber"" as LicenseNumber,
            ""ClinicName"" as ClinicName,
            ""ClinicAddress"" as ClinicAddress,
            ""Role"" as Role,
            ""Id"" as Id,
            created_at as CreatedAt,
            created_by as CreatedBy,
            updated_at as UpdatedAt,
            updated_by as UpdatedBy,
            ""IsDeleted"" as IsDeleted,
            deleted_at as DeletedAt,
            deleted_by as DeletedBy,
            ""RowVersion"" as RowVersion,
            ""TenantId"" as TenantId,
            ""Metadata"" as Metadata
        FROM ""Users""";

        return await connection.QueryAsync<User>(sql);
    }

    public async Task AddAsync(User entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = @"INSERT INTO ""Users"" (
            user_id, email, ""Username"", password_hash, password_salt, ""HashAlgorithm"",
            ""FirstName"", ""LastName"", ""MiddleName"", ""PrimaryPhone"", ""SecondaryPhone"", ""AlternateEmail"",
            ""DateOfBirth"", ""Gender"", ""Nationality"", ""PreferredLanguage"", ""TimeZone"",
            ""AddressLine1"", ""AddressLine2"", ""City"", ""StateProvince"", ""PostalCode"", ""Country"",
            ""IsEmailVerified"", ""IsPhoneVerified"", ""RequirePasswordChange"", last_password_change_at, last_login_at, ""LastActivityAt"",
            ""FailedLoginAttempts"", ""LastFailedLoginAt"", ""AccountLockedUntil"", ""LockoutReason"",
            ""IsTwoFactorEnabled"", two_factor_secret, ""TwoFactorBackupCodes"", ""TwoFactorEnabledAt"",
            ""AccountStatus"", ""AccountStatusChangedAt"", ""AccountStatusReason"",
            ""SecurityPreferences"", ""ForceLogoutAllDevices"", ""ForceLogoutAfter"",
            ""ProfilePictureUrl"", ""Bio"", ""Preferences"", ""NotificationSettings"",
            ""EmergencyContactName"", ""EmergencyContactPhone"", ""EmergencyContactRelation"",
            ""Specialty"", ""LicenseNumber"", ""ClinicName"", ""ClinicAddress"", ""Role"",
            ""Id"", created_at, created_by, updated_at, updated_by,
            ""IsDeleted"", deleted_at, deleted_by, ""RowVersion"", ""TenantId"", ""Metadata""
        ) VALUES (
            @UserId, @Email, @Username, @PasswordHash, @PasswordSalt, @HashAlgorithm,
            @FirstName, @LastName, @MiddleName, @PrimaryPhone, @SecondaryPhone, @AlternateEmail,
            @DateOfBirth, @Gender, @Nationality, @PreferredLanguage, @TimeZone,
            @AddressLine1, @AddressLine2, @City, @StateProvince, @PostalCode, @Country,
            @IsEmailVerified, @IsPhoneVerified, @RequirePasswordChange, @LastPasswordChangeAt, @LastLoginAt, @LastActivityAt,
            @FailedLoginAttempts, @LastFailedLoginAt, @AccountLockedUntil, @LockoutReason,
            @IsTwoFactorEnabled, @TwoFactorSecret, @TwoFactorBackupCodes, @TwoFactorEnabledAt,
            @AccountStatus, @AccountStatusChangedAt, @AccountStatusReason,
            @SecurityPreferences, @ForceLogoutAllDevices, @ForceLogoutAfter,
            @ProfilePictureUrl, @Bio, @Preferences, @NotificationSettings,
            @EmergencyContactName, @EmergencyContactPhone, @EmergencyContactRelation,
            @Specialty, @LicenseNumber, @ClinicName, @ClinicAddress, @Role,
            @Id, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata
        )";

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(User entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Set UpdatedAt to ensure it's current
        entity.UpdatedAt = DateTime.UtcNow;

        var sql = @"SELECT * FROM sp_upsertuserwithistory(
            @UserId, @Email, @Username, @PasswordHash, @PasswordSalt, @HashAlgorithm,
            @FirstName, @LastName, @MiddleName, @PrimaryPhone, @SecondaryPhone, @AlternateEmail,
            @DateOfBirth, @Gender, @Nationality, @PreferredLanguage, @TimeZone,
            @AddressLine1, @AddressLine2, @City, @StateProvince, @PostalCode, @Country,
            @IsEmailVerified, @IsPhoneVerified, @RequirePasswordChange, @LastPasswordChangeAt, @LastLoginAt, @LastActivityAt,
            @FailedLoginAttempts, @LastFailedLoginAt, @AccountLockedUntil, @LockoutReason,
            @IsTwoFactorEnabled, @TwoFactorSecret, @TwoFactorBackupCodes, @TwoFactorEnabledAt,
            @AccountStatus, @AccountStatusChangedAt, @AccountStatusReason,
            @SecurityPreferences, @ForceLogoutAllDevices, @ForceLogoutAfter,
            @ProfilePictureUrl, @Bio, @Preferences, @NotificationSettings,
            @EmergencyContactName, @EmergencyContactPhone, @EmergencyContactRelation,
            @Specialty, @LicenseNumber, @ClinicName, @ClinicAddress, @Role,
            @Id, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata,
            @PasswordHistoryId, @UserPasswordHistoryId, @ChangeReason, @ChangedByUserId, @IpAddress, @UserAgent,
            @CreatePasswordHistory
        )";

        var parameters = new
        {
            // User parameters
            UserId = entity.UserId,
            Email = entity.Email,
            Username = entity.Username,
            PasswordHash = entity.IsUpdatePassword == true ? entity.PasswordHash : (string?)null, // Don't update password in regular update
            PasswordSalt = entity.IsUpdatePassword == true ? entity.PasswordSalt : (string?)null, // Don't update password salt in regular update
            HashAlgorithm = entity.HashAlgorithm,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            MiddleName = entity.MiddleName,
            PrimaryPhone = entity.PrimaryPhone,
            SecondaryPhone = entity.SecondaryPhone,
            AlternateEmail = entity.AlternateEmail,
            DateOfBirth = entity.DateOfBirth,
            Gender = entity.Gender,
            Nationality = entity.Nationality,
            PreferredLanguage = entity.PreferredLanguage,
            TimeZone = entity.TimeZone,
            AddressLine1 = entity.AddressLine1,
            AddressLine2 = entity.AddressLine2,
            City = entity.City,
            StateProvince = entity.StateProvince,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            IsEmailVerified = entity.IsEmailVerified,
            IsPhoneVerified = entity.IsPhoneVerified,
            RequirePasswordChange = entity.RequirePasswordChange,
            LastPasswordChangeAt = entity.LastPasswordChangeAt,
            LastLoginAt = entity.LastLoginAt,
            LastActivityAt = entity.LastActivityAt,
            FailedLoginAttempts = entity.FailedLoginAttempts,
            LastFailedLoginAt = entity.LastFailedLoginAt,
            AccountLockedUntil = entity.AccountLockedUntil,
            LockoutReason = entity.LockoutReason,
            IsTwoFactorEnabled = entity.IsTwoFactorEnabled,
            TwoFactorSecret = entity.TwoFactorSecret,
            TwoFactorBackupCodes = entity.TwoFactorBackupCodes,
            TwoFactorEnabledAt = entity.TwoFactorEnabledAt,
            AccountStatus = entity.AccountStatus,
            AccountStatusChangedAt = entity.AccountStatusChangedAt,
            AccountStatusReason = entity.AccountStatusReason,
            SecurityPreferences = entity.SecurityPreferences,
            ForceLogoutAllDevices = entity.ForceLogoutAllDevices,
            ForceLogoutAfter = entity.ForceLogoutAfter,
            ProfilePictureUrl = entity.ProfilePictureUrl,
            Bio = entity.Bio,
            Preferences = entity.Preferences,
            NotificationSettings = entity.NotificationSettings,
            EmergencyContactName = entity.EmergencyContactName,
            EmergencyContactPhone = entity.EmergencyContactPhone,
            EmergencyContactRelation = entity.EmergencyContactRelation,
            Specialty = entity.Specialty,
            LicenseNumber = entity.LicenseNumber,
            ClinicName = entity.ClinicName,
            ClinicAddress = entity.ClinicAddress,
            Role = entity.Role,
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            IsDeleted = entity.IsDeleted,
            DeletedAt = entity.DeletedAt,
            DeletedBy = entity.DeletedBy,
            RowVersion = entity.RowVersion,
            TenantId = entity.TenantId,
            Metadata = entity.Metadata,

            // Password history parameters (used when IsUpdatePassword = true)
            PasswordHistoryId = entity.IsUpdatePassword == true ? Guid.NewGuid().ToString("N") : (string?)null,
            UserPasswordHistoryId = entity.IsUpdatePassword == true ? Guid.NewGuid().ToString("N") : (string?)null,
            ChangeReason = entity.IsUpdatePassword == true ? entity.ChangeReason : (string?)null,
            ChangedByUserId = entity.IsUpdatePassword == true ? entity.ChangedByUserId : (string?)null,
            IpAddress = entity.IsUpdatePassword == true ? entity.IpAddress : (string?)null,
            UserAgent = entity.IsUpdatePassword == true ? entity.UserAgent : (string?)null,
            CreatePasswordHistory = entity.IsUpdatePassword == true // Create password history only when updating password
        };

        await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);
    }

    public async Task DeleteAsync(User entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"DELETE FROM ""Users"" WHERE user_id = @UserId";
        await connection.ExecuteAsync(sql, new { UserId = entity.UserId });
    }

    public async Task<IEnumerable<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        // This would require expression tree parsing to SQL, which is complex
        // For now, implement specific find methods and return empty collection
        throw new NotImplementedException("Use specific find methods like FindByEmailAsync");
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT 
            user_id as UserId,
            email as Email,
            ""Username"" as Username,
            password_hash as PasswordHash,
            password_salt as PasswordSalt,
            ""HashAlgorithm"" as HashAlgorithm,
            ""FirstName"" as FirstName,
            ""LastName"" as LastName,
            ""MiddleName"" as MiddleName,
            ""PrimaryPhone"" as PrimaryPhone,
            ""SecondaryPhone"" as SecondaryPhone,
            ""AlternateEmail"" as AlternateEmail,
            ""DateOfBirth"" as DateOfBirth,
            ""Gender"" as Gender,
            ""Nationality"" as Nationality,
            ""PreferredLanguage"" as PreferredLanguage,
            ""TimeZone"" as TimeZone,
            ""AddressLine1"" as AddressLine1,
            ""AddressLine2"" as AddressLine2,
            ""City"" as City,
            ""StateProvince"" as StateProvince,
            ""PostalCode"" as PostalCode,
            ""Country"" as Country,
            ""IsEmailVerified"" as IsEmailVerified,
            ""IsPhoneVerified"" as IsPhoneVerified,
            ""RequirePasswordChange"" as RequirePasswordChange,
            last_password_change_at as LastPasswordChangeAt,
            last_login_at as LastLoginAt,
            ""LastActivityAt"" as LastActivityAt,
            ""FailedLoginAttempts"" as FailedLoginAttempts,
            ""LastFailedLoginAt"" as LastFailedLoginAt,
            ""AccountLockedUntil"" as AccountLockedUntil,
            ""LockoutReason"" as LockoutReason,
            ""IsTwoFactorEnabled"" as IsTwoFactorEnabled,
            two_factor_secret as TwoFactorSecret,
            ""TwoFactorBackupCodes"" as TwoFactorBackupCodes,
            ""TwoFactorEnabledAt"" as TwoFactorEnabledAt,
            ""AccountStatus"" as AccountStatus,
            ""AccountStatusChangedAt"" as AccountStatusChangedAt,
            ""AccountStatusReason"" as AccountStatusReason,
            ""SecurityPreferences"" as SecurityPreferences,
            ""ForceLogoutAllDevices"" as ForceLogoutAllDevices,
            ""ForceLogoutAfter"" as ForceLogoutAfter,
            ""ProfilePictureUrl"" as ProfilePictureUrl,
            ""Bio"" as Bio,
            ""Preferences"" as Preferences,
            ""NotificationSettings"" as NotificationSettings,
            ""EmergencyContactName"" as EmergencyContactName,
            ""EmergencyContactPhone"" as EmergencyContactPhone,
            ""EmergencyContactRelation"" as EmergencyContactRelation,
            ""Specialty"" as Specialty,
            ""LicenseNumber"" as LicenseNumber,
            ""ClinicName"" as ClinicName,
            ""ClinicAddress"" as ClinicAddress,
            ""Role"" as Role,
            ""Id"" as Id,
            created_at as CreatedAt,
            created_by as CreatedBy,
            updated_at as UpdatedAt,
            updated_by as UpdatedBy,
            ""IsDeleted"" as IsDeleted,
            deleted_at as DeletedAt,
            deleted_by as DeletedBy,
            ""RowVersion"" as RowVersion,
            ""TenantId"" as TenantId,
            ""Metadata"" as Metadata
        FROM ""Users"" 
        WHERE email = @Email AND ""IsDeleted"" = false";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }
}
