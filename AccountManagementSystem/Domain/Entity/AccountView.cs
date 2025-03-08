

namespace AccountManagementSystem
{
  public  class AccountView
    {
        public string? TransactionId { get; set; }
        public DateTime Date { get; set; }
        public char Type { get; set; }
        public decimal Amount { get; set; }
        public string? Account { get; set; }
        public decimal Balance { get; set; }
    }
}
