using System.Collections.Generic;

namespace Sbanken.Core.Dto.Integration
{
    public class SbankenAccountResponse
    {
        public List<SbankenAccount> Items { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
