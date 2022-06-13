namespace courseProject;

public static class Integration
{
    public static double GaussSegment(Func<Point2D, double> psi, Point2D firstPoint, Point2D secondPoint)
    {
        var quadratures = Quadratures.SegmentGaussOrder9();

        double result = 0.0;
        double lenghtEdge = Math.Sqrt(((firstPoint.R - secondPoint.R) * (firstPoint.R - secondPoint.R)) +
                            ((firstPoint.Z - secondPoint.Z) * (firstPoint.Z - secondPoint.Z)));

        foreach (var q in quadratures)
        {
            double qi = q.Weight;
            double pi = (1 + q.Node) / 2.0;

            var parameterized = Parameterization(pi);

            result += qi * psi(parameterized) * parameterized.R;
        }

        return result * lenghtEdge / 2.0;

        Point2D Parameterization(double t)
            => ((secondPoint - firstPoint) * t) + firstPoint;
    }

    public static double Triangle(Func<Point2D, double, double> f, Func<Point2D, double> psi, Point2D[] vertices, double t)
    {
        var quadratures = Quadratures.TriangleOrder6();

        double result = 0.0;
        double determinant = Math.Abs(Determinant());

        foreach (var q in quadratures)
        {
            var point = ((1 - q.Node.R - q.Node.Z) * vertices[0]) + (q.Node.R * vertices[1]) + (q.Node.Z * vertices[2]);

            result += f(point, t) * psi(point) * q.Weight * determinant * 0.5 * point.R;
        }

        return result;

        double Determinant()
        => ((vertices[1].R - vertices[0].R) * (vertices[2].Z - vertices[0].Z)) -
           ((vertices[2].R - vertices[0].R) * (vertices[1].Z - vertices[0].Z));
    }
}