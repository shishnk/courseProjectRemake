namespace courseProject;

public static class ArrayHelper
{
    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static double Norm(this double[] array)
    {
        double result = 0.0;

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i] * array[i];
        }

        return Math.Sqrt(result);
    }

    public static void Copy<T>(this T[] source, T[] destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }
    }
}