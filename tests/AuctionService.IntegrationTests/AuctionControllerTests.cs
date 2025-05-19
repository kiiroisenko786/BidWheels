using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
  private readonly CustomWebAppFactory _factory;
  private readonly HttpClient _httpClient;
  private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

  // Because this is not a fixture, this will initialize before every test in this class
  // and the dispose will activate after every test in this class

  public AuctionControllerTests(CustomWebAppFactory factory)
  {
    _factory = factory;
    _httpClient = _factory.CreateClient();
  }

  // TESTS ===================================================================================================

  [Fact]
  public async Task GetAuctions_ShouldReturn3Auctions()
  {
    // Arrange
    // Nothing needs to be arranged for this test, because the setup is done in the custom web app factory
    // and the http client, so we are just making a request to the endpoint

    // Act
    var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

    // Assert
    Assert.Equal(3, response.Count);
  }

  [Fact]
  public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
  {
    // No arrangement needed
    // Act
    var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

    // Assert
    Assert.Equal("GT", response.Model);
  }


  [Fact]
  public async Task GetAuctionById_WithInvalidId_ShouldReturn404()
  {
    // No arrangement needed

    // Act
    // Don't need to get an auctiondto for this test because we are expecting a 404
    var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400()
  {
    // No arrangement needed

    // Act
    // This should return a validation problem because the guid is not valid
    var response = await _httpClient.GetAsync("api/auctions/notaguid");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task CreateAuction_WithNoAuth_ShouldReturn401()
  {
    // Don't need to send much data for this test because it'll fail at authentication
    // though it wouldn't pass validation anyway
    var auction = new CreateAuctionDto { Make = "Test" };

    // Act
    // Send up auction as json
    var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CreateAuction_WithAuth_ShouldReturn201()
  {
    // Arrange
    // Use method to get test create auction dto, set fake jwt bearer token
    var auction = GetAuctionForCreate();
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    // Act
    // Send up auction as json
    var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

    // Assert
    response.EnsureSuccessStatusCode();
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    // Extract auction dto from the response
    var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
    Assert.Equal("bob", createdAuction.Seller);
  }

  [Fact]
  public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
  {
    // Arrange
    // This will need a valid auth but invalid create auction dto
    var auction = GetAuctionForCreate();
    auction.Model = null;
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    // Act
    // Send up auction as json
    var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
  {
    // Arrange
    var updateAuction = new UpdateAuctionDto { Make = "Updated" };
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

    // Act
    var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuction);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
  {
    // Arrange 
    var updateAuction = new UpdateAuctionDto { Make = "Updated" };
    _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("fakeid"));

    // Act
    var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuction);

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  // END OF TESTS ============================================================================================

  // Don't need to initialize anything here so we can just complete the task
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
