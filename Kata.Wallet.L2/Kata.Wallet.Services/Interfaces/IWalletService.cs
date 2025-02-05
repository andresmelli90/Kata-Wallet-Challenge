using Kata.Wallet.Dtos;

namespace Kata.Wallet.Services.Interfaces
{
    public interface IWalletService
    {
        Task<Domain.Wallet> CreateAsync(WalletDto walletDto);
        Task<Domain.Wallet?> GetByIdAsync(int id);
        Task<List<Domain.Wallet>> GetWalletsAsync(string? currency = null, string? userDocument = null);
        Task UpdateAsync(Domain.Wallet wallet);
    }
}
