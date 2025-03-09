using AccountManagementSystem.Application.Interfaces;
using System.Globalization;
using System.Transactions;

namespace AccountManagementSystem
{
    public class PrintAccountService : IHandler
    {
        TransactionService _transactionService;
        public PrintAccountService(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }
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

                var accountTransactions = _transactionService.CalculateBalance(account, date, RuleService.rules, TransactionService.transactions);
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
