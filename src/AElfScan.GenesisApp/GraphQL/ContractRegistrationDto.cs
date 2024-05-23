using AElfScan.GenesisApp.Entities;
using AeFinder.Sdk.Dtos;

namespace AElfScan.GenesisApp.GraphQL;

public class ContractRegistrationDto : AeFinderEntityDto
{
    public string CodeHash { get; set; }
    public string Code { get; set; }
    public string ProposedContractInputHash { get; set; }
    public int ContractCategory { get; set; }
    public ContractType ContractType { get; set; }
}