using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Dto.Api;

namespace Sbanken.Providers
{
    public interface IAccountProvider
    {
        Task<IEnumerable<AccountDto>> GetAccounts();
        Task<IEnumerable<AccountDto>> GetStandardAccounts();
        Task<IEnumerable<AccountDto>> GetCreditAccounts();
        Task<IEnumerable<AccountDto>> GetSavingsAccounts();

    }
}