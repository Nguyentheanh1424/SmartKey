namespace SmartKey.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }

        private Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            ErrorMessage = error;
        }

        public static Result Success() => new(true, null);

        public static Result Success(string message) => new(true, message);

        public static Result Failure(string error)
            => new(false, error);

        public static Result<T> Success<T>(T value)
            => Result<T>.Success(value);
    }

    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }

        private Result(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value)
            => new(true, value, null);

        public static Result<T> Success(T value, string message)
            => new(true, value, message);

        public static Result<T> Failure(string errorMessage)
            => new(false, default, errorMessage);
    }
}
