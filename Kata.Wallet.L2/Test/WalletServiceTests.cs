using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test
{
    public class WalletServiceTests
    {
        private DataContext GetInMemoryContext()
        {
            var configuration = new ConfigurationBuilder().Build();
            var context = new DataContext(configuration);
            context.Database.EnsureCreated();

            return context;
        }

        private Mock<ILogger<WalletService>> GetMockLogger()
        {
            return new Mock<ILogger<WalletService>>();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateWallet()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            var walletDto = new WalletDto
            {
                Id = 10,
                Balance = 100.00m,
                UserDocument = "12774213",
                UserName = "CarlosPeralta",
                Currency = Currency.ARS
            };

            // Act
            var wallet = await service.CreateAsync(walletDto);

            // Assert
            Assert.NotNull(wallet);
            Assert.Equal(walletDto.Id, wallet.Id);
            Assert.Equal(walletDto.Balance, wallet.Balance);
            Assert.Equal(walletDto.UserDocument, wallet.UserDocument);
            Assert.Equal(walletDto.UserName, wallet.UserName);
            Assert.Equal(walletDto.Currency, wallet.Currency);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenInvalidCurrency()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            var walletDto = new WalletDto
            {
                Id = 20,
                Balance = 50.00m,
                UserDocument = "24772952",
                UserName = "MatiasGutierrez",
                Currency = (Currency)999 // Valor inválido
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(walletDto));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWallet_WhenExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            var wallet = new Wallet
            {
                Id = 30,
                Balance = 150.00m,
                UserDocument = "30563992",
                UserName = "JorgeVelazquez",
                Currency = Currency.EUR
            };

            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(wallet.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(wallet.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            // Act
            var result = await service.GetByIdAsync(99); // ID inexistente

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetWalletsAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            var wallet1 = new Wallet { Id = 40, Balance = 200.00m, UserDocument = "12984223", UserName = "PabloMiranda", Currency = Currency.USD };
            var wallet2 = new Wallet { Id = 50, Balance = 300.00m, UserDocument = "16982451", UserName = "FedericoBarrios", Currency = Currency.EUR };

            context.Wallets.AddRange(wallet1, wallet2);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetWalletsAsync(currency: "USD");

            // Assert
            Assert.Single(result);
            Assert.Equal(wallet1.Id, result[0].Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyWalletBalance()
        {
            // Arrange
            var context = GetInMemoryContext();
            var mockLogger = GetMockLogger();
            var service = new WalletService(context, mockLogger.Object);

            var wallet = new Wallet { Id = 60, Balance = 500.00m, UserDocument = "35982112", UserName = "JavierMorales", Currency = Currency.ARS };

            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();

            // Act
            wallet.Balance = 1000.00m;
            await service.UpdateAsync(wallet);

            var updatedWallet = await service.GetByIdAsync(wallet.Id);

            // Assert
            Assert.NotNull(updatedWallet);
            Assert.Equal(1000.00m, updatedWallet.Balance);
        }
    }
}