using Kata.Wallet.Domain;

namespace Kata.Wallet.Dtos;

public class WalletDto
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public string? UserDocument { get; set; }
    public string? UserName { get; set; }
    public Currency Currency { get; set; }
}
