namespace courseProject;

public enum GridTypes
{
    SpaceRegular,
    TimeRegular,
    TimeIrregular
}

public interface ISpaceFactory
{
    public ISpaceGrid CreateGrid(GridTypes spaceGridType, SpaceGridParameters gridParameters);
}

public interface ITimeFactory
{
    public ITimeGrid CreateGrid(GridTypes timeGridType, TimeGridParameters gridParameters);
}

public class SpaceGridFactory : ISpaceFactory
{
    public ISpaceGrid CreateGrid(GridTypes spaceGridType, SpaceGridParameters gridParameters)
    {
        return spaceGridType switch
        {
            GridTypes.SpaceRegular => new SpaceRegularGrid(gridParameters),

            _ => throw new ArgumentOutOfRangeException(nameof(spaceGridType), $"This type of grid does not exist: {spaceGridType}"),
        };
    }
}

public class TimeGridFactory : ITimeFactory
{
    public ITimeGrid CreateGrid(GridTypes timeGridType, TimeGridParameters gridParameters)
    {
        return timeGridType switch
        {
            GridTypes.TimeRegular => new TimeRegularGrid(gridParameters),

            GridTypes.TimeIrregular => new TimeIrregularGrid(gridParameters),

            _ => throw new ArgumentOutOfRangeException(nameof(timeGridType), $"This type of grid does not exist: {timeGridType}"),
        };
    }
}