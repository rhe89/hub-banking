using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class CsvImportMapperProfile : Profile
{
    public CsvImportMapperProfile()
    {
        CreateMap<CsvImport, CsvImportDto>()
            .ReverseMap();
    }
}