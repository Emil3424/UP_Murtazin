using System.Text.Json;

namespace UP_Murtazin.Web.Extensions;

public static class SessionExtensions
{
    public static void SetJson<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value is null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
