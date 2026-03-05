namespace Blackjack.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public decimal Balance { get; set; } = 1000m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
