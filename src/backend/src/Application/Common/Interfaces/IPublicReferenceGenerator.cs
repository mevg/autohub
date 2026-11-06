namespace AutoHub.Application.Common.Interfaces;

public interface IPublicReferenceGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
