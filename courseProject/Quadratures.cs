namespace courseProject;

public class QuadratureNode<T> where T : notnull
{
    public T Node { get; init; }
    public double Weight { get; init; }

    public QuadratureNode(T node, double weight)
    {
        Node = node;
        Weight = weight;
    }
}

public class Quadrature<T> where T : notnull
{
    private readonly QuadratureNode<T>[] _nodes = default!;
    public ImmutableArray<QuadratureNode<T>> Nodes => _nodes.ToImmutableArray();

    public Quadrature(QuadratureNode<T>[] nodes)
    {
        _nodes = nodes;
    }
}

public static class Quadratures
{
    public static IEnumerable<QuadratureNode<double>> SegmentGaussOrder9()
    {
        const int n = 5;
        double[] points = { 0.0,
                            1.0 / 3.0 * Math.Sqrt(5 - (2 * Math.Sqrt(10.0 / 7.0))),
                            -1.0 / 3.0 * Math.Sqrt(5 - (2 * Math.Sqrt(10.0 / 7.0))),
                            1.0 / 3.0 * Math.Sqrt(5 + (2 * Math.Sqrt(10.0 / 7.0))),
                            -1.0 / 3.0 * Math.Sqrt(5 + (2 * Math.Sqrt(10.0 / 7.0)))};

        double[] weights = { 128.0 / 225.0,
                            (322.0 + (13.0 * Math.Sqrt(70.0))) / 900.0,
                            (322.0 + (13.0 * Math.Sqrt(70.0))) / 900.0,
                            (322.0 - (13.0 * Math.Sqrt(70.0))) / 900.0,
                            (322.0 - (13.0 * Math.Sqrt(70.0))) / 900.0 };

        for (int i = 0; i < n; i++)
        {
            yield return new QuadratureNode<double>(points[i], weights[i]);
        }
    }

    public static IEnumerable<QuadratureNode<Point2D>> TriangleOrder6()
    {
        const double x1a = 0.873821971016996;
        const double x1b = 0.063089014491502;
        const double x2a = 0.501426509658179;
        const double x2b = 0.249286745170910;
        const double x3a = 0.636502499121399;
        const double x3b = 0.310352451033785;
        const double x3c = 0.053145049844816;
        const double w1 = 0.050844906370207;
        const double w2 = 0.116786275726379;
        const double w3 = 0.082851075618374;

        double[] p1 = { x1a, x1b, x1b, x2a, x2b, x2b, x3a, x3b, x3a, x3c, x3b, x3c };
        double[] p2 = { x1b, x1a, x1b, x2b, x2a, x2b, x3b, x3a, x3c, x3a, x3c, x3b };
        double[] w = { w1, w1, w1, w2, w2, w2, w3, w3, w3, w3, w3, w3 };

        for (int i = 0; i < w.Length; i++)
        {
            yield return new QuadratureNode<Point2D>(new(p1[i], p2[i]), w[i]);
        }
    }
}
