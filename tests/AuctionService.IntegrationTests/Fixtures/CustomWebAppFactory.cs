using System;
using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures;

// What this class is supposed to do is create a test instance of our web application and we can add test services inside here and we can re-use that among each of the tests
// program.cs is loaded first
public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
  public async Task InitializeAsync()
  {
    // Starts a running instance of our test container's database server 
    await _postgreSqlContainer.StartAsync();
  }

  // Anything read from the program class is overridden here
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      services.RemoveDbContext<AuctionDbContext>();

      // Add the postgresql test container as the auctiondbcontext instead
      services.AddDbContext<AuctionDbContext>(options =>
      {
        options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
      });

      // Replaces masstransit configuration in program.cs with the test harness
      services.AddMassTransitTestHarness();

      services.EnsureCreated<AuctionDbContext>();

      // Set up fake authentication for testing without needing to go to IdentityServer
      services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
        .AddFakeJwtBearer(opt =>
        {
          opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
        });
    });
  }

  Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}
