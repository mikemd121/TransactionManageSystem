

namespace AccountManagementSystem
{
  public  class Transaction
    {   
        public string? TransactionId { get; set; }
        public DateTime Date { get; set; }
        public char Type { get; set; }
        public decimal Amount { get; set; }
        public string? Account { get; set; }

    }
}
