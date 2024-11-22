using AeFinder.App.TestBase;
using AElfScan.GenesisApp.Processors;

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AElfScan.GenesisApp;

[DependsOn(
    typeof(AeFinderAppTestBaseModule),
    typeof(GenesisAppModule))]
public class GenesisAppTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AeFinderAppEntityOptions>(options => { options.AddTypes<GenesisAppModule>(); });
        
        context.Services.AddSingleton<AuthorUpdatedProcessor>();
        context.Services.AddSingleton<CodeCheckRequiredProcessor>();
        context.Services.AddSingleton<CodeUpdatedProcessor>();
        context.Services.AddSingleton<ContractDeployedProcessor>();
        context.Services.AddSingleton<ContractProposedProcessor>();
        
    }
}