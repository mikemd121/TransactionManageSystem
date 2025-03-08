using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagementSystem
{
    public class ConsoleInputOutputHandler : IInputOutputHandler
    {
        public string? ReadLine() => Console.ReadLine();
        public void Write(string message) => Console.Write(message);
        public void WriteLine(string message) => Console.WriteLine(message);
    }
}
