import { Prescription, Patient } from '@shared/schema';

export interface PdfGeneratorProps {
  prescription: Prescription;
  patient: Patient;
}

export class PrescriptionPdfGenerator {
  static async generatePdf(prescription: Prescription, patient: Patient): Promise<Blob> {
    try {
      // Import Syncfusion dynamically with proper destructuring
      console.log('Importing Syncfusion PDF module...');
      const syncfusionModule = await import('@syncfusion/ej2-pdf-export');
      console.log('Syncfusion module keys:', Object.keys(syncfusionModule));
      
      // Access classes from the default export
      const {
        PdfDocument,
        PdfTextElement,
        PdfStandardFont,
        PdfFontFamily,
        PdfFontStyle
      } = syncfusionModule.default || syncfusionModule;
      
      console.log('PDF classes availability:', {
        PdfDocument: typeof PdfDocument,
        PdfTextElement: typeof PdfTextElement,
        PdfStandardFont: typeof PdfStandardFont,
        PdfFontFamily: typeof PdfFontFamily,
        PdfFontStyle: typeof PdfFontStyle
      });
      
      if (!PdfDocument || typeof PdfDocument !== 'function') {
        throw new Error(`PdfDocument is not available or not a constructor. Type: ${typeof PdfDocument}`);
      }
      
      if (!PdfTextElement || typeof PdfTextElement !== 'function') {
        throw new Error(`PdfTextElement is not available or not a constructor. Type: ${typeof PdfTextElement}`);
      }
      
      if (!PdfStandardFont || typeof PdfStandardFont !== 'function') {
        throw new Error(`PdfStandardFont is not available or not a constructor. Type: ${typeof PdfStandardFont}`);
      }
      
      console.log('Creating PDF document...');
      
      // Create a new PDF document
      const document = new PdfDocument();
      
      // Add a page to the document
      const page = document.pages.add();
      
      // Set up fonts using PdfStandardFont instead of PdfFont
      const titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
      const headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
      const normalFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
      
      let yPosition = 50;
      
      // Get page graphics for drawing
      const graphics = page.graphics;
      
      // Title
      const title = new PdfTextElement('MEDICAL PRESCRIPTION', titleFont);
      const titleBounds = title.drawText(page, 50, yPosition);
      yPosition = titleBounds.bounds.y + 40;
      
      // Doctor information
      const doctorInfo = new PdfTextElement('Dr. Sarah Johnson, MD\nMedical Center\nLicense: #12345678', headerFont);
      const doctorBounds = doctorInfo.drawText(page, 50, yPosition);
      yPosition = doctorBounds.bounds.y + 40;
      
      // Patient information
      const patientHeader = new PdfTextElement('PATIENT INFORMATION', headerFont);
      const patientHeaderBounds = patientHeader.drawText(page, 50, yPosition);
      yPosition = patientHeaderBounds.bounds.y + 20;
      
      const patientInfo = new PdfTextElement(
        `Name: ${patient.name}\nAge: ${patient.age} years\nGender: ${patient.gender}\nPhone: ${patient.phone || 'N/A'}`,
        normalFont
      );
      const patientBounds = patientInfo.drawText(page, 70, yPosition);
      yPosition = patientBounds.bounds.y + 30;
      
      // Prescription details
      const prescriptionHeader = new PdfTextElement('PRESCRIPTION DETAILS', headerFont);
      const prescriptionHeaderBounds = prescriptionHeader.drawText(page, 50, yPosition);
      yPosition = prescriptionHeaderBounds.bounds.y + 20;
      
      // Medications
      const medications = prescription.medications || 'No medications specified';
      const medText = new PdfTextElement(`Medications:\n${medications}`, normalFont);
      const medBounds = medText.drawText(page, 70, yPosition);
      yPosition = medBounds.bounds.y + 25;
      
      // Dosage instructions
      const dosageInstructions = prescription.dosageInstructions || 'No instructions provided';
      const instructionsText = new PdfTextElement(`Instructions:\n${dosageInstructions}`, normalFont);
      const instructionsBounds = instructionsText.drawText(page, 70, yPosition);
      yPosition = instructionsBounds.bounds.y + 40;
      
      // Footer
      const footerText = new PdfTextElement(
        `Date: ${new Date(prescription.createdAt).toLocaleDateString()}\nStatus: ${prescription.status.toUpperCase()}\n\nDoctor's Signature: _______________________`,
        normalFont
      );
      footerText.drawText(page, 50, yPosition);
      
      console.log('Saving PDF document...');
      
      // Save the document as blob
      const pdfBytes = await document.save();
      if (document.dispose) {
        document.dispose();
      }
      
      console.log('PDF generated successfully, size:', pdfBytes.length);
      
      return new Blob([pdfBytes], { type: 'application/pdf' });
      
    } catch (error) {
      console.error('Error in PDF generation:', error);
      throw new Error(`PDF generation failed: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  }
}

export default PrescriptionPdfGenerator;