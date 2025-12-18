using AutoMapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Application.Mapping;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        // Patient Entity to PatientDto
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => (src.FirstName + " " + src.LastName).Trim()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PrimaryPhone))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth != DateTime.MinValue ? DateTime.Now.Year - src.DateOfBirth.Year : 0))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth != DateTime.MinValue ? (DateTime?)src.DateOfBirth : null));

        // Patient Entity to PatientListDto
        CreateMap<Patient, PatientListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => (src.FirstName + " " + src.LastName).Trim()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PrimaryPhone))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth != DateTime.MinValue ? DateTime.Now.Year - src.DateOfBirth.Year : 0));

        // CreatePatientDto to Patient Entity
        CreateMap<CreatePatientDto, Patient>()
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => "P" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper())) // Generate P + 6-char ID
            .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => DateTime.Now.AddYears(-src.Age))) // Approximate DOB from age
            .AfterMap((src, dest) =>
            {
                // Split Name into FirstName and LastName
                var nameParts = src.Name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                dest.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                dest.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                dest.CreatedAt = DateTime.UtcNow;
                dest.IsActive = true;
                dest.Status = "Active";
            });

        // UpdatePatientDto to Patient Entity  
        CreateMap<UpdatePatientDto, Patient>()
            .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => DateTime.Now.AddYears(-src.Age))) // Approximate DOB from age
            .AfterMap((src, dest) =>
            {
                // Split Name into FirstName and LastName
                var nameParts = src.Name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                dest.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                dest.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                dest.UpdatedAt = DateTime.UtcNow;
            })
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
} 