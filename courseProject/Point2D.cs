namespace courseProject;

public readonly record struct Point2D(double R, double Z)
{
    public static Point2D operator *(double constant, Point2D point)
        => new(constant * point.R, constant * point.Z);

    public static Point2D operator *(Point2D point, double constant)
       => new(point.R * constant, point.Z * constant);

    public static Point2D operator +(Point2D firstPoint, Point2D secondPoint)
        => new(firstPoint.R + secondPoint.R, firstPoint.Z + secondPoint.Z);

    public static Point2D operator -(Point2D firstPoint, Point2D secondPoint)
        => new(firstPoint.R - secondPoint.R, firstPoint.Z - secondPoint.Z);

    public static Point2D operator /(Point2D point, double constant)
        => new(point.R / constant, point.Z / constant);

    public override string ToString()
        => $"{R} {Z}";
}