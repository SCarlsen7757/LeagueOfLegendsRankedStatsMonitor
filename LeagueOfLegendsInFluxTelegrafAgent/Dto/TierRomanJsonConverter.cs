using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfluxDbDataInsert.Dto
{
    public class TierRomanJsonConverter : JsonConverter<LeagueTier>
    {
        public override LeagueTier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string s = reader.GetString()!.Trim().ToUpper();

            return s switch
            {
                "IRON" => LeagueTier.Iron,
                "BRONZE" => LeagueTier.Bronze,
                "SILVER" => LeagueTier.Silver,
                "GOLD" => LeagueTier.Gold,
                "PLATINUM" => LeagueTier.Platinum,
                "EMERALD" => LeagueTier.Emerald,
                "DIAMOND" => LeagueTier.Diamond,
                "MASTER" => LeagueTier.Master,
                "GRANDMASTER" => LeagueTier.Grandmaster,
                "CHALLENGER" => LeagueTier.Challenger,
                _ => LeagueTier.Iron
            };
        }

        public override void Write(Utf8JsonWriter writer, LeagueTier value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString().ToUpper());
    }
}
