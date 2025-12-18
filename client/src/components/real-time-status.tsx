import { useWebSocket } from "@/hooks/use-websocket";
import { Badge } from "@/components/ui/badge";
import { Wifi, WifiOff } from "lucide-react";

export function RealTimeStatus() {
  const { isConnected } = useWebSocket();

  return (
    <div className="flex items-center space-x-2">
      {isConnected ? (
        <>
          <Wifi className="h-4 w-4 text-green-500" />
          <Badge variant="outline" className="bg-green-100 text-green-800">
            Live
          </Badge>
        </>
      ) : (
        <>
          <WifiOff className="h-4 w-4 text-red-500" />
          <Badge variant="outline" className="bg-red-100 text-red-800">
            Offline
          </Badge>
        </>
      )}
    </div>
  );
}