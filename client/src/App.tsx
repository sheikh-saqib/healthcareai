import { Switch, Route, useLocation } from "wouter";
import { queryClient } from "./lib/queryClient";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import Dashboard from "@/pages/dashboard";
import Login from "@/pages/login";
import NotFound from "@/pages/not-found";
import { useEffect, useState } from "react";

// Protected route component
function ProtectedRoute({ component: Component }: { component: () => JSX.Element }) {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);
  const [, setLocation] = useLocation();

  useEffect(() => {
    // Check if user is authenticated by checking for token
    const checkAuth = async () => {
      const accessToken = localStorage.getItem("accessToken");
      
      // If no token, redirect to login
      if (!accessToken) {
        setIsAuthenticated(false);
        setLocation("/login");
        return;
      }

      // If token exists, allow access
      // The API will validate the token on actual requests
      // If token is invalid, individual API calls will fail and can redirect
      setIsAuthenticated(true);
    };

    checkAuth();
  }, [setLocation]);

  // Show nothing while checking authentication
  if (isAuthenticated === null) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-muted-foreground">Loading...</div>
      </div>
    );
  }

  // If authenticated, show the component
  if (isAuthenticated) {
    return <Component />;
  }

  // Otherwise, redirect will happen via setLocation
  return null;
}

// Redirect component for root path
function RootRedirect() {
  const [, setLocation] = useLocation();
  
  useEffect(() => {
    setLocation("/login");
  }, [setLocation]);

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="text-muted-foreground">Redirecting...</div>
    </div>
  );
}

function Router() {
  return (
    <Switch>
      <Route path="/login" component={Login} />
      <Route path="/dashboard" component={() => <ProtectedRoute component={Dashboard} />} />
      <Route path="/" component={RootRedirect} />
      <Route component={NotFound} />
    </Switch>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <Router />
      </TooltipProvider>
    </QueryClientProvider>
  );
}

export default App;
