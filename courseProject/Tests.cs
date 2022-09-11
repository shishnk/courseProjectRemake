namespace courseProject;

public interface ITest
{
    public double U(Point2D point, double t);

    public double F(Point2D point, double t);
}

public class Test1 : ITest
{
    public double U(Point2D point, double t)
        => point.R * point.R + point.Z + t;

    public double F(Point2D point, double t)
        => -3;
}

public class Test2 : ITest
{
    public double U(Point2D point, double t)
        => point.R * point.R - point.Z * t * t;

    public double F(Point2D point, double t)
        => -8 - t * point.Z;
}

public class Test3 : ITest
{
    public double U(Point2D point, double t)
        => 2 * point.R * point.R * point.R * t * t * t;

    public double F(Point2D point, double t)
        => -9 * point.R * t * t * t + 12 * point.R * point.R * point.R * t * t;
}

public class Test4 : ITest
{
    public double U(Point2D point, double t)
       => point.R * point.R * point.R * point.R + point.Z * point.Z * point.Z * point.Z + t * t * t * t;

    public double F(Point2D point, double t)
        => -16 * point.R * point.R - 12 * point.Z * point.Z + 4 * t * t * t;
}

public class Test5 : ITest
{
    public double U(Point2D point, double t)
        => Math.Log(point.R);

    public double F(Point2D point, double t)
        => 0;
}

public class Test6 : ITest
{
    public double U(Point2D point, double t)
         => Math.Cos(t);

    public double F(Point2D point, double t)
        => -Math.Sin(t);
}

public class Test7 : ITest
{
    public double U(Point2D point, double t)
        => t * t * t * t;

    public double F(Point2D point, double t)
        => 4 * t * t * t;
}