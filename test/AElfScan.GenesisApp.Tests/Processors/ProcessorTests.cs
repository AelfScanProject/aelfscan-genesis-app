using AeFinder.Sdk;
using AElfScan.GenesisApp.Entities;
using AElfScan.GenesisApp.GraphQL;
using AeFinder.Sdk;
using AElf;
using AElf.Standards.ACS0;
using AElf.Types;
using Google.Protobuf;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace AElfScan.GenesisApp.Processors;

public class ProcessorTests : GenesisAppTestBase
{
    private readonly ContractDeployedProcessor _contractDeployedProcessor;
    private readonly CodeCheckRequiredProcessor _codeCheckRequiredProcessor;
    private readonly CodeUpdatedProcessor _codeUpdatedProcessor;
    private readonly AuthorUpdatedProcessor _authorUpdatedProcessor;
    private readonly ContractProposedProcessor _contractProposedProcessor;
    private readonly IObjectMapper _objectMapper;

    private readonly IReadOnlyRepository<AElfScan.GenesisApp.Entities.ContractInfo> _contractInfoRepository;
    private readonly IReadOnlyRepository<ContractRegistration> _contractRegistrationRepository;
    private readonly IReadOnlyRepository<ContractRecord> _contractRecordRepository;
    private readonly IReadOnlyRepository<ContractProposeInfo> _contractProposeInfoRepository;

    public ProcessorTests()
    {
        _contractDeployedProcessor = GetRequiredService<ContractDeployedProcessor>();
        _contractProposedProcessor = GetRequiredService<ContractProposedProcessor>();
        _codeCheckRequiredProcessor = GetRequiredService<CodeCheckRequiredProcessor>();
        _codeUpdatedProcessor = GetRequiredService<CodeUpdatedProcessor>();
        _authorUpdatedProcessor = GetRequiredService<AuthorUpdatedProcessor>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _contractInfoRepository = GetRequiredService<IReadOnlyRepository<AElfScan.GenesisApp.Entities.ContractInfo>>();
        _contractRegistrationRepository = GetRequiredService<IReadOnlyRepository<ContractRegistration>>();
        _contractRecordRepository = GetRequiredService<IReadOnlyRepository<ContractRecord>>();
        _contractProposeInfoRepository = GetRequiredService<IReadOnlyRepository<ContractProposeInfo>>();
    }

    [Fact]
    public async Task DeployAndUpdateContract_Test()
    {
        // Deploy contract
        var contractProposedEvent = new ContractProposed
        {
            ProposedContractInputHash = HashHelper.ComputeFrom("Deploy")
        };
        var logEventContext = GenerateLogEventContext(contractProposedEvent);
        await _contractProposedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();

        var contractProposeInfos = (await _contractProposeInfoRepository.GetQueryableAsync()).ToList();
        contractProposeInfos.Count.ShouldBe(1);
        contractProposeInfos[0].Proposer.ShouldBe(FromAddress);
        contractProposeInfos[0].ProposedContractInputHash
            .ShouldBe(contractProposedEvent.ProposedContractInputHash.ToHex());

        var codeCheckRequiredEvent = new CodeCheckRequired
        {
            Category = 1,
            Code = ByteString.CopyFromUtf8("code"),
            IsSystemContract = false,
            IsUserContract = true,
            ProposedContractInputHash = contractProposedEvent.ProposedContractInputHash,
        };
        logEventContext = GenerateLogEventContext(codeCheckRequiredEvent);
        await _codeCheckRequiredProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractRegistrations = await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper,
            new GetContractRegistrationDto
            {
                ChainId = ChainId,
                CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()).ToHex()
            });
        contractRegistrations.Count.ShouldBe(1);
        contractRegistrations[0].ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractRegistrations[0].Code.ShouldBe(codeCheckRequiredEvent.Code.ToBase64());
        contractRegistrations[0].ContractType.ShouldBe(ContractType.UserContract);
        contractRegistrations[0].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEvent.ProposedContractInputHash.ToHex());
        contractRegistrations[0].CodeHash
            .ShouldBe(HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()).ToHex());
        contractRegistrations[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);

        var contractDeployedEvent = new ContractDeployed
        {
            Address = TestAddress,
            CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()),
            Name = HashHelper.ComputeFrom("Name"),
            Author = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            ContractVersion = "1.0.0",
            Version = 1,
        };

        logEventContext = GenerateLogEventContext(contractDeployedEvent);
        await _contractDeployedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractInfoId = IdGenerateHelper.GetId(logEventContext.ChainId, contractDeployedEvent.Address.ToBase58());
        var contractInfo = (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId)
            .ToList()[0];
        contractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfo.NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfo.Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractInfo.Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfo.ContractType.ShouldBe(ContractType.UserContract);

        var contractInfos = Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        }).Result.Items;


        contractInfos.Count.ShouldBe(1);
        contractInfos[0].Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfos[0].CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfos[0].NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfos[0].Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfos[0].ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfos[0].Version.ShouldBe(contractDeployedEvent.Version);
        contractInfos[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfos[0].ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfos[0].ContractType.ShouldBe(ContractType.UserContract);

        var contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper,
            new GetContractRecordDto
            {
                ChainId = ChainId,
                Address = TestAddress.ToBase58()
            });
        contractRecords.Count.ShouldBe(1);
        contractRecords[0].ContractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractRecords[0].ContractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractRecords[0].ContractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractRecords[0].ContractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractRecords[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[0].ContractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractRecords[0].ContractInfo.ContractType.ShouldBe(ContractType.UserContract);
        contractRecords[0].Operator.ShouldBe(FromAddress);
        contractRecords[0].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[0].OperationType.ShouldBe(ContractOperationType.DeployContract);

        // Update contract
        var contractProposedEventUpdate = new ContractProposed
        {
            ProposedContractInputHash = HashHelper.ComputeFrom("Update")
        };
        logEventContext = GenerateLogEventContext(contractProposedEventUpdate);
        await _contractProposedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();

        contractProposeInfos = (await _contractProposeInfoRepository.GetQueryableAsync()).ToList();
        contractProposeInfos.Count.ShouldBe(2);
        contractProposeInfos[1].Proposer.ShouldBe(FromAddress);
        contractProposeInfos[1].ProposedContractInputHash
            .ShouldBe(contractProposedEventUpdate.ProposedContractInputHash.ToHex());

        var codeCheckRequiredEventUpdate = new CodeCheckRequired
        {
            Category = 1,
            Code = ByteString.CopyFromUtf8("code new"),
            IsSystemContract = false,
            IsUserContract = true,
            ProposedContractInputHash = contractProposedEventUpdate.ProposedContractInputHash
        };
        logEventContext = GenerateLogEventContext(codeCheckRequiredEventUpdate);
        await _codeCheckRequiredProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        contractRegistrations = await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper,
            new GetContractRegistrationDto
            {
                ChainId = ChainId,
                CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()).ToHex()
            });
        contractRegistrations.Count.ShouldBe(1);
        contractRegistrations[0].ContractCategory.ShouldBe(codeCheckRequiredEventUpdate.Category);
        contractRegistrations[0].Code.ShouldBe(codeCheckRequiredEventUpdate.Code.ToBase64());
        contractRegistrations[0].ContractType.ShouldBe(ContractType.UserContract);
        contractRegistrations[0].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEventUpdate.ProposedContractInputHash.ToHex());
        contractRegistrations[0].CodeHash
            .ShouldBe(HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()).ToHex());
        contractRegistrations[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);

        contractProposeInfos = (await _contractProposeInfoRepository.GetQueryableAsync()).ToList();
        contractProposeInfos.Count.ShouldBe(2);
        contractProposeInfos[1].Proposer.ShouldBe(FromAddress);
        contractProposeInfos[1].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEventUpdate.ProposedContractInputHash.ToHex());

        var codeUpdatedEvent = new CodeUpdated
        {
            Address = TestAddress,
            NewCodeHash = HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()),
            OldCodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()),
            ContractVersion = "1.1.0",
            Version = 2
        };

        logEventContext = GenerateLogEventContext(codeUpdatedEvent);
        await _codeUpdatedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        contractInfo =
            (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId).ToList()[0];
        contractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfo.CodeHash.ShouldBe(codeUpdatedEvent.NewCodeHash.ToHex());
        contractInfo.NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfo.Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfo.ContractVersion.ShouldBe(codeUpdatedEvent.ContractVersion);
        contractInfo.Version.ShouldBe(codeUpdatedEvent.Version);
        contractInfo.Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfo.ContractType.ShouldBe(ContractType.UserContract);

        contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        contractRecords.Count.ShouldBe(2);
        contractRecords[1].ContractInfo.Address.ShouldBe(codeUpdatedEvent.Address.ToBase58());
        contractRecords[1].ContractInfo.CodeHash.ShouldBe(contractInfo.CodeHash);
        contractRecords[1].ContractInfo.ContractVersion.ShouldBe(codeUpdatedEvent.ContractVersion);
        contractRecords[1].ContractInfo.Version.ShouldBe(codeUpdatedEvent.Version);
        contractRecords[1].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[1].ContractInfo.ContractCategory.ShouldBe(codeCheckRequiredEventUpdate.Category);
        contractRecords[1].ContractInfo.ContractType.ShouldBe(ContractType.UserContract);
        contractRecords[1].Operator.ShouldBe(FromAddress);
        contractRecords[1].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[1].OperationType.ShouldBe(ContractOperationType.UpdateContract);
    }

    [Fact]
    public async Task DeploySystemContract_Test()
    {
        var code = ByteString.CopyFromUtf8("code");

        // Deploy system contract
        var contractDeployedEvent = new ContractDeployed
        {
            Address = TestAddress,
            CodeHash = HashHelper.ComputeFrom(code.ToByteArray()),
            Name = HashHelper.ComputeFrom("Name"),
            Author = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            ContractVersion = "1.0.0",
            Version = 1
        };
        var logEventContext = GenerateLogEventContext(contractDeployedEvent);

        await _contractDeployedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractRegistrations = await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper,
            new GetContractRegistrationDto
            {
                ChainId = ChainId,
                CodeHash = contractDeployedEvent.CodeHash.ToHex()
            });
        contractRegistrations.Count.ShouldBe(1);
        contractRegistrations[0].ContractCategory.ShouldBe(1);
        contractRegistrations[0].Code.ShouldBe(code.ToBase64());
        contractRegistrations[0].ContractType.ShouldBe(ContractType.SystemContract);
        contractRegistrations[0].ProposedContractInputHash.ShouldBeNull();
        contractRegistrations[0].CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractRegistrations[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);

        var contractInfoId = IdGenerateHelper.GetId(logEventContext.ChainId, contractDeployedEvent.Address.ToBase58());
        var contractInfo = (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId)
            .ToList()[0];
        contractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfo.NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfo.Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractInfo.Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfo.ContractCategory.ShouldBe(1);
        contractInfo.ContractType.ShouldBe(ContractType.SystemContract);

        var contractInfos = Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        }).Result.Items;
        contractInfos.Count.ShouldBe(1);
        contractInfos[0].Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfos[0].CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfos[0].NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfos[0].Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfos[0].ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfos[0].Version.ShouldBe(contractDeployedEvent.Version);
        contractInfos[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfos[0].ContractCategory.ShouldBe(1);
        contractInfos[0].ContractType.ShouldBe(ContractType.SystemContract);

        var contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper,
            new GetContractRecordDto
            {
                ChainId = ChainId,
                Address = TestAddress.ToBase58()
            });
        contractRecords.Count.ShouldBe(1);
        contractRecords[0].ContractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractRecords[0].ContractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractRecords[0].ContractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractRecords[0].ContractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractRecords[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[0].ContractInfo.ContractCategory.ShouldBe(1);
        contractRecords[0].ContractInfo.ContractType.ShouldBe(ContractType.SystemContract);
        contractRecords[0].Operator.ShouldBe(FromAddress);
        contractRecords[0].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[0].OperationType.ShouldBe(ContractOperationType.DeployContract);
    }

    [Fact]
    public async Task DeployAndUpdateUserContract_Test()
    {
        // Deploy contract
        var codeCheckRequiredEvent = new CodeCheckRequired
        {
            Category = 1,
            Code = ByteString.CopyFromUtf8("code"),
            IsSystemContract = false,
            IsUserContract = true,
            ProposedContractInputHash = HashHelper.ComputeFrom("Deploy")
        };
        var logEventContext = GenerateLogEventContext(codeCheckRequiredEvent);
        await _codeCheckRequiredProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractRegistrations = await Query.ContractRegistration(_contractRegistrationRepository, _objectMapper,
            new GetContractRegistrationDto
            {
                ChainId = ChainId,
                CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()).ToHex()
            });
        contractRegistrations.Count.ShouldBe(1);
        contractRegistrations[0].ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractRegistrations[0].Code.ShouldBe(codeCheckRequiredEvent.Code.ToBase64());
        contractRegistrations[0].ContractType.ShouldBe(ContractType.UserContract);
        contractRegistrations[0].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEvent.ProposedContractInputHash.ToHex());
        contractRegistrations[0].CodeHash
            .ShouldBe(HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()).ToHex());
        contractRegistrations[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);

        var contractProposeInfos = (await _contractProposeInfoRepository.GetQueryableAsync()).ToList();
        contractProposeInfos.Count.ShouldBe(1);
        contractProposeInfos[0].Proposer.ShouldBe(FromAddress);
        contractProposeInfos[0].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEvent.ProposedContractInputHash.ToHex());

        var contractDeployedEvent = new ContractDeployed
        {
            Address = TestAddress,
            CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()),
            Name = HashHelper.ComputeFrom("Name"),
            Author = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            ContractVersion = "1.0.0",
            Version = 1
        };

        logEventContext = GenerateLogEventContext(contractDeployedEvent);
        await _contractDeployedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractInfoId = IdGenerateHelper.GetId(logEventContext.ChainId, contractDeployedEvent.Address.ToBase58());
        var contractInfo = (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId)
            .ToList()[0];
        contractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfo.NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfo.Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractInfo.Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfo.ContractType.ShouldBe(ContractType.UserContract);

        var contractInfos =  Query.ContractList(_contractInfoRepository, _objectMapper, new GetContractInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        }).Result.Items;
        contractInfos.Count.ShouldBe(1);
        contractInfos[0].Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfos[0].CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractInfos[0].NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfos[0].Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfos[0].ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractInfos[0].Version.ShouldBe(contractDeployedEvent.Version);
        contractInfos[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfos[0].ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfos[0].ContractType.ShouldBe(ContractType.UserContract);

        var contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper,
            new GetContractRecordDto
            {
                ChainId = ChainId,
                Address = TestAddress.ToBase58()
            });
        contractRecords.Count.ShouldBe(1);
        contractRecords[0].ContractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractRecords[0].ContractInfo.CodeHash.ShouldBe(contractDeployedEvent.CodeHash.ToHex());
        contractRecords[0].ContractInfo.ContractVersion.ShouldBe(contractDeployedEvent.ContractVersion);
        contractRecords[0].ContractInfo.Version.ShouldBe(contractDeployedEvent.Version);
        contractRecords[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[0].ContractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractRecords[0].ContractInfo.ContractType.ShouldBe(ContractType.UserContract);
        contractRecords[0].Operator.ShouldBe(FromAddress);
        contractRecords[0].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[0].OperationType.ShouldBe(ContractOperationType.DeployContract);
        contractRecords[0].ContractInfo.Author.ShouldBe(contractInfos[0].Author);

        // Update contract
        var codeCheckRequiredEventUpdate = new CodeCheckRequired
        {
            Category = 1,
            Code = ByteString.CopyFromUtf8("code new"),
            IsSystemContract = false,
            IsUserContract = true,
            ProposedContractInputHash = HashHelper.ComputeFrom("Update")
        };
        logEventContext = GenerateLogEventContext(codeCheckRequiredEventUpdate);
        await _codeCheckRequiredProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        var contractUpdateRegistrations = await Query.ContractRegistration(_contractRegistrationRepository,
            _objectMapper,
            new GetContractRegistrationDto
            {
                ChainId = ChainId,
                CodeHash = HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()).ToHex()
            });
        contractUpdateRegistrations.Count.ShouldBe(1);
        contractUpdateRegistrations[0].ContractCategory.ShouldBe(codeCheckRequiredEventUpdate.Category);
        contractUpdateRegistrations[0].Code.ShouldBe(codeCheckRequiredEventUpdate.Code.ToBase64());
        contractUpdateRegistrations[0].ContractType.ShouldBe(ContractType.UserContract);
        contractUpdateRegistrations[0].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEventUpdate.ProposedContractInputHash.ToHex());
        contractUpdateRegistrations[0].CodeHash
            .ShouldBe(HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()).ToHex());
        contractUpdateRegistrations[0].Metadata.ChainId.ShouldBe(logEventContext.ChainId);

        contractProposeInfos = (await _contractProposeInfoRepository.GetQueryableAsync()).ToList();
        contractProposeInfos.Count.ShouldBe(2);
        contractProposeInfos[1].Proposer.ShouldBe(FromAddress);
        contractProposeInfos[1].ProposedContractInputHash
            .ShouldBe(codeCheckRequiredEventUpdate.ProposedContractInputHash.ToHex());

        var codeUpdatedEvent = new CodeUpdated
        {
            Address = TestAddress,
            NewCodeHash = HashHelper.ComputeFrom(codeCheckRequiredEventUpdate.Code.ToByteArray()),
            OldCodeHash = HashHelper.ComputeFrom(codeCheckRequiredEvent.Code.ToByteArray()),
            ContractVersion = "1.1.0",
            Version = 2
        };

        logEventContext = GenerateLogEventContext(codeUpdatedEvent);
        await _codeUpdatedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        contractInfo =
            (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId).ToList()[0];
        contractInfo.Address.ShouldBe(contractDeployedEvent.Address.ToBase58());
        contractInfo.CodeHash.ShouldBe(codeUpdatedEvent.NewCodeHash.ToHex());
        contractInfo.NameHash.ShouldBe(contractDeployedEvent.Name.ToHex());
        contractInfo.Author.ShouldBe(contractDeployedEvent.Author.ToBase58());
        contractInfo.ContractVersion.ShouldBe(codeUpdatedEvent.ContractVersion);
        contractInfo.Version.ShouldBe(codeUpdatedEvent.Version);
        contractInfo.Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractInfo.ContractCategory.ShouldBe(codeCheckRequiredEvent.Category);
        contractInfo.ContractType.ShouldBe(ContractType.UserContract);

        contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        contractRecords.Count.ShouldBe(2);
        contractRecords[1].ContractInfo.Address.ShouldBe(codeUpdatedEvent.Address.ToBase58());
        contractRecords[1].ContractInfo.CodeHash.ShouldBe(contractInfo.CodeHash);
        contractRecords[1].ContractInfo.ContractVersion.ShouldBe(codeUpdatedEvent.ContractVersion);
        contractRecords[1].ContractInfo.Version.ShouldBe(codeUpdatedEvent.Version);
        contractRecords[1].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[1].ContractInfo.ContractCategory.ShouldBe(codeCheckRequiredEventUpdate.Category);
        contractRecords[1].ContractInfo.ContractType.ShouldBe(ContractType.UserContract);
        contractRecords[1].Operator.ShouldBe(FromAddress);
        contractRecords[1].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[1].OperationType.ShouldBe(ContractOperationType.UpdateContract);
        contractRecords[1].ContractInfo.Author.ShouldBe(contractInfo.Author);

        // Update author
        var authorUpdatedEvent = new AuthorUpdated
        {
            Address = TestAddress,
            OldAuthor = contractDeployedEvent.Author,
            NewAuthor = Address.FromBase58("YgRDkJECvrJsfcrM3KbjMjNSPfZPhmbrPjTpssWiWZmGxGiWy")
        };
        logEventContext = GenerateLogEventContext(authorUpdatedEvent);
        await _authorUpdatedProcessor.ProcessAsync(logEventContext);

        await SaveDataAsync();

        contractInfo =
            (await _contractInfoRepository.GetQueryableAsync()).Where(o => o.Id == contractInfoId).ToList()[0];
        contractInfo.Author.ShouldBe(authorUpdatedEvent.NewAuthor.ToBase58());

        contractRecords = await Query.ContractRecord(_contractRecordRepository, _objectMapper, new GetContractRecordDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        contractRecords.Count.ShouldBe(3);
        contractRecords[2].ContractInfo.Address.ShouldBe(codeUpdatedEvent.Address.ToBase58());
        contractRecords[2].ContractInfo.CodeHash.ShouldBe(contractInfo.CodeHash);
        contractRecords[2].ContractInfo.ContractVersion.ShouldBe(codeUpdatedEvent.ContractVersion);
        contractRecords[2].ContractInfo.Version.ShouldBe(codeUpdatedEvent.Version);
        contractRecords[2].Metadata.ChainId.ShouldBe(logEventContext.ChainId);
        contractRecords[2].ContractInfo.ContractCategory.ShouldBe(codeCheckRequiredEventUpdate.Category);
        contractRecords[2].ContractInfo.ContractType.ShouldBe(ContractType.UserContract);
        contractRecords[2].Operator.ShouldBe(FromAddress);
        contractRecords[2].TransactionId.ShouldBe(logEventContext.Transaction.TransactionId);
        contractRecords[2].OperationType.ShouldBe(ContractOperationType.SetAuthor);
        contractRecords[2].ContractInfo.Author.ShouldBe(authorUpdatedEvent.NewAuthor.ToBase58());
    }
}