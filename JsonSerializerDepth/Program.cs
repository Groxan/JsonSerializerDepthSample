using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonSerializerDepth
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText("Sample.json");

            var options = new JsonSerializerOptions { MaxDepth = 256 };
            options.Converters.Add(new CustomConverter());

            // works fine
            var deepArray = JsonSerializer.Deserialize<DeepArray>(json, options);

            // fails with exception:
            // "The maximum configured depth of 64 has been exceeded. Cannot read next JSON array"
            var content = JsonSerializer.Deserialize<IContent>(json, options);
        }
    }

    interface IContent { }

    class DeepArray : IContent
    {
        public JsonElement array { get; set; }
    }

    class CustomConverter : JsonConverter<IContent>
    {
        public override IContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var sideReader = reader;

            sideReader.Read();
            sideReader.Read();
            var type = sideReader.GetString();

            return type switch
            {
                "array" => JsonSerializer.Deserialize<DeepArray>(ref reader, options),
                _ => throw new JsonException()
            };
        }

        public override void Write(Utf8JsonWriter writer, IContent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
