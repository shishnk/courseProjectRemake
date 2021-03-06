namespace courseProject;

public class SparseMatrix
{
    // public fields - its bad, but the readability is better
    public int[] ig = default!;
    public int[] jg = default!;
    public double[] di = default!;
    public double[] gg = default!;
    public int Size { get; init; }

    public SparseMatrix(int size, int sizeOffDiag)
    {
        Size = size;
        ig = new int[size + 1];
        jg = new int[sizeOffDiag];
        gg = new double[sizeOffDiag];
        di = new double[size];
    }

    public static Vector<double> operator *(SparseMatrix matrix, Vector<double> vector)
    {
        Vector<double> product = new(vector.Length);

        for (int i = 0; i < vector.Length; i++)
        {
            product[i] = matrix.di[i] * vector[i];

            for (int j = matrix.ig[i]; j < matrix.ig[i + 1]; j++)
            {
                product[i] += matrix.gg[j] * vector[matrix.jg[j]];
                product[matrix.jg[j]] += matrix.gg[j] * vector[i];
            }
        }

        return product;
    }

    public void PrintDense(string path)
    {
        double[,] A = new double[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            A[i, i] = di[i];

            for (int j = ig[i]; j < ig[i + 1]; j++)
            {
                A[i, jg[j]] = gg[j];
                A[jg[j], i] = gg[j];
            }
        }

        using var sw = new StreamWriter(path);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                sw.Write(A[i, j].ToString("0.00") + "\t");
            }

            sw.WriteLine();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            di[i] = 0.0;

            for (int k = ig[i]; k < ig[i + 1]; k++)
            {
                gg[k] = 0.0;
            }
        }
    }
}