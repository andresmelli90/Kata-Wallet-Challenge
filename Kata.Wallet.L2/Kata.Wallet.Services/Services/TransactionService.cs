using Kata.Wallet.Database;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.Services.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IWalletService _walletService;
        private readonly DataContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IWalletService walletService, DataContext context, ILogger<TransactionService> logger)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new transaction between two wallets.
        /// </summary>
        /// <param name="transactionData">The transaction data.</param>
        /// <exception cref="ArgumentException">If any of the wallets does not exist.</exception>
        /// <exception cref="InvalidOperationException">If the wallets do not have the same currency or if the wallet of origin does not have sufficient balance.</exception>
        public async Task CreateAsync(TransactionRequestDto transactionData)
        {
            if (transactionData == null)
            {
                throw new ArgumentNullException(nameof(transactionData));
            }

            try
            {
                // Validate if the wallets exist
                var sourceWallet = await _walletService.GetByIdAsync(transactionData.SourceWalletId);
                var destinationWallet = await _walletService.GetByIdAsync(transactionData.DestinationWalletId);

                if (sourceWallet == null)
                {
                    throw new ArgumentException("Non-existent source wallet.");
                }

                if (destinationWallet == null)
                {
                    throw new ArgumentException("Non-existent destination wallet.");
                }

                // Validate that wallets have the same currency
                if (sourceWallet.Currency != destinationWallet.Currency)
                {
                    throw new InvalidOperationException("Wallets must have the same currency.");
                }

                // Validate whether the originating wallet has sufficient balance
                if (sourceWallet.Balance < transactionData.Amount)
                {
                    throw new InvalidOperationException("Insufficient balance in the originating wallet.");
                }

                // Create the transaction
                var transaction = new Domain.Transaction
                {
                    WalletIncoming = sourceWallet,
                    WalletOutgoing = destinationWallet,
                    Amount = transactionData.Amount,
                    Date = DateTime.UtcNow,
                    Description = transactionData.Description
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Update wallet balances
                sourceWallet.Balance -= transactionData.Amount;
                destinationWallet.Balance += transactionData.Amount;

                await _walletService.UpdateAsync(sourceWallet);
                await _walletService.UpdateAsync(destinationWallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction between source wallet {SourceWalletId} and destination wallet {DestinationWalletId}.", transactionData.SourceWalletId, transactionData.DestinationWalletId);
                throw;
            }
        }

        /// <summary>
        /// Gets all transactions associated with a specific wallet.
        /// </summary>
        /// <param name="walletId">The wallet ID.</param>
        /// <returns>A list of transactions.</returns>
        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsByWalletAsync(int walletId)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Where(t => t.WalletIncoming.Id == walletId || t.WalletOutgoing.Id == walletId)
                    .OrderByDescending(t => t.Date)
                    .Select(t => new TransactionResponseDto
                    {
                        Id = t.Id,
                        Amount = t.Amount,
                        Date = t.Date,
                        Description = t.Description
                    })
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions from wallet {WalletId}", walletId);
                throw; 
            }
        }
    }
}