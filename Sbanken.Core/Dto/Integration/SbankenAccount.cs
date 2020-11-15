namespace Sbanken.Core.Dto.Integration
{
    public class SbankenAccount
    {
        public string AccountId { get; set; }
        public string Name { get; set; }
        public decimal Available { get; set; }
        public decimal Balance { get; set; }
        public string AccountType { get; set; }
    }
}
