using System.Collections.Generic;

namespace Sbanken.Dto.Sbanken
{
    public class AccountResponseDto
    {
        public List<AccountDto> Items { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
