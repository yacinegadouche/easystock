namespace EasyStock.Web.Services;

public record ServiceResult(bool Succeeded, string? Error = null)
{
    public static ServiceResult Success() => new(true);
    public static ServiceResult Failure(string error) => new(false, error);
}