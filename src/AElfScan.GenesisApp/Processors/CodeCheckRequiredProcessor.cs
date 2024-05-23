using AElfScan.GenesisApp.Entities;
using AeFinder.Sdk.Processor;
using AElf.Standards.ACS0;

namespace AElfScan.GenesisApp.Processors;

public class CodeCheckRequiredProcessor : GenesisProcessorBase<CodeCheckRequired>
{
    public override async Task ProcessAsync(CodeCheckRequired logEvent, LogEventContext context)
    {
        var codeHash = GetCodeHash(logEvent.Code);
        var id = GetContractRegistrationId(context.ChainId, codeHash);

        var contractRegistration = new ContractRegistration
        {
            Id = id,
            CodeHash = codeHash
        };
        ObjectMapper.Map(logEvent, contractRegistration);
        
        await SaveEntityAsync(contractRegistration);

        if (logEvent.IsUserContract)
        {
            await AddContractProposeInfoAsync(context, logEvent.ProposedContractInputHash.ToHex());
        }
    }
}