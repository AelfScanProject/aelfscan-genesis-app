namespace AElfScan.GenesisApp.GraphQL;

public class GetContractInfoDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }

    public List<string> AddressList { get; set; }
    public string Author { get; set; }
}