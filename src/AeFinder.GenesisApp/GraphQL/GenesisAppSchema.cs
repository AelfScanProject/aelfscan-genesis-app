using AeFinder.Sdk;

namespace AeFinder.GenesisApp.GraphQL;

public class GenesisAppSchema : AppSchema<Query>
{
    public GenesisAppSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}