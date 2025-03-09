using AccountManagementSystem.Application.Interfaces;
using System.Globalization;
using System.Transactions;

namespace AccountManagementSystem
{
    public class TransactionService : IHandler
    {
        public static List<Transaction> transactions = new List<Transaction>();
        private Dictionary<string, int> transactionCountByDate = new Dictionary<string, int>();

        private bool TryParseDate(string dateInput, out DateTime date)
        {
            if (!DateTime.TryParseExact(dateInput, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                ShowError("Invalid date format! Use YYYYMMDD.");
                return false;
            }
            return true;
        }

        private bool TryParseTransactionType(string typeInput, out char type)
        {
            type = char.ToUpper(typeInput[0]);
            if (type != 'D' && type != 'W')
            {
                ShowError("Invalid transaction type! Use 'D' for DEPOSIT or 'W' for WITHDRAW.");
                return false;
            }
            return true;
        }

        private bool TryParseAmount(string amountInput, out decimal amount)
        {
            if (!decimal.TryParse(amountInput, out amount) || amount <= 0)
            {
                ShowError("Invalid amount! Please enter a positive number.");
                return false;
            }
            return true;
        }

        private bool IsTransactionValid(string account, char type, decimal amount)
        {
            if (!transactions.Any() && type == 'W')
            {
                ShowError("Invalid: The first transaction on an account cannot be a withdrawal.");
                return false;
            }

            decimal totalBalance = transactions.Where(t => t.Account == account).Sum(t => t.Amount);
            if (type == 'W' && amount > totalBalance)
            {
                ShowError($"Insufficient funds! You can withdraw up to {totalBalance:F2}.");
                return false;
            }

            return true;
        }

        private string GenerateTransactionId(DateTime date)
        {
            string dateKey = date.ToString("yyyyMMdd");
            int count = transactionCountByDate.ContainsKey(dateKey) ? transactionCountByDate[dateKey] + 1 : 1;
            transactionCountByDate[dateKey] = count;
            return $"{dateKey}-{count:00}";
        }

        public void PrintStatement(string account)
        {
            var accountTransactions = transactions
                .Where(t => t.Account == account)
                .OrderBy(t => t.Date)
                .ThenBy(t => t.TransactionId)
                .ToList();

            if (!accountTransactions.Any())
            {
                Console.WriteLine($"No transactions found for account {account}.");
                return;
            }

            Console.Clear();
            Console.WriteLine($"Account: {account}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount |");
            Console.WriteLine("|----------|-------------|------|--------|");

            foreach (var txn in accountTransactions)
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type}    | {txn.Amount:F2} |");
        }

        private void ShowError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
        }

        public void Handler()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Enter transaction details in <Date> <Account> <Type> <Amount> format");
                Console.WriteLine("(or press Enter to return to the main menu):");
                Console.Write("> ");

                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4)
                {
                    ShowError("Invalid format! Please enter in <Date> <Account> <Type> <Amount> format.");
                    continue;
                }

                if (!TryParseDate(parts[0], out DateTime date)) continue;
                string account = parts[1];

                if (!TryParseTransactionType(parts[2], out char type)) continue;
                if (!TryParseAmount(parts[3], out decimal amount)) continue;
                if (!IsTransactionValid(account, type, amount)) continue;

                string transactionId = GenerateTransactionId(date);
                transactions.Add(new Transaction
                {
                    Date = date,
                    Account = account,
                    Type = type,
                    Amount = amount,
                    TransactionId = transactionId
                });

                Console.WriteLine("Transaction recorded successfully!");
                PrintStatement(account);
                Console.WriteLine("Press any key to enter another transaction or leave blank to go back.");
                Console.ReadKey();
            }
        }

        public bool AuthoriseHandler(string type)
        {
            return type == TransactionType.T.ToString();
        }

        public List<AccountView> CalculateBalance(string account, DateTime date, List<InterestRule> rules, List<Transaction> transactions)
        {
            var formattedDate = date.ToString("yyyyMM");
            List<AccountView> accountTransactions;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                   new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    accountTransactions = GetAccountTransactions(account, formattedDate, transactions);
                    var interest = CalculateInterest(accountTransactions, rules, date);
                    var endOfMonth = GetEndOfMonth(date);

                    accountTransactions.Add(new AccountView
                    {
                        Account = account,
                        Date = endOfMonth,
                        TransactionId = "           ",
                        Type = 'I',
                        Amount = interest,
                        Balance = accountTransactions.LastOrDefault()?.Balance + interest ?? interest
                    });

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Transaction failed: {ex.Message}");
                    throw;
                }
            }

            return accountTransactions;
        }

        public virtual List<AccountView> GetAccountTransactions(string account, string formattedDate, List<Transaction> transactions)
        {
            decimal totalBalance = 0;
            var result = transactions
                .OrderBy(t => t.Date)
                .Select(t => new AccountView
                {
                    Account = t.Account,
                    Date = t.Date,
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    Balance = totalBalance += (t.Type == 'D' ? t.Amount : (t.Type == 'W' ? -t.Amount : 0))
                })
                .ToList();

            return result.Where(t => t.Date.ToString("yyyyMM") == formattedDate).ToList();
        }

        private InterestRule? GetPreviousRate(List<InterestRule> rules, InterestRule rule)
        {
            int index = rules.IndexOf(rule);
            return (index > 0) ? rules[index - 1] : null;
        }

        public virtual decimal CalculateInterest(List<AccountView> transactions, List<InterestRule> rules, DateTime date)
        {
            var interestList = new List<decimal>();
            var previousRateList = new List<decimal>();
            int Counter = 0;
            foreach (var transaction in transactions)
            {
                Counter++;
                var previousItem = GetPreviousItem(transactions, transaction);
                var latestRule = rules.Where(r => r.Date <= transaction.Date)
                                      .OrderByDescending(r => r.Date)
                                      .FirstOrDefault();

                if (latestRule == null) continue;
                previousRateList.Add(latestRule.Rate);

                if (previousItem?.Date < latestRule.Date && transaction.Date > latestRule.Date)
                {
                    var lastRate = GetPreviousRate(rules, latestRule);

                    var daysBeforeRule = (latestRule.Date - previousItem.Date).Days;
                    var daysAfterRule = (transaction.Date - latestRule.Date).Days;
                    interestList.Add(CalculateInterestAmount(previousItem.Balance, lastRate.Rate, daysBeforeRule));
                    interestList.Add(CalculateInterestAmount(previousItem.Balance, latestRule.Rate, daysAfterRule));

                }
                if (latestRule.Date < previousItem?.Date && transaction.Date > previousItem.Date)
                {
                    int daysBetween = (transaction.Date - previousItem.Date).Days;
                    interestList.Add(CalculateInterestAmount(previousItem.Balance, latestRule.Rate, daysBetween));
                }
                if (Counter == transactions.Count())
                {
                    var endOfMonth = GetEndOfMonth(transaction);
                    int lastdaysBetween = (endOfMonth - transaction.Date.AddDays(-1)).Days;
                    interestList.Add(CalculateInterestAmount(transaction.Balance, latestRule.Rate, lastdaysBetween));
                }
            }

            return Math.Round(interestList.Sum() / 365, 2);
        }
        public DateTime GetEndOfMonth(AccountView transaction)
        {
            return new DateTime(transaction.Date.Year, transaction.Date.Month, 1)
              .AddMonths(1)
              .AddDays(-1);
        }

        public decimal CalculateInterestAmount(decimal balance, decimal rate, int days)
        {
            return (rate * days * balance) / 100;
        }

        private AccountView? GetPreviousItem(List<AccountView> transactions, AccountView currentItem)
        {
            int index = transactions.IndexOf(currentItem);
            return index > 0 ? transactions[index - 1] : null;
        }

        private DateTime GetEndOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }
    }
}
