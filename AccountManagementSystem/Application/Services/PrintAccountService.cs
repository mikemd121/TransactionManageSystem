using AccountManagementSystem.Application.Interfaces;
using System.Globalization;

namespace AccountManagementSystem
{
    public class PrintAccountService : IHandler
    {
        public void Handler()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
                Console.WriteLine("(or enter blank to go back to the main menu):");
                Console.Write("> ");

                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    ShowError("Invalid format! Please enter in <Account> <Year><Month> format.");
                    continue;
                }

                string account = parts[0];
                if (!DateTime.TryParseExact(parts[1], "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    ShowError("Invalid date format! Use yyyyMM.");
                    continue;
                }
                var accountTransactions = CalculateBalance(account, date, RuleService.rules, TransactionService.transactions);
                PrintStatement(accountTransactions, account);
                Console.WriteLine("Press any key to view another print statement or leave blank to go back.");
                Console.ReadKey();
            }
        }

        private void ShowError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
        }

        public List<AccountView> CalculateBalance(string account, DateTime date,List<InterestRule> rules,List<Transaction> transactions)
        {
            var formattedDate = date.ToString("yyyyMM");

            var accountTransactions = GetAccountTransactions(account, formattedDate, transactions);
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

        public void PrintStatement(List<AccountView> transactions, string account)
        {
            if (!transactions.Any())
            {
                Console.WriteLine($"No transactions found for account {account}.");
                return;
            }

            Console.Clear();
            Console.WriteLine($"Account: {account}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount  | Balance  |");
            Console.WriteLine("|----------|------------|------|---------|----------|");

            foreach (var txn in transactions)
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type}    | {txn.Amount:F2} | {txn.Balance:F2} |");
        }

        public bool AuthoriseHandler(string type)
        {
            return type == TransactionType.P.ToString();
        }
    }
}
