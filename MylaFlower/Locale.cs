using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Modding;
using System;
using System.Collections.Generic;

namespace MylaFlower
{
    [JsonConverter(typeof(LocaleConverter))]
    public class Locale
    {
        private static readonly Random _rng = new();

        public Dictionary<string, List<string>> Data;

        public bool TryGetString(string key, out string value)
        {
            if (!Data.TryGetValue(key, out List<string> data) || data.Count == 0)
            {
                value = $"%!%{key}%!%";
                return false;
            }

            value = data[_rng.Next(data.Count)];
            return true;
        }
    }

    public class LocaleConverter : JsonConverter<Locale>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override Locale ReadJson(JsonReader reader, Type objectType, Locale existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Dictionary<string, List<string>> data = new();

            JObject obj = JObject.Load(reader);
            foreach ((string key, JToken value) in obj)
            {
                if (value.Type == JTokenType.String)
                {
                    data[key] = new List<string>() { value.ToObject<string>() };
                }
                else if (value.Type == JTokenType.Array)
                {
                    data[key] = value.ToObject<List<string>>();
                }
            }

            return new() { Data = data };
        }

        public override void WriteJson(JsonWriter writer, Locale value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
