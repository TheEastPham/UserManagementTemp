using AutoMapper;
using Base.UserManagement.Domain.DTOs.User;
using Base.UserManagement.Domain.DTOs.Role;
using Base.UserManagement.Domain.DTOs.Security;
using Base.UserManagement.Domain.DTOs.Account;
using Base.UserManagement.Domain.Models;
using Base.UserManagement.EFCore.Entities.User;
using Base.UserManagement.EFCore.Entities.Security;
using System.Text.Json;

namespace Base.UserManagement.Domain.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Entity to Domain
        CreateMap<UserEntity, User>()
            .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile));

        CreateMap<UserProfileEntity, UserProfile>()
            .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src => 
                ParsePreferences(src.Preferences)));

        CreateMap<SecurityEventEntity, SecurityEvent>()
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => 
                ParseDetails(src.Details)));

        // Domain to Entity
        CreateMap<User, UserEntity>()
            .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile));

        CreateMap<UserProfile, UserProfileEntity>()
            .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src => 
                SerializePreferences(src.Preferences)));

        CreateMap<SecurityEvent, SecurityEventEntity>()
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => 
                SerializeDetails(src.Details)));

        // Domain to DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<SecurityEvent, SecurityEventDto>();

        // DTO to Domain
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }

    private static Dictionary<string, object> ParsePreferences(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, object>();
        
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static Dictionary<string, object> ParseDetails(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, object>();
        
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static string SerializePreferences(Dictionary<string, object> preferences)
    {
        return JsonSerializer.Serialize(preferences);
    }

    private static string SerializeDetails(Dictionary<string, object> details)
    {
        return JsonSerializer.Serialize(details);
    }
}
