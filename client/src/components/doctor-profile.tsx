import { useState } from "react";
import { useLocation } from "wouter";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Settings, User, LogOut, Shield } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useForm } from "react-hook-form";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";
import { queryClient } from "@/lib/queryClient";

interface DoctorProfile {
  id: number;
  name: string;
  specialty: string;
  licenseNumber: string;
  email: string;
  phone: string;
  clinicName: string;
  clinicAddress: string;
}

export function DoctorProfile() {
  const { toast } = useToast();
  const [, setLocation] = useLocation();
  const [showProfileDialog, setShowProfileDialog] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  
  // In production, this would come from authentication context
  const [doctor, setDoctor] = useState<DoctorProfile>({
    id: 1,
    name: "Dr. Sarah Johnson",
    specialty: "Cardiologist",
    licenseNumber: "MD12345678",
    email: "sarah.johnson@medcenter.com",
    phone: "(555) 123-4567",
    clinicName: "Advanced Medical Center",
    clinicAddress: "123 Medical Drive, Healthcare City, HC 12345"
  });

  const form = useForm<DoctorProfile>({
    defaultValues: doctor,
  });

  const onSubmit = async (values: DoctorProfile) => {
    try {
      // In production, this would call API to update doctor profile
      setDoctor(values);
      toast({ title: "Profile updated successfully" });
      setShowProfileDialog(false);
    } catch (error) {
      toast({ 
        title: "Error updating profile", 
        description: "Please try again later",
        variant: "destructive"
      });
    }
  };

  const handleLogout = async () => {
    setIsLoggingOut(true);
    
    try {
      // Call logout API endpoint
      const accessToken = localStorage.getItem("accessToken");
      
      if (accessToken) {
        try {
          await apiRequest("POST", "/api/authentication/logout", {});
        } catch (error) {
          // Even if API call fails, we'll still clear local tokens
          console.warn("Logout API call failed, but clearing local tokens anyway:", error);
        }
      }

      // Clear tokens from localStorage
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");

      // Clear all cached queries
      queryClient.clear();

      toast({
        title: "Logged out",
        description: "You have been successfully logged out",
      });

      // Redirect to login page
      setLocation("/login");
    } catch (error: any) {
      console.error("Logout error:", error);
      
      // Even if there's an error, clear local tokens and redirect
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      queryClient.clear();
      
      toast({
        title: "Logged out",
        description: "You have been logged out",
      });
      
      setLocation("/login");
    } finally {
      setIsLoggingOut(false);
    }
  };

  return (
    <div className="flex items-center space-x-4">
      <Badge variant="outline" className="hidden md:flex">
        <Shield className="h-3 w-3 mr-1" />
        Licensed
      </Badge>
      
      <Dialog open={showProfileDialog} onOpenChange={setShowProfileDialog}>
        <DialogTrigger asChild>
          <Button variant="ghost" size="icon">
            <Settings className="h-5 w-5" />
          </Button>
        </DialogTrigger>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Doctor Profile Settings</DialogTitle>
          </DialogHeader>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Full Name</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="specialty"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Specialty</FormLabel>
                      <Select onValueChange={field.onChange} defaultValue={field.value}>
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select specialty" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="General Medicine">General Medicine</SelectItem>
                          <SelectItem value="Cardiologist">Cardiologist</SelectItem>
                          <SelectItem value="Pediatrician">Pediatrician</SelectItem>
                          <SelectItem value="Orthopedic">Orthopedic</SelectItem>
                          <SelectItem value="Dermatologist">Dermatologist</SelectItem>
                          <SelectItem value="Neurologist">Neurologist</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="licenseNumber"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>License Number</FormLabel>
                      <FormControl>
                        <Input {...field} />
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
                        <Input type="email" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="phone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Phone</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="clinicName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Clinic Name</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
              <FormField
                control={form.control}
                name="clinicAddress"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Clinic Address</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="flex justify-end space-x-2">
                <Button type="button" variant="outline" onClick={() => setShowProfileDialog(false)}>
                  Cancel
                </Button>
                <Button type="submit">Save Changes</Button>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
      
      <div className="flex items-center space-x-3">
        <div className="h-8 w-8 rounded-full bg-primary text-primary-foreground flex items-center justify-center text-sm font-medium">
          {doctor.name.split(' ').map(n => n[0]).join('')}
        </div>
        <div className="hidden md:block">
          <p className="text-sm font-medium text-primary">{doctor.name}</p>
          <p className="text-xs text-muted-foreground">{doctor.specialty}</p>
        </div>
      </div>
      
      <Button 
        variant="ghost" 
        size="icon" 
        onClick={handleLogout}
        disabled={isLoggingOut}
        title="Logout"
      >
        <LogOut className="h-5 w-5" />
      </Button>
    </div>
  );
}