using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Data;

namespace Sbanken.Core.Providers
{
    public interface IAccountProvider
    {
        Task<IEnumerable<AccountDto>> GetAccounts();
        Task<IEnumerable<AccountDto>> GetStandardAccounts();
        Task<IEnumerable<AccountDto>> GetCreditAccounts();
        Task<IEnumerable<AccountDto>> GetSavingsAccounts();

    }
}