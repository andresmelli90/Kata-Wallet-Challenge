using AutoMapper;
using Kata.Wallet.Dtos;

namespace Kata.Wallet.Api.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Domain.Transaction, TransactionResponseDto>();
        CreateMap<TransactionResponseDto, Domain.Transaction>();
        CreateMap<Domain.Wallet, WalletDto>();
        CreateMap<WalletDto, Domain.Wallet>();
    }
}
