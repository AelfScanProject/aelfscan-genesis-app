using Volo.Abp.DependencyInjection;

namespace AElfScan.GenesisApp;

public interface IGenesisContractAddressProvider
{
    string GetContractAddress(string chainId);
}

public class GenesisContractAddressProvider : IGenesisContractAddressProvider, ISingletonDependency
{
    private readonly Dictionary<string, string> _contractAddresses = new()
    {
        { "AELF", "pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i" },
        { "tDVW", "2UKQnHcQvhBT6X6ULtfnuh3b9PVRvVMEroHHkcK4YfcoH1Z1x2" }
    };

    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}