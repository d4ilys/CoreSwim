using System.Text.Json;
using System.Text.Json.Serialization;

namespace Daily.CoreSwim.Dashboard;

// 处理非空DateTime
internal class DateTimeConverter : JsonConverter<DateTime>
{
    // 序列化：将DateTime转换为指定格式字符串
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // 格式化为 "yyyy-MM-dd HH:mm:ss"（HH为24小时制）
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    // 反序列化：将字符串转换为DateTime（按需实现，若无需反序列化可简化）
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString()!);
    }
}

// 处理可空DateTime?
internal class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        else
        {
            writer.WriteNullValue(); // 空值直接写null
        }
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetDateTime(out DateTime date) ? date : null;
    }
}