namespace courseProject;

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

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.TokenType != JsonToken.StartObject)
            return null;

        Interval intervalR;
        Interval intervalZ;
        int splitsR;
        int splitsZ;
        double? coef;
        double lambda, sigma;

        var maintoken = JObject.Load(reader);

        var token = maintoken["Initial area in R"];
        intervalR = serializer.Deserialize<Interval>(token!.CreateReader());
        token = maintoken["Splits by R"];
        splitsR = Convert.ToInt32(token);

        token = maintoken["Initial area in Z"];
        intervalZ = serializer.Deserialize<Interval>(token!.CreateReader());
        token = maintoken["Splits by Z"];
        splitsZ = Convert.ToInt32(token);

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
        lambda = Convert.ToDouble(token);

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
public readonly record struct SpaceGridParameters
{
    public Interval IntervalR { get; init; }
    public int SplitsR { get; init; }
    public Interval IntervalZ { get; init; }
    public int SplitsZ { get; init; }
    public double? K { get; init; }

    // Uniform area
    public double Lambda { get; init; }
    public double Sigma { get; init; }

    public SpaceGridParameters(Interval intervalR, int splitsR, Interval intervalZ, int splitsZ, double? k, double lambda, double sigma)
    {
        IntervalR = intervalR;
        SplitsR = splitsR;
        IntervalZ = intervalZ;
        SplitsZ = splitsZ;
        K = k;
        Lambda = lambda;
        Sigma = sigma;
    }

    public static SpaceGridParameters? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
                throw new Exception("File does not exist");

            var sr = new StreamReader(jsonPath);
            using (sr)
            {
                return JsonConvert.DeserializeObject<SpaceGridParameters>(sr.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return null;
        }
    }
}

public readonly record struct TimeGridParameters
{
    [JsonProperty("Initial area")]
    public Interval Interval { get; init; }

    [JsonProperty("Number of splits")]
    public int Splits { get; init; }

    [JsonProperty("Coef")]
    public double? K { get; init; } // коэффициент разрядки

    public TimeGridParameters(Interval interval, int splits, double? k)
    {
        Interval = interval;
        Splits = splits;
        K = k;
    }

    public static TimeGridParameters? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
                throw new Exception("File does not exist");

            var sr = new StreamReader(jsonPath);
            using (sr)
            {
                return JsonConvert.DeserializeObject<TimeGridParameters>(sr.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return null;
        }
    }
}