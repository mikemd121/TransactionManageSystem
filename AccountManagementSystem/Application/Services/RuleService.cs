using AccountManagementSystem.Application.Interfaces;
using System.Globalization;


namespace AccountManagementSystem
{
   public class RuleService : IHandler
    {
        public static List<InterestRule> rules = new List<InterestRule>();
        private int ruleCounter = 1;

        public void Handler()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Please enter interest rule details in <Date> <RuleId> <Rate in %> format");
                Console.WriteLine("(or press Enter to return to the main menu):");
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) return;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    ShowError("Invalid format! Please use <Date> <RuleId> <Rate in %>.");
                    continue;
                }

                if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    ShowError("Invalid date format! Use YYYYMMDD.");
                    continue;
                }

                if (!decimal.TryParse(parts[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal rate) || rate <= 0 || rate > 100)
                {
                    ShowError("Interest rate should be greater than 0 and less than or equal to 100.");
                    continue;
                }

                rules.RemoveAll(x => x.Date == date);
                var newRule = new InterestRule
                {
                    Date = date,
                    RuleId = "RULE" + ruleCounter++,
                    Rate = rate
                };
                rules.Add(newRule);

                DisplayInterestRules();
                Console.WriteLine("Press any key to enter another transaction or leave blank to go back.");
                Console.ReadKey();
            }
        }

        public void DisplayInterestRules()
        {
            Console.Clear();
            Console.WriteLine("| Date     | RuleId  | Rate (%) |");
            Console.WriteLine("|----------|---------|----------|");

            foreach (var rule in rules)
                Console.WriteLine($"| {rule.Date:yyyyMMdd} | {rule.RuleId,-6} | {rule.Rate,8:N2} |");
        }

        private void ShowError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
        }

        public bool AuthoriseHandler(string type)
        {
            return type == TransactionType.I.ToString();
        }
    }
}
