namespace courseProject;

public abstract class Solver
{
    protected SparseMatrix _matrix = default!;
    protected Vector<double> _vector = default!;
    protected Vector<double>? _solution;
    public int MaxIters { get; init; }
    public double Eps { get; init; }
    public ImmutableArray<double>? Solution => _solution?.ToImmutableArray();

    protected Solver(int maxIters, double eps)
        => (MaxIters, Eps) = (maxIters, eps);

    public void SetMatrix(SparseMatrix matrix)
        => _matrix = matrix;

    public void SetVector(Vector<double> vector)
        => _vector = vector;

    public abstract void Compute();

    protected void Cholesky(double[] ggnew, double[] dinew)
    {
        double suml = 0.0;
        double sumdi = 0.0;

        for (int i = 0; i < _matrix.Size; i++)
        {
            int i0 = _matrix.ig[i];
            int i1 = _matrix.ig[i + 1];

            for (int k = i0; k < i1; k++)
            {
                int j = _matrix.jg[k];
                int j0 = _matrix.ig[j];
                int j1 = _matrix.ig[j + 1];
                int ik = i0;
                int kj = j0;

                while (ik < k && kj < j1)
                {
                    if (_matrix.jg[ik] == _matrix.jg[kj])
                    {
                        suml += ggnew[ik] * ggnew[kj];
                        ik++;
                        kj++;
                    }
                    else
                    {
                        if (_matrix.jg[ik] > _matrix.jg[kj])
                            kj++;
                        else
                            ik++;
                    }
                }

                ggnew[k] = (ggnew[k] - suml) / dinew[j];
                sumdi += ggnew[k] * ggnew[k];
                suml = 0.0;
            }

            dinew[i] = Math.Sqrt(dinew[i] - sumdi);
            sumdi = 0.0;
        }
    }

    protected Vector<double> MoveForCholesky(Vector<double> vector, double[] ggnew, double[] dinew)
    {
        Vector<double> y = new(vector.Length);
        Vector<double> x = new(vector.Length);
        Vector<double>.Copy(vector, y);

        double sum = 0.0;

        for (int i = 0; i < _matrix.Size; i++) // Прямой ход
        {
            int i0 = _matrix.ig[i];
            int i1 = _matrix.ig[i + 1];

            for (int k = i0; k < i1; k++)
                sum += ggnew[k] * y[_matrix.jg[k]];

            y[i] = (y[i] - sum) / dinew[i];
            sum = 0.0;
        }

        Vector<double>.Copy(y, x);

        for (int i = _matrix.Size - 1; i >= 0; i--) // Обратный ход
        {
            int i0 = _matrix.ig[i];
            int i1 = _matrix.ig[i + 1];
            x[i] = y[i] / dinew[i];

            for (int k = i0; k < i1; k++)
                y[_matrix.jg[k]] -= ggnew[k] * x[i];
        }

        return x;
    }
}

public class LOS : Solver
{
    public LOS(int maxIters, double eps) : base(maxIters, eps) { }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            double alpha, beta;
            double squareNorm;

            _solution = new(_vector.Length);

            Vector<double> r = new(_vector.Length);
            Vector<double> z = new(_vector.Length);
            Vector<double> p = new(_vector.Length);
            Vector<double> tmp = new(_vector.Length);

            r = _vector - (_matrix * _solution);

            Vector<double>.Copy(r, z);

            p = _matrix * z;

            squareNorm = r * r;

            for (int iter = 0; iter < MaxIters && squareNorm > Eps; iter++)
            {
                alpha = p * r / (p * p);
                _solution += alpha * z;
                squareNorm = (r * r) - (alpha * alpha * (p * p));
                r -= alpha * p;

                tmp = _matrix * r;

                beta = -(p * tmp) / (p * p);
                z = r + (beta * z);
                p = tmp + (beta * p);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}

public class CGM : Solver
{
    public CGM(int maxIters, double eps) : base(maxIters, eps) { }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            double alpha, beta;
            double norm, squareNorm;
            double vectorNorm = _vector.Norm();

            _solution = new(_vector.Length);

            Vector<double> r = new(_vector.Length);
            Vector<double> z = new(_vector.Length);
            Vector<double> p = new(_vector.Length);
            Vector<double> tmp = new(_vector.Length);

            r = _vector - (_matrix * _solution);

            Vector<double>.Copy(r, z);

            for (int iter = 0; iter < MaxIters && (norm = r.Norm() / vectorNorm) >= Eps; iter++)
            {
                tmp = _matrix * z;
                alpha = r * r / (tmp * z);
                _solution += alpha * z;
                squareNorm = r * r;
                r -= alpha * tmp;
                beta = r * r / squareNorm;
                z = r + (beta * z);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}

public class CGMCholesky : Solver
{
    public CGMCholesky(int maxIters, double eps) : base(maxIters, eps) { }

    public override void Compute()
    {
        try
        {
            double alpha, beta;
            double tmp;

            double vectorNorm = _vector.Norm();

            _solution = new(_vector.Length);

            double[] ggnew = new double[_matrix.gg.Length];
            double[] dinew = new double[_matrix.di.Length];

            _matrix.gg.Copy(ggnew);
            _matrix.di.Copy(dinew);

            Vector<double> r = new(_vector.Length);
            Vector<double> z = new(_vector.Length);
            Vector<double> fstTemp = new(_vector.Length);
            Vector<double> sndTemp = new(_vector.Length);

            Cholesky(ggnew, dinew);

            r = _vector - (_matrix * _solution);
            z = MoveForCholesky(r, ggnew, dinew);

            for (int iter = 0; iter < MaxIters && r.Norm() / vectorNorm >= Eps; iter++)
            {
                tmp = MoveForCholesky(r, ggnew, dinew) * r;
                sndTemp = _matrix * z;
                alpha = tmp / (sndTemp * z);
                _solution += alpha * z;
                r -= alpha * sndTemp;
                fstTemp = MoveForCholesky(r, ggnew, dinew);
                beta = fstTemp * r / tmp;
                z = fstTemp + (beta * z);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}