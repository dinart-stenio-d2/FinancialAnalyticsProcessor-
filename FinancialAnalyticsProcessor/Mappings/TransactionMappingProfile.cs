using AutoMapper;
using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Mappings
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, Infrastructure.DbEntities.Transaction>().ReverseMap();
            CreateMap<IEnumerable<Transaction>, IEnumerable<Infrastructure.DbEntities.Transaction>>().ReverseMap();
        }
    }
}
