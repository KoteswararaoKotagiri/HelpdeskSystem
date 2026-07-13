namespace Helpdesk.Application.Common
{
    public enum ResultStatus
    {
        Success,
        NotFound,
        Invalid,
        Conflict
    }

    // Lightweight result so the Application layer can signal outcomes (not-found / invalid / conflict)
    // and thin controllers can map them to the correct HTTP status without throwing for control flow.
    public class Result
    {
        public ResultStatus Status { get; protected init; }

        public string? Error { get; protected init; }

        public bool IsSuccess => Status == ResultStatus.Success;

        public static Result Success() => new() { Status = ResultStatus.Success };

        public static Result NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };

        public static Result Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };

        public static Result Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    }

    public class Result<T> : Result
    {
        public T? Data { get; private init; }

        public static Result<T> Success(T data) => new() { Status = ResultStatus.Success, Data = data };

        public static new Result<T> NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };

        public static new Result<T> Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };

        public static new Result<T> Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    }
}
