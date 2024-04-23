using AeFinder.Sdk.Entities;
using Nest;

namespace AeFinder.GenesisApp.Entities;

public class ContractInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword]
    public string Address { get; set; }
    [Keyword]
    public string CodeHash { get; set; }
    [Keyword]
    public string Author { get; set; }
    public int Version { get; set; }
    [Keyword]
    public string NameHash { get; set; }
    [Keyword]
    public string ContractVersion { get; set; }
    public int ContractCategory { get; set; }
    public ContractType ContractType { get; set; }
}