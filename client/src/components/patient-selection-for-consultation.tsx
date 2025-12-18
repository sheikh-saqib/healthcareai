import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Search, Users, ChevronRight } from "lucide-react";
import { Patient } from "@shared/schema";

interface PatientSelectionProps {
  onPatientSelected: (patientId: string) => void;
}

export default function PatientSelectionForConsultation({ onPatientSelected }: PatientSelectionProps) {
  const [searchQuery, setSearchQuery] = useState("");

  const { data: patients, isLoading } = useQuery({
    queryKey: ["/api/patients", { search: searchQuery }],
    enabled: searchQuery.length > 0,
  });

  const { data: recentPatients } = useQuery({
    queryKey: ["/api/patients"],
  });

  const PatientCard = ({ patient }: { patient: Patient }) => (
    <Card className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => onPatientSelected(patient.id)}>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
              <Users className="h-6 w-6 text-primary" />
            </div>
            <div>
              <h3 className="font-semibold text-primary">{patient.name}</h3>
              <p className="text-sm text-muted-foreground">
                {patient.age} years, {patient.gender}
              </p>
              <p className="text-xs text-muted-foreground">{patient.phone}</p>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            {patient.medicalHistory && (
              <Badge variant="secondary" className="text-xs">
                {patient.medicalHistory.split(',')[0].trim()}
              </Badge>
            )}
            <ChevronRight className="h-4 w-4 text-muted-foreground" />
          </div>
        </div>
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-6">
      {/* Search Bar */}
      <div className="relative">
        <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search patients by name..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10"
        />
      </div>

      {/* Search Results */}
      {searchQuery.length > 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-primary">Search Results</h3>
          {isLoading ? (
            <div className="text-center py-8">
              <div className="animate-pulse text-muted-foreground">Searching patients...</div>
            </div>
          ) : patients && patients.length > 0 ? (
            <div className="space-y-3">
              {patients.map((patient: Patient) => (
                <PatientCard key={patient.id} patient={patient} />
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No patients found matching your search.</p>
            </div>
          )}
        </div>
      )}

      {/* Recent Patients */}
      {searchQuery.length === 0 && (
        <div className="space-y-4">
          <h3 className="text-lg font-semibold text-primary">Recent Patients</h3>
          {recentPatients && recentPatients.length > 0 ? (
            <div className="space-y-3">
              {recentPatients.slice(0, 5).map((patient: Patient) => (
                <PatientCard key={patient.id} patient={patient} />
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">No patients available.</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}