namespace AElfScan.GenesisApp.GraphQL;

public class GetContractRecordDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Author { get; set; }
}