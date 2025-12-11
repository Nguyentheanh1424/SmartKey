namespace SmartKey.Application.Common.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string message = "Lỗi ứng dụng.")
            : base(message)
        {
        }
    }
}
