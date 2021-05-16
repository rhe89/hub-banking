﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Data;

namespace Sbanken.Core.Providers
{
    public interface IAccountProvider
    {
        Task<IList<AccountDto>> GetAccounts(string accountName, string accountType);
    }
}