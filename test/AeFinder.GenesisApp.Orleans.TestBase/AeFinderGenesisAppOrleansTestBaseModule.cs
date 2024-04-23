using Microsoft.Extensions.DependencyInjection;
using Orleans;
using AeFinder.GenesisApp.TestBase;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AeFinder.GenesisApp.Orleans.TestBase;

[DependsOn(typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(AeFinderGenesisAppTestBaseModule)
    )]
public class AeFinderGenesisAppOrleansTestBaseModule:AbpModule
{
    private ClusterFixture _fixture;
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        if(_fixture == null)
            _fixture = new ClusterFixture();
        // var fixture = new ClusterFixture();
        context.Services.AddSingleton<ClusterFixture>(_fixture);
        context.Services.AddSingleton<IClusterClient>(sp => _fixture.Cluster.Client);
    }
}