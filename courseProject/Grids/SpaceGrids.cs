namespace courseProject;

public class SpaceRegularGrid : ISpaceGrid
{
    private readonly List<Point2D> _points = default!;
    private readonly int[][] _elements = default!;
    private readonly int[][][] _edges = default!;
    public ImmutableList<Point2D> Points => _points.ToImmutableList();
    public ImmutableArray<ImmutableArray<int>> Elements => _elements.Select(item => item.ToImmutableArray()).ToImmutableArray();
    public ImmutableArray<ImmutableArray<ImmutableArray<int>>> Edges => _edges.Select(item => item.ToImmutableArray().Select(item => item.ToImmutableArray()).ToImmutableArray()).ToImmutableArray();
    public double Lambda { get; init; }
    public double Sigma { get; init; }

    public SpaceRegularGrid(SpaceGridParameters gridParameters)
    {
        Lambda = gridParameters.Lambda;
        Sigma = gridParameters.Sigma;
        _points = new();
        _elements = new int[2 * gridParameters.SplitsR * gridParameters.SplitsZ].Select(_ => new int[10]).ToArray();
        _edges = new int[2 * gridParameters.SplitsR * gridParameters.SplitsZ].Select(_ => new int[3].ToArray().Select(_ => new int[4]).ToArray()).ToArray();
        Build(gridParameters);
    }

    private void Build(SpaceGridParameters gridParameters)
    {
        try
        {
            if (gridParameters.SplitsR < 1 || gridParameters.SplitsZ < 1)
            {
                throw new Exception("The number of splits must be greater than or equal to 1");
            }

            double hr = gridParameters.IntervalR.Lenght / gridParameters.SplitsR;
            double hz = gridParameters.IntervalZ.Lenght / gridParameters.SplitsZ;

            double[] pointsR = new double[(3 * gridParameters.SplitsR) + 1];
            double[] pointsZ = new double[(3 * gridParameters.SplitsZ) + 1];

            pointsR[0] = gridParameters.IntervalR.LeftBorder;
            pointsZ[0] = gridParameters.IntervalZ.LeftBorder;

            for (int i = 1; i < gridParameters.SplitsR + 1; i++)
            {
                pointsR[i] = pointsR[i - 1] + hr;
            }

            for (int i = 1; i < gridParameters.SplitsZ + 1; i++)
            {
                pointsZ[i] = pointsZ[i - 1] + hz;
            }

            for (int j = 0; j < gridParameters.SplitsZ + 1; j++)
            {
                for (int i = 0; i < gridParameters.SplitsR + 1; i++)
                {
                    _points.Add(new(pointsR[i], pointsZ[j]));
                }
            }

            Point2D[] allNodes = new Point2D[10];
            Point2D[] vertices = new Point2D[3];

            const int dummySplits = 2;
            int nr = gridParameters.SplitsR + 1;
            int index = 0;

            for (int j = 0; j < gridParameters.SplitsZ; j++)
            {
                for (int i = 0; i < gridParameters.SplitsR; i++)
                {
                    _elements[index][0] = i + (j * nr);
                    _elements[index][1] = i + 1 + (j * nr);
                    _elements[index++][2] = i + ((j + 1) * nr);

                    _elements[index][0] = i + 1 + (j * nr);
                    _elements[index][1] = i + nr + (j * nr);
                    _elements[index++][2] = i + 1 + nr + (j * nr);
                }
            }

            WritePoints("forGraphics/pointsLinear.txt");
            WriteElements("forGraphics/elements.txt");
            pointsR.Fill(-1.0);
            pointsZ.Fill(-1.0);

            for (int ielem = 0; ielem < _elements.Length; ielem++)
            {
                vertices[0] = _points[_elements[ielem][0]];
                vertices[1] = _points[_elements[ielem][1]];
                vertices[2] = _points[_elements[ielem][2]];

                GetNodes(allNodes, vertices);

                pointsR = pointsR.Concat(allNodes.Select(point => point.R)).ToArray();
                pointsR = pointsR.Distinct().OrderBy(value => value).ToArray();

                pointsZ = pointsZ.Concat(allNodes.Select(point => point.Z)).ToArray();
                pointsZ = pointsZ.Distinct().OrderBy(value => value).ToArray();
            }

            _points.Clear();

            foreach (var pointZ in pointsZ.Skip(1))
            {
                foreach (var pointR in pointsR.Skip(1))
                {
                    _points.Add(new(pointR, pointZ));
                }
            }

            nr = pointsR.Length - 1;
            index = 0;

            for (int j = 0; j < gridParameters.SplitsZ; j++)
            {
                for (int i = 0; i < gridParameters.SplitsR; i++)
                {
                    _elements[index][0] = i + (3 * j * nr) + (i * dummySplits); // 0
                    _elements[index][1] = i + (3 * nr) + (3 * j * nr) + (i * dummySplits); // 12
                    _elements[index][2] = i + 3 + (3 * j * nr) + (i * dummySplits); // 3
                    _elements[index][3] = i + nr + (3 * j * nr) + (i * dummySplits); // 4
                    _elements[index][4] = i + (2 * nr) + (3 * j * nr) + (i * dummySplits); // 8
                    _elements[index][5] = i + 1 + (2 * nr) + (3 * j * nr) + (i * dummySplits); //9
                    _elements[index][6] = i + nr + 2 + (3 * j * nr) + (i * dummySplits); // 6
                    _elements[index][7] = i + 2 + (3 * j * nr) + (i * dummySplits); // 2
                    _elements[index][8] = i + 1 + (3 * j * nr) + (i * dummySplits); // 1
                    _elements[index][9] = i + nr + 1 + (3 * j * nr) + (i * dummySplits); // 5

                    _edges[index][0][0] = _elements[index][0];
                    _edges[index][0][1] = _elements[index][8];
                    _edges[index][0][2] = _elements[index][7];
                    _edges[index][0][3] = _elements[index][2];

                    _edges[index][1][0] = _elements[index][0];
                    _edges[index][1][1] = _elements[index][3];
                    _edges[index][1][2] = _elements[index][4];
                    _edges[index][1][3] = _elements[index][1];

                    _edges[index][2][0] = _elements[index][2];
                    _edges[index][2][1] = _elements[index][6];
                    _edges[index][2][2] = _elements[index][5];
                    _edges[index][2][3] = _elements[index++][1];

                    _elements[index][0] = i + (3 * nr) + (3 * j * nr) + (i * dummySplits); // 12
                    _elements[index][1] = i + 3 + (3 * j * nr) + (i * dummySplits); //3
                    _elements[index][2] = i + 3 + (3 * nr) + (3 * j * nr) + (i * dummySplits); // 15
                    _elements[index][3] = i + 1 + (2 * nr) + (3 * j * nr) + (i * dummySplits); // 9
                    _elements[index][4] = i + nr + 2 + (3 * j * nr) + (i * dummySplits); // 6
                    _elements[index][5] = i + nr + 3 + (3 * j * nr) + (i * dummySplits); // 7
                    _elements[index][6] = i + 3 + (2 * nr) + (3 * j * nr) + (i * dummySplits); // 11
                    _elements[index][7] = i + 2 + (3 * nr) + (3 * j * nr) + (i * dummySplits); //14
                    _elements[index][8] = i + 1 + (3 * nr) + (3 * j * nr) + (i * dummySplits); // 13
                    _elements[index][9] = i + 2 + (2 * nr) + (3 * j * nr) + (i * dummySplits); // 10

                    _edges[index][0][0] = _elements[index][0];
                    _edges[index][0][1] = _elements[index][8];
                    _edges[index][0][2] = _elements[index][7];
                    _edges[index][0][3] = _elements[index][2];

                    _edges[index][1][0] = _elements[index][1];
                    _edges[index][1][1] = _elements[index][4];
                    _edges[index][1][2] = _elements[index][3];
                    _edges[index][1][3] = _elements[index][0];

                    _edges[index][2][0] = _elements[index][1];
                    _edges[index][2][1] = _elements[index][5];
                    _edges[index][2][2] = _elements[index][6];
                    _edges[index][2][3] = _elements[index++][2];
                }
            }

            WritePoints("forGraphics/allPoints.txt");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }

    private static void GetNodes(Point2D[] allNodes, Point2D[] vertices)
    {
        allNodes[0] = vertices[0];
        allNodes[1] = vertices[1];
        allNodes[2] = vertices[2];
        allNodes[3] = ((2 * vertices[0]) + vertices[1]) / 3;
        allNodes[4] = ((2 * vertices[1]) + vertices[0]) / 3;
        allNodes[5] = ((2 * vertices[1]) + vertices[2]) / 3;
        allNodes[6] = ((2 * vertices[2]) + vertices[1]) / 3;
        allNodes[7] = ((2 * vertices[2]) + vertices[0]) / 3;
        allNodes[8] = ((2 * vertices[0]) + vertices[2]) / 3;
        allNodes[9] = (vertices[0] + vertices[1] + vertices[2]) / 3;
    }

    private void WritePoints(string path)
    {
        using var sw = new StreamWriter(path);
        for (int i = 0; i < _points.Count; i++)
        {
            sw.WriteLine(_points[i]);
        }
    }

    private void WriteElements(string path)
    {
        using var sw = new StreamWriter(path);
        for (int i = 0; i < _elements.Length; i++)
        {
            sw.WriteLine($"{_elements[i][0]} {_elements[i][1]} {_elements[i][2]}");
        }
    }
}