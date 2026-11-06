namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Guarda archivos bajo wwwroot/{folder} con nombre hasheado y devuelve la URL relativa.
/// La extensión se deriva del Content-Type validado, nunca del nombre original.
/// </summary>
public interface ILocalFileStorage
{
    Task<string> SaveAsync(string folder, string prefix, string contentType, byte[] data, CancellationToken cancellationToken = default);
    void Delete(string folder, string url);
}
