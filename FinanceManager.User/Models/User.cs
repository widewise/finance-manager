using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanceManager.User.Models;

public class User: IdentityUser<long>
{
    [NotMapped]
    public string Username
    {
        get => base.UserName;
        set => base.UserName = value;
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    [NotMapped]
    public string Phone
    {
        get => base.PhoneNumber;
        set => base.PhoneNumber = value;
    }
}