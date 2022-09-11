namespace courseProject.Grids;

public class SpaceGridParametersJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var gridParameters = (SpaceGridParameters)value;

        writer.WriteStartObject();
        writer.WritePropertyName("Initial area in R");
        serializer.Serialize(writer, gridParameters.IntervalR);
        writer.WritePropertyName("Splits by R");
        writer.WriteValue(gridParameters.SplitsR);
        writer.WriteWhitespace("\n");

        writer.WritePropertyName("Initial area in Z");
        serializer.Serialize(writer, gridParameters.IntervalZ);
        writer.WritePropertyName("Splits by Z");
        writer.WriteValue(gridParameters.SplitsZ);

        writer.WriteComment("Коэффициент разрядки");
        writer.WritePropertyName("Coef");
        writer.WriteValue(gridParameters.K);
        writer.WriteWhitespace("\n");
        writer.WriteComment("Uniform area");
        writer.WritePropertyName("Lambda");
        writer.WriteValue(gridParameters.Lambda);
        writer.WritePropertyName("Sigma");
        writer.WriteValue(gridParameters.Sigma);
        writer.WriteEndObject();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.TokenType != JsonToken.StartObject) return null;

        double? coef;
        double sigma;

        var maintoken = JObject.Load(reader);

        var token = maintoken["Initial area in R"];
        var intervalR = serializer.Deserialize<Interval>(token!.CreateReader());
        token = maintoken["Splits by R"];
        var splitsR = Convert.ToInt32(token);

        token = maintoken["Initial area in Z"];
        var intervalZ = serializer.Deserialize<Interval>(token!.CreateReader());
        token = maintoken["Splits by Z"];
        var splitsZ = Convert.ToInt32(token);

        token = maintoken["Coef"];

        if (token is not null)
        {
            coef = double.TryParse(token.ToString(), out double res) ? res : null;
        }
        else
        {
            coef = null;
        }

        token = maintoken["Lambda"];
        var lambda = Convert.ToDouble(token);

        token = maintoken["Sigma"];

        if (token is not null)
        {
            sigma = double.TryParse(token.ToString(), out double res) ? res : 0.0;
        }
        else
        {
            sigma = 0.0;
        }

        return new SpaceGridParameters(intervalR, splitsR, intervalZ, splitsZ, coef, lambda, sigma);
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(SpaceGridParameters);
}

[JsonConverter(typeof(SpaceGridParametersJsonConverter))]
public readonly record struct SpaceGridParameters(Interval IntervalR, int SplitsR, Interval IntervalZ, int SplitsZ,
    double? K, double Lambda, double Sigma)
{
    public static SpaceGridParameters? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<SpaceGridParameters>(sr.ReadToEnd());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
    }
}

public readonly record struct TimeGridParameters([property: JsonProperty("Initial area")]
    Interval Interval, [property: JsonProperty("Number of splits")]
    int Splits, [property: JsonProperty("Coef")] double? K)
{
    public static TimeGridParameters? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<TimeGridParameters>(sr.ReadToEnd());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
    }
}