import { useState, useRef, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Mic, Square, Pause, Play, Brain, Users } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { apiRequest, queryClient } from "@/lib/queryClient";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface EnhancedConsultationRecorderProps {
  onClose: () => void;
  selectedPatientId?: string;
}

export default function EnhancedConsultationRecorder({ onClose, selectedPatientId }: EnhancedConsultationRecorderProps) {
  const [isRecording, setIsRecording] = useState(false);
  const [isPaused, setIsPaused] = useState(false);
  const [duration, setDuration] = useState(0);
  const [transcription, setTranscription] = useState("");
  const [analysis, setAnalysis] = useState<any>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [doctorName, setDoctorName] = useState("Dr. Sarah Johnson");
  const [patientName, setPatientName] = useState("");
  const [speakerChanges, setSpeakerChanges] = useState<Array<{time: number, speaker: string, text: string}>>([]);
  
  // Debug logging
  console.log("EnhancedConsultationRecorder - selectedPatientId:", selectedPatientId);
  
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const durationIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const { toast } = useToast();

  useEffect(() => {
    return () => {
      if (durationIntervalRef.current) {
        clearInterval(durationIntervalRef.current);
      }
    };
  }, []);

  const startRecording = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const mediaRecorder = new MediaRecorder(stream);
      
      mediaRecorderRef.current = mediaRecorder;
      audioChunksRef.current = [];
      
      mediaRecorder.ondataavailable = (event) => {
        audioChunksRef.current.push(event.data);
      };
      
      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/wav' });
        await processAudio(audioBlob);
      };
      
      mediaRecorder.start();
      setIsRecording(true);
      setDuration(0);
      
      durationIntervalRef.current = setInterval(() => {
        setDuration(prev => prev + 1);
      }, 1000);
      
    } catch (error) {
      toast({ title: "Error accessing microphone", variant: "destructive" });
    }
  };

  const stopRecording = () => {
    if (mediaRecorderRef.current && isRecording) {
      mediaRecorderRef.current.stop();
      setIsRecording(false);
      setIsPaused(false);
      
      if (durationIntervalRef.current) {
        clearInterval(durationIntervalRef.current);
      }
      
      // Stop all audio tracks
      mediaRecorderRef.current.stream.getTracks().forEach(track => track.stop());
    }
  };

  const pauseRecording = () => {
    if (mediaRecorderRef.current && isRecording) {
      if (isPaused) {
        mediaRecorderRef.current.resume();
        durationIntervalRef.current = setInterval(() => {
          setDuration(prev => prev + 1);
        }, 1000);
      } else {
        mediaRecorderRef.current.pause();
        if (durationIntervalRef.current) {
          clearInterval(durationIntervalRef.current);
        }
      }
      setIsPaused(!isPaused);
    }
  };

  const processAudio = async (audioBlob: Blob) => {
    try {
      const formData = new FormData();
      formData.append('audio', audioBlob, 'recording.wav');
      
      // Transcribe audio
      const response = await fetch('/api/transcribe', {
        method: 'POST',
        body: formData,
      });
      
      if (!response.ok) {
        throw new Error('Transcription failed');
      }
      
      const { text } = await response.json();
      setTranscription(text);
      
      // Enhanced analysis with speaker context
      await analyzeConsultationWithSpeakers(text);
      
    } catch (error) {
      toast({ title: "Error processing audio", variant: "destructive" });
    }
  };

  const analyzeConsultationWithSpeakers = async (transcriptionText: string) => {
    let analysisResult: any = null;
    try {
      setIsAnalyzing(true);
      
      // Enhanced prompt with speaker context
      const enhancedPrompt = `
        You are analyzing a medical consultation between ${doctorName} (doctor) and ${patientName || "a patient"} (patient).
        
        Conversation transcript: ${transcriptionText}
        
        Please provide detailed analysis including speaker identification based on:
        1. Medical terminology usage (typically doctor)
        2. Symptom descriptions (typically patient)
        3. Questions vs answers pattern
        4. Professional language vs personal experience
        
        Return a JSON response with enhanced speaker analysis.
      `;
      
      console.log("About to send analysis request with patientId:", selectedPatientId);
      
      const response = await apiRequest("POST", "/api/analyze-consultation", {
        transcription: enhancedPrompt,
        patientId: selectedPatientId, // Use selected patient
        doctorName,
        patientName,
      });
      
      analysisResult = await response.json();
      console.log("Analysis result:", analysisResult);
      
      // The API returns analysis wrapped in an 'analysis' property
      const analysisData = analysisResult.analysis || analysisResult;
      setAnalysis(analysisData);
      
      // Show success message if data was saved to database
      if (analysisResult.consultation || analysisResult.prescription) {
        toast({ 
          title: "Consultation saved successfully!", 
          description: analysisResult.prescription ? "Prescription created and sent for approval" : "Consultation recorded"
        });
        
        // Invalidate relevant queries to refresh the UI
        queryClient.invalidateQueries({ queryKey: ["/api/prescriptions"] });
        queryClient.invalidateQueries({ queryKey: ["/api/prescriptions", { status: "pending" }] });
        queryClient.invalidateQueries({ queryKey: ["/api/stats"] });
      }
      
    } catch (error) {
      toast({ title: "Error analyzing consultation", variant: "destructive" });
    } finally {
      setIsAnalyzing(false);
    }
  };

  const formatDuration = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const WaveformVisualization = () => (
    <div className="flex items-center justify-center space-x-1 h-16">
      {Array.from({ length: 20 }, (_, i) => (
        <div
          key={i}
          className={`w-1 bg-primary rounded-full ${isRecording && !isPaused ? 'animate-pulse' : ''}`}
          style={{
            height: `${Math.random() * 40 + 10}px`,
            animationDelay: `${i * 0.1}s`
          }}
        />
      ))}
    </div>
  );

  return (
    <Card>
      <CardContent className="p-6">
        <h2 className="text-xl font-semibold text-primary mb-6 flex items-center">
          <Users className="mr-2 h-5 w-5" />
          Enhanced AI Consultation Recording
        </h2>
        
        {/* Speaker Setup */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div>
            <Label htmlFor="doctorName">Doctor Name</Label>
            <Input
              id="doctorName"
              value={doctorName}
              onChange={(e) => setDoctorName(e.target.value)}
              placeholder="Dr. Sarah Johnson"
            />
          </div>
          <div>
            <Label htmlFor="patientName">Patient Name (optional)</Label>
            <Input
              id="patientName"
              value={patientName}
              onChange={(e) => setPatientName(e.target.value)}
              placeholder="Patient name..."
            />
          </div>
        </div>
        
        {/* Recording Controls */}
        <div className="bg-muted rounded-lg p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center space-x-3">
              <div className={`w-4 h-4 rounded-full ${isRecording ? 'bg-red-500 animate-pulse' : 'bg-gray-400'}`} />
              <span className="text-sm font-medium">
                {isRecording ? (isPaused ? 'Recording Paused' : 'Recording Active') : 'Ready to Record'}
              </span>
            </div>
            <div className="text-sm text-muted-foreground">
              {formatDuration(duration)}
            </div>
          </div>
          
          <div className="flex items-center justify-center space-x-4 mb-4">
            {!isRecording ? (
              <Button 
                onClick={startRecording} 
                className="bg-red-500 hover:bg-red-600 text-white w-16 h-16 rounded-full"
              >
                <Mic className="h-6 w-6" />
              </Button>
            ) : (
              <>
                <Button 
                  onClick={stopRecording} 
                  className="bg-red-500 hover:bg-red-600 text-white w-16 h-16 rounded-full"
                >
                  <Square className="h-6 w-6" />
                </Button>
                <Button 
                  onClick={pauseRecording} 
                  variant="outline" 
                  className="w-12 h-12 rounded-full"
                >
                  {isPaused ? <Play className="h-5 w-5" /> : <Pause className="h-5 w-5" />}
                </Button>
              </>
            )}
          </div>

          {/* Audio Waveform Visualization */}
          <div className="bg-white rounded-lg p-4">
            <WaveformVisualization />
          </div>
        </div>

        {/* Real-time Transcription */}
        {transcription && (
          <div className="mb-6">
            <h3 className="text-lg font-medium text-primary mb-3">Transcription</h3>
            <div className="bg-muted rounded-lg p-4 h-32 overflow-y-auto">
              <div className="text-sm text-muted-foreground whitespace-pre-wrap">
                {transcription}
              </div>
            </div>
          </div>
        )}

        {/* Enhanced AI Analysis */}
        {(analysis || isAnalyzing) && (
          <div className="bg-blue-50 rounded-lg p-4">
            <h3 className="text-lg font-medium text-primary mb-3 flex items-center">
              <Brain className="text-primary mr-2 h-5 w-5" />
              Enhanced AI Analysis
            </h3>
            
            {isAnalyzing ? (
              <div className="flex items-center justify-center py-4">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                <span className="ml-2 text-sm text-muted-foreground">Analyzing conversation with speaker identification...</span>
              </div>
            ) : analysis ? (
              <div className="space-y-4">
                {/* Speaker Analysis */}
                {analysis.conversationAnalysis && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                    <div className="bg-white rounded-lg p-3">
                      <p className="font-medium text-blue-600 mb-2">👨‍⚕️ {doctorName} Statements:</p>
                      <ul className="text-muted-foreground space-y-1">
                        {analysis.conversationAnalysis.doctorStatements?.map((statement: string, index: number) => (
                          <li key={index} className="text-xs">• {statement}</li>
                        ))}
                      </ul>
                    </div>
                    <div className="bg-white rounded-lg p-3">
                      <p className="font-medium text-green-600 mb-2">👤 {patientName || "Patient"} Statements:</p>
                      <ul className="text-muted-foreground space-y-1">
                        {analysis.conversationAnalysis.patientStatements?.map((statement: string, index: number) => (
                          <li key={index} className="text-xs">• {statement}</li>
                        ))}
                      </ul>
                    </div>
                  </div>
                )}
                
                {/* Medical Analysis */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="font-medium text-primary mb-1">Symptoms Detected:</p>
                    <ul className="text-muted-foreground space-y-1">
                      {analysis.symptoms?.map((symptom: string, index: number) => (
                        <li key={index}>• {symptom}</li>
                      ))}
                    </ul>
                  </div>
                  <div>
                    <p className="font-medium text-primary mb-1">Suggested Actions:</p>
                    <ul className="text-muted-foreground space-y-1">
                      {analysis.suggestedActions?.map((action: string, index: number) => (
                        <li key={index}>• {action}</li>
                      ))}
                    </ul>
                  </div>
                  {analysis.prescriptionNeeded && (
                    <div className="md:col-span-2">
                      <p className="font-medium text-primary mb-1">Prescription Status:</p>
                      <Badge variant="outline" className="bg-orange-100 text-orange-800">
                        {analysis.prescriptionCreated ? "Prescription Sent for Approval" : "Prescription Required"}
                      </Badge>
                    </div>
                  )}
                  
                  {analysis.medications && analysis.medications.length > 0 && (
                    <div className="md:col-span-2">
                      <p className="font-medium text-primary mb-1">Recommended Medications:</p>
                      <div className="space-y-2">
                        {analysis.medications.map((med: any, index: number) => (
                          <div key={index} className="text-sm bg-blue-50 p-2 rounded">
                            <p className="font-medium">{med.name}</p>
                            <p className="text-muted-foreground">{med.dosage}</p>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            ) : null}
          </div>
        )}
        
        {onClose && (
          <div className="flex justify-end mt-6">
            <Button variant="outline" onClick={onClose}>
              Close
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}