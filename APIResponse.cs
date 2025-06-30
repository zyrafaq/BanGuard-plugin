namespace BanGuard;

public class APIResponse<T>
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }

    public APIResponse(bool success, T? data = default, string? errorMessage = null)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }
}