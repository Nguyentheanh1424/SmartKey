namespace SmartKey.Application.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message = "Lỗi nghiệp vụ.")
            : base(message)
        {
        }
    }
}
