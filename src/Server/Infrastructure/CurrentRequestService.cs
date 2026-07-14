namespace Server.Infrastructure;

public sealed class CurrentRequestService(IHttpContextAccessor accessor) : ICurrentRequestService
{
    public Guid? UserId => null;   // auth qo'shilganda claims'dan olinadi
    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => accessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
