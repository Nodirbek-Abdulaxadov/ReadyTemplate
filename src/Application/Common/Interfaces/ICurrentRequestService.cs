namespace Application.Common.Interfaces;

public interface ICurrentRequestService
{
    Guid? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
