import { useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Printer, Download } from "lucide-react";
import { Prescription, Patient } from "@shared/schema";
import { PrescriptionPdfGenerator } from "./pdf-generator";
import { useToast } from "@/hooks/use-toast";
import { formatDateLong, formatMedications } from "@/lib/utils";

interface PrescriptionPrintProps {
  prescription: Prescription;
  patient: Patient;
  onPrint?: () => void;
}

export function PrescriptionPrint({ prescription, patient, onPrint }: PrescriptionPrintProps) {
  const printRef = useRef<HTMLDivElement>(null);
  const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);
  const { toast } = useToast();

  const handleGeneratePdf = async () => {
    setIsGeneratingPdf(true);
    try {
      const pdfBlob = await PrescriptionPdfGenerator.generatePdf(prescription, patient);
      
      // Create download link
      const url = URL.createObjectURL(pdfBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `prescription_${patient.name.replace(/\s+/g, '_')}_${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      toast({
        title: "PDF Generated Successfully",
        description: "Prescription PDF has been downloaded"
      });
    } catch (error) {
      console.error('Error generating PDF:', error);
      toast({
        title: "PDF Generation Failed",
        description: "Could not generate PDF. Please try again.",
        variant: "destructive"
      });
    } finally {
      setIsGeneratingPdf(false);
    }
  };

  const handlePrint = () => {
    if (printRef.current) {
      const printContent = printRef.current.innerHTML;
      const originalContent = document.body.innerHTML;
      
      // Create print styles
      const printStyles = `
        <style>
          @media print {
            body { 
              font-family: 'Times New Roman', serif;
              font-size: 12pt;
              line-height: 1.4;
              color: black;
              margin: 0;
              padding: 20px;
            }
            .prescription-header {
              border-bottom: 2px solid #000;
              padding-bottom: 10px;
              margin-bottom: 20px;
            }
            .prescription-content {
              margin-bottom: 20px;
            }
            .medication-item {
              margin-bottom: 15px;
              padding-left: 20px;
            }
            .signature-section {
              margin-top: 40px;
              border-top: 1px solid #000;
              padding-top: 20px;
            }
            .no-print { display: none !important; }
          }
        </style>
      `;
      
      document.body.innerHTML = printStyles + printContent;
      window.print();
      document.body.innerHTML = originalContent;
      window.location.reload(); // Restore the page
    }
    onPrint?.();
  };

  const parseMedications = (medications: string) => {
    try {
      const meds = JSON.parse(medications);
      if (Array.isArray(meds)) {
        return meds;
      }
      return [{ name: medications, dosage: "As prescribed", instructions: "Follow doctor's instructions" }];
    } catch {
      return [{ name: medications, dosage: "As prescribed", instructions: "Follow doctor's instructions" }];
    }
  };

  return (
    <div className="space-y-4">
      <div className="no-print flex gap-2">
        <Button onClick={handlePrint} className="flex items-center gap-2">
          <Printer className="h-4 w-4" />
          Print Prescription
        </Button>
        <Button 
          onClick={handleGeneratePdf} 
          disabled={isGeneratingPdf}
          className="flex items-center gap-2"
          variant="outline"
        >
          <Download className="h-4 w-4" />
          {isGeneratingPdf ? "Generating PDF..." : "Generate PDF"}
        </Button>
      </div>

      <Card className="max-w-2xl">
        <div ref={printRef}>
          <CardHeader className="prescription-header">
            <div className="text-center space-y-2">
              <h1 className="text-2xl font-bold">MEDICAL PRESCRIPTION</h1>
              <div className="text-lg">
                <p className="font-semibold">Dr. Sarah Johnson, MD</p>
                <p>Cardiologist</p>
                <p>Medical License: #12345678</p>
              </div>
              <div className="text-sm text-muted-foreground">
                <p>123 Medical Center Drive, Healthcare City, HC 12345</p>
                <p>Phone: (555) 123-4567 | Email: dr.johnson@medcenter.com</p>
              </div>
            </div>
          </CardHeader>

          <CardContent className="prescription-content space-y-6">
            {/* Patient Information */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <h3 className="font-semibold text-lg mb-2">Patient Information</h3>
                <div className="space-y-1">
                  <p><strong>Name:</strong> {patient.name}</p>
                  <p><strong>Age:</strong> {patient.age} years</p>
                  <p><strong>Gender:</strong> {patient.gender}</p>
                  <p><strong>Patient ID:</strong> {patient.id}</p>
                </div>
              </div>
              <div>
                <h3 className="font-semibold text-lg mb-2">Prescription Details</h3>
                <div className="space-y-1">
                  <p><strong>Date:</strong> {formatDateLong(prescription.createdAt)}</p>
                  <p><strong>Prescription ID:</strong> {prescription.id}</p>
                  <p><strong>Status:</strong> {prescription.status.toUpperCase()}</p>
                </div>
              </div>
            </div>

            {/* Diagnosis */}
            {prescription.diagnosis && (
              <div>
                <h3 className="font-semibold text-lg mb-2">Diagnosis</h3>
                <p className="border-l-4 border-blue-500 pl-4 py-2 bg-blue-50">
                  {prescription.diagnosis}
                </p>
              </div>
            )}

            {/* Medications */}
            <div>
              <h3 className="font-semibold text-lg mb-4">Prescribed Medications</h3>
              <div className="space-y-4">
                {parseMedications(prescription.medications).map((med: any, index: number) => (
                  <div key={index} className="medication-item border-l-4 border-green-500 pl-4 py-2 bg-green-50">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <p className="font-semibold text-lg">
                          {typeof med === 'string' ? med : med.name}
                        </p>
                        {typeof med === 'object' && (
                          <>
                            <p className="text-sm text-gray-600 mt-1">
                              <strong>Dosage:</strong> {med.dosage || 'As prescribed'}
                            </p>
                            <p className="text-sm text-gray-600">
                              <strong>Instructions:</strong> {med.instructions || 'Follow doctor\'s instructions'}
                            </p>
                            {med.frequency && (
                              <p className="text-sm text-gray-600">
                                <strong>Frequency:</strong> {med.frequency}
                              </p>
                            )}
                            {med.duration && (
                              <p className="text-sm text-gray-600">
                                <strong>Duration:</strong> {med.duration}
                              </p>
                            )}
                          </>
                        )}
                      </div>
                      <div className="text-right text-sm text-gray-500">
                        #{index + 1}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Special Instructions */}
            {prescription.instructions && (
              <div>
                <h3 className="font-semibold text-lg mb-2">Special Instructions</h3>
                <p className="border-l-4 border-yellow-500 pl-4 py-2 bg-yellow-50">
                  {prescription.instructions}
                </p>
              </div>
            )}

            {/* Signature Section */}
            <div className="signature-section">
              <div className="grid grid-cols-2 gap-8">
                <div>
                  <p className="mb-8">Doctor's Signature:</p>
                  <div className="border-b border-black w-full mb-2"></div>
                  <p className="text-sm text-center">Dr. Sarah Johnson, MD</p>
                  <p className="text-sm text-center">Date: {prescription.updatedAt ? formatDateLong(prescription.updatedAt) : formatDateLong(prescription.createdAt)}</p>
                </div>
                <div>
                  <p className="mb-8">Pharmacy Use Only:</p>
                  <div className="border-b border-black w-full mb-2"></div>
                  <p className="text-sm text-center">Pharmacist Signature & Date</p>
                </div>
              </div>
            </div>

            {/* Footer */}
            <div className="text-center text-xs text-gray-500 mt-8 pt-4 border-t">
              <p>This prescription is valid for 30 days from the date of issue.</p>
              <p>For questions or concerns, contact our office at (555) 123-4567</p>
              <p className="mt-2">Generated by Medical Consultation System - {formatDateLong(new Date())}</p>
            </div>
          </CardContent>
        </div>
      </Card>
    </div>
  );
}