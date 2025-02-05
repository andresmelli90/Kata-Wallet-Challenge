using Kata.Wallet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kata.Wallet.Database;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration) => Configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase("WalletDb");
    }

    public DbSet<Domain.Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>().HasKey(x => x.Id);
        modelBuilder.Entity<Transaction>().Property(x => x.Amount).HasPrecision(16, 2).IsRequired();
        modelBuilder.Entity<Transaction>().HasOne(x => x.WalletIncoming).WithMany(x => x.IncomingTransactions);
        modelBuilder.Entity<Transaction>().HasOne(x => x.WalletOutgoing).WithMany(x => x.OutgoingTransactions);
        modelBuilder.Entity<Domain.Wallet>().HasKey(x => x.Id);
        modelBuilder.Entity<Domain.Wallet>().Property(x => x.Balance).HasPrecision(16, 2).IsRequired();
        modelBuilder.Entity<Domain.Wallet>().Property(x => x.Currency).IsRequired();
        modelBuilder.Entity<Domain.Wallet>().HasMany(x => x.IncomingTransactions).WithOne(x => x.WalletIncoming);
        modelBuilder.Entity<Domain.Wallet>().HasMany(x => x.OutgoingTransactions).WithOne(x => x.WalletOutgoing);
    }
}
