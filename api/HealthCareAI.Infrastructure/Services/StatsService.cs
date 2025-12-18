using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Repositories;

namespace HealthCareAI.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IConsultationRepository _consultationRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;

    public StatsService(
        IPatientRepository patientRepository,
        IConsultationRepository consultationRepository,
        IPrescriptionRepository prescriptionRepository)
    {
        _patientRepository = patientRepository;
        _consultationRepository = consultationRepository;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<StatsDto> GetStatsAsync()
    {
        var totalPatients = await _patientRepository.GetCountAsync();
        var todayConsultations = await _consultationRepository.GetTodayCountAsync();
        var pendingPrescriptions = await _prescriptionRepository.GetPendingCountAsync();
        var recordedConsultations = await _consultationRepository.GetCountAsync();

        return new StatsDto
        {
            TotalPatients = totalPatients,
            TodayConsultations = todayConsultations,
            PendingPrescriptions = pendingPrescriptions,
            RecordedConsultations = recordedConsultations
        };
    }
} 