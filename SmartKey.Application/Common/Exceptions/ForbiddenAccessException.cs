namespace SmartKey.Application.Common.Exceptions
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException()
            : base("Bạn không có quyền thực hiện hành động này.")
        {
        }

        public ForbiddenAccessException(string message)
            : base(message)
        {
        }
    }
}
