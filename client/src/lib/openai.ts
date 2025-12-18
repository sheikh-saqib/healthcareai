import OpenAI from "openai";

// the newest OpenAI model is "gpt-4o" which was released May 13, 2024. do not change this unless explicitly requested by the user
const openai = new OpenAI({ 
  apiKey: import.meta.env.VITE_OPENAI_API_KEY || "your-api-key-here"
});

export async function analyzePatientConversation(
  transcription: string,
  patientInfo?: { name: string; age: number; medicalHistory?: string }
): Promise<{
  symptoms: string[];
  patientInfo: any;
  suggestedActions: string[];
  prescriptionNeeded: boolean;
  medications?: any[];
  conversationAnalysis?: {
    doctorStatements: string[];
    patientStatements: string[];
    speakerIdentification: string;
  };
}> {
  const prompt = `
    You are a medical AI assistant analyzing a doctor-patient consultation. 
    
    ${patientInfo ? `Patient Information:
    Name: ${patientInfo.name}
    Age: ${patientInfo.age}
    Medical History: ${patientInfo.medicalHistory || 'None provided'}` : 'No existing patient information available.'}
    
    Consultation Transcription:
    ${transcription}
    
    Please analyze this consultation and provide a JSON response with:
    1. symptoms: Array of symptoms mentioned by the patient
    2. patientInfo: Any new patient information discovered (age, medical history updates, etc.)
    3. suggestedActions: Recommended next steps for the doctor
    4. prescriptionNeeded: Boolean indicating if a prescription is needed
    5. medications: If prescription needed, suggest medications with dosage information
    6. conversationAnalysis: Object with:
       - doctorStatements: Key statements made by the doctor
       - patientStatements: Key statements made by the patient
       - speakerIdentification: Analysis of who said what (based on context clues)
    
    Use context clues to identify speakers:
    - Medical terminology, diagnosis, and treatment suggestions typically come from doctors
    - Symptom descriptions, pain levels, and personal experiences typically come from patients
    - Questions about symptoms usually come from doctors
    - Answers about how the patient feels usually come from patients
    
    Be thorough but concise. Only suggest medications if clearly indicated.
    Respond only in valid JSON format.
  `;

  try {
    const response = await openai.chat.completions.create({
      model: "gpt-4o",
      messages: [
        {
          role: "system",
          content: "You are a medical AI assistant. Analyze consultations and provide structured medical insights in JSON format. Be conservative with medication recommendations."
        },
        {
          role: "user",
          content: prompt
        }
      ],
      response_format: { type: "json_object" }
    });

    const analysis = JSON.parse(response.choices[0].message.content || '{}');
    return analysis;
  } catch (error) {
    console.error("OpenAI analysis error:", error);
    throw new Error("Failed to analyze consultation");
  }
}

export async function generatePrescription(
  symptoms: string[],
  patientInfo: { age: number; medicalHistory?: string },
  consultationNotes: string
): Promise<{
  medications: Array<{
    name: string;
    dosage: string;
    frequency: string;
    duration: string;
    instructions: string;
  }>;
  notes: string;
}> {
  const prompt = `
    Generate a prescription based on the following information:
    
    Patient Age: ${patientInfo.age}
    Medical History: ${patientInfo.medicalHistory || 'None'}
    Symptoms: ${symptoms.join(', ')}
    Consultation Notes: ${consultationNotes}
    
    Provide a JSON response with:
    1. medications: Array of medication objects with name, dosage, frequency, duration, and instructions
    2. notes: General prescription notes and warnings
    
    Be conservative and only recommend standard, safe medications. Include appropriate warnings.
    Respond only in valid JSON format.
  `;

  try {
    const response = await openai.chat.completions.create({
      model: "gpt-4o",
      messages: [
        {
          role: "system",
          content: "You are a medical AI assistant helping to generate prescriptions. Be conservative and include safety warnings."
        },
        {
          role: "user",
          content: prompt
        }
      ],
      response_format: { type: "json_object" }
    });

    const prescription = JSON.parse(response.choices[0].message.content || '{}');
    return prescription;
  } catch (error) {
    console.error("OpenAI prescription generation error:", error);
    throw new Error("Failed to generate prescription");
  }
}
