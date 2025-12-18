import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Users, Calendar, Pill, Mic, Bell, Search, Plus, UserPlus } from "lucide-react";
import PatientSearch from "@/components/patient-search";
import ConsultationRecorder from "@/components/consultation-recorder";
import EnhancedConsultationRecorder from "@/components/enhanced-consultation-recorder";
import PatientSelectionForConsultation from "@/components/patient-selection-for-consultation";
import PendingReviews from "@/components/pending-reviews";
import PrescriptionList from "@/components/prescription-list";
import { RealTimeStatus } from "@/components/real-time-status";
import { LiveActivityFeed } from "@/components/live-activity-feed";
import { DoctorProfile } from "@/components/doctor-profile";
import { ProductionWarnings } from "@/components/production-warnings";
import { ErrorBoundary } from "@/components/error-boundary";
import { useWebSocket } from "@/hooks/use-websocket";
import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";

interface StatsData {
  totalPatients?: number;
  todayConsultations?: number;
  pendingPrescriptions?: number;
  recordedConsultations?: number;
}

export default function Dashboard() {
  const { toast } = useToast();
  const [showNewPatientDialog, setShowNewPatientDialog] = useState(false);
  const [showNewConsultationDialog, setShowNewConsultationDialog] = useState(false);
  const [selectedPatientId, setSelectedPatientId] = useState<string | undefined>(undefined);
  const [showPatientSelectionDialog, setShowPatientSelectionDialog] = useState(false);
  const [showAllPrescriptionsDialog, setShowAllPrescriptionsDialog] = useState(false);
  const [showPatientHistoryDialog, setShowPatientHistoryDialog] = useState(false);
  const { lastMessage } = useWebSocket();

  const { data: stats, isLoading: statsLoading, error: statsError, refetch: refetchStats } = useQuery<StatsData>({
    queryKey: ["/api/stats"],
    refetchInterval: 30000, // Refresh every 30 seconds
    retry: false, // Don't retry on failure
  });

  // Auto-refresh data when receiving real-time updates
  useEffect(() => {
    if (lastMessage) {
      refetchStats();
    }
  }, [lastMessage, refetchStats]);

  const handleNewConsultation = () => {
    setShowPatientSelectionDialog(true);
  };

  const handlePatientSelected = (patientId: string) => {
    setSelectedPatientId(patientId);
    setShowPatientSelectionDialog(false);
    setShowNewConsultationDialog(true);
  };

  const handleAddPatient = () => {
    setShowNewPatientDialog(true);
  };

  const handleViewAllPrescriptions = () => {
    setShowAllPrescriptionsDialog(true);
  };

  const handlePatientHistory = () => {
    setShowPatientHistoryDialog(true);
  };

  const StatCard = ({ icon: Icon, title, value, color }: { 
    icon: any, 
    title: string, 
    value: number | string, 
    color: string 
  }) => (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-6">
        <div className="flex items-center">
          <div className={`flex-shrink-0 p-3 rounded-full ${color}`}>
            <Icon className="h-6 w-6 text-white" />
          </div>
          <div className="ml-4">
            <p className="text-2xl font-semibold text-primary">{value}</p>
            <p className="text-sm text-muted-foreground">{title}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );

  return (
    <ErrorBoundary>
      <div className="min-h-screen bg-medical-bg">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-border sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            {/* Logo and Brand */}
            <div className="flex items-center">
              <div className="flex-shrink-0 flex items-center">
                <div className="bg-primary text-primary-foreground rounded-lg p-2 mr-3">
                  <div className="w-6 h-6 relative">
                    <svg viewBox="0 0 24 24" fill="currentColor" className="w-full h-full">
                      <path d="M19 8h-2v3h-3v2h3v3h2v-3h3v-2h-3V8zM4 18h14v-2H4v2zM4 6v2h14V6H4zm0 5h14v-2H4v2z"/>
                    </svg>
                  </div>
                </div>
                <h1 className="text-2xl font-semibold text-primary">MedConsult</h1>
              </div>
            </div>

            {/* Navigation Items */}
            <nav className="hidden md:flex space-x-8">
              <a href="#dashboard" className="text-primary border-b-2 border-primary pb-2 px-1 text-sm font-medium">
                Dashboard
              </a>
              <a href="#patients" className="text-muted-foreground hover:text-primary pb-2 px-1 text-sm font-medium">
                Patients
              </a>
              <a href="#consultations" className="text-muted-foreground hover:text-primary pb-2 px-1 text-sm font-medium">
                Consultations
              </a>
              <a href="#prescriptions" className="text-muted-foreground hover:text-primary pb-2 px-1 text-sm font-medium">
                Prescriptions
              </a>
            </nav>

            {/* Real-time Status & User Profile */}
            <div className="flex items-center space-x-4">
              <RealTimeStatus />
              <Button variant="ghost" size="icon">
                <Bell className="h-5 w-5" />
              </Button>
              <DoctorProfile />
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        
        {/* Backend Status */}
        {/* {statsError && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">Backend Connection Issue</h3>
                <div className="mt-2 text-sm text-red-700">
                  <p>Unable to connect to the backend API. This might be because:</p>
                  <ul className="list-disc list-inside mt-1 space-y-1">
                    <li>The .NET backend server isn't running (run: <code className="bg-red-100 px-1 rounded">dotnet run --project HealthCareAI.API</code>)</li>
                    <li>PostgreSQL database isn't set up or running</li>
                    <li>Database migrations need to be applied</li>
                  </ul>
                  <p className="mt-2">
                    <strong>Backend should be running on:</strong> <a href="http://localhost:5044/swagger" target="_blank" rel="noopener noreferrer" className="text-red-600 underline">http://localhost:5044/swagger</a>
                  </p>
                </div>
              </div>
            </div>
          </div>
        )} */}

        {statsLoading && !statsError && (
          <div className="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <svg className="animate-spin h-5 w-5 text-blue-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </div>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-blue-800">Connecting to Backend...</h3>
                <p className="text-sm text-blue-700">Loading application data</p>
              </div>
            </div>
          </div>
        )}

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard
            title="Total Patients"
            value={stats?.totalPatients ?? 0}
            icon={Users}
            color="blue"
          />
          <StatCard
            title="Today's Consultations"
            value={stats?.todayConsultations ?? 0}
            icon={Calendar}
            color="green"
          />
          <StatCard
            title="Pending Reviews"
            value={stats?.pendingPrescriptions ?? 0}
            icon={Pill}
            color="yellow"
          />
          <StatCard
            title="Recorded Consultations"
            value={stats?.recordedConsultations ?? 0}
            icon={Mic}
            color="purple"
          />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column - Patient Search & New Consultation */}
          <div className="lg:col-span-2 space-y-6">
            <PatientSearch onNewConsultation={handleNewConsultation} onAddPatient={handleAddPatient} />
            <EnhancedConsultationRecorder selectedPatientId={selectedPatientId} />
          </div>

          {/* Right Column - Live Activity, Pending Reviews & Quick Actions */}
          <div className="space-y-6">
            <LiveActivityFeed />
            <PendingReviews />
            
            {/* Quick Actions */}
            <Card>
              <CardContent className="p-6">
                <h2 className="text-xl font-semibold text-primary mb-6">Quick Actions</h2>
                <div className="space-y-3">
                  <Button 
                    className="w-full justify-between h-auto p-4" 
                    onClick={handleNewConsultation}
                  >
                    <div className="flex items-center">
                      <Mic className="mr-3 h-5 w-5" />
                      <span>Start New Recording</span>
                    </div>
                    <div className="ml-2">→</div>
                  </Button>
                  
                  <Button 
                    variant="outline" 
                    className="w-full justify-between h-auto p-4"
                    onClick={handleViewAllPrescriptions}
                  >
                    <div className="flex items-center">
                      <Pill className="mr-3 h-5 w-5" />
                      <span>View All Prescriptions</span>
                    </div>
                    <div className="ml-2">→</div>
                  </Button>
                  
                  <Button 
                    variant="outline" 
                    className="w-full justify-between h-auto p-4"
                    onClick={handlePatientHistory}
                  >
                    <div className="flex items-center">
                      <Users className="mr-3 h-5 w-5" />
                      <span>Patient History</span>
                    </div>
                    <div className="ml-2">→</div>
                  </Button>
                </div>
              </CardContent>
            </Card>

            <PrescriptionList />
          </div>
        </div>
      </main>

      {/* Floating Action Button */}
      <div className="fixed bottom-6 right-6 z-50">
        <Button 
          size="lg" 
          className="w-16 h-16 rounded-full shadow-lg"
          onClick={handleNewConsultation}
        >
          <Plus className="h-8 w-8" />
        </Button>
      </div>

      {/* Dialogs */}
      <Dialog open={showPatientSelectionDialog} onOpenChange={setShowPatientSelectionDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Select Patient for Consultation</DialogTitle>
          </DialogHeader>
          <PatientSelectionForConsultation onPatientSelected={handlePatientSelected} />
        </DialogContent>
      </Dialog>

      <Dialog open={showNewConsultationDialog} onOpenChange={setShowNewConsultationDialog}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Start New Consultation</DialogTitle>
          </DialogHeader>
          <EnhancedConsultationRecorder 
            onClose={() => setShowNewConsultationDialog(false)} 
            selectedPatientId={selectedPatientId || undefined}
          />
        </DialogContent>
      </Dialog>

      {/* Add Patient Dialog */}
      <Dialog open={showNewPatientDialog} onOpenChange={setShowNewPatientDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add New Patient</DialogTitle>
          </DialogHeader>
          <PatientSearch onNewConsultation={handleNewConsultation} onAddPatient={handleAddPatient} />
        </DialogContent>
      </Dialog>

      {/* View All Prescriptions Dialog */}
      <Dialog open={showAllPrescriptionsDialog} onOpenChange={setShowAllPrescriptionsDialog}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>All Prescriptions</DialogTitle>
          </DialogHeader>
          <PrescriptionList />
        </DialogContent>
      </Dialog>

      {/* Patient History Dialog */}
      <Dialog open={showPatientHistoryDialog} onOpenChange={setShowPatientHistoryDialog}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Patient History</DialogTitle>
          </DialogHeader>
          <PatientSearch onNewConsultation={handleNewConsultation} onAddPatient={handleAddPatient} />
        </DialogContent>
      </Dialog>

      {/* Production Warnings */}
      <ProductionWarnings isDevelopment={import.meta.env.DEV} />
    </div>
    </ErrorBoundary>
  );
}
