using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountManagementSystem.Tests
{
    public class PrintAccountServiceTests
    {
        private readonly PrintAccountService _printAccountService;
        private readonly Mock<PrintAccountService> _printMockAccountService;
        public PrintAccountServiceTests()
        {

            _printAccountService = new PrintAccountService();
            _printMockAccountService = new Mock<PrintAccountService>();
        }

        [Fact]
        public void CalculateBalance_ShouldAddInterestToEndOfMonthAndUpdateBalance()
        {
            var account = "123";
            var date = new DateTime(2025, 03, 31); // End of month date
            var rules = new List<InterestRule>
            {
                new InterestRule
                {
                    Date = new DateTime(2025, 01, 01),
                    Rate = 5
                }
            };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Account = account,
                    Date = new DateTime(2025, 03, 01),
                    Amount = 1000,
                    Type = 'D',
                    TransactionId = "T1"
                }
            };
            _printMockAccountService.Setup(service => service.GetAccountTransactions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Transaction>>()))
                                .Returns((string account, string formattedDate, List<Transaction> transactionList) =>
                                {
                                    return transactionList.Where(t => t.Account == account && t.Date.ToString("yyyyMM") == formattedDate)
                                                          .Select(t => new AccountView
                                                          {
                                                              Account = t.Account,
                                                              Date = t.Date,
                                                              TransactionId = t.TransactionId,
                                                              Type = t.Type,
                                                              Amount = t.Amount,
                                                              Balance = t.Type == 'D' ? t.Amount : -t.Amount
                                                          })
                                                          .ToList();
                                });

            _printMockAccountService.Setup(service => service.CalculateInterest(It.IsAny<List<AccountView>>(), It.IsAny<List<InterestRule>>(), It.IsAny<DateTime>()))
                                .Returns(5.00m);
            var result = _printMockAccountService.Object.CalculateBalance(account, date, rules, transactions);

            var lastAccountView = result.LastOrDefault();
            Assert.NotNull(lastAccountView);
            Assert.Equal(1005.00m, lastAccountView?.Balance);
        }


        [Fact]
        public void CalculateInterest_ShouldReturnCorrectInterestAmount()
        {
            var transactions = new List<AccountView>
            {
                new AccountView
                {
                    Account = "123",
                    Date = new DateTime(2023, 06, 01),
                    Type = 'D',
                    Balance = 250,
                    TransactionId = "T1"
                },
                 new AccountView
                {
                    Account = "123",
                    Date = new DateTime(2023, 06, 26),
                    Type = 'W',
                    Balance = 130,
                    TransactionId = "T1"
                }
            };

            var rules = new List<InterestRule>
            {
                new InterestRule
                {
                    Date = new DateTime(2023, 05, 20),
                    Rate = 1.90m
                },
                 new InterestRule
                {
                    Date = new DateTime(2023, 06, 15),
                    Rate = 2.2m
                }
            };

            DateTime date = new DateTime(2023, 06, 26);
            var interest = _printAccountService.CalculateInterest(transactions, rules, date);
            Assert.Equal(0.39m, interest);
        }

        [Fact]
        public void CalculateInterestAmount_ShouldReturnZero_ForZeroBalance()
        {
            decimal balance = 0;
            decimal rate = 5.0m;
            int days = 30;

            var result = _printAccountService.CalculateInterestAmount(balance, rate, days);
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateInterestAmount_ShouldReturnCorrectInterest_ForValidInput()
        {
            decimal balance = 1000;
            decimal rate = 5.0m;
            int days = 30;

            var result = _printAccountService.CalculateInterestAmount(balance, rate, days);
            Assert.Equal(1500, result);
        }

        [Fact]
        public void GetAccountTransactions_ShouldReturnFilteredTransactions()
        {
            string account = "12345";
            string formattedDate = "202401";
            var transactions = new List<Transaction>
            {
                new Transaction { Account = "12345", Date = new DateTime(2024, 1, 5), Type = 'D', Amount = 100 },
                new Transaction { Account = "12345", Date = new DateTime(2024, 2, 5), Type = 'W', Amount = 50 }
            };


            var result = _printAccountService.GetAccountTransactions(account, formattedDate, transactions);
            Assert.Single(result);
            Assert.Equal(new DateTime(2024, 1, 5), result[0].Date);
        }

        [Fact]
        public void CalculateInterest_ShouldReturnZero_WhenNoTransaction()
        {
            var transactions = new List<AccountView>();
            var rules = new List<InterestRule>
            {
                new InterestRule { Date = new DateTime(2024, 1, 1), Rate = 5.0m }
            };
            var result = _printAccountService.CalculateInterest(transactions, rules, new DateTime(2024, 1, 1));
            Assert.Equal(0.00m, result);
        }
    }
}
