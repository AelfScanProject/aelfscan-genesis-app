using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.GenesisApp.Entities;

public class ContractRegistration : AeFinderEntity, IAeFinderEntity
{
    [Keyword] 
    public string CodeHash { get; set; }
    [Text(Index = false)] 
    public string Code { get; set; }
    [Keyword] 
    public string ProposedContractInputHash { get; set; }
    public int ContractCategory { get; set; }
    public ContractType ContractType { get; set; }
}