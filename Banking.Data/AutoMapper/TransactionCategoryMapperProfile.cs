using System.Linq;
using AutoMapper;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Data.AutoMapper;

public class TransactionCategoryMapperProfile : Profile
{
    public TransactionCategoryMapperProfile()
    {
        CreateMap<TransactionCategory, TransactionCategoryDto>()
            .ReverseMap()
            .ForMember(x => x.Name, opt => opt.MapFrom(y => y.Name.ToLower()))
            .ForMember(dest => dest.TransactionSubCategories, opt => opt.Ignore());

        CreateMap<TransactionSubCategory, TransactionSubCategoryDto>()
            .ForMember(dest => dest.KeywordList, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Keywords,
                opt => opt.MapFrom(x => string.Join(",", x.KeywordList.Select(keyword => keyword.Value))))
            .ForMember(dest => dest.TransactionCategory, opt => opt.Ignore());
    }
}