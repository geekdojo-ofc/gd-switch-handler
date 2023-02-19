
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GD.SwitchHandler.VendorSwitchModels
{
    public sealed class HexStringJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(uint).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue($"0x{value:x}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {            
            var value = reader.Value as string;
            if (value == null || !value.StartsWith("0x"))
                throw new JsonSerializationException();
            return Convert.ToInt32(value, 16);
        }
    }
}