using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Interfaces;

public interface IStatsService
{
    Task<StatsDto> GetStatsAsync();
} 