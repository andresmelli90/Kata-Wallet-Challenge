using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.Services.Services
{
    public class WalletService : IWalletService
    {
        private readonly DataContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(DataContext context, ILogger<WalletService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new wallet.
        /// </summary>
        /// <param name="walletDto">Data of the wallet to be created.</param>
        /// <returns>The wallet created.</returns>
        /// <exception cref="ArgumentException">If the currency provided is not valid.</exception>
        public async Task<Domain.Wallet> CreateAsync(WalletDto walletDto)
        {
            if (walletDto == null)
            {
                throw new ArgumentNullException(nameof(walletDto));
            }

            try
            {
                // Validate that the currency provided is valid
                if (!Enum.IsDefined(typeof(Currency), walletDto.Currency))
                {
                    throw new ArgumentException("Invalid currency value. Allowed values are: USD, EUR, ARS.");
                }

                var wallet = new Domain.Wallet
                {
                    Id = walletDto.Id,
                    Balance = walletDto.Balance,
                    UserDocument = walletDto.UserDocument,
                    UserName = walletDto.UserName,
                    Currency = walletDto.Currency
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Wallet successfully created: {WalletId}", wallet.Id);
                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating the wallet with the data: {@WalletDto}", walletDto);
                throw;
            }
        }

        /// <summary>
        /// Gets a wallet for the ID.
        /// </summary>
        /// <param name="id">The wallet ID.</param>
        /// <returns>Wallet found or null if nonexistent.</returns>
        public async Task<Domain.Wallet?> GetByIdAsync(int id)
        {
            try
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(a => a.Id == id);

                if (wallet == null)
                {
                    _logger.LogWarning("Wallet not found with ID: {WalletId}", id);
                }

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when obtaining the wallet with ID: {WalletId}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets a list of wallets, optionally filtered by currency and user document.
        /// </summary>
        /// <param name="currency">Currency to filter wallets (optional).</param>
        /// <param name="userDocument">User document to filter wallets (optional).</param>
        /// <returns>A list of wallets.</returns>
        public async Task<List<Domain.Wallet>> GetWalletsAsync(string? currency = null, string? userDocument = null)
        {
            try
            {
                var query = _context.Wallets.AsQueryable();

                if (!string.IsNullOrEmpty(currency))
                {
                    query = query.Where(w => w.Currency.ToString() == currency);
                }

                if (!string.IsNullOrEmpty(userDocument))
                {
                    query = query.Where(w => w.UserDocument == userDocument);
                }

                var wallets = await query.ToListAsync();
                return wallets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when obtaining wallets with currency: {Currency} and user document: {UserDocument}.", currency, userDocument);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing wallet.
        /// </summary>
        /// <param name="wallet">The wallet to update.</param>
        public async Task UpdateAsync(Domain.Wallet wallet)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            try
            {
                _context.Wallets.Update(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Wallet successfully updated: {WalletId}", wallet.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wallet with ID: {WalletId}", wallet.Id);
                throw;
            }
        }
    }
}