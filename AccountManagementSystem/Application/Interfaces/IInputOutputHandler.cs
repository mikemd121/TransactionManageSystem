using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagementSystem
{
    public interface IInputOutputHandler
    {
        string? ReadLine();
        void Write(string message);
        void WriteLine(string message);
    }

}
