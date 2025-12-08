using AeFinder.Sdk.Processor;
using AElf.Standards.ACS0;

namespace AElfScan.GenesisApp.Processors;

public class ContractProposedProcessor : GenesisProcessorBase<ContractProposed>
{
    public override async Task ProcessAsync(ContractProposed logEvent, LogEventContext context)
    {
        await AddContractProposeInfoAsync(context, logEvent.ProposedContractInputHash.ToHex());
    }
}