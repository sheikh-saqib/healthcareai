import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { AlertTriangle, Shield, Database, Users, Lock, Settings } from "lucide-react";
import { useState } from "react";

interface ProductionWarningsProps {
  isDevelopment?: boolean;
}

export function ProductionWarnings({ isDevelopment = true }: ProductionWarningsProps) {
  const [showWarnings, setShowWarnings] = useState(isDevelopment);

  if (!showWarnings) return null;

  const warnings = [
    {
      icon: Lock,
      title: "Authentication Required",
      description: "User login/logout system needs implementation",
      priority: "critical",
      status: "missing"
    },
    {
      icon: Shield,
      title: "HIPAA Compliance",
      description: "Patient data encryption and privacy controls needed",
      priority: "critical",
      status: "missing"
    },
    {
      icon: Database,
      title: "Data Backup",
      description: "Database backup and recovery system required",
      priority: "high",
      status: "missing"
    },
    {
      icon: Users,
      title: "Role-Based Access",
      description: "Different user roles and permissions needed",
      priority: "high",
      status: "missing"
    },
    {
      icon: Settings,
      title: "Environment Config",
      description: "Production vs development environment separation",
      priority: "medium",
      status: "partial"
    }
  ];

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "critical": return "bg-red-500";
      case "high": return "bg-orange-500";
      case "medium": return "bg-yellow-500";
      default: return "bg-gray-500";
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "missing": return "destructive";
      case "partial": return "secondary";
      case "complete": return "default";
      default: return "outline";
    }
  };

  return (
    <div className="fixed bottom-4 left-4 z-50 max-w-md">
      <Alert className="border-orange-500 bg-orange-50">
        <AlertTriangle className="h-4 w-4" />
        <AlertDescription className="font-medium">
          Development Mode - Production Checklist
        </AlertDescription>
      </Alert>
      
      <Card className="mt-2">
        <CardContent className="p-4">
          <div className="flex justify-between items-center mb-3">
            <h3 className="font-semibold text-sm">Production Readiness</h3>
            <Button 
              variant="ghost" 
              size="sm"
              onClick={() => setShowWarnings(false)}
              className="h-6 w-6 p-0"
            >
              ×
            </Button>
          </div>
          
          <div className="space-y-2">
            {warnings.map((warning, index) => (
              <div key={index} className="flex items-start space-x-2 text-xs">
                <div className={`rounded-full p-1 ${getPriorityColor(warning.priority)}`}>
                  <warning.icon className="h-3 w-3 text-white" />
                </div>
                <div className="flex-1">
                  <div className="flex items-center space-x-2">
                    <span className="font-medium">{warning.title}</span>
                    <Badge variant={getStatusColor(warning.status)} className="text-xs">
                      {warning.status}
                    </Badge>
                  </div>
                  <p className="text-muted-foreground mt-1">{warning.description}</p>
                </div>
              </div>
            ))}
          </div>
          
          <div className="mt-3 pt-3 border-t">
            <div className="flex justify-between text-xs text-muted-foreground">
              <span>Critical: 2 • High: 2 • Medium: 1</span>
              <span>Ready: 0/5</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}