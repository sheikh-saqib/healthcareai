using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;

namespace HealthCareAI.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;
    private readonly IActivityService _activityService;

    public PatientService(IPatientRepository patientRepository, IMapper mapper, IActivityService activityService)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
        _activityService = activityService;
    }

    public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync(string? searchTerm = null)
    {
        var patients = string.IsNullOrWhiteSpace(searchTerm)
            ? await _patientRepository.GetAllAsync()
            : await _patientRepository.SearchAsync(searchTerm);

        return _mapper.Map<IEnumerable<PatientDto>>(patients);
    }

    public async Task<PatientDto?> GetPatientByIdAsync(string id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        return patient == null ? null : _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto> CreatePatientAsync(PatientDto patientDto)
    {
        var patient = _mapper.Map<Patient>(patientDto);
        patient.PatientId = Guid.NewGuid().ToString("N");
        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;

        await _patientRepository.AddAsync(patient);
        await _patientRepository.SaveChangesAsync();

        var result = _mapper.Map<PatientDto>(patient);
        
        // Broadcast real-time update
        await _activityService.BroadcastPatientCreatedAsync(result);
        
        return result;
    }

    public async Task<PatientDto> UpdatePatientAsync(string id, PatientDto updateDto)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
        {
            throw new KeyNotFoundException($"Patient with ID {id} not found.");
        }

        _mapper.Map(updateDto, patient);
        await _patientRepository.UpdateAsync(patient);
        await _patientRepository.SaveChangesAsync();

        var result = _mapper.Map<PatientDto>(patient);
        
        // Broadcast real-time update
        await _activityService.BroadcastPatientUpdatedAsync(result);
        
        return result;
    }

    public async Task DeletePatientAsync(string id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null)
        {
            throw new KeyNotFoundException($"Patient with ID {id} not found.");
        }

        await _patientRepository.DeleteAsync(patient);
        await _patientRepository.SaveChangesAsync();
    }
} 