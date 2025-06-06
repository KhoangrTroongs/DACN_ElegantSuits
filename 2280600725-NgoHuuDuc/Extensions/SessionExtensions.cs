using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace NgoHuuDuc_2280600725.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson<T>(this ISession session, string key, T value)
        {
            // Lưu đối tượng bất kỳ vào session dưới dạng chuỗi JSON
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            // Lấy đối tượng từ session, chuyển từ JSON về kiểu T
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
    }
}
