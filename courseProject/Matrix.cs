namespace courseProject;

public class Matrix
{
    private readonly double[,] storage;
    public int Size { get; init; }

    public double this[int i, int j]
    {
        get => storage[i, j];
        set => storage[i, j] = value;
    }

    public Matrix(int size)
    {
        storage = new double[size, size];
        Size = size;
    }

    public void Clear()
        => Array.Clear(storage, 0, storage.Length);

    public void Copy(Matrix destination)
    {
        for (int i = 0; i < destination.Size; i++)
        {
            for (int j = 0; j < destination.Size; j++)
            {
                destination[i, j] = storage[i, j];
            }
        }
    }

    public static Matrix operator +(Matrix fstMatrix, Matrix sndMatrix)
    {
        Matrix resultMatrix = new(fstMatrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = fstMatrix[i, j] + sndMatrix[i, j];
            }
        }

        return resultMatrix;
    }
}