using AutoMapper;
using CodeBase.API.Controller.Dtos;
using CodeBase.EFCore.Data.Model;

namespace CodeBase.API.Mapper;

public class CommonMapper : Profile
{
    public CommonMapper()
    {
        // create mapper            
        CreateMap<Milestone, MilestoneDto>();
        CreateMap<Quest, QuestDto>();
    }
}