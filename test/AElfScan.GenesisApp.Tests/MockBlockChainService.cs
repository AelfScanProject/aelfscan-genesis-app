using AeFinder.Sdk;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;

namespace AElfScan.GenesisApp;

public class MockBlockChainService : IBlockChainService, ITransientDependency
{
    public async Task<T> ViewContractAsync<T>(string chainId, string contractAddress, string methodName, IMessage parameter) where T : IMessage<T>, new()
    {
        switch (methodName)
        {
            case "GetSmartContractRegistrationByCodeHash":
                var result = new T();
                result.MergeFrom(new SmartContractRegistration
                {
                    Category = 1,
                    Code = ByteString.CopyFromUtf8("code")
                }.ToByteArray());
                return result;
        }
        throw new NotImplementedException();
    }
}