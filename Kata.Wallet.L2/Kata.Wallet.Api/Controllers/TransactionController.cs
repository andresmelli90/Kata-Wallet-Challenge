using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets transactions from a specific wallet.
        /// </summary>
        /// <param name="walletId">The wallet ID.</param>
        /// <returns>A list of transactions.</returns>
        [HttpGet]
        public async Task<ActionResult<WalletDto>> GetTransactions(int walletId)
        {
            try
            {
                var wallet = await _transactionService.GetTransactionsByWalletAsync(walletId);

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions from wallet {WalletId}", walletId);

                return Problem(
                    title: "An error occurred when obtaining transactions.",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }

        /// <summary>
        /// Processes a new transaction.
        /// </summary>
        /// <param name="transactionData">The transaction data.</param>
        /// <returns>A success or error message.</returns>
        [HttpPost]
        public async Task<ActionResult> ProcessTransaction([FromBody] TransactionRequestDto transactionData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _transactionService.CreateAsync(transactionData);

                return Ok(new { Message = "Transfer completed successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when processing the transaction.");

                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid argument",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid transaction when processing the transaction.");

                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid operation",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while processing the transaction.");

                return Problem(
                    title: "An internal error occurred",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }
    }
}