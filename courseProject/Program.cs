using courseProject;

SpaceGridFactory spaceGridFactory = new();
TimeGridFactory timeGridFactory = new();

// FEM fem = FEM.CreateBuilder().SetTest(new Test1())
// .SetSpaceGrid(spaceGridFactory.CreateGrid(GridTypes.SpaceRegular, SpaceGridParameters.ReadJson("input/spaceGrid.jsonc")!.Value))
// .SetTimeGrid(timeGridFactory.CreateGrid(GridTypes.TimeRegular, TimeGridParameters.ReadJson("input/timeGrid.json")!.Value))
// .SetSolverSLAE(new CGMCholesky(1000, 1E-14))
// .SetDiriclhetBoundaries(DirichletBoundary.ReadJson("input/DirichletBoundaries.json")!)
// .SetNeumannBoundaries(NeumannBoundary.ReadJson("input/NeumannBoundaries.json")!)
// .IsPhysical(false);

FEM fem = FEM.CreateBuilder().SetTest(new Test5())
.SetSpaceGrid(spaceGridFactory.CreateGrid(GridTypes.SpaceRegular, SpaceGridParameters.ReadJson("input/spaceGrid.jsonc")!.Value))
.SetTimeGrid(timeGridFactory.CreateGrid(GridTypes.TimeRegular, TimeGridParameters.ReadJson("input/timeGrid.json")!.Value))
.SetSolverSLAE(new CGMCholesky(1000, 1E-14))
.SetDiriclhetBoundaries(DirichletBoundary.ReadJson("input/DirichletBoundaries.json")!)
.IsPhysical(false);

fem.Compute();
fem.WriteToFile("results/q.txt");

//var valuesAtPoints = fem.ValueAtPoint(new Point2D[] { new(5, 1) });
//Array.ForEach(valuesAtPoints, Console.WriteLine);
