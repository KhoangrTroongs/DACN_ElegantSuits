namespace ElegantSuits.Application.Common.Models;

public class ResponseDTO<T>
{
    public bool IsSuccess { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ResponseDTO<T> Success(T data, string message = "Operation completed successfully")
    {
        return new ResponseDTO<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static ResponseDTO<T> Fail(string message, List<string>? errors = null, string? traceId = null)
    {
        return new ResponseDTO<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }
}
