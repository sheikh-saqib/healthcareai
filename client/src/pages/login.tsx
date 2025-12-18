import { useState, useEffect } from "react";
import { useLocation } from "wouter";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useToast } from "@/hooks/use-toast";
import { apiRequest } from "@/lib/queryClient";
import { queryClient } from "@/lib/queryClient";
import { Stethoscope } from "lucide-react";

export default function Login() {
  const [, setLocation] = useLocation();
  const { toast } = useToast();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);

  // Check if user is already authenticated
  useEffect(() => {
    const checkAuth = async () => {
      const accessToken = localStorage.getItem("accessToken");
      
      // If no token, show login page
      if (!accessToken) {
        setIsCheckingAuth(false);
        return;
      }

      // If token exists, assume user is authenticated and redirect
      // The ProtectedRoute will handle actual validation
      setLocation("/dashboard");
      setIsCheckingAuth(false);
    };

    checkAuth();
  }, [setLocation]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      // Make the request directly to handle 401 responses
      const response = await fetch("/api/authentication/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email, password }),
      });

      const data = await response.json();
      
      console.log("Login response:", data); // Debug log
      
      // Check if login was successful (200 OK and success flag)
      if (response.ok && data.success && data.data) {
        // Store access token if provided
        if (data.data.accessToken) {
          localStorage.setItem("accessToken", data.data.accessToken);
          console.log("Access token stored"); // Debug log
        }
        
        // Store refresh token if provided
        if (data.data.refreshToken) {
          localStorage.setItem("refreshToken", data.data.refreshToken);
          console.log("Refresh token stored"); // Debug log
        }

        toast({
          title: "Login successful",
          description: "Welcome back!",
        });

        // Invalidate any cached queries to force fresh data
        queryClient.invalidateQueries();

        // Redirect to dashboard (home page)
        console.log("Redirecting to dashboard..."); // Debug log
        setLocation("/dashboard");
      } else {
        // Login failed - show error message
        console.error("Login failed:", data.message); // Debug log
        toast({
          title: "Login failed",
          description: data.message || "Invalid email or password",
          variant: "destructive",
        });
        setIsLoading(false);
      }
    } catch (error: any) {
      console.error("Login error:", error); // Debug log
      toast({
        title: "Login failed",
        description: error.message || "An error occurred during login",
        variant: "destructive",
      });
      setIsLoading(false);
    }
  };

  // Show loading while checking authentication
  if (isCheckingAuth) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100">
        <div className="text-muted-foreground">Loading...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="flex justify-center mb-4">
            <div className="rounded-full bg-primary/10 p-3">
              <Stethoscope className="h-8 w-8 text-primary" />
            </div>
          </div>
          <CardTitle className="text-2xl font-bold">HealthCare AI</CardTitle>
          <CardDescription>
            Sign in to your account to continue
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="doctor@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={isLoading}
                autoComplete="email"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="Enter your password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                disabled={isLoading}
                autoComplete="current-password"
              />
            </div>
            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
            >
              {isLoading ? "Signing in..." : "Sign in"}
            </Button>
          </form>
          <div className="mt-4 text-center text-sm text-muted-foreground">
            <p>
              Don't have an account?{" "}
              <button
                onClick={() => {
                  toast({
                    title: "Registration",
                    description: "Please contact your administrator to create an account.",
                  });
                }}
                className="text-primary hover:underline"
              >
                Contact Admin
              </button>
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
