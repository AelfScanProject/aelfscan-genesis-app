using AElfScan.GenesisApp.Entities;
using AeFinder.Sdk.Processor;
using AElf.Standards.ACS0;

namespace AElfScan.GenesisApp.Processors;

public class AuthorUpdatedProcessor : GenesisProcessorBase<AuthorUpdated>
{
    public override async Task ProcessAsync(AuthorUpdated logEvent, LogEventContext context)
    {
        var id = GetContractInfoId(context.ChainId, logEvent.Address);

        var contractInfo = await GetEntityAsync<Entities.ContractInfo>(id);
        contractInfo.Author = logEvent.NewAuthor.ToBase58();

        await SaveEntityAsync(contractInfo);
        
        await AddRecordAsync(ContractOperationType.SetAuthor, contractInfo, context);
    }
}