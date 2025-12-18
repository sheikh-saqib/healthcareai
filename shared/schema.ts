import { z } from "zod";

// Patient Types - ✅ Aligned with PatientDto
export interface Patient {
  id: string; // Guid → string
  name: string;
  email: string;
  age: number;
  gender: string;
  phone: string;
  address?: string;
  medicalHistory?: string;
  createdAt: string; // DateTime → string
  updatedAt?: string;
}

export interface PatientList {
  id: string;
  name: string;
  email: string;
  age: number;
  gender: string;
  phone: string;
  createdAt: string;
}

// Prescription Types - Updated to match PrescriptionDto exactly
export interface Prescription {
  id: string; // Guid → string
  patientId: string; // Guid → string
  consultationId: string; // Guid → string
  medications: string;
  dosageInstructions?: string;
  status: string; // Backend uses string, not enum
  notes?: string;
  createdAt: string; // DateTime → string
  updatedAt?: string;
  reviewedAt?: string;
  // Navigation properties
  patient?: Patient;
  consultation?: Consultation;
}

export interface PrescriptionList {
  id: string;
  patientId: string;
  patientName: string;
  medications: string;
  status: string;
  createdAt: string;
  reviewedAt?: string;
}

// Consultation Types - Updated to match ConsultationDto exactly
export interface Consultation {
  id: string; // Guid → string
  patientId: string; // Guid → string
  audioUrl?: string;
  transcription?: string;
  aiAnalysis?: any; // JsonDocument → any (for flexibility)
  symptoms?: string;
  duration?: number;
  status: string;
  createdAt: string; // DateTime → string
  // Navigation properties
  patient?: Patient;
}

export interface ConsultationList {
  id: string;
  patientId: string;
  patientName: string;
  symptoms?: string;
  status: string;
  createdAt: string;
}

// Zod Schemas for Validation - Aligned with Create DTOs
export const insertPatientSchema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  email: z.string().email("Invalid email").max(100),
  age: z.number().min(0).max(150),
  gender: z.string().min(1, "Gender is required").max(20),
  phone: z.string().min(1, "Phone is required").max(20),
  address: z.string().max(500).optional(),
  medicalHistory: z.string().max(2000).optional(),
});

export const updatePatientSchema = insertPatientSchema.partial();

// Aligned with CreatePrescriptionDto
export const insertPrescriptionSchema = z.object({
  patientId: z.string().uuid(),
  consultationId: z.string().uuid(),
  medications: z.string().min(1, "Medications are required"),
  dosageInstructions: z.string().optional(),
  notes: z.string().optional(),
});

// Aligned with UpdatePrescriptionDto
export const updatePrescriptionSchema = z.object({
  medications: z.string().optional(),
  dosageInstructions: z.string().optional(),
  status: z.string(),
  notes: z.string().optional(),
  reviewedAt: z.string().optional(),
});

// Aligned with CreateConsultationDto
export const insertConsultationSchema = z.object({
  patientId: z.string().uuid(),
  audioUrl: z.string().optional(),
  transcription: z.string().optional(),
  symptoms: z.string().optional(),
  duration: z.number().optional(),
});

// Aligned with UpdateConsultationDto
export const updateConsultationSchema = z.object({
  audioUrl: z.string().optional(),
  transcription: z.string().optional(),
  aiAnalysis: z.any().optional(),
  symptoms: z.string().optional(),
  duration: z.number().optional(),
  status: z.string(),
});

// Stats Types - ✅ Already perfectly aligned with StatsDto
export interface Stats {
  totalPatients: number;
  todayConsultations: number;
  pendingPrescriptions: number;
  recordedConsultations: number;
}

// Form Types
export type PatientFormData = z.infer<typeof insertPatientSchema>;
export type PrescriptionFormData = z.infer<typeof insertPrescriptionSchema>;
export type UpdatePrescriptionFormData = z.infer<typeof updatePrescriptionSchema>;
export type ConsultationFormData = z.infer<typeof insertConsultationSchema>;
export type UpdateConsultationFormData = z.infer<typeof updateConsultationSchema>; 