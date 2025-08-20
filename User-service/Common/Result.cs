namespace Common
{
    public class Result<T>
    {
        public Result(bool isSuccess, string? errorMessage, T? data)
        {
            IsSuccess = isSuccess;
            Message = errorMessage;
            Data = data;
        }

       public bool IsSuccess { get; set; }
       public string? Message { get; set; }
       public T? Data { get; set; }

        public static Result<T> Success(T data, string errorMessage)
        {
            return new Result<T>(true, errorMessage, data);
        }
        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, errorMessage, default);
        }

    }
}
