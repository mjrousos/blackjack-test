using Blackjack.Infrastructure;
using Blackjack.Infrastructure.Data;
using Blackjack.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add infrastructure services (EF Core, Identity, repositories)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddBlackjackInfrastructure(connectionString);
builder.Services.AddScoped<Blackjack.Web.Services.GameSessionService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<BlackjackDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Auto-create database in development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BlackjackDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
