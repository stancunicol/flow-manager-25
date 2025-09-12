
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
