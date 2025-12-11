using FluentValidation.Results;

namespace SmartKey.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base("Đã xảy ra một hoặc nhiều lỗi xác thực.")
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(fg => fg.Key, fg => fg.ToArray());
        }
    }
}
