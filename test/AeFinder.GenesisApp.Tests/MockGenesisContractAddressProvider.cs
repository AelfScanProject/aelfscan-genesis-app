using Volo.Abp.DependencyInjection;

namespace AeFinder.GenesisApp;

public class MockGenesisContractAddressProvider : IGenesisContractAddressProvider, ISingletonDependency
{
    private readonly Dictionary<string, string> _contractAddresses = new()
    {
        { "AELF", "GenesisContractAddress" }
    };

    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}