using AutoMapper;
using FinanceManager.Account.Domain;
using FinanceManager.Account.Models;
using FinanceManager.Account.Repositories;

namespace FinanceManager.Account.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CurrencyQueryParameters, CurrencySpecification>();
        CreateMap<CategoryQueryParameters, CategorySpecification>();
        CreateMap<AccountQueryParameters, AccountSpecification>();
        CreateMap<Currency, CurrencyResponse>();
        CreateMap<Category, CategoryResponse>();
    }
}