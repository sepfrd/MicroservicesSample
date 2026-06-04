using System.Net;

namespace ToDoListManager.Common.Dtos;

public class CustomResponse<T>
{
    public T? Data { get; private init; }

    public bool IsSuccess { get; private init; }

    public string? Message { get; private init; }

    public HttpStatusCode HttpStatusCode { get; private init; }

    public static CustomResponse<T> CreateUnsuccessfulResponse(HttpStatusCode httpStatusCode, string? message = null) =>
        new()
        {
            IsSuccess = false,
            Message = message,
            HttpStatusCode = httpStatusCode
        };

    public static CustomResponse<T> CreateSuccessfulResponse(T data, string? message = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK) =>
        new()
        {
            Data = data,
            IsSuccess = true,
            Message = message,
            HttpStatusCode = httpStatusCode
        };
}

public class CustomResponse
{
    public bool IsSuccess { get; init; }

    public string? Message { get; init; }

    public HttpStatusCode HttpStatusCode { get; init; }

    public static CustomResponse CreateUnsuccessfulResponse(HttpStatusCode httpStatusCode, string? message = null) =>
        new()
        {
            IsSuccess = false,
            Message = message,
            HttpStatusCode = httpStatusCode
        };

    public static CustomResponse CreateSuccessfulResponse(string? message = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK) =>
        new()
        {
            IsSuccess = true,
            Message = message,
            HttpStatusCode = httpStatusCode
        };
}