import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Printer, Eye, Download } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { Prescription } from "@shared/schema";
import { PrescriptionPrint } from "./prescription-print";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export default function PrescriptionList() {
  const [showPrintDialog, setShowPrintDialog] = useState(false);
  const [selectedPrescription, setSelectedPrescription] = useState<Prescription | null>(null);
  const { toast } = useToast();

  const { data: prescriptions, isLoading } = useQuery({
    queryKey: ["/api/prescriptions"],
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  // Get patient data for the selected prescription
  const { data: selectedPatient } = useQuery({
    queryKey: [`/api/patients/${selectedPrescription?.patientId}`],
    enabled: !!selectedPrescription?.patientId,
  });

  const handlePrint = (prescription: Prescription) => {
    setSelectedPrescription(prescription);
    setShowPrintDialog(true);
  };

  const handleDownloadPDF = async (prescription: Prescription) => {
    try {
      console.log('Starting PDF generation for prescription:', prescription.id);
      
      // Get patient information
      const patientResponse = await fetch(`/api/patients/${prescription.patientId}`);
      if (!patientResponse.ok) {
        throw new Error('Failed to fetch patient data');
      }
      const patient = await patientResponse.json();
      console.log('Patient data retrieved:', patient.name);
      
      // Try Syncfusion PDF generator first
      try {
        console.log('Attempting to import Syncfusion PDF generator...');
        const { PrescriptionPdfGenerator } = await import('@/components/pdf-generator');
        console.log('PDF generator imported successfully');
        
        console.log('Calling generatePdf with data:', {
          prescriptionId: prescription.id,
          patientName: patient.name
        });
        
        const pdfBlob = await PrescriptionPdfGenerator.generatePdf(prescription, patient);
        console.log('PDF generated successfully! Blob size:', pdfBlob.size);
        
        // Create download link
        const url = URL.createObjectURL(pdfBlob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `prescription-${patient.name.replace(/\s+/g, '-')}-${new Date().toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        toast({ 
          title: "PDF Generated Successfully", 
          description: `Prescription PDF downloaded for ${patient.name}` 
        });
        
      } catch (pdfError) {
        console.warn('Syncfusion PDF failed, trying alternative method:', pdfError);
        console.warn('Error details:', pdfError.message);
        
        // Fallback to simple HTML generator
        const { SimplePdfGenerator } = await import('@/components/simple-pdf-generator');
        await SimplePdfGenerator.generateAndDownloadPdf(prescription, patient);
        
        toast({ 
          title: "Prescription Downloaded", 
          description: `Prescription document downloaded for ${patient.name} (Professional HTML format)` 
        });
      }
      
    } catch (error) {
      console.error('Error generating PDF:', error);
      toast({ 
        title: "PDF Generation Failed", 
        description: error instanceof Error ? error.message : "Could not generate prescription PDF",
        variant: "destructive"
      });
    }
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  };

  const getBadgeVariant = (status: string) => {
    switch (status) {
      case 'approved':
        return 'default';
      case 'rejected':
        return 'destructive';
      case 'pending':
        return 'secondary';
      default:
        return 'outline';
    }
  };

  const getBadgeColor = (status: string) => {
    switch (status) {
      case 'approved':
        return 'bg-green-100 text-green-800';
      case 'rejected':
        return 'bg-red-100 text-red-800';
      case 'pending':
        return 'bg-orange-100 text-orange-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const PrescriptionCard = ({ prescription }: { prescription: Prescription }) => (
    <div className="border border-border rounded-lg p-4 hover:bg-muted/50 transition-colors">
      <div className="flex items-start justify-between mb-2">
        <div>
          <p className="font-medium text-primary">Patient ID: {prescription.patientId}</p>
          <p className="text-sm text-muted-foreground">{formatDate(prescription.createdAt)}</p>
        </div>
        <Badge 
          variant="outline" 
          className={getBadgeColor(prescription.status)}
        >
          {prescription.status.charAt(0).toUpperCase() + prescription.status.slice(1)}
        </Badge>
      </div>
      
      <p className="text-sm text-muted-foreground mb-3">
        {prescription.notes || "AI-generated prescription"}
      </p>
      
      <div className="flex space-x-2">
        {prescription.status === 'approved' && (
          <Button 
            size="sm" 
            className="bg-blue-500 hover:bg-blue-600 text-white"
            onClick={() => handleDownloadPDF(prescription)}
          >
            <Download className="h-4 w-4 mr-1" />
            Download PDF
          </Button>
        )}
        
        <Button 
          size="sm" 
          variant="outline"
          onClick={() => handlePrint(prescription)}
          disabled={prescription.status !== 'approved'}
        >
          <Printer className="h-4 w-4 mr-1" />
          Print
        </Button>
        
        <Button 
          size="sm" 
          variant="ghost"
          onClick={() => handlePrint(prescription)}
        >
          <Eye className="h-4 w-4 mr-1" />
          View
        </Button>
      </div>
    </div>
  );

  return (
    <Card>
      <CardContent className="p-6">
        <h2 className="text-xl font-semibold text-primary mb-6">Recent Prescriptions</h2>
        
        {isLoading ? (
          <div className="space-y-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="border border-border rounded-lg p-4 animate-pulse">
                <div className="h-4 bg-muted rounded w-3/4 mb-2"></div>
                <div className="h-3 bg-muted rounded w-1/2 mb-3"></div>
                <div className="h-8 bg-muted rounded w-1/4"></div>
              </div>
            ))}
          </div>
        ) : !prescriptions || prescriptions.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            <Printer className="h-12 w-12 mx-auto mb-2 opacity-50" />
            <p>No prescriptions found</p>
            <p className="text-sm">Prescriptions will appear here after consultations</p>
          </div>
        ) : (
          <div className="space-y-4">
            {prescriptions.slice(0, 5).map((prescription: Prescription) => (
              <PrescriptionCard key={prescription.id} prescription={prescription} />
            ))}
          </div>
        )}
      </CardContent>
      
      {/* Print Dialog */}
      <Dialog open={showPrintDialog} onOpenChange={setShowPrintDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Prescription Details</DialogTitle>
          </DialogHeader>
          {selectedPrescription && selectedPatient && (
            <PrescriptionPrint 
              prescription={selectedPrescription}
              patient={selectedPatient}
              onPrint={() => setShowPrintDialog(false)}
            />
          )}
        </DialogContent>
      </Dialog>
    </Card>
  );
}