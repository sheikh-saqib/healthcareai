import { Prescription, Patient } from '@shared/schema';

export class SimplePdfGenerator {
  static async generatePdf(prescription: Prescription, patient: Patient): Promise<Blob> {
    // Create HTML content for the prescription
    const htmlContent = `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Medical Prescription</title>
        <style>
          body { 
            font-family: Arial, sans-serif; 
            margin: 40px; 
            line-height: 1.6; 
            color: #333;
          }
          .header { 
            text-align: center; 
            border-bottom: 2px solid #333; 
            padding-bottom: 20px; 
            margin-bottom: 30px;
          }
          .title { 
            font-size: 24px; 
            font-weight: bold; 
            margin-bottom: 10px;
          }
          .doctor-info { 
            font-size: 14px; 
            color: #666;
          }
          .section { 
            margin: 25px 0; 
            padding: 15px; 
            border-left: 4px solid #007bff;
            background-color: #f8f9fa;
          }
          .section-title { 
            font-size: 16px; 
            font-weight: bold; 
            margin-bottom: 10px;
            color: #007bff;
          }
          .field { 
            margin: 8px 0; 
          }
          .field-label { 
            font-weight: bold; 
            display: inline-block; 
            width: 120px;
          }
          .medications { 
            background-color: #e8f5e8; 
            padding: 15px; 
            border-radius: 5px;
            margin: 10px 0;
          }
          .instructions { 
            background-color: #fff3cd; 
            padding: 15px; 
            border-radius: 5px;
            margin: 10px 0;
          }
          .footer { 
            margin-top: 50px; 
            border-top: 1px solid #ccc; 
            padding-top: 20px;
          }
          .signature-line { 
            border-bottom: 1px solid #333; 
            width: 300px; 
            margin: 20px 0;
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div class="title">MEDICAL PRESCRIPTION</div>
          <div class="doctor-info">
            Dr. Sarah Johnson, MD<br>
            Medical Center<br>
            License: #12345678
          </div>
        </div>
        
        <div class="section">
          <div class="section-title">PATIENT INFORMATION</div>
          <div class="field">
            <span class="field-label">Name:</span> ${patient.name}
          </div>
          <div class="field">
            <span class="field-label">Age:</span> ${patient.age} years
          </div>
          <div class="field">
            <span class="field-label">Gender:</span> ${patient.gender}
          </div>
          <div class="field">
            <span class="field-label">Phone:</span> ${patient.phone || 'N/A'}
          </div>
          ${patient.medicalHistory ? `
          <div class="field">
            <span class="field-label">Medical History:</span> ${patient.medicalHistory}
          </div>
          ` : ''}
        </div>
        
        <div class="section">
          <div class="section-title">PRESCRIPTION DETAILS</div>
          
          <div class="medications">
            <strong>Prescribed Medications:</strong><br>
            ${prescription.medications || 'No medications specified'}
          </div>
          
          <div class="instructions">
            <strong>Dosage Instructions:</strong><br>
            ${prescription.dosageInstructions || 'No instructions provided'}
          </div>
          
          ${prescription.notes ? `
          <div class="field">
            <span class="field-label">Additional Notes:</span> ${prescription.notes}
          </div>
          ` : ''}
        </div>
        
        <div class="footer">
          <div class="field">
            <span class="field-label">Date:</span> ${new Date(prescription.createdAt).toLocaleDateString()}
          </div>
          <div class="field">
            <span class="field-label">Status:</span> ${prescription.status.toUpperCase()}
          </div>
          <div class="field">
            <span class="field-label">Prescription ID:</span> ${prescription.id}
          </div>
          
          <div style="margin-top: 40px;">
            <div>Doctor's Signature:</div>
            <div class="signature-line"></div>
            <div style="margin-top: 10px; font-size: 12px; color: #666;">
              This prescription is AI-generated and requires medical review.
            </div>
          </div>
        </div>
      </body>
      </html>
    `;

    // Create a blob with the HTML content
    const htmlBlob = new Blob([htmlContent], { type: 'text/html' });
    
    // For now, we'll return the HTML blob
    // In a real implementation, you'd convert this to PDF using a library like Puppeteer
    return htmlBlob;
  }
  
  static async generateAndDownloadPdf(prescription: Prescription, patient: Patient): Promise<void> {
    const blob = await this.generatePdf(prescription, patient);
    
    // Create download link
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `prescription-${patient.name.replace(/\s+/g, '-')}-${new Date().toISOString().split('T')[0]}.html`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
}