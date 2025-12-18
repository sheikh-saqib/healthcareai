using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Document : BaseEntity
{
    public string DocumentId { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Medical, Legal, Image, Report, Lab, Prescription
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    
    // File metadata
    public string? FileHash { get; set; } // For integrity checking
    public string? StorageProvider { get; set; } // Local, S3, Azure, etc.
    public string? StoragePath { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public string? EncryptionKey { get; set; }
    
    // Version control
    public int Version { get; set; } = 1;
    public string? ParentDocumentId { get; set; }
    public bool IsLatestVersion { get; set; } = true;
    public string? VersionNotes { get; set; }
    
    // Access control
    public string AccessLevel { get; set; } = "Private"; // Public, Private, Restricted
    public JsonDocument? AccessRules { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int AccessCount { get; set; } = 0;
    
    // Processing status
    public string ProcessingStatus { get; set; } = "Uploaded"; // Uploaded, Processing, Processed, Failed
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }
    
    // Medical document specific
    public string? PatientId { get; set; }
    public string? ConsultationId { get; set; }
    public string? PrescriptionId { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string? DocumentSource { get; set; }
    public JsonDocument? ExtractedData { get; set; } // OCR/AI extracted data
    
    // Navigation properties
    public User UploadedByUser { get; set; } = null!;
    public Document? ParentDocument { get; set; }
    public ICollection<Document> ChildDocuments { get; set; } = new List<Document>();
    public ICollection<ConsultationDocument> ConsultationDocuments { get; set; } = new List<ConsultationDocument>();
    public ICollection<PatientDocument> PatientDocuments { get; set; } = new List<PatientDocument>();
    public ICollection<DocumentShare> DocumentShares { get; set; } = new List<DocumentShare>();
} 