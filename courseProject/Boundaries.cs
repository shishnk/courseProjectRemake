namespace courseProject;

public readonly record struct DirichletBoundary(int Element, int Edge)
{
    public static DirichletBoundary[]? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
                throw new Exception("File does not exist");

            var sr = new StreamReader(jsonPath);
            using (sr)
            {
                return JsonConvert.DeserializeObject<DirichletBoundary[]>(sr.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return null;
        }
    }
}

public readonly record struct NeumannBoundary(int Element, int Edge, double Value)
{
    public static NeumannBoundary[]? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
                throw new Exception("File does not exist");

            var sr = new StreamReader(jsonPath);
            using (sr)
            {
                return JsonConvert.DeserializeObject<NeumannBoundary[]>(sr.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return null;
        }
    }
}