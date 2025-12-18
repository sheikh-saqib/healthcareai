import { useState, useEffect } from "react";
import { useLocation } from "wouter";
import { useToast } from "@/hooks/use-toast";
import { queryClient } from "@/lib/queryClient";
import { useForm } from "react-hook-form";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { 
  faStethoscope,
  faBrain,
  faFileLines,
  faUsers,
  faShield,
  faBolt,
  faChartLine,
  faClipboardList
} from "@fortawesome/free-solid-svg-icons";


interface LoginFormData {
  email: string;
  password: string;
}

export default function Login() {
  const [, setLocation] = useLocation();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);
  
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>();

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

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);

    try {
      // Make the request directly to handle 401 responses
      const response = await fetch("/api/authentication/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email: data.email, password: data.password }),
      });

      const responseData = await response.json();
      
      console.log("Login response:", responseData); // Debug log
      
      // Check if login was successful (200 OK and success flag)
      if (response.ok && responseData.success && responseData.data) {
        // Store access token if provided
        if (responseData.data.accessToken) {
          localStorage.setItem("accessToken", responseData.data.accessToken);
          console.log("Access token stored"); // Debug log
        }
        
        // Store refresh token if provided
        if (responseData.data.refreshToken) {
          localStorage.setItem("refreshToken", responseData.data.refreshToken);
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
        console.error("Login failed:", responseData.message); // Debug log
        toast({
          title: "Login failed",
          description: responseData.message || "Invalid email or password",
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
      <div className="min-h-screen d-flex align-items-center justify-content-center" style={{ backgroundColor: '#f8f9fa' }}>
        <div className="text-muted">Loading...</div>
      </div>
    );
  }

  const features = [
    {
      icon: faBrain,
      title: "AI-Powered Analysis",
      description: "Get intelligent insights from patient consultations using advanced AI technology"
    },
    {
      icon: faFileLines,
      title: "Smart Transcription",
      description: "Automatically transcribe consultations and extract key medical information"
    },
    {
      icon: faUsers,
      title: "Patient Management",
      description: "Comprehensive patient records with medical history and treatment plans"
    },
    {
      icon: faClipboardList,
      title: "Prescription Management",
      description: "Create and manage prescriptions with AI-assisted medication recommendations"
    },
    {
      icon: faChartLine,
      title: "Real-time Analytics",
      description: "Track consultation statistics and patient outcomes with detailed dashboards"
    },
    {
      icon: faShield,
      title: "Secure & Compliant",
      description: "HIPAA-compliant platform ensuring patient data security and privacy"
    }
  ];

  return (
    <div className="min-h-screen d-flex" style={{ backgroundColor: '#f8f9fa' }}>
      {/* Left Side - 70% - Portal Information */}
      <div className="col-lg-7 d-none d-lg-flex flex-column justify-content-center align-items-start p-5" 
           style={{ 
             background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
             color: 'white',
             minHeight: '100vh'
           }}>
        <div className="w-100" style={{ maxWidth: '600px', margin: '0 auto' }}>
          <div className="mb-4">
            <div className="d-flex align-items-center mb-3">
              <div className="bg-white bg-opacity-20 rounded-circle p-3 me-3 d-flex align-items-center justify-content-center" style={{ width: '56px', height: '56px' }}>
                <FontAwesomeIcon icon={faStethoscope} style={{ color: '#764ba2', fontSize: '28px', opacity: 1, display: 'block' }} />
              </div>
              <h1 className="h2 mb-0 fw-bold">HealthCare AI</h1>
            </div>
            <p className="lead mb-4" style={{ opacity: 0.9 }}>
              Revolutionizing healthcare with intelligent AI-powered consultation management
            </p>
          </div>

          <div className="mt-5">
            <h3 className="h4 mb-4 fw-semibold">Transform Your Practice</h3>
            <div className="row g-4">
              {features.map((feature, index) => (
                <div key={index} className="col-md-6">
                  <div className="d-flex align-items-start">
                    <div className="bg-white bg-opacity-20 rounded-circle p-2 me-3 flex-shrink-0 d-flex align-items-center justify-content-center" 
                         style={{ width: '40px', height: '40px' }}>
                      <FontAwesomeIcon icon={feature.icon} style={{ color: '#764ba2', fontSize: '20px', opacity: 1, display: 'block' }} />
                    </div>
                    <div>
                      <h5 className="h6 mb-2 fw-semibold">{feature.title}</h5>
                      <p className="small mb-0" style={{ opacity: 0.85 }}>
                        {feature.description}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="mt-5 pt-4 border-top border-white border-opacity-25">
            <div className="d-flex align-items-center">
              <FontAwesomeIcon icon={faBolt} style={{ color: '#ffffff', fontSize: '18px', marginRight: '8px', opacity: 1, display: 'block' }} />
              <p className="mb-0 small" style={{ opacity: 0.9 }}>
                Trusted by healthcare professionals worldwide
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Right Side - 30% - Login Form */}
      <div className="col-lg-5 d-flex align-items-center justify-content-center p-4 position-relative" 
           style={{ 
             background: 'linear-gradient(135deg, #f5f7fa 0%, #e8ecf1 100%)',
             minHeight: '100vh',
             overflow: 'hidden'
           }}>
        {/* Decorative Background Elements */}
        <div className="position-absolute top-0 end-0 d-flex align-items-center justify-content-center" style={{ width: '300px', height: '300px', opacity: 0.05 }}>
          <FontAwesomeIcon icon={faStethoscope} style={{ color: '#667eea', fontSize: '300px', transform: 'rotate(-15deg)' }} />
        </div>
        <div className="position-absolute bottom-0 start-0 d-flex align-items-center justify-content-center" style={{ width: '200px', height: '200px', opacity: 0.05 }}>
          <FontAwesomeIcon icon={faBrain} style={{ color: '#764ba2', fontSize: '200px', transform: 'rotate(15deg)' }} />
        </div>
        <div className="position-absolute d-flex align-items-center justify-content-center" style={{ top: '20%', right: '10%', width: '150px', height: '150px', opacity: 0.03 }}>
          <FontAwesomeIcon icon={faChartLine} style={{ color: '#667eea', fontSize: '150px' }} />
        </div>
        
        <div className="w-100 bg-white rounded-4 shadow-lg border position-relative z-1 p-4 p-md-5" 
             style={{ maxWidth: '450px', borderColor: '#e9ecef' }}>
          {/* Mobile Logo */}
          <div className="d-lg-none text-center mb-4">
            <div className="d-inline-flex align-items-center justify-content-center bg-primary bg-opacity-10 rounded-circle p-3 mb-3" style={{ width: '56px', height: '56px' }}>
              <FontAwesomeIcon icon={faStethoscope} className="text-primary" style={{ fontSize: '24px' }} />
            </div>
            <h2 className="h4 fw-bold mb-1">HealthCare AI</h2>
            <p className="text-muted small">Sign in to continue</p>
          </div>

          {/* Desktop Title */}
          <div className="d-none d-lg-block mb-5">
            <div className="text-center mb-4">
              <div className="d-inline-flex align-items-center justify-content-center mb-3" 
                   style={{ 
                     background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                     width: '64px',
                     height: '64px',
                     borderRadius: '16px',
                     boxShadow: '0 4px 12px rgba(102, 126, 234, 0.3)'
                   }}>
                <FontAwesomeIcon icon={faStethoscope} style={{ color: '#ffffff', fontSize: '32px', opacity: 1, display: 'block' }} />
              </div>
              <h2 className="h2 fw-bold mb-2" style={{ 
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                backgroundClip: 'text'
              }}>
                Welcome Back
              </h2>
              <p className="text-muted mb-3" style={{ fontSize: '1rem' }}>
                Sign in to access your dashboard and continue managing your practice
              </p>
            </div>
            
          </div>

          {/* Login Form */}
          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            {/* Email Field */}
            <div className="mb-3">
              <label htmlFor="email" className="form-label fw-semibold">
                Email Address
              </label>
              <input
                type="email"
                id="email"
                className={`form-control form-control-lg ${errors.email ? 'is-invalid' : ''}`}
                placeholder="doctor@example.com"
                autoComplete="email"
                disabled={isLoading}
                {...register("email", {
                  required: "Email address is required",
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: "Please enter a valid email address"
                  }
                })}
              />
              {errors.email && (
                <div className="invalid-feedback d-block">
                  {errors.email.message}
                </div>
              )}
            </div>

            {/* Password Field */}
            <div className="mb-4">
              <label htmlFor="password" className="form-label fw-semibold">
                Password
              </label>
              <input
                type="password"
                id="password"
                className={`form-control form-control-lg ${errors.password ? 'is-invalid' : ''}`}
                placeholder="Enter your password"
                autoComplete="current-password"
                disabled={isLoading}
                {...register("password", {
                  required: "Password is required",
                  minLength: {
                    value: 6,
                    message: "Password must be at least 6 characters"
                  }
                })}
              />
              {errors.password && (
                <div className="invalid-feedback d-block">
                  {errors.password.message}
                </div>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              className="btn btn-primary btn-lg w-100 mb-3"
              disabled={isLoading}
            >
              {isLoading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  Signing in...
                </>
              ) : (
                "Sign In"
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="text-center mt-4">
            <p className="text-muted small mb-0">
              Don't have an account?{" "}
              <button
                type="button"
                className="btn btn-link p-0 text-decoration-none"
                onClick={() => {
                  toast({
                    title: "Registration",
                    description: "Please contact your administrator to create an account.",
                  });
                }}
              >
                Contact Admin
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
