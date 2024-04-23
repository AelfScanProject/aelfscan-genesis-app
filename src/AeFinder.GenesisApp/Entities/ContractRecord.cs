using AeFinder.Sdk.Entities;
using Nest;

namespace AeFinder.GenesisApp.Entities;

public class ContractRecord : AeFinderEntity, IAeFinderEntity
{
    public ContractOperationType OperationType { get; set; }
    [Keyword]
    public string Operator { get; set; }
    [Keyword]
    public string TransactionId { get; set; }
    public ContractInfo ContractInfo { get; set; }
}