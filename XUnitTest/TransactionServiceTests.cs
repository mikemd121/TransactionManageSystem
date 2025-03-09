using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using AccountManagementSystem;
using AccountManagementSystem.Application.Interfaces;
using System.Reflection;

public class TransactionServiceTests
{
    private readonly Mock<IHandler> _mockHandler;

    public TransactionServiceTests()
    {
        _mockHandler = new Mock<IHandler>();
    }

    [Theory]
    [InlineData("20250308", true)] // Valid format
    [InlineData("2025-03-08", false)] // Invalid format
    [InlineData("08-03-2025", false)] // Invalid format
    [InlineData("03/08/2025", false)] // Invalid format
    [InlineData("20251308", false)] // Invalid month
    [InlineData("20250230", false)] // Invalid day
    [InlineData("", false)] // Empty input
    [InlineData("abcdef", false)] // Non-numeric input
    public void Should_Validate_Date_Format(string input, bool expectedResult)
    {
        object[] parameters = new object[] { input, null };
       MethodInfo _tryParseDateMethod = typeof(TransactionService).GetMethod("TryParseDate", BindingFlags.NonPublic | BindingFlags.Instance);
        bool result = (bool)_tryParseDateMethod.Invoke(_tryParseDateMethod, parameters);
        Assert.Equal(expectedResult, result);
    }


    [Theory]
    [InlineData("X")]
    [InlineData("deposit")]
    [InlineData("")]
    public void Should_Reject_Invalid_Transaction_Type(string input)
    {
        var service = new TransactionService();
        var method = typeof(TransactionService).GetMethod("TryParseTransactionType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool result = (bool)method.Invoke(service, new object[] { input, null });
        Assert.False(result);
    }

    [Theory]
    [InlineData("-100")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData(" ")]
    public void Should_Reject_Invalid_Amounts(string input)
    {
        var service = new TransactionService();
        var method = typeof(TransactionService).GetMethod("TryParseAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool result = (bool)method.Invoke(service, new object[] { input, null });
        Assert.False(result);
    }

    [Fact]
    public void Should_Reject_First_Transaction_As_Withdrawal()
    {
        var service = new TransactionService();
        var method = typeof(TransactionService).GetMethod("IsTransactionValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        bool result = (bool)method.Invoke(service, new object[] { "A001", 'W', 500.00m });
        Assert.False(result);
    }

    [Fact]
    public void Should_Reject_Withdrawal_Exceeding_Balance()
    {
        var service = new TransactionService();
        var method = typeof(TransactionService).GetMethod("IsTransactionValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        TransactionService.transactions.Add(new Transaction
        {
            Account = "A001",
            Type = 'D',
            Amount = 100.00m
        });

        bool result = (bool)method.Invoke(service, new object[] { "A001", 'W', 500.00m });
        Assert.False(result);
    }

    [Fact]
    public void Should_Generate_Correct_Transaction_Id()
    {
        var service = new TransactionService();
        var method = typeof(TransactionService).GetMethod("GenerateTransactionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        DateTime testDate = DateTime.ParseExact("20250308", "yyyyMMdd", CultureInfo.InvariantCulture);

        string firstId = (string)method.Invoke(service, new object[] { testDate });
        string secondId = (string)method.Invoke(service, new object[] { testDate });

        Assert.Equal("20250308-01", firstId);
        Assert.Equal("20250308-02", secondId);
    }

    [Fact]
    public void Should_Authorize_Handler_For_Valid_Type()
    {
        _mockHandler.Setup(h => h.AuthoriseHandler(It.IsAny<string>())).Returns(true);
        bool result = _mockHandler.Object.AuthoriseHandler("T");
        Assert.True(result);
    }

    [Fact]
    public void Should_Not_Authorize_Handler_For_Invalid_Type()
    {
        _mockHandler.Setup(h => h.AuthoriseHandler(It.IsAny<string>())).Returns(false);
        bool result = _mockHandler.Object.AuthoriseHandler("X");
        Assert.False(result);
    }

    [Fact]
    public void Should_Invoke_Handler_When_Authorized()
    {
        _mockHandler.Setup(h => h.AuthoriseHandler("T")).Returns(true);
        _mockHandler.Setup(h => h.Handler());

        if (_mockHandler.Object.AuthoriseHandler("T"))
            _mockHandler.Object.Handler();

        _mockHandler.Verify(h => h.Handler(), Times.Once);
    }
}
