-- HealthCareAI Database Schema
-- This file contains the complete database schema for the HealthCareAI application
-- Run this script to create all necessary tables and indexes

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Core entities
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Username" VARCHAR(100) NOT NULL UNIQUE,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "FirstName" VARCHAR(100),
    "LastName" VARCHAR(100),
    "PhoneNumber" VARCHAR(20),
    "IsActive" BOOLEAN DEFAULT true,
    "EmailVerified" BOOLEAN DEFAULT false,
    "PhoneVerified" BOOLEAN DEFAULT false,
    "TwoFactorEnabled" BOOLEAN DEFAULT false,
    "TwoFactorSecret" VARCHAR(255),
    "LastLoginDate" TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Patients" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "MedicalRecordNumber" VARCHAR(50) UNIQUE,
    "DateOfBirth" DATE,
    "Gender" VARCHAR(10),
    "BloodType" VARCHAR(5),
    "Allergies" TEXT[],
    "EmergencyContactName" VARCHAR(100),
    "EmergencyContactPhone" VARCHAR(20),
    "EmergencyContactRelationship" VARCHAR(50),
    "InsuranceProvider" VARCHAR(100),
    "InsurancePolicyNumber" VARCHAR(50),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Consultations" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "DoctorId" UUID REFERENCES "Users"("Id"),
    "ConsultationDate" TIMESTAMP NOT NULL,
    "Status" VARCHAR(20) DEFAULT 'Scheduled',
    "Type" VARCHAR(50),
    "Notes" TEXT,
    "Diagnosis" TEXT,
    "TreatmentPlan" TEXT,
    "FollowUpDate" DATE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Prescriptions" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "DoctorId" UUID REFERENCES "Users"("Id"),
    "ConsultationId" UUID REFERENCES "Consultations"("Id"),
    "PrescriptionDate" DATE NOT NULL,
    "Status" VARCHAR(20) DEFAULT 'Active',
    "Notes" TEXT,
    "ExpiryDate" DATE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Core Infrastructure
CREATE TABLE IF NOT EXISTS "Organizations" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(255) NOT NULL,
    "Type" VARCHAR(50),
    "Address" TEXT,
    "Phone" VARCHAR(20),
    "Email" VARCHAR(255),
    "Website" VARCHAR(255),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Departments" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrganizationId" UUID REFERENCES "Organizations"("Id"),
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "OrganizationSettings" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrganizationId" UUID REFERENCES "Organizations"("Id"),
    "SettingKey" VARCHAR(100) NOT NULL,
    "SettingValue" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Authentication & Security
CREATE TABLE IF NOT EXISTS "UserSessions" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "SessionToken" VARCHAR(255) NOT NULL UNIQUE,
    "RefreshToken" VARCHAR(255),
    "ExpiresAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN DEFAULT true,
    "IpAddress" VARCHAR(45),
    "UserAgent" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "VerificationTokens" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "Token" VARCHAR(255) NOT NULL UNIQUE,
    "Type" VARCHAR(20) NOT NULL,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "UsedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "TrustedDevices" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "DeviceId" VARCHAR(255) NOT NULL,
    "DeviceName" VARCHAR(100),
    "DeviceType" VARCHAR(50),
    "LastUsedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "IsTrusted" BOOLEAN DEFAULT true,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "LoginAttempts" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID,
    "IpAddress" VARCHAR(45) NOT NULL,
    "Username" VARCHAR(100),
    "Success" BOOLEAN NOT NULL,
    "FailureReason" VARCHAR(100),
    "AttemptedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "UserPasswordHistories" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "PasswordHash" VARCHAR(255) NOT NULL,
    "ChangedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "OrganizationAuthPolicies" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrganizationId" UUID REFERENCES "Organizations"("Id"),
    "PolicyName" VARCHAR(100) NOT NULL,
    "PolicyValue" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Role Management
CREATE TABLE IF NOT EXISTS "AccessRoles" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(100) NOT NULL UNIQUE,
    "Description" TEXT,
    "IsActive" BOOLEAN DEFAULT true,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "UserRoles" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "RoleId" UUID REFERENCES "AccessRoles"("Id"),
    "AssignedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "AssignedBy" UUID REFERENCES "Users"("Id"),
    "IsActive" BOOLEAN DEFAULT true
);

CREATE TABLE IF NOT EXISTS "AccessPermissions" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "RoleId" UUID REFERENCES "AccessRoles"("Id"),
    "Resource" VARCHAR(100) NOT NULL,
    "Action" VARCHAR(50) NOT NULL,
    "IsGranted" BOOLEAN DEFAULT true,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Medical Core
CREATE TABLE IF NOT EXISTS "DoctorProfiles" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "LicenseNumber" VARCHAR(50) UNIQUE,
    "Specialization" VARCHAR(100),
    "YearsOfExperience" INTEGER,
    "Education" TEXT[],
    "Certifications" TEXT[],
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "DoctorSchedules" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "DoctorId" UUID REFERENCES "Users"("Id"),
    "DayOfWeek" INTEGER NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "IsAvailable" BOOLEAN DEFAULT true,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "ConsultationParticipants" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ConsultationId" UUID REFERENCES "Consultations"("Id"),
    "UserId" UUID REFERENCES "Users"("Id"),
    "Role" VARCHAR(50) NOT NULL,
    "JoinedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "ConsultationNotes" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ConsultationId" UUID REFERENCES "Consultations"("Id"),
    "AuthorId" UUID REFERENCES "Users"("Id"),
    "NoteType" VARCHAR(50),
    "Content" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Prescription System
CREATE TABLE IF NOT EXISTS "PrescriptionMedications" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PrescriptionId" UUID REFERENCES "Prescriptions"("Id"),
    "MedicationName" VARCHAR(255) NOT NULL,
    "Dosage" VARCHAR(100),
    "Frequency" VARCHAR(100),
    "Duration" VARCHAR(100),
    "Instructions" TEXT,
    "Quantity" INTEGER,
    "RefillsAllowed" INTEGER DEFAULT 0,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "PrescriptionNotes" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PrescriptionId" UUID REFERENCES "Prescriptions"("Id"),
    "AuthorId" UUID REFERENCES "Users"("Id"),
    "Content" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "PrescriptionRefills" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PrescriptionId" UUID REFERENCES "Prescriptions"("Id"),
    "RequestedBy" UUID REFERENCES "Users"("Id"),
    "ApprovedBy" UUID REFERENCES "Users"("Id"),
    "RequestDate" DATE NOT NULL,
    "ApprovalDate" DATE,
    "Status" VARCHAR(20) DEFAULT 'Pending',
    "Notes" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Supporting Systems
CREATE TABLE IF NOT EXISTS "VitalSigns" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "ConsultationId" UUID REFERENCES "Consultations"("Id"),
    "BloodPressure" VARCHAR(20),
    "HeartRate" INTEGER,
    "Temperature" DECIMAL(4,1),
    "RespiratoryRate" INTEGER,
    "OxygenSaturation" INTEGER,
    "Weight" DECIMAL(5,2),
    "Height" DECIMAL(5,2),
    "RecordedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "LabResults" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "TestName" VARCHAR(255) NOT NULL,
    "TestResult" TEXT,
    "ReferenceRange" VARCHAR(100),
    "Units" VARCHAR(50),
    "TestDate" DATE NOT NULL,
    "ReportedBy" UUID REFERENCES "Users"("Id"),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Documents" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "FileName" VARCHAR(255) NOT NULL,
    "FilePath" TEXT NOT NULL,
    "FileType" VARCHAR(50),
    "FileSize" BIGINT,
    "UploadedBy" UUID REFERENCES "Users"("Id"),
    "UploadedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "ConsultationDocuments" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ConsultationId" UUID REFERENCES "Consultations"("Id"),
    "DocumentId" UUID REFERENCES "Documents"("Id"),
    "DocumentType" VARCHAR(50),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "PatientDocuments" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "DocumentId" UUID REFERENCES "Documents"("Id"),
    "DocumentType" VARCHAR(50),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "DocumentShares" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "DocumentId" UUID REFERENCES "Documents"("Id"),
    "SharedWith" UUID REFERENCES "Users"("Id"),
    "PermissionLevel" VARCHAR(20) DEFAULT 'Read',
    "SharedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ExpiresAt" TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "PatientOrganizations" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "PatientId" UUID REFERENCES "Patients"("Id"),
    "OrganizationId" UUID REFERENCES "Organizations"("Id"),
    "RelationshipType" VARCHAR(50),
    "IsActive" BOOLEAN DEFAULT true,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES "Users"("Id"),
    "Action" VARCHAR(100) NOT NULL,
    "Resource" VARCHAR(100),
    "ResourceId" UUID,
    "Details" TEXT,
    "IpAddress" VARCHAR(45),
    "UserAgent" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");
CREATE INDEX IF NOT EXISTS idx_users_username ON "Users"("Username");
CREATE INDEX IF NOT EXISTS idx_patients_userid ON "Patients"("UserId");
CREATE INDEX IF NOT EXISTS idx_consultations_patientid ON "Consultations"("PatientId");
CREATE INDEX IF NOT EXISTS idx_consultations_doctorid ON "Consultations"("DoctorId");
CREATE INDEX IF NOT EXISTS idx_prescriptions_patientid ON "Prescriptions"("PatientId");
CREATE INDEX IF NOT EXISTS idx_usersessions_userid ON "UserSessions"("UserId");
CREATE INDEX IF NOT EXISTS idx_verificationtokens_userid ON "VerificationTokens"("UserId");
CREATE INDEX IF NOT EXISTS idx_userroles_userid ON "UserRoles"("UserId");
CREATE INDEX IF NOT EXISTS idx_auditlogs_userid ON "AuditLogs"("UserId");
CREATE INDEX IF NOT EXISTS idx_auditlogs_createdat ON "AuditLogs"("CreatedAt");

-- Insert default roles
INSERT INTO "AccessRoles" ("Id", "Name", "Description") VALUES 
    (uuid_generate_v4(), 'Admin', 'System Administrator with full access'),
    (uuid_generate_v4(), 'Doctor', 'Medical professional with patient care access'),
    (uuid_generate_v4(), 'Nurse', 'Nursing staff with limited patient access'),
    (uuid_generate_v4(), 'Patient', 'Patient with access to own records'),
    (uuid_generate_v4(), 'Staff', 'General staff member with basic access')
ON CONFLICT ("Name") DO NOTHING;
