import { useEffect, useRef, useState } from 'react';
import { useToast } from './use-toast';
import * as signalR from '@microsoft/signalr';

interface WebSocketMessage {
  type: string;
  data?: any;
  message?: string;
}

export function useWebSocket() {
  const [isConnected, setIsConnected] = useState(false);
  const [lastMessage, setLastMessage] = useState<WebSocketMessage | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const { toast } = useToast();

  useEffect(() => {
    const connectSignalR = async () => {
      try {
        const connection = new signalR.HubConnectionBuilder()
          .withUrl('/activityHub')
          .withAutomaticReconnect()
          .build();

        connectionRef.current = connection;

        connection.on('ReceiveMessage', (message: WebSocketMessage) => {
          setLastMessage(message);
          
          // Handle different message types
          switch (message.type) {
            case 'connection_established':
              toast({ 
                title: "Real-time updates enabled",
                description: "You'll receive live updates for consultations and prescriptions"
              });
              break;
            case 'patient_created':
              toast({ 
                title: "New patient added",
                description: `${message.data?.name} has been added to the system`
              });
              break;
            case 'consultation_created':
              toast({ 
                title: "New consultation started",
                description: "A new consultation has been recorded"
              });
              break;
            case 'consultation_analyzed':
              toast({ 
                title: "AI analysis complete",
                description: "Consultation has been analyzed and processed"
              });
              break;
            case 'prescription_created':
              toast({ 
                title: "New prescription generated",
                description: "AI has generated a prescription for review"
              });
              break;
            case 'prescription_updated':
              const status = message.data?.status;
              toast({ 
                title: `Prescription ${status}`,
                description: `A prescription has been ${status}`,
                variant: status === 'approved' ? 'default' : status === 'rejected' ? 'destructive' : 'default'
              });
              break;
          }
        });

        connection.onreconnecting(() => {
          console.log('Reconnecting to SignalR hub...');
          setIsConnected(false);
        });

        connection.onreconnected(() => {
          console.log('Reconnected to SignalR hub');
          setIsConnected(true);
        });

        connection.onclose(() => {
          console.log('Disconnected from SignalR hub');
          setIsConnected(false);
        });

        await connection.start();
        setIsConnected(true);
        console.log('Connected to SignalR hub');

      } catch (error) {
        console.error('Error connecting to SignalR hub:', error);
        setIsConnected(false);
        
        // Attempt to reconnect after 3 seconds
        setTimeout(connectSignalR, 3000);
      }
    };

    connectSignalR();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, [toast]);

  const sendMessage = async (message: WebSocketMessage) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke('SendMessage', message);
    }
  };

  return {
    isConnected,
    lastMessage,
    sendMessage
  };
}