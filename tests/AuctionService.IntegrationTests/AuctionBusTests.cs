using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
  private readonly CustomWebAppFactory _factory;
  private readonly HttpClient _httpClient;
  private readonly ITestHarness _testHarness;

  public AuctionBusTests(CustomWebAppFactory factory)
  {
    _factory = factory;
    _httpClient = _factory.CreateClient();
    _testHarness = _factory.Services.GetTestHarness();
  }

  // TESTS ===================================================================================================

  [Fact]
  public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
  {
    // Arrange
    var auction = GetAuctionForCreate();
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    // Act
    var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);

    // Assert
    response.EnsureSuccessStatusCode();
    Assert.True(await _testHarness.Published.Any<AuctionCreated>());
  }

  // END OF TESTS ============================================================================================

  public Task InitializeAsync() => Task.CompletedTask;

  public Task DisposeAsync()
  {
    // Reinitialize the database after every test
    // Get the scope the test is running in so we can access the database context
    using var scope = _factory.Services.CreateScope();

    // Reinit db
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    DbHelper.ReinitDbForTests(db);

    return Task.CompletedTask;
  }

  // This is a private method that will be used to create an auction
  private static CreateAuctionDto GetAuctionForCreate()
  {
    return new CreateAuctionDto
    {
      Make = "test",
      Model = "testModel",
      ImageUrl = "test",
      Colour = "test",
      Mileage = 10,
      Year = 2025,
      ReservePrice = 1000,
      AuctionEnd = DateTime.UtcNow.AddDays(5)
    };
  }
}
