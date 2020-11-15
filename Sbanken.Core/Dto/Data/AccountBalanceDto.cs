using Hub.Storage.Core.Dto;

namespace Sbanken.Core.Dto.Data
{
    public class AccountBalanceDto : EntityDtoBase
    {
        public long AccountId { get; set; }
        public decimal Balance { get; set; }
    }
}