using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SaveLoad
{
    public class GemMatrixConveter : JsonConverter<Gem[,]>
    {
        public override Gem[,] Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            return null;
        }
        public override void Write(
            Utf8JsonWriter writer,
            Gem[,] gemMatrix,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartArray();
            gemMatrix.Length
            writer.WriteEndArray();
        }

    }
    public class TupleConveter : JsonConverter<ValueTuple<int, CustomGem>>
    {
        public override ValueTuple<int, CustomGem> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            return (1, null);
        }
        public override void Write(
            Utf8JsonWriter writer,
            ValueTuple<int, CustomGem> x,
            JsonSerializerOptions options
        )
        {
            writer.WriteNull("x");
        }

    }
    public static void Test()
    {
        JsonSerializerOptions op = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        op.Converters.Add(new GemMatrixConveter());
        op.Converters.Add(new TupleConveter());
        string s = JsonSerializer.Serialize(
            GameState.Persistent, op
        );
        Console.WriteLine(s);
    }
}
