using Kata.Wallet.Domain;
using System.ComponentModel.DataAnnotations;

namespace Kata.Wallet.Dtos;

public class WalletDto
{
    public int Id { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "The balance cannot be negative.")]
    public decimal Balance { get; set; }

    [StringLength(20, ErrorMessage = "The document cannot be longer than 20 characters.")]
    public string? UserDocument { get; set; }

    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters..")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    public Currency Currency { get; set; }
}
