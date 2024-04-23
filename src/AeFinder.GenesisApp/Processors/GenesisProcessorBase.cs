using AeFinder.GenesisApp.Entities;
using AeFinder.Sdk.Processor;
using AElf;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.ObjectMapping;

namespace AeFinder.GenesisApp.Processors;

public abstract class GenesisProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    public IGenesisContractAddressProvider ContractAddressProvider { get; set; }

    public override string GetContractAddress(string chainId)
    {
        return ContractAddressProvider.GetContractAddress(chainId);
    }
    
    protected string GetCodeHash(ByteString code)
    {
        return HashHelper.ComputeFrom(code.ToByteArray()).ToHex();
    }
    
    protected string GetContractInfoId(string chainId, Address address)
    {
        return IdGenerateHelper.GetId(chainId, address.ToBase58());
    }
    
    protected string GetContractRegistrationId(string chainId, string codeHash)
    {
        return IdGenerateHelper.GetId(chainId, codeHash);
    }
    
    protected string GetContractProposeInfoId(string chainId, string proposedContractInputHash)
    {
        return IdGenerateHelper.GetId(chainId, proposedContractInputHash);
    }

    protected async Task AddRecordAsync(ContractOperationType type, Entities.ContractInfo contractInfo, LogEventContext context)
    {
        var contractRecord = new ContractRecord
        {
            Id = Guid.NewGuid().ToString(),
            OperationType = type,
            TransactionId = context.Transaction.TransactionId,
            ContractInfo = contractInfo
        };

        if (type == ContractOperationType.SetAuthor)
        {
            contractRecord.Operator = context.Transaction.From;
        }
        else
        {
            contractRecord.Operator = await GetContractOperatorAsync(context, contractInfo.CodeHash);
        }

        await SaveEntityAsync(contractRecord);
    }

    protected async Task AddContractProposeInfoAsync(LogEventContext context, string proposedContractInputHash)
    {
        var id = GetContractProposeInfoId(context.ChainId, proposedContractInputHash);

        var contractProposeInfo = new Entities.ContractProposeInfo()
        {
            Id = id,
            ProposedContractInputHash = proposedContractInputHash,
            Proposer = context.Transaction.From
        };
        await SaveEntityAsync(contractProposeInfo);
    }

    protected async Task<string> GetContractOperatorAsync(LogEventContext context, string codeHash)
    {
        var contractRegistration =
            await GetEntityAsync<ContractRegistration>(GetContractRegistrationId(context.ChainId, codeHash));
        
        var id = GetContractProposeInfoId(context.ChainId, contractRegistration.ProposedContractInputHash);
        var contractProposeInfo = await GetEntityAsync<ContractProposeInfo>(id);
        return contractProposeInfo != null
            ? contractProposeInfo.Proposer
            : context.Transaction.From;
    }
}