namespace AElfScan.GenesisApp.GraphQL;

public class GetContractRegistrationDto
{
    public string ChainId { get; set; }
    public string CodeHash { get; set; }
    
    public virtual void Validate()
    {
        if (ChainId.IsNullOrWhiteSpace())
        {
            throw new ArgumentOutOfRangeException($"{nameof(ChainId)} is required.");
        }
        
        if (CodeHash.IsNullOrWhiteSpace())
        {
            throw new ArgumentOutOfRangeException($"{nameof(CodeHash)} is required.");
        }
    }
}