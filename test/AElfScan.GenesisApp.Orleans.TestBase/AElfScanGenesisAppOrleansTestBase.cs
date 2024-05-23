using Orleans.TestingHost;
using AElfScan.GenesisApp.TestBase;
using Volo.Abp.Modularity;

namespace AElfScan.GenesisApp.Orleans.TestBase;

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