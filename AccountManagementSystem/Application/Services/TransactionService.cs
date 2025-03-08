using AccountManagementSystem.Application.Interfaces;
using System.Globalization;

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
    }
}
