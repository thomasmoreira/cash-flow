using System.Text.Json;

namespace CashFlow.Shared.Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object obj, JsonSerializerOptions options = null)
    {
        return JsonSerializer.Serialize(obj);
    }
}
