using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Application.Features.Idempotency;

public static class IdempotencyHasher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string HashRequest<T>(T request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}
