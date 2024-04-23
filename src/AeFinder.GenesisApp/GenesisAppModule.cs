using AeFinder.GenesisApp.GraphQL;
using AeFinder.GenesisApp.Processors;
using AeFinder.Sdk.Processor;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AeFinder.GenesisApp;

public class GenesisAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<GenesisAppSchema>(); });
        context.Services.AddSingleton<ISchema, GenesisAppSchema>();
        context.Services.AddSingleton<ILogEventProcessor, AuthorUpdatedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, CodeCheckRequiredProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, CodeUpdatedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, ContractDeployedProcessor>();
        context.Services.AddSingleton<ILogEventProcessor, ContractProposedProcessor>();
    }
}