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
        ObjectMapper.Map(logEvent, contractInfo);
        var contractRegistration =
            await GetEntityAsync<ContractRegistration>(
                GetContractRegistrationId(context.ChainId, logEvent.NewCodeHash.ToHex()));
        ObjectMapper.Map(contractRegistration, contractInfo);
        
        await SaveEntityAsync(contractInfo);
        
        await AddRecordAsync(ContractOperationType.UpdateContract, contractInfo, context);
    }
}