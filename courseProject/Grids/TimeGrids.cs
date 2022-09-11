namespace courseProject.Grids;

public class TimeRegularGrid : ITimeGrid
{
    private double[] _points = default!;
    public ImmutableArray<double> Points => _points.ToImmutableArray();

    public TimeRegularGrid(TimeGridParameters gridParameters)
    {
        Build(gridParameters);
    }

    private void Build(TimeGridParameters gridParameters)
    {
        try
        {
            if (gridParameters.Interval.LeftBorder < 0)
            {
                throw new Exception("The beginning of the time segment cannot be less than 0");
            }

            if (gridParameters.Splits < 1)
            {
                throw new Exception("The number of splits must be greater than or equal to 1");
            }

            _points = new double[gridParameters.Splits + 1];

            double h = gridParameters.Interval.Lenght / gridParameters.Splits;

            _points[0] = gridParameters.Interval.LeftBorder;
            _points[^1] = gridParameters.Interval.RightBorder;

            for (int i = 1; i < _points.Length - 1; i++)
            {
                _points[i] = _points[i - 1] + h;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
    }
}

public class TimeIrregularGrid : ITimeGrid
{
    private double[] _points = default!;
    public ImmutableArray<double> Points => _points.ToImmutableArray();

    public TimeIrregularGrid(TimeGridParameters gridParameters)
    {
        Build(gridParameters);
    }

    private void Build(TimeGridParameters gridParameters)
    {
        try
        {
            if (gridParameters.Interval.LeftBorder < 0)
            {
                throw new Exception("The beginning of the time segment cannot be less than 0");
            }

            if (gridParameters.Splits < 1)
            {
                throw new Exception("The number of splits must be greater than or equal to 1");
            }

            ArgumentNullException.ThrowIfNull(gridParameters.K, $"{nameof(gridParameters.K)} cannot be null");

            _points = new double[gridParameters.Splits + 1];

            double sum = 0.0;

            for (int k = 0; k < gridParameters.Splits; k++)
                sum += Math.Pow(gridParameters.K.Value, k);

            var h = gridParameters.Interval.Lenght / sum;
            _points[0] = gridParameters.Interval.LeftBorder;
            _points[^1] = gridParameters.Interval.RightBorder;

            for (int i = 1; i < _points.Length - 1; i++)
            {
                _points[i] = _points[i - 1] + h;
                h *= gridParameters.K.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
    }
}