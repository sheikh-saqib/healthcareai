using Microsoft.AspNetCore.SignalR;

namespace HealthCareAI.API.Hubs
{
    public class ActivityHub : Hub
    {
        private readonly ILogger<ActivityHub> _logger;

        public ActivityHub(ILogger<ActivityHub> logger)
        {
            _logger = logger;
        }

        public async Task SendMessage(object message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} joined group: {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} left group: {groupName}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await Clients.Caller.SendAsync("ReceiveMessage", new { 
                type = "connection_established",
                message = "Connected to real-time updates"
            });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogError(exception, $"Client disconnected with error: {Context.ConnectionId}");
            }
            else
            {
                _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
} 