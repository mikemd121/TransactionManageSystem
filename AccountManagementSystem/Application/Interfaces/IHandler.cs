using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagementSystem.Application.Interfaces
{
   public interface IHandler
    {
        public void Handler();

        public bool AuthoriseHandler(string type);
    }
}
