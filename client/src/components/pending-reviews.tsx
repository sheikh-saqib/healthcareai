import { useQuery, useMutation } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Check, Edit, X, Printer, FileText } from "lucide-react";
import { Prescription } from "@shared/schema";
import { apiRequest, queryClient } from "@/lib/queryClient";
import { useToast } from "@/hooks/use-toast";
import { useWebSocket } from "@/hooks/use-websocket";
import { PrescriptionPrint } from "./prescription-print";
import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export default function PendingReviews() {
  const { toast } = useToast();
  const { lastMessage } = useWebSocket();
  const [showPrintDialog, setShowPrintDialog] = useState(false);
  const [selectedPrescription, setSelectedPrescription] = useState<Prescription | null>(null);
  
  const { data: pendingPrescriptions, isLoading, refetch } = useQuery({
    queryKey: ["/api/prescriptions", { status: "pending" }],
    refetchInterval: 10000, // Refresh every 10 seconds
  });

  // Get patient data for the selected prescription
  const { data: selectedPatient } = useQuery({
    queryKey: [`/api/patients/${selectedPrescription?.patientId}`],
    enabled: !!selectedPrescription?.patientId,
  });

  // Auto-refresh when receiving real-time updates
  useEffect(() => {
    if (lastMessage && (lastMessage.type === 'prescription_created' || lastMessage.type === 'prescription_updated')) {
      refetch();
    }
  }, [lastMessage, refetch]);

  const updatePrescriptionMutation = useMutation({
    mutationFn: async ({ id, status, medications }: { id: string; status: string; medications?: string }) => {
      const updateData: any = { status };
      if (medications) {
        updateData.medications = medications;
      }
      return apiRequest("PUT", `/api/prescriptions/${id}`, updateData);
    },
    onSuccess: (data, { id, status }) => {
      if (status === "approved") {
        const approvedPrescription = pendingPrescriptions?.find(p => p.id === id);
        if (approvedPrescription) {
          setSelectedPrescription(approvedPrescription);
          setShowPrintDialog(true);
        }
      }
      queryClient.invalidateQueries({ queryKey: ["/api/prescriptions"] });
      queryClient.invalidateQueries({ queryKey: ["/api/prescriptions", { status: "pending" }] });
      queryClient.invalidateQueries({ queryKey: ["/api/stats"] });
    },
    onError: (error) => {
      console.error("Error updating prescription:", error);
      toast({ 
        title: "Error updating prescription", 
        description: error.message || "An error occurred while updating the prescription",
        variant: "destructive"
      });
    },
  });

  const handleApprove = (prescriptionId: string) => {
    updatePrescriptionMutation.mutate({ id: prescriptionId, status: "approved" });
    toast({ 
      title: "Prescription approved successfully", 
      description: "Prescription is now ready for PDF generation" 
    });
  };

  const handleReject = (prescriptionId: string) => {
    updatePrescriptionMutation.mutate({ id: prescriptionId, status: "rejected" });
    toast({ title: "Prescription rejected" });
  };

  const handleEdit = (prescriptionId: string) => {
    // TODO: Open edit dialog
    toast({ title: "Edit prescription", description: "Edit functionality coming soon" });
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  };

  const PrescriptionCard = ({ prescription }: { prescription: Prescription }) => {
    const { data: patient } = useQuery({
      queryKey: ["/api/patients", prescription.patientId],
      enabled: !!prescription.patientId,
    });

    return (
      <div className="border border-border rounded-lg p-6 hover:bg-muted/50 transition-colors">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h3 className="font-semibold text-primary text-lg">
              {patient ? patient.name : `Patient ID: ${prescription.patientId}`}
            </h3>
            <p className="text-sm text-muted-foreground">
              {patient && `${patient.age} years old, ${patient.gender}`}
            </p>
            <p className="text-sm text-muted-foreground">{formatDate(prescription.createdAt)}</p>
          </div>
          <Badge variant="outline" className="bg-orange-100 text-orange-800">
            Pending Review
          </Badge>
        </div>
        
        <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-4 mb-4">
          <h4 className="font-medium text-primary mb-2">Prescribed Medications:</h4>
          <p className="text-sm mb-3">{prescription.medications || "No medications specified"}</p>
          
          <h4 className="font-medium text-primary mb-2">Dosage Instructions:</h4>
          <p className="text-sm">{prescription.dosageInstructions || "No instructions provided"}</p>
          
          {prescription.notes && (
            <>
              <h4 className="font-medium text-primary mb-2 mt-3">Additional Notes:</h4>
              <p className="text-sm">{prescription.notes}</p>
            </>
          )}
        </div>
        
        <div className="flex space-x-3">
          <Button 
            size="sm" 
            className="bg-green-500 hover:bg-green-600 text-white"
            onClick={() => handleApprove(prescription.id)}
            disabled={updatePrescriptionMutation.isPending}
          >
            <Check className="h-4 w-4 mr-1" />
            Approve
          </Button>
          
          <Button 
            size="sm" 
            variant="outline"
            onClick={() => handleEdit(prescription.id)}
            disabled={updatePrescriptionMutation.isPending}
          >
            <Edit className="h-4 w-4 mr-1" />
            Edit
          </Button>
          
          <Button 
            size="sm" 
            variant="destructive"
            onClick={() => handleReject(prescription.id)}
            disabled={updatePrescriptionMutation.isPending}
          >
            <X className="h-4 w-4 mr-1" />
            Reject
          </Button>
        </div>
      </div>
    );
  };

  return (
    <Card>
      <CardContent className="p-6">
        <h2 className="text-xl font-semibold text-primary mb-6">Pending Reviews</h2>
        
        <div className="space-y-4">
          {isLoading ? (
            <div className="flex items-center justify-center py-8">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
            </div>
          ) : pendingPrescriptions?.length ? (
            pendingPrescriptions.map((prescription: Prescription) => (
              <PrescriptionCard key={prescription.id} prescription={prescription} />
            ))
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="h-8 w-8 mx-auto mb-2" />
              <p>No pending prescriptions for review</p>
              <p className="text-sm mt-1">Approved prescriptions will appear in the main prescription list</p>
            </div>
          )}
        </div>
      </CardContent>
      
      {/* Print Dialog */}
      <Dialog open={showPrintDialog} onOpenChange={setShowPrintDialog}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Print Prescription</DialogTitle>
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
