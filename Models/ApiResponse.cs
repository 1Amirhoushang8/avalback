using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AvalWebBack.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> ErrorResponse(string message) =>
        new() { Success = false, Message = message };

    public static ApiResponse<T> ErrorResponse(ModelStateDictionary modelState)
    {
        var errors = string.Join("; ", modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        return new() { Success = false, Message = errors };
    }
}