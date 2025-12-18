import { useEffect, useState } from "react";
import { useWebSocket } from "@/hooks/use-websocket";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Users, Brain, Pill, UserPlus, Activity } from "lucide-react";

interface ActivityItem {
  id: string;
  type: string;
  message: string;
  timestamp: Date;
  data?: any;
}

export function LiveActivityFeed() {
  const { lastMessage } = useWebSocket();
  const [activities, setActivities] = useState<ActivityItem[]>([]);

  useEffect(() => {
    if (lastMessage && lastMessage.type !== 'connection_established') {
      const newActivity: ActivityItem = {
        id: Date.now().toString(),
        type: lastMessage.type,
        message: getActivityMessage(lastMessage),
        timestamp: new Date(),
        data: lastMessage.data
      };

      setActivities(prev => [newActivity, ...prev.slice(0, 9)]); // Keep only last 10 activities
    }
  }, [lastMessage]);

  const getActivityMessage = (message: any): string => {
    switch (message.type) {
      case 'patient_created':
        return `New patient "${message.data?.name}" added to system`;
      case 'consultation_created':
        return `Consultation started for patient ID ${message.data?.patientId}`;
      case 'consultation_analyzed':
        return `AI analysis completed for consultation`;
      case 'prescription_created':
        return `New prescription generated for patient ID ${message.data?.patientId}`;
      case 'prescription_updated':
        return `Prescription ${message.data?.status} by doctor`;
      default:
        return 'System activity detected';
    }
  };

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'patient_created':
        return <UserPlus className="h-4 w-4 text-blue-500" />;
      case 'consultation_created':
        return <Users className="h-4 w-4 text-green-500" />;
      case 'consultation_analyzed':
        return <Brain className="h-4 w-4 text-purple-500" />;
      case 'prescription_created':
        return <Pill className="h-4 w-4 text-orange-500" />;
      case 'prescription_updated':
        return <Pill className="h-4 w-4 text-red-500" />;
      default:
        return <Activity className="h-4 w-4 text-gray-500" />;
    }
  };

  const getActivityBadge = (type: string) => {
    switch (type) {
      case 'patient_created':
        return <Badge variant="outline" className="bg-blue-100 text-blue-800">Patient</Badge>;
      case 'consultation_created':
        return <Badge variant="outline" className="bg-green-100 text-green-800">Consultation</Badge>;
      case 'consultation_analyzed':
        return <Badge variant="outline" className="bg-purple-100 text-purple-800">AI Analysis</Badge>;
      case 'prescription_created':
        return <Badge variant="outline" className="bg-orange-100 text-orange-800">Prescription</Badge>;
      case 'prescription_updated':
        return <Badge variant="outline" className="bg-red-100 text-red-800">Review</Badge>;
      default:
        return <Badge variant="outline">System</Badge>;
    }
  };

  const formatTime = (timestamp: Date) => {
    return timestamp.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  };

  return (
    <Card>
      <CardHeader className="pb-3">
        <CardTitle className="text-lg flex items-center space-x-2">
          <Activity className="h-5 w-5 text-primary" />
          <span>Live Activity</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <ScrollArea className="h-64">
          {activities.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <Activity className="h-12 w-12 mx-auto mb-2 opacity-50" />
              <p>No recent activity</p>
              <p className="text-sm">Real-time updates will appear here</p>
            </div>
          ) : (
            <div className="space-y-3">
              {activities.map((activity) => (
                <div key={activity.id} className="flex items-start space-x-3 p-2 rounded-lg hover:bg-muted/50 transition-colors">
                  <div className="flex-shrink-0 mt-1">
                    {getActivityIcon(activity.type)}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-1">
                      {getActivityBadge(activity.type)}
                      <span className="text-xs text-muted-foreground">
                        {formatTime(activity.timestamp)}
                      </span>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      {activity.message}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </ScrollArea>
      </CardContent>
    </Card>
  );
}