using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    
        public interface IAuthService
        {
            Task<bool> Login(string email, string password);
            Task Logout();
        }
    

}
