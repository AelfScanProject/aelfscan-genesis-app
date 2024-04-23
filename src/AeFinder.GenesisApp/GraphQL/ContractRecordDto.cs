using AeFinder.GenesisApp.Entities;
using AeFinder.Sdk.Dtos;

namespace AeFinder.GenesisApp.GraphQL;

public class ContractRecordDto : AeFinderEntityDto
{
    public ContractOperationType OperationType { get; set; }
    public string Operator { get; set; }
    public string TransactionId { get; set; }
    public ContractInfoDto ContractInfo { get; set; }
}