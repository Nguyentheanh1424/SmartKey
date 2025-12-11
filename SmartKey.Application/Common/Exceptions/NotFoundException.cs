namespace SmartKey.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Thực thể \"{name}\" ({key}) không tìm thấy.")
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
