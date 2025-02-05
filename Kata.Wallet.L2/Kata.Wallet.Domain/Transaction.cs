namespace Kata.Wallet.Domain;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Wallet WalletIncoming { get; set; }
    public Wallet WalletOutgoing { get; set; }
}
