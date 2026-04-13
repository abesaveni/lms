namespace LiveExpert.Application.Common;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public Error? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public static Result<T> SuccessResult(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data
        };
    }

    public static Result<T> FailureResult(string code, string message, object? details = null)
    {
        return new Result<T>
        {
            Success = false,
            Error = new Error
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

public class Result
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Error? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static Result SuccessResult(string? message = null)
    {
        return new Result
        {
            Success = true,
            Message = message
        };
    }

    public static Result FailureResult(string code, string message, object? details = null)
    {
        return new Result
        {
            Success = false,
            Error = new Error
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

public class Error
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
