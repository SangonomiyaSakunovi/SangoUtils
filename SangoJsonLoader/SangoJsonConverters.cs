using System;
using System.Text.Json;
using System.Text.Json.Serialization;

//Developer: SangonomiyaSakunovi

namespace SangoJsonLoader
{
    public class SangoJsonConverters
    {
        public class NullableEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
        {
            public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return default;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (Enum.TryParse<TEnum>(reader.GetInt32().ToString(), out TEnum value))
                    {
                        return value;
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    if (Enum.TryParse<TEnum>(reader.GetString(), out TEnum value))
                    {
                        return value;
                    }
                }                
                throw new JsonException($"Unable to parse enum value: {reader.GetString()}");
            }

            public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        public class NullableInt32Converter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return 0;
                }

                return reader.GetInt32();
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        public class NullableFloatConverter : JsonConverter<float>
        {
            public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return 0.0f;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetSingle();
                }

                throw new JsonException($"Unable to parse float value: {reader.GetString()}");
            }

            public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }

        public class NullableStringConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt32().ToString();
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString();
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                throw new JsonException($"Unable to parse string value: {reader.GetString()}");
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
