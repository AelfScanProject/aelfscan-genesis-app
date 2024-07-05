using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.GenesisApp.Entities;

public class ContractProposeInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword]
    public string ProposedContractInputHash { get; set; }
    [Keyword]
    public string Proposer { get; set; }
}