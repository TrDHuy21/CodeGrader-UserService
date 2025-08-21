namespace Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public ErrorDetail errorDetail { get; set; }

        public Result(bool isSuccess, string? message, T? data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }
        public static Result<T> Success(T data, string Message)
        {
            return new Result<T>(true, Message, data);
        }
        public static Result<T> Failure(string Message)
        {
            return new Result<T>(false, Message, default);
        }

        public static Result<T> Failure(List<ErrorField> error)
        {
            return new Result<T>(false, "Input validation failed", default)
            {
                errorDetail = new ErrorDetail { Errors = error }
            };
        }

    }
    public class ErrorDetail
    {
        public List<ErrorField>? Errors { get; set; } = new List<ErrorField>();
    }
    public class ErrorField
    {
        public string Field { get; set; }
        public string ErrorMessage { get; set; }
    }
}
