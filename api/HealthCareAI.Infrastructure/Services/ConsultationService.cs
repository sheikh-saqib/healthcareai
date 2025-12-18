using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Exceptions;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;

namespace HealthCareAI.Infrastructure.Services;

public class ConsultationService : IConsultationService
{
    private readonly IConsultationRepository _consultationRepository;
    private readonly IMapper _mapper;

    public ConsultationService(IConsultationRepository consultationRepository, IMapper mapper)
    {
        _consultationRepository = consultationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ConsultationDto>> GetAllConsultationsAsync(string? patientId = null)
    {
        var consultations = !string.IsNullOrWhiteSpace(patientId)
            ? await _consultationRepository.GetByPatientIdAsync(patientId)
            : await _consultationRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<ConsultationDto>>(consultations);
    }

    public async Task<ConsultationDto?> GetConsultationByIdAsync(string id)
    {
        var consultation = await _consultationRepository.GetByIdAsync(id);
        return consultation == null ? null : _mapper.Map<ConsultationDto>(consultation);
    }

    public async Task<ConsultationDto> CreateConsultationAsync(ConsultationDto createDto)
    {
        var consultation = _mapper.Map<Consultation>(createDto);
        consultation.ConsultationId = Guid.NewGuid().ToString("N");
        consultation.CreatedAt = DateTime.UtcNow;
        consultation.UpdatedAt = DateTime.UtcNow;
        
        await _consultationRepository.AddAsync(consultation);
        await _consultationRepository.SaveChangesAsync();
        
        return _mapper.Map<ConsultationDto>(consultation);
    }

    public async Task<ConsultationDto> UpdateConsultationAsync(string id, ConsultationDto updateDto)
    {
        var consultation = await _consultationRepository.GetByIdAsync(id);
        if (consultation == null)
        {
            throw new ConsultationNotFoundException(id);
        }

        _mapper.Map(updateDto, consultation);
        await _consultationRepository.SaveChangesAsync();
        
        return _mapper.Map<ConsultationDto>(consultation);
    }

    public async Task DeleteConsultationAsync(string id)
    {
        var consultation = await _consultationRepository.GetByIdAsync(id);
        if (consultation == null)
        {
            throw new ConsultationNotFoundException(id);
        }

        await _consultationRepository.DeleteAsync(consultation);
        await _consultationRepository.SaveChangesAsync();
    }
} 