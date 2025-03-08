using AccountManagementSystem.Application.Interfaces;
using AccountManagementSystem;

public class AccountManagementService
{
    private readonly IEnumerable<IHandler> _handlers;
    private readonly IInputOutputHandler _ioHandler;

    public AccountManagementService(IEnumerable<IHandler> handlers, IInputOutputHandler ioHandler)
    {
        _handlers = handlers;
        _ioHandler = ioHandler;
    }

    public void Run()
    {
        while (true)
        {
            _ioHandler.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
            _ioHandler.WriteLine("[T] Input transactions");
            _ioHandler.WriteLine("[I] Define interest rules");
            _ioHandler.WriteLine("[P] Print statement");
            _ioHandler.WriteLine("[Q] Quit");
            _ioHandler.Write("Enter your choice: ");

            string? choice = _ioHandler.ReadLine()?.ToUpper();
            if (choice == "Q")
            {
                _ioHandler.WriteLine("Thank you for using AwesomeGIC Bank!");
                return;
            }

            bool isHandled = false;
            foreach (var handler in _handlers)
            {
                if (handler.AuthoriseHandler(choice))
                {
                    handler.Handler();
                    isHandled = true;
                    break;
                }
            }

            if (!isHandled)
            {
                _ioHandler.WriteLine("Invalid choice! Press any key to try again.");
            }
        }
    }
}
