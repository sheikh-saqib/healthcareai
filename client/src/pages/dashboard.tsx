import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Users, Calendar, Pill, Mic, Bell, Search, Plus, UserPlus, LayoutDashboard, FileText, Stethoscope, Clock, AlertTriangle, CheckCircle2, ArrowRight } from "lucide-react";
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
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarRail,
  SidebarTrigger,
} from "@/components/ui/sidebar";

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

  const { data: stats, isLoading: statsLoading, error: statsError } = useQuery<StatsData>({
    queryKey: ["/api/stats"],
    retry: false, // Don't retry on failure
  });

  // Fetch today's consultations/appointments
  const { data: todayConsultations } = useQuery({
    queryKey: ["/api/consultations", { date: new Date().toISOString().split('T')[0] }],
  });

  // Fetch recent consultations
  const { data: recentConsultations } = useQuery({
    queryKey: ["/api/consultations", { limit: 5, sort: "desc" }],
  });

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

  const scrollToPendingReviews = () => {
    const element = document.getElementById('pending-reviews-section');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  };

  const StatCard = ({ icon: Icon, title, value, color }: { 
    icon: any, 
    title: string, 
    value: number | string, 
    color: string 
  }) => {
    const colorClasses: Record<string, { bg: string; gradient: string; iconBg: string }> = {
      blue: { 
        bg: "bg-blue-50 border-blue-100", 
        gradient: "from-blue-500 to-blue-600",
        iconBg: "bg-gradient-to-br from-blue-500 to-blue-600"
      },
      green: { 
        bg: "bg-green-50 border-green-100", 
        gradient: "from-green-500 to-green-600",
        iconBg: "bg-gradient-to-br from-green-500 to-green-600"
      },
      yellow: { 
        bg: "bg-amber-50 border-amber-100", 
        gradient: "from-amber-500 to-amber-600",
        iconBg: "bg-gradient-to-br from-amber-500 to-amber-600"
      },
      purple: { 
        bg: "bg-purple-50 border-purple-100", 
        gradient: "from-purple-500 to-purple-600",
        iconBg: "bg-gradient-to-br from-purple-500 to-purple-600"
      },
    };
    
    const colors = colorClasses[color] || colorClasses.blue;
    
    return (
      <Card className={`hover:shadow-lg transition-all duration-200 border-2 ${colors.bg} shadow-sm`}>
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className={`flex-shrink-0 p-3.5 rounded-xl ${colors.iconBg} shadow-md`}>
                <Icon className="h-6 w-6 text-white" />
              </div>
              <div>
                <p className="text-3xl font-bold text-gray-900 mb-1">{value}</p>
                <p className="text-sm font-medium text-gray-600">{title}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  };

  return (
    <ErrorBoundary>
      <SidebarProvider>
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex w-full">
          <Sidebar>
            <SidebarHeader className="border-b border-sidebar-border bg-white">
              <div className="flex items-center gap-3 px-4 py-4">
                <div className="bg-gradient-to-br from-blue-600 to-blue-700 text-white rounded-xl p-2.5 shadow-lg">
                  <div className="w-7 h-7 relative">
                    <svg viewBox="0 0 24 24" fill="currentColor" className="w-full h-full">
                      <path d="M19 8h-2v3h-3v2h3v3h2v-3h3v-2h-3V8zM4 18h14v-2H4v2zM4 6v2h14V6H4zm0 5h14v-2H4v2z"/>
                    </svg>
                  </div>
                </div>
                <div>
                  <h1 className="text-xl font-bold text-gray-900">MedConsult</h1>
                  <p className="text-xs text-gray-500 font-medium">Healthcare AI Platform</p>
                </div>
              </div>
            </SidebarHeader>
            <SidebarContent className="px-2 py-4">
              <SidebarGroup>
                <SidebarGroupLabel className="px-3 text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                  Navigation
                </SidebarGroupLabel>
                <SidebarGroupContent>
                  <SidebarMenu className="space-y-1">
                    <SidebarMenuItem>
                      <SidebarMenuButton isActive className="rounded-lg">
                        <LayoutDashboard className="h-5 w-5" />
                        <span className="font-medium">Dashboard</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                    <SidebarMenuItem>
                      <SidebarMenuButton className="rounded-lg">
                        <Users className="h-5 w-5" />
                        <span className="font-medium">Patients</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                    <SidebarMenuItem>
                      <SidebarMenuButton className="rounded-lg">
                        <Stethoscope className="h-5 w-5" />
                        <span className="font-medium">Consultations</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                    <SidebarMenuItem>
                      <SidebarMenuButton className="rounded-lg">
                        <Pill className="h-5 w-5" />
                        <span className="font-medium">Prescriptions</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  </SidebarMenu>
                </SidebarGroupContent>
              </SidebarGroup>
            </SidebarContent>
            <SidebarFooter>
              <div className="px-2 py-2">
                <RealTimeStatus />
              </div>
            </SidebarFooter>
            <SidebarRail />
          </Sidebar>
          <SidebarInset>
            {/* Header */}
            <header className="bg-white/95 backdrop-blur-sm shadow-md border-b border-gray-200 sticky top-0 z-50">
              <div className="flex h-16 items-center gap-4 px-6">
                <SidebarTrigger className="hover:bg-gray-100" />
                <div className="h-6 w-px bg-gray-200" />
                <div className="flex-1" />
                <div className="flex items-center gap-3">
                  <Button variant="ghost" size="icon" className="relative hover:bg-gray-100">
                    <Bell className="h-5 w-5 text-gray-600" />
                  </Button>
                  <DoctorProfile />
                </div>
              </div>
            </header>

            {/* Main Content */}
            <main className="flex-1 overflow-auto p-8 bg-gradient-to-br from-gray-50 to-gray-100">
        
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

        {/* Priority Section - Today's Schedule & Urgent Items */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-gray-900">Today's Schedule</h2>
            <Button variant="outline" size="sm" onClick={handleNewConsultation}>
              <Plus className="h-4 w-4 mr-2" />
              New Consultation
            </Button>
          </div>
          
          {/* Today's Stats - Quick Overview */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <StatCard
              title="Today's Patients"
              value={stats?.todayConsultations ?? 0}
              icon={Users}
              color="blue"
            />
            <StatCard
              title="Pending Reviews"
              value={stats?.pendingPrescriptions ?? 0}
              icon={Pill}
              color="yellow"
            />
            <StatCard
              title="Completed Today"
              value={stats?.recordedConsultations ?? 0}
              icon={CheckCircle2}
              color="green"
            />
            <StatCard
              title="Total Patients"
              value={stats?.totalPatients ?? 0}
              icon={Users}
              color="purple"
            />
          </div>

          {/* Urgent Pending Reviews - Make it prominent */}
          {stats?.pendingPrescriptions && stats.pendingPrescriptions > 0 && (
            <Card className="mb-6 border-2 border-amber-300 bg-amber-50">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="p-3 bg-amber-500 rounded-lg">
                      <AlertTriangle className="h-6 w-6 text-white" />
                    </div>
                    <div>
                      <h3 className="text-lg font-bold text-gray-900">
                        {stats.pendingPrescriptions} Prescription{stats.pendingPrescriptions > 1 ? 's' : ''} Awaiting Review
                      </h3>
                      <p className="text-sm text-gray-600">Action required: Review and approve pending prescriptions</p>
                    </div>
                  </div>
                  <Button 
                    onClick={scrollToPendingReviews}
                    className="bg-amber-600 hover:bg-amber-700"
                  >
                    Review Now
                    <ArrowRight className="h-4 w-4 ml-2" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left Column - Main Content */}
          <div className="lg:col-span-2 space-y-6">
            {/* Today's Appointments/Schedule */}
            <Card>
              <CardContent className="p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                    <Clock className="h-5 w-5 text-blue-600" />
                    Today's Appointments
                  </h3>
                  <Button variant="ghost" size="sm">
                    View All
                    <ArrowRight className="h-4 w-4 ml-1" />
                  </Button>
                </div>
                {todayConsultations && Array.isArray(todayConsultations) && todayConsultations.length > 0 ? (
                  <div className="space-y-3">
                    {todayConsultations.slice(0, 5).map((consultation: any) => (
                      <div key={consultation.id} className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                            <Users className="h-5 w-5 text-blue-600" />
                          </div>
                          <div>
                            <p className="font-medium text-gray-900">Patient ID: {consultation.patientId}</p>
                            <p className="text-sm text-gray-500">
                              {consultation.scheduledDateTime 
                                ? new Date(consultation.scheduledDateTime).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' })
                                : 'No time specified'}
                            </p>
                          </div>
                        </div>
                        <Badge variant={consultation.status === 'Completed' ? 'default' : 'secondary'}>
                          {consultation.status}
                        </Badge>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <Calendar className="h-12 w-12 mx-auto mb-2 opacity-50" />
                    <p>No appointments scheduled for today</p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Recent Consultations */}
            <Card>
              <CardContent className="p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                    <Stethoscope className="h-5 w-5 text-green-600" />
                    Recent Consultations
                  </h3>
                  <Button variant="ghost" size="sm">
                    View All
                    <ArrowRight className="h-4 w-4 ml-1" />
                  </Button>
                </div>
                {recentConsultations && Array.isArray(recentConsultations) && recentConsultations.length > 0 ? (
                  <div className="space-y-3">
                    {recentConsultations.map((consultation: any) => (
                      <div key={consultation.id} className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors cursor-pointer">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
                            <FileText className="h-5 w-5 text-green-600" />
                          </div>
                          <div>
                            <p className="font-medium text-gray-900">Patient ID: {consultation.patientId}</p>
                            <p className="text-sm text-gray-500">
                              {consultation.createdAt 
                                ? new Date(consultation.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' })
                                : 'Date not available'}
                            </p>
                            {consultation.diagnosis && (
                              <p className="text-sm text-blue-600 mt-1">{consultation.diagnosis}</p>
                            )}
                          </div>
                        </div>
                        <Badge variant="outline">{consultation.status}</Badge>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <Stethoscope className="h-12 w-12 mx-auto mb-2 opacity-50" />
                    <p>No recent consultations</p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Patient Search */}
            <PatientSearch onNewConsultation={handleNewConsultation} onAddPatient={handleAddPatient} />
          </div>

          {/* Right Column - Quick Actions & Important Items */}
          <div className="space-y-6">
            {/* Quick Actions Card */}
            <Card className="border-2 border-blue-200 bg-blue-50/50">
              <CardContent className="p-6">
                <h3 className="text-lg font-bold text-gray-900 mb-4">Quick Actions</h3>
                <div className="space-y-2.5">
                  <Button 
                    className="w-full justify-start h-auto p-4 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 shadow-md" 
                    onClick={handleNewConsultation}
                  >
                    <Mic className="mr-3 h-5 w-5" />
                    <span className="font-medium">Start New Consultation</span>
                  </Button>
                  
                  <Button 
                    variant="outline" 
                    className="w-full justify-start h-auto p-4 border-gray-300 hover:bg-gray-50 hover:border-gray-400"
                    onClick={handleAddPatient}
                  >
                    <UserPlus className="mr-3 h-5 w-5" />
                    <span className="font-medium">Add New Patient</span>
                  </Button>
                  
                  <Button 
                    variant="outline" 
                    className="w-full justify-start h-auto p-4 border-gray-300 hover:bg-gray-50 hover:border-gray-400"
                    onClick={handleViewAllPrescriptions}
                  >
                    <Pill className="mr-3 h-5 w-5" />
                    <span className="font-medium">View Prescriptions</span>
                  </Button>
                </div>
              </CardContent>
            </Card>

            {/* Pending Reviews - Prominent */}
            <div id="pending-reviews-section">
              <PendingReviews />
            </div>

            {/* Live Activity */}
            <LiveActivityFeed />

            {/* Recent Prescriptions */}
            <PrescriptionList />
          </div>
        </div>
            </main>

            {/* Floating Action Button */}
            <div className="fixed bottom-8 right-8 z-50">
              <Button 
                size="lg" 
                className="w-16 h-16 rounded-full shadow-xl bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 border-0"
                onClick={handleNewConsultation}
              >
                <Plus className="h-8 w-8" />
              </Button>
            </div>
          </SidebarInset>
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
      </SidebarProvider>
    </ErrorBoundary>
  );
}
