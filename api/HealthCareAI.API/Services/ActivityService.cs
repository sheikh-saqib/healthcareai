using HealthCareAI.Application.Interfaces;
using HealthCareAI.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HealthCareAI.API.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(IHubContext<ActivityHub> hubContext, ILogger<ActivityService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastMessageAsync(object message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                _logger.LogDebug("Broadcasted message to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting message to all clients");
            }
        }

        public async Task BroadcastToGroupAsync(string groupName, object message)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
                _logger.LogDebug($"Broadcasted message to group: {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error broadcasting message to group: {groupName}");
            }
        }

        public async Task BroadcastConsultationAnalyzedAsync(object data)
        {
            var message = new
            {
                type = "consultation_analyzed",
                data = data,
                timestamp = DateTime.UtcNow
            };

            await BroadcastMessageAsync(message);
        }

        public async Task BroadcastPrescriptionCreatedAsync(object data)
        {
            var message = new
            {
                type = "prescription_created",
                data = data,
                timestamp = DateTime.UtcNow
            };

            await BroadcastMessageAsync(message);
        }

        public async Task BroadcastPrescriptionUpdatedAsync(object data)
        {
            var message = new
            {
                type = "prescription_updated",
                data = data,
                timestamp = DateTime.UtcNow
            };

            await BroadcastMessageAsync(message);
        }

        public async Task BroadcastPatientCreatedAsync(object data)
        {
            var message = new
            {
                type = "patient_created",
                data = data,
                timestamp = DateTime.UtcNow
            };

            await BroadcastMessageAsync(message);
        }

        public async Task BroadcastPatientUpdatedAsync(object data)
        {
            var message = new
            {
                type = "patient_updated",
                data = data,
                timestamp = DateTime.UtcNow
            };

            await BroadcastMessageAsync(message);
        }
    }
} 