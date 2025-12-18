namespace HealthCareAI.Application.Interfaces
{
    public interface IActivityService
    {
        Task BroadcastMessageAsync(object message);
        Task BroadcastToGroupAsync(string groupName, object message);
        Task BroadcastConsultationAnalyzedAsync(object data);
        Task BroadcastPrescriptionCreatedAsync(object data);
        Task BroadcastPrescriptionUpdatedAsync(object data);
        Task BroadcastPatientCreatedAsync(object data);
        Task BroadcastPatientUpdatedAsync(object data);
    }
} 