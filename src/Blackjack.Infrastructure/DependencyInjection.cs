namespace Blackjack.Infrastructure;

using Blackjack.Infrastructure.Data;
using Blackjack.Infrastructure.Identity;
using Blackjack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBlackjackInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BlackjackDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddBlackjackIdentity();

        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IGameHistoryRepository, GameHistoryRepository>();

        return services;
    }
}
