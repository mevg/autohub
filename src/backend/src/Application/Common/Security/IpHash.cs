using System.Security.Cryptography;
using System.Text;

namespace AutoHub.Application.Common.Security;

public static class IpHash
{
    public static string? Create(string? ipAddress, string? salt)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var value = $"{ipAddress.Trim()}|{salt ?? string.Empty}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexStringLower(bytes);
    }
}
