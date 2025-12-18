using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Application.Mapping;

public class ConsultationMappingProfile : Profile
{
    public ConsultationMappingProfile()
    {
        CreateMap<Consultation, ConsultationDto>();
        CreateMap<CreateConsultationDto, Consultation>();
        CreateMap<UpdateConsultationDto, Consultation>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        CreateMap<Consultation, ConsultationListDto>()
            .ForMember(dest => dest.PatientName, 
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty));
            
        CreateMap<Patient, PatientDto>();
    }
} 