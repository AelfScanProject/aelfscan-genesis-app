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
        { "tDVV", "2dtnkWDyJJXeDRcREhKSZHrYdDGMbn3eus5KYpXonfoTygFHZm" }
    };

    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}