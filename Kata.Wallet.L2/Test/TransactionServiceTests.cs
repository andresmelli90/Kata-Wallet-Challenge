using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Interfaces;
using Kata.Wallet.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kata.Wallet.Tests
{
    public class TransactionServiceTests
    {
        private DataContext GetInMemoryContext()
        {
            var configuration = new ConfigurationBuilder().Build();
            var context = new DataContext(configuration);
            context.Database.EnsureCreated();
            return context;
        }

        private Mock<ILogger<TransactionService>> GetMockLogger()
        {
            return new Mock<ILogger<TransactionService>>();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTransaction_WhenValid()
        {
            // Arrange
            var context = GetInMemoryContext();
            var walletServiceMock = new Mock<IWalletService>();
            var mockLogger = GetMockLogger();
            var service = new TransactionService(walletServiceMock.Object, context, mockLogger.Object);

            var sourceWallet = new Domain.Wallet { Id = 16, Balance = 500, Currency = Currency.EUR };
            var destinationWallet = new Domain.Wallet { Id = 17, Balance = 200, Currency = Currency.EUR };

            walletServiceMock.Setup(w => w.GetByIdAsync(16)).ReturnsAsync(sourceWallet);
            walletServiceMock.Setup(w => w.GetByIdAsync(17)).ReturnsAsync(destinationWallet);

            var transactionDto = new TransactionRequestDto
            {
                SourceWalletId = 16,
                DestinationWalletId = 17,
                Amount = 100,
                Description = "Pago de servicio de internet"
            };

            // Act
            await service.CreateAsync(transactionDto);

            // Assert
            var transactions = await context.Transactions.ToListAsync();
            Assert.Single(transactions);
            Assert.Equal(400, sourceWallet.Balance);
            Assert.Equal(300, destinationWallet.Balance);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenSourceWalletNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var walletServiceMock = new Mock<IWalletService>();
            var mockLogger = GetMockLogger();
            var service = new TransactionService(walletServiceMock.Object, context, mockLogger.Object);

            walletServiceMock.Setup(w => w.GetByIdAsync(1)).ReturnsAsync((Domain.Wallet?)null);

            var transactionDto = new TransactionRequestDto
            {
                SourceWalletId = 1,
                DestinationWalletId = 2,
                Amount = 50,
                Description = "Transferencia a Jose"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(transactionDto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenDestinationWalletNotFound()
        {
            // Arrange
            var context = GetInMemoryContext();
            var walletServiceMock = new Mock<IWalletService>();
            var mockLogger = GetMockLogger();
            var service = new TransactionService(walletServiceMock.Object, context, mockLogger.Object);

            var sourceWallet = new Domain.Wallet { Id = 1, Balance = 500, Currency = Currency.USD };
            walletServiceMock.Setup(w => w.GetByIdAsync(1)).ReturnsAsync(sourceWallet);
            walletServiceMock.Setup(w => w.GetByIdAsync(2)).ReturnsAsync((Domain.Wallet?)null);

            var transactionDto = new TransactionRequestDto
            {
                SourceWalletId = 1,
                DestinationWalletId = 2,
                Amount = 50,
                Description = "Transferencia a Juan"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(transactionDto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenCurrenciesDoNotMatch()
        {
            // Arrange
            var context = GetInMemoryContext();
            var walletServiceMock = new Mock<IWalletService>();
            var mockLogger = GetMockLogger();
            var service = new TransactionService(walletServiceMock.Object, context, mockLogger.Object);

            var sourceWallet = new Domain.Wallet { Id = 1, Balance = 500, Currency = Currency.USD };
            var destinationWallet = new Domain.Wallet { Id = 2, Balance = 200, Currency = Currency.EUR };

            walletServiceMock.Setup(w => w.GetByIdAsync(1)).ReturnsAsync(sourceWallet);
            walletServiceMock.Setup(w => w.GetByIdAsync(2)).ReturnsAsync(destinationWallet);

            var transactionDto = new TransactionRequestDto
            {
                SourceWalletId = 1,
                DestinationWalletId = 2,
                Amount = 50,
                Description = "Pago de servicio de luz"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(transactionDto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenInsufficientBalance()
        {
            // Arrange
            var context = GetInMemoryContext();
            var walletServiceMock = new Mock<IWalletService>();
            var mockLogger = GetMockLogger();
            var service = new TransactionService(walletServiceMock.Object, context, mockLogger.Object);

            var sourceWallet = new Domain.Wallet { Id = 1, Balance = 30, Currency = Currency.USD };
            var destinationWallet = new Domain.Wallet { Id = 2, Balance = 200, Currency = Currency.USD };

            walletServiceMock.Setup(w => w.GetByIdAsync(1)).ReturnsAsync(sourceWallet);
            walletServiceMock.Setup(w => w.GetByIdAsync(2)).ReturnsAsync(destinationWallet);

            var transactionDto = new TransactionRequestDto
            {
                SourceWalletId = 1,
                DestinationWalletId = 2,
                Amount = 50,
                Description = "Compra de producto"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(transactionDto));
        }
    }
}
