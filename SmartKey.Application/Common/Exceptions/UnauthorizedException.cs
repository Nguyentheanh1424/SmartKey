namespace SmartKey.Application.Common.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
        {
        }

        public UnauthorizedException(string message = "Truy cập trái phép.")
            : base(message)
        {
        }

        public UnauthorizedException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
