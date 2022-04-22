using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class PreferenceMapperProfile : Profile
{
    public PreferenceMapperProfile()
    {
        CreateMap<Preference, PreferenceDto>()
            .ReverseMap();
    }
}