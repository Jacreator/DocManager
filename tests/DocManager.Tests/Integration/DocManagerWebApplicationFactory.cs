using System.Data;
using System.Globalization;
using DocManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DocManager.Tests.Integration;

/// <summary>
/// Runs the real web app against an in-memory SQLite database containing the seed data
/// (companies 1 and 2). Clients act as company 1 unless created with <see cref="CreateClientForCompany"/>.
/// </summary>
public sealed class DocManagerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly string _storagePath = Path.Combine(Path.GetTempPath(), "docmanager-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates a client whose requests run as the given company, sharing the same database.</summary>
    public HttpClient CreateClientForCompany(int companyId) =>
        WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Company:DefaultCompanyId"] = companyId.ToString(CultureInfo.InvariantCulture)
                })))
            .CreateClient();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:RootPath"] = _storagePath
            }));

        builder.ConfigureServices(services =>
        {
            // EF Core 9+ keeps the provider setup in IDbContextOptionsConfiguration<T>; drop SQL Server's.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Also called for each per-company client, which shares this connection.
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
