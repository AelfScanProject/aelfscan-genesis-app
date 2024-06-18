using AeFinder.Sdk.Logging;
using AElfScan.GenesisApp.Entities;
using AeFinder.Sdk.Processor;
using AElf.Standards.ACS0;

namespace AElfScan.GenesisApp.Processors;

public class CodeUpdatedProcessor : GenesisProcessorBase<CodeUpdated>
{
    public override async Task ProcessAsync(CodeUpdated logEvent, LogEventContext context)
    {
        var id = GetContractInfoId(context.ChainId, logEvent.Address);

        var contractInfo = await GetEntityAsync<Entities.ContractInfo>(id);
        if (contractInfo == null)
        {
            if (id == "AELF-pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i")
            {
                contractInfo = new Entities.ContractInfo()
                {
                    Address = "pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i",
                    CodeHash = "321d000a078787469d1b794cb28bea4ed8c682a7224c0e8fca7067606bb7567a",
                    Author = "pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i",
                    Version = 1,
                    NameHash = "0000000000000000000000000000000000000000000000000000000000000000",
                    ContractCategory = 0,
                    ContractType = ContractType.SystemContract,
                    Id = "AELF-pykr77ft9UUKJZLVq15wCH8PinBSjVRQ12sD1Ayq92mKFsJ1i"
                };
            }
            
        }


        ObjectMapper.Map(logEvent, contractInfo);
        var contractRegistration =
            await GetEntityAsync<ContractRegistration>(
                GetContractRegistrationId(context.ChainId, logEvent.NewCodeHash.ToHex()));
        ObjectMapper.Map(contractRegistration, contractInfo);

        await SaveEntityAsync(contractInfo);

        await AddRecordAsync(ContractOperationType.UpdateContract, contractInfo, context);
    }
}