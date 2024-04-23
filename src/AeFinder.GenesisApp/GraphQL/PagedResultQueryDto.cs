
namespace AeFinder.GenesisApp.GraphQL;

public class PagedResultQueryDto
{
    public static int DefaultMaxResultCount { get; set; } = 10;
    public static int MaxMaxResultCount { get; set; } = 1000;
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    public virtual void Validate()
    {
        if (MaxResultCount > MaxMaxResultCount)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResultCount),
                $"Max allowed value for {nameof(MaxResultCount)} is {MaxMaxResultCount}.");
        }
    }
}