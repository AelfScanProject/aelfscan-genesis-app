using Orleans.TestingHost;
using AeFinder.GenesisApp.TestBase;
using Volo.Abp.Modularity;

namespace AeFinder.GenesisApp.Orleans.TestBase;

public abstract class
    AeFinerGenesisAppOrleansTestBase<TStartupModule> : AeFinerGenesisAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public AeFinerGenesisAppOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}