using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Application.Mapping;

public class PrescriptionMappingProfile : Profile
{
    public PrescriptionMappingProfile()
    {
        CreateMap<Prescription, PrescriptionDto>();
        CreateMap<CreatePrescriptionDto, Prescription>();
        CreateMap<UpdatePrescriptionDto, Prescription>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        CreateMap<Prescription, PrescriptionListDto>()
            .ForMember(dest => dest.PatientName, 
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty));
    }
} 