using Xunit;
using Moq;
using System.Collections.Generic;
using AccountManagementSystem.Application.Interfaces;
using AccountManagementSystem;

public class AccountManagementServiceTests
{
    private readonly Mock<IInputOutputHandler> _ioMock;
    private readonly Mock<IHandler> _transactionHandlerMock;
    private readonly Mock<IHandler> _interestHandlerMock;
    private readonly Mock<IHandler> _printHandlerMock;
    private readonly AccountManagementService _service;

    public AccountManagementServiceTests()
    {
        _ioMock = new Mock<IInputOutputHandler>();

        _transactionHandlerMock = new Mock<IHandler>();
        _transactionHandlerMock.Setup(h => h.AuthoriseHandler("T")).Returns(true);

        _interestHandlerMock = new Mock<IHandler>();
        _interestHandlerMock.Setup(h => h.AuthoriseHandler("I")).Returns(true);

        _printHandlerMock = new Mock<IHandler>();
        _printHandlerMock.Setup(h => h.AuthoriseHandler("P")).Returns(true);

        var handlers = new List<IHandler>
        {
            _transactionHandlerMock.Object,
            _interestHandlerMock.Object,
            _printHandlerMock.Object
        };

        _service = new AccountManagementService(handlers, _ioMock.Object);
    }

    [Fact]
    public void Should_Display_Menu_On_Start()
    {
        _service.Run();
        _ioMock.Verify(io => io.WriteLine("Welcome to AwesomeGIC Bank!"), Times.Once);
        _ioMock.Verify(io => io.WriteLine("[T] Input transactions"), Times.Once);
        _ioMock.Verify(io => io.WriteLine("[Q] Quit"), Times.Once);
    }

    [Fact]
    public void Should_Exit_On_Q_Input()
    {
        _ioMock.SetupSequence(io => io.ReadLine())
            .Returns("Q");
        _service.Run();
        _ioMock.Verify(io => io.WriteLine("Thank you for using AwesomeGIC Bank!"), Times.Once);
    }

    [Fact]
    public void Should_Call_Correct_Handler()
    {
        _ioMock.SetupSequence(io => io.ReadLine())
            .Returns("T") 
            .Returns("Q"); 

        _service.Run();
        _transactionHandlerMock.Verify(h => h.Handler(), Times.Once);
    }

    [Fact]
    public void Should_Show_Error_On_Invalid_Input()
    {
        _ioMock.SetupSequence(io => io.ReadLine())
            .Returns("X")
            .Returns("Q"); 

        _service.Run();
        _ioMock.Verify(io => io.WriteLine("Invalid choice! Press any key to try again."), Times.Once);
    }
}
