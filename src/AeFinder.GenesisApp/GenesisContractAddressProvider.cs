using Volo.Abp.DependencyInjection;

namespace AeFinder.GenesisApp;

public interface IGenesisContractAddressProvider
{
    string GetContractAddress(string chainId);
}

public class GenesisContractAddressProvider : IGenesisContractAddressProvider, ISingletonDependency
{
    private readonly Dictionary<string, string> _contractAddresses = new()
    {
        { "AELF", "" },
        { "tDVV", "" }
    };

    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}