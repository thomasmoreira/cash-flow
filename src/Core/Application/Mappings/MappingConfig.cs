using CashFlow.Application.Dtos;
using CashFlow.Domain.Entities;
using Mapster;

namespace CashFlow.Application.Mappings
{
    public class MappingConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Transaction, TransactionDto>
                .NewConfig()
                .Map(dest => dest.Type, src => src.Type.GetDisplayName());
        }
    }
}
