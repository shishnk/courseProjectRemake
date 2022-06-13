namespace courseProject;

public interface ISpaceGrid
{
    public double Lambda { get; init; }
    public double Sigma { get; init; }
    public ImmutableList<Point2D> Points { get; }
    public ImmutableArray<ImmutableArray<int>> Elements { get; }
    public ImmutableArray<ImmutableArray<ImmutableArray<int>>> Edges { get; }
}

public interface ITimeGrid
{
    public ImmutableArray<double> Points { get; }
}