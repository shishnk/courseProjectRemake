namespace courseProject;

public static class CubicBasis
{
    private static readonly Matrix _alphas;

    static CubicBasis()
    {
        _alphas = FEM.GetAlphas();
    }

    private static double L1(Point2D point)
        => _alphas[0, 0] + (_alphas[0, 1] * point.R) + (_alphas[0, 2] * point.Z);

    private static double L2(Point2D point)
        => _alphas[1, 0] + (_alphas[1, 1] * point.R) + (_alphas[1, 2] * point.Z);

    private static double L3(Point2D point)
        => _alphas[2, 0] + (_alphas[2, 1] * point.R) + (_alphas[2, 2] * point.Z);

    public static double Psi1(Point2D point)
    {
        double l1 = L1(point);
        return 0.5 * l1 * ((3 * l1) - 1) * ((3 * l1) - 2);
    }
    public static double Psi2(Point2D point)
    {
        double l2 = L2(point);
        return 0.5 * l2 * ((3 * l2) - 1) * ((3 * l2) - 2);
    }

    public static double Psi3(Point2D point)
    {
        double l3 = L3(point);
        return 0.5 * l3 * ((3 * l3) - 1) * ((3 * l3) - 2);
    }

    public static double Psi4(Point2D point)
    {
        double l1 = L1(point);
        double l2 = L2(point);
        return 4.5 * l1 * l2 * ((3 * l1) - 1);
    }

    public static double Psi5(Point2D point)
    {
        double l1 = L1(point);
        double l2 = L2(point);
        return 4.5 * l1 * l2 * ((3 * l2) - 1);
    }

    public static double Psi6(Point2D point)
    {
        double l2 = L2(point);
        double l3 = L3(point);
        return 4.5 * l2 * l3 * ((3 * l2) - 1);
    }

    public static double Psi7(Point2D point)
    {
        double l2 = L2(point);
        double l3 = L3(point);
        return 4.5 * l2 * l3 * ((3 * l3) - 1);
    }

    public static double Psi8(Point2D point)
    {
        double l1 = L1(point);
        double l3 = L3(point);
        return 4.5 * l1 * l3 * ((3 * l3) - 1);
    }

    public static double Psi9(Point2D point)
    {
        double l1 = L1(point);
        double l3 = L3(point);
        return 4.5 * l1 * l3 * ((3 * l1) - 1);
    }

    public static double Psi10(Point2D point)
    {
        double l1 = L1(point);
        double l2 = L2(point);
        double l3 = L3(point);
        return 27 * l1 * l2 * l3;
    }
}