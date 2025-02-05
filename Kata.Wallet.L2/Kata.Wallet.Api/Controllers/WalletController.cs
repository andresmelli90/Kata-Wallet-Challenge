using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all wallets, optionally filtered by currency and user document.
        /// </summary>
        /// <param name="currency">Currency to filter wallets (optional).</param>
        /// <param name="userDocument">User document to filter wallets (optional).</param>
        /// <returns>A list of wallets.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Domain.Wallet>>> GetAll([FromQuery] string? currency, [FromQuery] string? userDocument)
        {
            try
            {
                var wallets = await _walletService.GetWalletsAsync(currency, userDocument);

                return Ok(wallets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when obtaining wallets with currency: {Currency} and user document: {UserDocument}.", currency, userDocument);
                
                return Problem(
                    title: "An error occurred when obtaining wallets.",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }

        /// <summary>
        /// Creates a new wallet.
        /// </summary>
        /// <param name="wallet">Data of the wallet to be created.</param>
        /// <returns>The wallet created.</returns>
        [HttpPost]
        public async Task<ActionResult<Domain.Wallet>> Create([FromBody] WalletDto wallet)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid wallet data.");

                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid wallet data",
                    Detail = "The data provided are not valid.",
                    Status = 400
                });
            }

            try
            {
                var walletEntity = await _walletService.CreateAsync(wallet);

                return CreatedAtAction(nameof(Create), new { id = walletEntity.Id }, walletEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating the wallet.");

                return Problem(
                    title: "An error occurred while creating the wallet.",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }
    }
}