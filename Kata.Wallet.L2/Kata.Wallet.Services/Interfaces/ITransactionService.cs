using Kata.Wallet.Dtos;

namespace Kata.Wallet.Services.Interfaces
{
    public interface ITransactionService
    {
        Task CreateAsync(TransactionRequestDto transactionData);
        Task<IEnumerable<TransactionResponseDto>> GetTransactionsByWalletAsync(int walletId);
    }
}
