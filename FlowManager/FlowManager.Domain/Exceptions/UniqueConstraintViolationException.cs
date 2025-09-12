
namespace FlowManager.Domain.Exceptions
{
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(string message) : base(message)
        {
        }
    }
}
