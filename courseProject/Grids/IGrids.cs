namespace courseProject.Grids;

public interface ISpaceGrid
{
    public double Lambda { get; }
    public double Sigma { get; }
    public ImmutableList<Point2D> Points { get; }
    public ImmutableArray<ImmutableArray<int>> Elements { get; }
    public ImmutableArray<ImmutableArray<ImmutableArray<int>>> Edges { get; }
}

public interface ITimeGrid
{
    public ImmutableArray<double> Points { get; }
}