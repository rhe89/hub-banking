using System.Collections.Generic;

namespace Sbanken.Dto.Sbanken
{
    public class TransactionResponseDto
    {
        public List<TransactionDto> Items { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
