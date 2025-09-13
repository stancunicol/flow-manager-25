using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Exceptions
{
    public class EmailNotSentException : Exception
    {
        public EmailNotSentException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
