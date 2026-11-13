using System.Net;

using EasyStock.Tests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Web;

[Collection(DatabaseCollection.Name)]
public class WebApplicationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private const string ConnectionStringVariable = "ConnectionStrings__DefaultConnection";

    private string connectionString = string.Empty;
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        connectionString = fixture.CreateDatabaseConnectionString();

        await using (var db = PostgresFixture.CreateContext(connectionString))
        {
            await db.Database.MigrateAsync();
        }

        Environment.SetEnvironmentVariable(ConnectionStringVariable, connectionString);

        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DisposeAsync();
        Environment.SetEnvironmentVariable(ConnectionStringVariable, null);

        await using var db = PostgresFixture.CreateContext(connectionString);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task AnonymousUser_IsRedirectedToTheLoginPage()
    {
        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LoginPage_IsAvailableToAnonymousUsers()
    {
        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_IsAnonymousAndHealthy()
    {
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}