using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Account.Common
{
    // 编写一个通用的转换器（支持带空格的常见格式）
    public class MyDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateStr = reader.GetString();

            // 1. 尝试常见格式 "yyyy-MM-dd HH:mm:ss"
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                return dt;
            }

            // 2. 如果不行，再尝试 "yyyy/MM/dd HH:mm:ss" 
            if (DateTime.TryParseExact(dateStr, "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dt2))
            {
                return dt2;
            }

            // 3. 保底：让系统用默认逻辑自己试一下
            return DateTime.Parse(dateStr ?? "");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
