using System.Collections.Generic;

namespace Sbanken.Core.Dto.Integration
{
    public class SbankenTransactionResponse
    {
        public List<SbankenTransaction> Items { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
