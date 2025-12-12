using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeagueOfLegendsInFluxTelegrafAgent.Dto
{
    public class RomanIntJsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var n))
                return n;
            if (reader.TokenType != JsonTokenType.String)
                return 0;

            var s = reader.GetString()!.Trim().ToUpper();
            return s switch
            {
                "I" => 1,
                "II" => 2,
                "III" => 3,
                "IV" => 4,
                _ => 0
            };
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            string s = value switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                _ => value.ToString()
            };

            writer.WriteStringValue(s);
        }
    }
}
