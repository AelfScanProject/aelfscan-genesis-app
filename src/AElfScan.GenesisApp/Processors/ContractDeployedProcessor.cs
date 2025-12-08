using AElfScan.GenesisApp.Entities;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElf.Standards.ACS0;
using AElf.Types;

namespace AElfScan.GenesisApp.Processors;

public class ContractDeployedProcessor : GenesisProcessorBase<ContractDeployed>
{
    private readonly IBlockChainService _blockChainService;

    public ContractDeployedProcessor(IBlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }

    public override async Task ProcessAsync(ContractDeployed logEvent, LogEventContext context)
    {
        var id = GetContractInfoId(context.ChainId, logEvent.Address);

        var contractInfo = new Entities.ContractInfo()
        {
            Id = id
        };


        ObjectMapper.Map(logEvent, contractInfo);

        var contractRegistration =
            await GetEntityAsync<ContractRegistration>(
                GetContractRegistrationId(context.ChainId, logEvent.CodeHash.ToHex()));
        if (contractRegistration == null)
        {
            // Only for system contracts
            var smartContractRegistration = await _blockChainService.ViewContractAsync<SmartContractRegistration>(
                context.ChainId, GetContractAddress(context.ChainId),
                "GetSmartContractRegistrationByCodeHash", logEvent.CodeHash);

            var codeHashHex = logEvent.CodeHash.ToHex();
            contractRegistration = new ContractRegistration
            {
                Id = GetContractRegistrationId(context.ChainId, codeHashHex),
                CodeHash = codeHashHex,
                Code = smartContractRegistration.Code.ToBase64(),
                ContractCategory = smartContractRegistration.Category,
                ContractType = ContractType.SystemContract
            };
            await SaveEntityAsync(contractRegistration);
        }

        ObjectMapper.Map(contractRegistration, contractInfo);
        await SaveEntityAsync(contractInfo);

        await AddRecordAsync(ContractOperationType.DeployContract, contractInfo, context);
    }
}