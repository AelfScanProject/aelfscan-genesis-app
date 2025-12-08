using AElfScan.GenesisApp.Entities;
using AElfScan.GenesisApp.GraphQL;
using AElf.Standards.ACS0;
using AElf.Types;
using AutoMapper;
using Google.Protobuf;
using Volo.Abp.AutoMapper;

namespace AElfScan.GenesisApp;

public class GenesisAppMapperProfile : Profile
{
    public GenesisAppMapperProfile()
    {
        // Common
        CreateMap<Hash, string>().ConvertUsing(s => s == null ? string.Empty : s.ToHex());
        CreateMap<Address, string>().ConvertUsing(s => s.ToBase58());
        CreateMap<ByteString, string>().ConvertUsing(s => s.ToBase64());

        CreateMap<CodeUpdated, Entities.ContractInfo>();
        CreateMap<ContractRegistration, Entities.ContractInfo>()
            .Ignore(o => o.Id);
        CreateMap<ContractDeployed, Entities.ContractInfo>()
            .ForMember(d => d.NameHash, opt => opt.MapFrom(s => s.Name.ToHex()));
        CreateMap<Entities.ContractInfo, ContractRecord>()
            .Ignore(o => o.Id);
        
        CreateMap<CodeCheckRequired, ContractRegistration>()
            .ForMember(d => d.ContractCategory, opt => opt.MapFrom(s => s.Category))
            .ForMember(d => d.ContractType,
                opt => opt.MapFrom(s => s.IsSystemContract ? ContractType.SystemContract : ContractType.UserContract));

        CreateMap<ContractProposed, ContractProposeInfo>();

        CreateMap<ContractRegistration, ContractRegistrationDto>();
        CreateMap<Entities.ContractInfo, ContractInfoDto>();
        CreateMap<Entities.ContractRecord, ContractRecordDto>();
    }
}