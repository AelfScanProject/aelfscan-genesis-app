using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AeFinder.Sdk;
using GraphQL;
using Volo.Abp.ObjectMapping;

namespace AElfScan.GenesisApp.GraphQL;

public class Query
{
    public static async Task<List<ContractInfoDto>> ContractInfo(
        [FromServices] IReadOnlyRepository<Entities.ContractInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetContractInfoDto input)
    {
        input.Validate();

        var queryable = await repository.GetQueryableAsync();
        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }

        if (!input.Address.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Address == input.Address);
        }

        if (!input.Author.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Author == input.Author);
        }

        if (!input.AddressList.IsNullOrEmpty())
        {
            var predicates = input.AddressList.Select(s =>
                (Expression<Func<Entities.ContractInfo, bool>>)(o => o.Address == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }


        // queryable = queryable.OrderBy(o => o.Metadata.Block.BlockHeight);

        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return objectMapper.Map<List<Entities.ContractInfo>, List<ContractInfoDto>>(result);
    }

    public static async Task<List<ContractRecordDto>> ContractRecord(
        [FromServices] IReadOnlyRepository<Entities.ContractRecord> repository,
        [FromServices] IObjectMapper objectMapper, GetContractRecordDto input)
    {
        input.Validate();

        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }

        if (!input.Address.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.ContractInfo.Address == input.Address);
        }

        if (!input.Author.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.ContractInfo.Author == input.Author);
        }

        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();

        return objectMapper.Map<List<Entities.ContractRecord>, List<ContractRecordDto>>(result);
    }

    public static async Task<List<ContractRegistrationDto>> ContractRegistration(
        [FromServices] IReadOnlyRepository<Entities.ContractRegistration> repository,
        [FromServices] IObjectMapper objectMapper, GetContractRegistrationDto input)
    {
        input.Validate();

        var queryable = await repository.GetQueryableAsync();

        var result = queryable.Where(o => o.Metadata.ChainId == input.ChainId).Where(o => o.CodeHash == input.CodeHash)
            .ToList();
        return objectMapper.Map<List<Entities.ContractRegistration>, List<ContractRegistrationDto>>(result);
    }
}