using System.ComponentModel.DataAnnotations;

namespace FinanceManager.User.Models;

public class UserModel
{
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Phone { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}