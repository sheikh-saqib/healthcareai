import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Search, Plus, UserPlus, ChevronRight } from "lucide-react";
import { Patient } from "@shared/schema";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { insertPatientSchema } from "@shared/schema";
import { z } from "zod";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/hooks/use-toast";
import { apiRequest, queryClient } from "@/lib/queryClient";

interface PatientSearchProps {
  onNewConsultation: () => void;
  onAddPatient: () => void;
}

const patientFormSchema = insertPatientSchema.extend({
  email: z.string().email().optional().or(z.literal("")),
});

export default function PatientSearch({ onNewConsultation, onAddPatient }: PatientSearchProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [showAddPatientDialog, setShowAddPatientDialog] = useState(false);
  const { toast } = useToast();

  const { data: patients, isLoading } = useQuery({
    queryKey: ["/api/patients", { search: searchQuery }],
    enabled: searchQuery.length > 0,
  });

  const { data: recentPatients } = useQuery({
    queryKey: ["/api/patients"],
  });

  const form = useForm<z.infer<typeof patientFormSchema>>({
    resolver: zodResolver(patientFormSchema),
    defaultValues: {
      name: "",
      age: 0,
      gender: "",
      phone: "",
      email: "",
      medicalHistory: "",
    },
  });

  const handleAddPatient = () => {
    setShowAddPatientDialog(true);
  };

  const handlePatientClick = (patientId: string) => {
    // Navigate to patient details or history
    toast({ title: "Patient selected", description: "Patient details functionality coming soon" });
  };

  const onSubmit = async (values: z.infer<typeof patientFormSchema>) => {
    try {
      await apiRequest("POST", "/api/patients", values);
      queryClient.invalidateQueries({ queryKey: ["/api/patients"] });
      toast({ title: "Patient added successfully!" });
      setShowAddPatientDialog(false);
      form.reset();
    } catch (error) {
      toast({ title: "Error adding patient", variant: "destructive" });
    }
  };

  const getPatientInitials = (name: string) => {
    return name
      .split(" ")
      .map(n => n[0])
      .join("")
      .toUpperCase()
      .substring(0, 2);
  };

  const formatLastVisit = (createdAt: Date) => {
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - new Date(createdAt).getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 1) return "1 day ago";
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    return `${Math.floor(diffDays / 30)} months ago`;
  };

  const PatientCard = ({ patient }: { patient: Patient }) => (
    <div className="flex items-center justify-between p-4 bg-muted rounded-lg hover:bg-muted/80 transition-colors cursor-pointer" onClick={() => handlePatientClick(patient.id)}>
      <div className="flex items-center space-x-4">
        <div className="w-10 h-10 bg-primary text-primary-foreground rounded-full flex items-center justify-center font-medium">
          {getPatientInitials(patient.name)}
        </div>
        <div>
          <p className="font-medium text-primary">{patient.name}</p>
          <p className="text-sm text-muted-foreground">
            Last visit: {formatLastVisit(patient.createdAt)}
          </p>
        </div>
      </div>
      <div className="flex items-center space-x-2">
        <Badge variant="outline" className="bg-green-100 text-green-800">
          Active
        </Badge>
        <Button 
          variant="ghost" 
          size="sm" 
          onClick={(e) => {
            e.stopPropagation();
            onNewConsultation();
          }}
        >
          <Plus className="h-4 w-4" />
        </Button>
        <Button variant="ghost" size="sm">
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );

  return (
    <>
      <Card>
        <CardContent className="p-6">
          <h2 className="text-xl font-semibold text-primary mb-6">Patient Search & Management</h2>
          
          {/* Search Bar */}
          <div className="mb-6">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search patients by name, ID, or phone number..."
                className="pl-10"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>

          {/* Quick Actions */}
          <div className="flex flex-wrap gap-3 mb-6">
            <Button onClick={onNewConsultation} className="flex items-center space-x-2">
              <Plus className="h-4 w-4" />
              <span>New Consultation</span>
            </Button>
            <Button variant="outline" onClick={handleAddPatient} className="flex items-center space-x-2">
              <UserPlus className="h-4 w-4" />
              <span>Add Patient</span>
            </Button>
          </div>

          {/* Search Results or Recent Patients */}
          <div>
            <h3 className="text-lg font-medium text-primary mb-4">
              {searchQuery ? "Search Results" : "Recent Patients"}
            </h3>
            <div className="space-y-3">
              {isLoading ? (
                <div className="text-center py-4">
                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary mx-auto"></div>
                </div>
              ) : searchQuery ? (
                patients?.length ? (
                  patients.map((patient: Patient) => (
                    <PatientCard key={patient.id} patient={patient} />
                  ))
                ) : (
                  <div className="text-center py-4 text-muted-foreground">
                    No patients found matching "{searchQuery}"
                  </div>
                )
              ) : (
                recentPatients?.slice(0, 3).map((patient: Patient) => (
                  <PatientCard key={patient.id} patient={patient} />
                ))
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Add Patient Dialog */}
      <Dialog open={showAddPatientDialog} onOpenChange={setShowAddPatientDialog}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Add New Patient</DialogTitle>
          </DialogHeader>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Full Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Enter patient name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              
              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="age"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Age</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          placeholder="Age" 
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="gender"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Gender</FormLabel>
                      <Select onValueChange={field.onChange} defaultValue={field.value}>
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select..." />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="Male">Male</SelectItem>
                          <SelectItem value="Female">Female</SelectItem>
                          <SelectItem value="Other">Other</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
              
              <FormField
                control={form.control}
                name="phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Phone Number</FormLabel>
                    <FormControl>
                      <Input placeholder="(555) 123-4567" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input placeholder="patient@email.com" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              
              <FormField
                control={form.control}
                name="medicalHistory"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Medical History</FormLabel>
                    <FormControl>
                      <Textarea 
                        placeholder="Brief medical history..." 
                        className="resize-none"
                        rows={3}
                        {...field} 
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              
              <div className="flex space-x-3 pt-4">
                <Button type="submit" className="flex-1">
                  Create Patient
                </Button>
                <Button 
                  type="button" 
                  variant="outline" 
                  className="flex-1"
                  onClick={() => setShowAddPatientDialog(false)}
                >
                  Cancel
                </Button>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </>
  );
}
