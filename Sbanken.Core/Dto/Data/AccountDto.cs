using System.Collections.Generic;
using Hub.Storage.Repository.Dto;

namespace Sbanken.Core.Dto.Data
{
    public class AccountDto : EntityDtoBase
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string AccountType { get; set; }
    }
}