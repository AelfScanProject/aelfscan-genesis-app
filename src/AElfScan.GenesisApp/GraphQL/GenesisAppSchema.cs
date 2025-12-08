using AeFinder.Sdk;

namespace AElfScan.GenesisApp.GraphQL;

public class GenesisAppSchema : AppSchema<Query>
{
    public GenesisAppSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}