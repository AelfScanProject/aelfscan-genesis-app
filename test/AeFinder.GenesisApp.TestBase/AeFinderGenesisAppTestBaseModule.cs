using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace AeFinder.GenesisApp.TestBase;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpTestBaseModule),
    typeof(AbpEventBusModule)
    )]
public class AeFinderGenesisAppTestBaseModule:AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = false;
        });

        context.Services.AddAlwaysAllowAuthorization();
        
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        SeedTestData(context);
    }

    private static void SeedTestData(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(async () =>
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            }
        });
    }
}