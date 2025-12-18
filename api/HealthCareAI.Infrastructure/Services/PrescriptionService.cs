using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;

namespace HealthCareAI.Infrastructure.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository, IMapper mapper, IActivityService activityService)
    {
        _prescriptionRepository = prescriptionRepository;
        _mapper = mapper;
        _activityService = activityService;
    }

    public async Task<IEnumerable<PrescriptionDto>> GetAllPrescriptionsAsync(string? patientId = null, string? status = null)
    {
        IEnumerable<Prescription> prescriptions;

        if (!string.IsNullOrWhiteSpace(patientId) && !string.IsNullOrWhiteSpace(status))
        {
            prescriptions = await _prescriptionRepository.GetByPatientIdAndStatusAsync(patientId, status);
        }
        else if (!string.IsNullOrWhiteSpace(patientId))
        {
            prescriptions = await _prescriptionRepository.GetByPatientIdAsync(patientId);
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            prescriptions = await _prescriptionRepository.GetByStatusAsync(status);
        }
        else
        {
            prescriptions = await _prescriptionRepository.GetAllAsync();
        }

        return _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
    }

    public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(string id)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(id);
        return prescription == null ? null : _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<PrescriptionDto> CreatePrescriptionAsync(PrescriptionDto prescriptionDto)
    {
        var prescription = _mapper.Map<Prescription>(prescriptionDto);
        prescription.PrescriptionId = Guid.NewGuid().ToString("N");
        prescription.CreatedAt = DateTime.UtcNow;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _prescriptionRepository.AddAsync(prescription);
        await _prescriptionRepository.SaveChangesAsync();

        return _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<PrescriptionDto> UpdatePrescriptionAsync(string id, PrescriptionDto updateDto)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(id);
        if (prescription == null)
        {
            throw new KeyNotFoundException($"Prescription with ID {id} not found.");
        }

        _mapper.Map(updateDto, prescription);
        await _prescriptionRepository.UpdateAsync(prescription);
        await _prescriptionRepository.SaveChangesAsync();

        var result = _mapper.Map<PrescriptionDto>(prescription);
        
        // Broadcast real-time update
        await _activityService.BroadcastPrescriptionUpdatedAsync(result);
        
        return result;
    }

    public async Task DeletePrescriptionAsync(string id)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(id);
        if (prescription == null)
        {
            throw new KeyNotFoundException($"Prescription with ID {id} not found.");
        }

        await _prescriptionRepository.DeleteAsync(prescription);
        await _prescriptionRepository.SaveChangesAsync();
    }
} 