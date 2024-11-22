using AeFinder.Sdk;
using AElfScan.GenesisApp.Entities;
using AElfScan.GenesisApp.Processors;
using AeFinder.Sdk;
using AElf;
using AElf.Standards.ACS0;
using AElf.Types;
using Google.Protobuf;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace AElfScan.GenesisApp.GraphQL;

public class QueryTests : GenesisAppTestBase
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractDeployedProcessor _contractDeployedProcessor;
    private readonly CodeCheckRequiredProcessor _codeCheckRequiredProcessor;
    private readonly ContractProposedProcessor _contractProposedProcessor;
    
    private readonly IReadOnlyRepository<AElfScan.GenesisApp.Entities.ContractInfo> _contractInfoRepository;
    private readonly IReadOnlyRepository<ContractRegistration> _contractRegistrationRepository;
    private readonly IReadOnlyRepository<ContractRecord> _contractRecordRepository;

    public QueryTests()
    {
        _contractInfoRepository = GetRequiredService<IReadOnlyRepository<AElfScan.GenesisApp.Entities.ContractInfo>>();
        _contractRegistrationRepository = GetRequiredService<IReadOnlyRepository<ContractRegistration>>();
        _contractRecordRepository = GetRequiredService<IReadOnlyRepository<ContractRecord>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _contractProposedProcessor = GetRequiredService<ContractProposedProcessor>();
        _contractDeployedProcessor = GetRequiredService<ContractDeployedProcessor>();
        _codeCheckRequiredProcessor = GetRequiredService<CodeCheckRequiredProcessor>();
    }
    
    [Fact]
    public async Task ContractInfo_WrongMaxResultCount_Test()
    {
        await Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            MaxResultCount = 1001
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public async Task ContractRecord_WrongMaxResultCount_Test()
    {
        await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto()
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            MaxResultCount = 1001
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public async Task ContractRegistration_WrongInput_Test()
    {
        await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper, new GetContractRegistrationDto()
        {
            ChainId = ChainId
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
        
        await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper, new GetContractRegistrationDto()
        {
            CodeHash = "CodeHash"
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Query_Test()
    {
        var author1 = "xUgvBLughMpZp1w2E1GmgACU9h8EzqY5X4ZBqSKRRc4g9QL72";
        var author2 = "zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8";
        var address1 = "2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG";
        var address2 = "ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea";
        await DeployContractAsync(author1, address1,"code1", "hash1");
        await DeployContractAsync(author2, address2,"code2", "hash2");
        
        var contractInfo = await Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 10,
            BlockHeight = 10
        });
        contractInfo.Items.Count().ShouldBe(2);
        
        contractInfo = await Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Address = address1,
            SkipCount = 0,
            MaxResultCount = 10
        });
        contractInfo.Items.Count().ShouldBe(1);
        
        contractInfo = await Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Author = author1,
            SkipCount = 0,
            MaxResultCount = 10
        });
        contractInfo.Items.Count().ShouldBe(1);
        
        var contractRecord = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto()
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 10
        });
        contractRecord.Count.ShouldBe(2);
        
        contractRecord = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto()
        {
            ChainId = ChainId,
            Address = address1,
            SkipCount = 0,
            MaxResultCount = 10
        });
        contractRecord.Count.ShouldBe(1);
        
        contractRecord = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto()
        {
            ChainId = ChainId,
            Author = author1,
            SkipCount = 0,
            MaxResultCount = 10
        });
        contractRecord.Count.ShouldBe(1);
        
        var contractRegistration = await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper, new GetContractRegistrationDto()
        {
            ChainId = ChainId,
            CodeHash = HashHelper.ComputeFrom(ByteString.CopyFromUtf8("code1").ToByteArray()).ToHex(),
        });
        contractRegistration.Count.ShouldBe(1);
    }
    
    private async Task DeployContractAsync(string author, string address, string code, string inputHash)
    {
        var contractProposedEvent = new ContractProposed
        {
            ProposedContractInputHash = HashHelper.ComputeFrom(inputHash)
        };
        var logEventContext = GenerateLogEventContext(contractProposedEvent);
        await _contractProposedProcessor.ProcessAsync(logEventContext);
        
        var codeCheckRequiredEvent = new CodeCheckRequired
        {
            Category = 1,
            Code = ByteString.CopyFromUtf8(code),
            IsSystemContract = false,
            IsUserContract = true,
            ProposedContractInputHash = contractProposedEvent.ProposedContractInputHash,
        };
        logEventContext = GenerateLogEventContext(codeCheckRequiredEvent);
        await _codeCheckRequiredProcessor.ProcessAsync(logEventContext);
        
        var contractDeployedEvent = new ContractDeployed
        {
            Address = Address.FromBase58(address),
            CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()),
            Name = HashHelper.ComputeFrom("Name"),
            Author = Address.FromBase58(author),
            ContractVersion = "1.0.0",
            Version = 1,
        };
        
        logEventContext = GenerateLogEventContext(contractDeployedEvent);
        await _contractDeployedProcessor.ProcessAsync(logEventContext);
    }

}