using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IPasswordResetService
    {
        Task SendResetCodeAsync(string email);
        Task<bool> VerifyResetCodeAsync(string email, string code);
        Task<bool> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
