namespace courseProject;

public class FEM
{
    public class FEMBuilder
    {
        private readonly FEM _fem = new();

        public FEMBuilder SetTest(ITest test)
        {
            _fem._test = test;
            return this;
        }

        public FEMBuilder SetSpaceGrid(ISpaceGrid spaceGrid)
        {
            _fem._spaceGrid = spaceGrid;
            return this;
        }

        public FEMBuilder SetTimeGrid(ITimeGrid grid)
        {
            _fem._timeGrid = grid;
            return this;
        }

        public FEMBuilder SetSolverSLAE(Solver solver)
        {
            _fem._solver = solver;
            return this;
        }

        public FEMBuilder SetDiriclhetBoundaries(DirichletBoundary[] boundaries)
        {
            _fem._dirichletBoundaries = boundaries;
            return this;
        }

        public FEMBuilder SetNeumannBoundaries(NeumannBoundary[] boundaries)
        {
            _fem._neumannBoundaries = boundaries;
            return this;
        }

        public FEMBuilder IsPhysical(bool flag)
        {
            _fem.IsPhysical = flag;
            return this;
        }

        public static implicit operator FEM(FEMBuilder builder)
            => builder._fem;
    }

    // default ~ cannot be null
    private delegate double Basis(Point2D point);

    private Basis[] _basis = default!;
    private ISpaceGrid _spaceGrid = default!;
    private ITimeGrid _timeGrid = default!;
    private Point2D[] _vertices = default!;
    private ITest _test = default!;
    private Solver _solver = default!;
    private DirichletBoundary[] _dirichletBoundaries = default!;
    private NeumannBoundary[]? _neumannBoundaries;
    private Matrix _stiffnessMatrix = default!;
    private Matrix _massMatrix = default!;
    private SparseMatrix _globalMatrix = default!;
    private Vector<double> _vector = default!;
    private static Matrix _alphas = default!; // коэффициенты альфа, костыль :(
    private double[][][] _M = default!;
    private double[][][][] _G = default!;
    private double[][] _layers = default!;
    private Matrix _massMatrixCopy = default!;
    public bool IsPhysical { get; private set; }

    public void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_test, $"{nameof(_test)} cannot be null, set the test");
            ArgumentNullException.ThrowIfNull(_solver, $"{nameof(_solver)} cannot be null, set the method of solving SLAE");
            ArgumentNullException.ThrowIfNull(_dirichletBoundaries, $"{nameof(_dirichletBoundaries)} cannot be null, set the Dirichlet boundaries");

            Init();
            ConstructPortrait();
            Prepare();
            Solve();
            // Err();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }

    private void Init()
    {
        _basis = new Basis[] { CubicBasis.Psi1, CubicBasis.Psi2, CubicBasis.Psi3, CubicBasis.Psi4,
        CubicBasis.Psi5, CubicBasis.Psi6, CubicBasis.Psi7, CubicBasis.Psi8, CubicBasis.Psi9, CubicBasis.Psi10};

        _stiffnessMatrix = new(10);
        _massMatrix = new(10);
        _massMatrixCopy = new(10);
        _alphas = new(3);

        _vertices = new Point2D[3];

        _layers = new double[3].Select(_ => new double[_spaceGrid.Points.Count]).ToArray();

        _M = new double[10].Select(_ => new double[10].ToArray().
                         Select(_ => new double[3]).ToArray()).ToArray();

        _G = new double[10].Select(_ => new double[10].ToArray().
                        Select(_ => new double[6].ToArray().
                        Select(_ => new double[3]).ToArray()).ToArray()).ToArray();

        using StreamReader sr1 = new("input/Grz.txt"), sr2 = new("input/Mrz.txt");
        string[] vars;

        for (int i = 0; i < 600; i++)
        {
            vars = sr1.ReadLine()!.Split(" ").ToArray();

            _G[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][0] = double.Parse(vars[3]);
            _G[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][1] = double.Parse(vars[4]);
            _G[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][2] = double.Parse(vars[5]);
        }

        for (int i = 0; i < 100; i++)
        {
            vars = sr2.ReadLine()!.Split(" ").ToArray();

            _M[int.Parse(vars[0])][int.Parse(vars[1])][0] = double.Parse(vars[2]);
            _M[int.Parse(vars[0])][int.Parse(vars[1])][1] = double.Parse(vars[3]);
            _M[int.Parse(vars[0])][int.Parse(vars[1])][2] = double.Parse(vars[4]);
        }
    }

    private void Prepare()
    {
        if (!IsPhysical)
        {
            for (int i = 0; i < _spaceGrid.Points.Count; i++)
            {
                _layers[0][i] = _test.U(_spaceGrid.Points[i], _timeGrid.Points[0]);
                _layers[1][i] = _test.U(_spaceGrid.Points[i], _timeGrid.Points[1]);
                _layers[2][i] = _test.U(_spaceGrid.Points[i], _timeGrid.Points[2]);
            }
        }
        else
        {
            for (int i = 0; i < _spaceGrid.Points.Count; i++)
            {
                _layers[0][i] = _test.U(_spaceGrid.Points[i], _timeGrid.Points[0]);
            }

            double t01 = _timeGrid.Points[1] - _timeGrid.Points[0];

            AssemblyGlobalMatrix(1, t01: t01);

            if (_neumannBoundaries is not null)
            {
                AccountingNeumannBoundaries();
            }

            AccountingDirichletBoundaries(1);

            _solver.SetMatrix(_globalMatrix);
            _solver.SetVector(_vector);
            _solver.Compute();
            Err(1);

            _solver.Solution!.Value.CopyTo(0, _layers[1], 0, _layers[1].Length);

            _globalMatrix.Clear();
            _vector.Fill(0);

            double t02 = _timeGrid.Points[2] - _timeGrid.Points[0];
            double t12 = _timeGrid.Points[1] - _timeGrid.Points[0];
            t01 = _timeGrid.Points[2] - _timeGrid.Points[1];

            AssemblyGlobalMatrix(2, t02: t02, t01: t01, t12: t12);

            if (_neumannBoundaries is not null)
            {
                AccountingNeumannBoundaries();
            }

            AccountingDirichletBoundaries(2);

            _solver.SetMatrix(_globalMatrix);
            _solver.SetVector(_vector);
            _solver.Compute();
            Err(2);

            _solver.Solution!.Value.CopyTo(0, _layers[2], 0, _layers[2].Length);

            _globalMatrix.Clear();
            _vector.Fill(0);
        }
    }

    private void Solve()
    {
        for (int itime = 3; itime < 4; itime++)
        {
            double t03 = _timeGrid.Points[itime] - _timeGrid.Points[itime - 3];
            double t02 = _timeGrid.Points[itime] - _timeGrid.Points[itime - 2];
            double t01 = _timeGrid.Points[itime] - _timeGrid.Points[itime - 1];
            double t13 = _timeGrid.Points[itime - 1] - _timeGrid.Points[itime - 3];
            double t12 = _timeGrid.Points[itime - 1] - _timeGrid.Points[itime - 2];
            double t23 = _timeGrid.Points[itime - 2] - _timeGrid.Points[itime - 3];

            AssemblyGlobalMatrix(itime, t03, t02, t01, t13, t12, t23);

            if (_neumannBoundaries is not null)
            {
                AccountingNeumannBoundaries();
            }

            AccountingDirichletBoundaries(itime);
            _globalMatrix.PrintDense("results/matrix.txt");

            _solver.SetMatrix(_globalMatrix);
            _solver.SetVector(_vector);
            _solver.Compute();
            Err(itime);

            _layers[1].Copy(_layers[0]);
            _layers[2].Copy(_layers[1]);
            _solver.Solution!.Value.CopyTo(0, _layers[2], 0, _layers[2].Length);

            _vector.Fill(0);
            _globalMatrix.Clear();
        }
    }

    private void ConstructPortrait()
    {
        List<int>[] list = new List<int>[_spaceGrid.Points.Count].Select(_ => new List<int>()).ToArray();

        foreach (var element in _spaceGrid.Elements.Select(array => array.OrderBy(value => value).ToArray()).ToArray())
        {
            for (int i = 0; i < element.Length; i++)
            {
                for (int j = i + 1; j < element.Length; j++)
                {
                    int pos = element[j];
                    int elem = element[i];

                    if (!list[pos].Contains(elem))
                    {
                        list[pos].Add(elem);
                    }
                }
            }
        }

        list = list.Select(list => list.OrderBy(value => value).ToList()).ToArray();
        int count = list.Sum(childList => childList.Count);

        InitSLAE(count);

        _globalMatrix.ig[0] = 0;

        for (int i = 0; i < list.Length; i++)
            _globalMatrix.ig[i + 1] = _globalMatrix.ig[i] + list[i].Count;

        int k = 0;

        for (int i = 0; i < list.Length; i++)
        {
            for (int j = 0; j < list[i].Count; j++)
            {
                _globalMatrix.jg[k] = list[i][j];
                k++;
            }
        }
    }

    private void InitSLAE(int count)
    {
        _globalMatrix = new(_spaceGrid.Points.Count, count);
        _vector = new(_spaceGrid.Points.Count);
    }

    private void AddElement(int i, int j, double value)
    {
        if (i == j)
        {
            _globalMatrix.di[i] += value;
            return;
        }

        if (i > j)
        {
            for (int index = _globalMatrix.ig[i]; index < _globalMatrix.ig[i + 1]; index++)
            {
                if (_globalMatrix.jg[index] == j)
                {
                    _globalMatrix.gg[index] += value;
                    return;
                }
            }
        }
    }

    private void AssemblyGlobalMatrix(int itime = 1, double t03 = 1, double t02 = 1, double t01 = 1, double t13 = 1, double t12 = 1, double t23 = 1)
    {
        for (int ielem = 0; ielem < _spaceGrid.Elements.Length; ielem++)
        {
            _vertices[0] = _spaceGrid.Points[_spaceGrid.Elements[ielem][0]];
            _vertices[1] = _spaceGrid.Points[_spaceGrid.Elements[ielem][1]];
            _vertices[2] = _spaceGrid.Points[_spaceGrid.Elements[ielem][2]];

            AssemblyLocalMatrices(itime, t03, t02, t01);

            _stiffnessMatrix += _massMatrix;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    AddElement(_spaceGrid.Elements[ielem][i], _spaceGrid.Elements[ielem][j], _stiffnessMatrix[i, j]);
                }
            }

            AssemblyGlobalVector(itime, ielem, t03, t02, t01, t13, t12, t23);

            _stiffnessMatrix.Clear();
            _massMatrix.Clear();
        }
    }

    private void AssemblyGlobalVector(int itime = 1, int ielem = 1, double t03 = 1, double t02 = 1, double t01 = 1, double t13 = 1, double t12 = 1, double t23 = 1)
    {
        double[] qj1 = new double[10];
        double[] qj2 = new double[10];
        double[] qj3 = new double[10];

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (itime == 1)
                {
                    qj1[i] += _massMatrixCopy[i, j] * _layers[0][_spaceGrid.Elements[ielem][j]];
                }
                else if (itime == 2)
                {
                    qj2[i] += _massMatrixCopy[i, j] * _layers[0][_spaceGrid.Elements[ielem][j]];
                    qj1[i] += _massMatrixCopy[i, j] * _layers[1][_spaceGrid.Elements[ielem][j]];
                }
                else
                {
                    qj3[i] += _massMatrixCopy[i, j] * _layers[0][_spaceGrid.Elements[ielem][j]];
                    qj2[i] += _massMatrixCopy[i, j] * _layers[1][_spaceGrid.Elements[ielem][j]];
                    qj1[i] += _massMatrixCopy[i, j] * _layers[2][_spaceGrid.Elements[ielem][j]];
                }
            }
        }

        for (int j = 0; j < 10; j++)
        {
            if (itime == 1)
            {
                _vector[_spaceGrid.Elements[ielem][j]] += Integration.Triangle(_test.F, _basis[j].Invoke, _vertices, _timeGrid.Points[2]) -
                                                      (-1.0 / t01 * qj1[j]);
            }
            else if (itime == 2)
            {
                _vector[_spaceGrid.Elements[ielem][j]] += Integration.Triangle(_test.F, _basis[j].Invoke, _vertices, _timeGrid.Points[2]) -
                                          (t01 / (t02 * t12) * qj2[j]) + (t02 / (t12 * t01) * qj1[j]);
            }
            else
            {
                _vector[_spaceGrid.Elements[ielem][j]] += Integration.Triangle(_test.F, _basis[j].Invoke, _vertices, _timeGrid.Points[itime]) +
                                                          (t02 * t01 / (t23 * t13 * t03) * qj3[j]) -
                                                          (t03 * t01 / (t23 * t12 * t02) * qj2[j]) +
                                                          (t03 * t02 / (t13 * t12 * t01) * qj1[j]);
            }
        }
    }

    private double DeterminantD()
        => ((_vertices[1].R - _vertices[0].R) * (_vertices[2].Z - _vertices[0].Z)) -
           ((_vertices[2].R - _vertices[0].R) * (_vertices[1].Z - _vertices[0].Z));

    private void CalcAlphas()
    {
        double dD = DeterminantD();

        _alphas[0, 0] = ((_vertices[1].R * _vertices[2].Z) - (_vertices[2].R * _vertices[1].Z)) / dD;
        _alphas[0, 1] = (_vertices[1].Z - _vertices[2].Z) / dD;
        _alphas[0, 2] = (_vertices[2].R - _vertices[1].R) / dD;

        _alphas[1, 0] = ((_vertices[2].R * _vertices[0].Z) - (_vertices[0].R * _vertices[2].Z)) / dD;
        _alphas[1, 1] = (_vertices[2].Z - _vertices[0].Z) / dD;
        _alphas[1, 2] = (_vertices[0].R - _vertices[2].R) / dD;

        _alphas[2, 0] = ((_vertices[0].R * _vertices[1].Z) - (_vertices[1].R * _vertices[0].Z)) / dD;
        _alphas[2, 1] = (_vertices[0].Z - _vertices[1].Z) / dD;
        _alphas[2, 2] = (_vertices[1].R - _vertices[0].R) / dD;
    }

    private void AssemblyLocalMatrices(int itime, double t03, double t02, double t01)
    {
        double dD = Math.Abs(DeterminantD());
        CalcAlphas();

        //rs=[dl1^2 dl1dl2 dl1dl3 dl2^2 dl2dl3 dl3dl3]
        //каждая из 6 пар дифференциалов состоит из 3 составляющих (т.к. вектор r=r1L1+r2L2+r3L3)
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                _stiffnessMatrix[i, j] += _G[i][j][0][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[0, 1]) + (_alphas[0, 2] * _alphas[0, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][0][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[0, 1]) + (_alphas[0, 2] * _alphas[0, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][0][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[0, 1]) + (_alphas[0, 2] * _alphas[0, 2])) * dD;

                _stiffnessMatrix[i, j] += _G[i][j][1][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[1, 1]) + (_alphas[0, 2] * _alphas[1, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][1][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[1, 1]) + (_alphas[0, 2] * _alphas[1, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][1][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[1, 1]) + (_alphas[0, 2] * _alphas[1, 2])) * dD;

                _stiffnessMatrix[i, j] += _G[i][j][2][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[2, 1]) + (_alphas[0, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][2][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[2, 1]) + (_alphas[0, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][2][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[0, 1] * _alphas[2, 1]) + (_alphas[0, 2] * _alphas[2, 2])) * dD;

                _stiffnessMatrix[i, j] += _G[i][j][3][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[1, 1]) + (_alphas[1, 2] * _alphas[1, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][3][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[1, 1]) + (_alphas[1, 2] * _alphas[1, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][3][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[1, 1]) + (_alphas[1, 2] * _alphas[1, 2])) * dD;

                _stiffnessMatrix[i, j] += _G[i][j][4][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[2, 1]) + (_alphas[1, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][4][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[2, 1]) + (_alphas[1, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][4][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[1, 1] * _alphas[2, 1]) + (_alphas[1, 2] * _alphas[2, 2])) * dD;

                _stiffnessMatrix[i, j] += _G[i][j][5][0] * _vertices[0].R * _spaceGrid.Lambda * ((_alphas[2, 1] * _alphas[2, 1]) + (_alphas[2, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][5][1] * _vertices[1].R * _spaceGrid.Lambda * ((_alphas[2, 1] * _alphas[2, 1]) + (_alphas[2, 2] * _alphas[2, 2])) * dD;
                _stiffnessMatrix[i, j] += _G[i][j][5][2] * _vertices[2].R * _spaceGrid.Lambda * ((_alphas[2, 1] * _alphas[2, 1]) + (_alphas[2, 2] * _alphas[2, 2])) * dD;
            }
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    _massMatrix[i, j] += _M[i][j][k] * _vertices[k].R * _spaceGrid.Sigma * dD;
                }
            }
        }

        _massMatrix.Copy(_massMatrixCopy);

        if (itime == 1)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    _massMatrix[i, j] *= 1.0 / t01;
                }
            }
        }
        else if (itime == 2)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    _massMatrix[i, j] *= (t02 + t01) / (t02 * t01);
                }
            }
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    _massMatrix[i, j] *= (1.0 / t03) + (1.0 / t02) + (1.0 / t01);
                }
            }
        }
    }

    private void AccountingDirichletBoundaries(int itime)
    {
        (int Node, double Value)[] boundaries = new (int, double)[4 * _dirichletBoundaries.Length];
        int[] checkBC = new int[_spaceGrid.Points.Count];

        int index = 0;

        for (int i = 0; i < _dirichletBoundaries.Length; i++)
        {
            int ielem = _dirichletBoundaries[i].Element;
            int edge = _dirichletBoundaries[i].Edge;

            for (int j = 0; j < 4; j++)
            {
                boundaries[index++] = (_spaceGrid.Edges[ielem][edge][j],
                                      _test.U(_spaceGrid.Points[_spaceGrid.Edges[ielem][edge][j]], _timeGrid.Points[itime]));
            }
        }

        boundaries = boundaries.Distinct().OrderBy(boundary => boundary.Node).ToArray();
        checkBC.Fill(-1);

        for (int i = 0; i < boundaries.Length; i++)
            checkBC[boundaries[i].Node] = i;

        for (int i = 0; i < _spaceGrid.Points.Count; i++)
        {
            if (checkBC[i] != -1)
            {
                _globalMatrix.di[i] = 1;
                _vector[i] = boundaries[checkBC[i]].Value;

                for (int k = _globalMatrix.ig[i]; k < _globalMatrix.ig[i + 1]; k++)
                {
                    index = _globalMatrix.jg[k];

                    if (checkBC[index] == -1)
                        _vector[index] -= _globalMatrix.gg[k] * _vector[i];

                    _globalMatrix.gg[k] = 0;
                }
            }
            else
            {
                for (int k = _globalMatrix.ig[i]; k < _globalMatrix.ig[i + 1]; k++)
                {
                    index = _globalMatrix.jg[k];

                    if (checkBC[index] != -1)
                    {
                        _vector[i] -= _globalMatrix.gg[k] * _vector[index];
                        _globalMatrix.gg[k] = 0;
                    }
                }
            }
        }
    }

    private void AccountingNeumannBoundaries()
    {
        Vector<double> localVector = new(10);
        int[] localEdge = new int[4];

        for (int i = 0; i < _neumannBoundaries!.Length; i++)
        {
            int ielem = _neumannBoundaries[i].Element;

            _vertices[0] = _spaceGrid.Points[_spaceGrid.Elements[ielem][0]];
            _vertices[1] = _spaceGrid.Points[_spaceGrid.Elements[ielem][1]];
            _vertices[2] = _spaceGrid.Points[_spaceGrid.Elements[ielem][2]];

            CalcAlphas();

            // second else if you want to enter a single element

            if (_neumannBoundaries[i].Edge == 0)
            {
                localEdge[0] = 0;
                localEdge[1] = 8;
                localEdge[2] = 7;
                localEdge[3] = 2;
            }
            else if (_neumannBoundaries[i].Edge == 1)
            {
                localEdge[0] = 0;
                localEdge[1] = 3;
                localEdge[2] = 4;
                localEdge[3] = 1;
            }
            else
            {
                localEdge[0] = 1;
                localEdge[1] = 5;
                localEdge[2] = 6;
                localEdge[3] = 2;
            }

            for (int j = 0; j < 4; j++)
            {
                localVector[localEdge[j]] = _spaceGrid.Lambda * _neumannBoundaries[i].Value *
                                  Integration.GaussSegment(_basis[localEdge[j]].Invoke,
                                  _spaceGrid.Points[_spaceGrid.Edges[ielem][_neumannBoundaries[i].Edge][0]],
                                  _spaceGrid.Points[_spaceGrid.Edges[ielem][_neumannBoundaries[i].Edge][3]]);
            }

            for (int j = 0; j < 10; j++)
            {
                _vector[_spaceGrid.Elements[ielem][j]] += localVector[j];
            }

            localVector.Fill(0);
        }
    }

    public double[] ValueAtPoint(Point2D[] points)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_solver.Solution, $"{nameof(_solver.Solution)} cannot be null, solve problem first");

            double[] result = new double[points.Length];

            for (int ipoint = 0; ipoint < points.Length; ipoint++)
            {
                int ielem = FindElement(points[ipoint]);
                double determinant = DeterminantD();
                CalcAlphas();

                for (int i = 0; i < 10; i++)
                {
                    result[ipoint] += _solver.Solution!.Value[_spaceGrid.Elements[ielem][i]] *
                                      _basis[i](points[ipoint]);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return Array.Empty<double>();
        }
    }

    private int FindElement(Point2D point)
    {
        for (int ielem = 0; ielem < _spaceGrid.Elements.Length; ielem++)
        {
            _vertices[0] = _spaceGrid.Points[_spaceGrid.Elements[ielem][0]];
            _vertices[1] = _spaceGrid.Points[_spaceGrid.Elements[ielem][1]];
            _vertices[2] = _spaceGrid.Points[_spaceGrid.Elements[ielem][2]];

            double a = ((_vertices[0].R - point.R) * (_vertices[1].Z - _vertices[0].Z)) -
                     ((_vertices[1].R - _vertices[0].R) * (_vertices[0].Z - point.Z));

            double b = ((_vertices[1].R - point.R) * (_vertices[2].Z - _vertices[1].Z)) -
                     (_vertices[2].R - (_vertices[1].R * (_vertices[1].Z - point.Z)));

            double c = ((_vertices[2].R - point.R) * (_vertices[0].Z - _vertices[2].Z)) -
                     ((_vertices[0].R - _vertices[2].R) * (_vertices[2].Z - point.Z));

            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
                return ielem;
        }

        throw new Exception("The point does not belong to the grid");
    }

    public void WriteToFile(string path)
    {
        using var sw = new StreamWriter(path);
        for (int i = 0; i < _solver.Solution!.Value.Length; i++)
        {
            sw.WriteLine(i + ") " + _solver.Solution!.Value[i]);
        }
    }

    // for report
    private void Err(int itime)
    {
        double[] err = new double[_spaceGrid.Points.Count];
        double[] exact = new double[_spaceGrid.Points.Count];
        double sum = 0.0;

        using StreamWriter sw1 = new("results/err.txt"),
                           sw2 = new("results/Exact.txt"),
                           sw3 = new("results/rms.txt");
                           //sw4 = new("csv2/2.csv", true);

        for (int i = 0; i < _spaceGrid.Points.Count; i++)
        {
            err[i] = Math.Abs(_solver.Solution!.Value[i] - _test.U(_spaceGrid.Points[i], _timeGrid.Points[itime]));
            sw1.WriteLine(err[i]);
        }

        for (int i = 0; i < _spaceGrid.Points.Count; i++)
        {
            exact[i] = _test.U(_spaceGrid.Points[i], _timeGrid.Points[itime]);
            sw2.WriteLine(exact[i]);
        }

        for (int i = 0; i < _spaceGrid.Points.Count; i++)
        {
            sum += err[i] * err[i];
        }

        // sum = Math.Sqrt(sum); // относительная погрешность
        // sum /= exact.Norm();

        sum = Math.Sqrt(sum / _spaceGrid.Points.Count); // среднеквадратичное отклонение
        sw3.Write(sum);

        // sw4.WriteLine("$r $z, Точное, Численное, Вектор погрешности, Погрешность");

        // for (int i = 0; i < _spaceGrid.Points.Count; i++) {
        //     if (i == 0) {
        //         sw4.WriteLine($"{_spaceGrid.Points[i]},{exact[i]},{_solver.Solution!.Value[i]},{err[i].ToString(err[i] == 0 ? "0" : "0.00E+0")},{sum:0.00E+0}");
        //     }

        //     sw4.WriteLine($"{_spaceGrid.Points[i]},{exact[i]},{_solver.Solution!.Value[i]},{err[i].ToString(err[i] == 0 ? "0" : "0.00E+0")},");
        // }

        //if (itime == 1)
        //{
        //    sw4.WriteLine("$t_i$, Погрешность");
        //}

        //sw4.WriteLine($"{_timeGrid.Points[itime]:0.0000}, {sum:0.00E+0}");
    }

    public static FEMBuilder CreateBuilder()
            => new();

    // костыль
    public static Matrix GetAlphas()
        => _alphas;
}