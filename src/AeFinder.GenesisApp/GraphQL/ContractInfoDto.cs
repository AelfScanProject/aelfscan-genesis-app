using AeFinder.GenesisApp.Entities;
using AeFinder.Sdk.Dtos;

namespace AeFinder.GenesisApp.GraphQL;

public class ContractInfoDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public string CodeHash { get; set; }
    public string Author { get; set; }
    public int Version { get; set; }
    public string NameHash { get; set; }
    public string ContractVersion { get; set; }
    public int ContractCategory { get; set; }
    public ContractType ContractType { get; set; }
}