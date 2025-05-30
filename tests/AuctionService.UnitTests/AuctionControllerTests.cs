using System;
using System.ComponentModel;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
  private readonly Mock<IAuctionRepository> _auctionRepo;
  private readonly Mock<IPublishEndpoint> _publishEndpoint;
  private readonly Fixture _fixture;
  private readonly AuctionsController _controller;
  private readonly IMapper _mapper;
  public AuctionControllerTests()
  {
    _fixture = new Fixture();
    _auctionRepo = new Mock<IAuctionRepository>();
    _publishEndpoint = new Mock<IPublishEndpoint>();

    var mockMapper = new MapperConfiguration(mc =>
    {
      mc.AddMaps(typeof(MappingProfiles).Assembly);
    }).CreateMapper().ConfigurationProvider;

    _mapper = new Mapper(mockMapper);
    _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
    // Give controller a user so that it can pass id checks using the GetClaimsPrinciple method
    // from the Helpers class
    {
      ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
      }
    };
  }

  // TESTS ===================================================================================================
  [Fact]
  public async Task GetAuctions_WithNoParams_Returns10Auctions()
  {
    // Arrange example auctions for mock repo to return
    var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
    _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

    // Act
    var result = await _controller.GetAllAuctions(null);

    // Assert
    Assert.Equal(10, result.Value.Count);
    Assert.IsType<ActionResult<List<AuctionDto>>>(result);
  }

  [Fact]
  public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
  {
    // Arrange example auction for mock repo to return
    var auction = _fixture.Create<AuctionDto>();
    // Guid doesn't matter because we're not testing the database, this is just a mock
    _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

    // Act
    var result = await _controller.GetAuctionById(auction.Id);

    // Assert
    Assert.Equal(auction.Make, result.Value.Make);
    Assert.IsType<ActionResult<AuctionDto>>(result);
  }

  [Fact]
  public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
  {
    // Arrange
    // Don't need to create an auction because we're testing the case where it doesn't exist
    _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

    // Act
    // New guid because it doesn't exist
    var result = await _controller.GetAuctionById(Guid.NewGuid());

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
  {
    // Arrange
    var auction = _fixture.Create<CreateAuctionDto>();
    // No return because the method is void
    _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
    _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

    // Act
    var result = await _controller.CreateAuction(auction);
    // Cast result to CreatedAtActionResult
    var createdResult = result.Result as CreatedAtActionResult;

    // Assert
    Assert.NotNull(createdResult);
    Assert.Equal("GetAuctionById", createdResult.ActionName);
    Assert.IsType<AuctionDto>(createdResult.Value);
  }

  [Fact]
  public async Task CreateAuction_FailedSave_Returns400BadRequest()
  {
    // Arrange
    var auctionDto = _fixture.Create<CreateAuctionDto>();
    _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
    // We need to return false to simulate a failed save
    _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

    // Act
    var result = await _controller.CreateAuction(auctionDto);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
  }

  [Fact]
  public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
    auction.Seller = "test";
    var updateDto = _fixture.Create<UpdateAuctionDto>();
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
    _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

    // Act
    var result = await _controller.UpdateAuction(auction.Id, updateDto);

    // Assert
    Assert.IsType<OkResult>(result);
  }

  [Fact]
  public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    var updateDto = _fixture.Create<UpdateAuctionDto>();
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

    // Act
    var result = await _controller.UpdateAuction(auction.Id, updateDto);

    // Assert
    Assert.IsType<ForbidResult>(result);
  }

  [Fact]
  public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    var updateDto = _fixture.Create<UpdateAuctionDto>();
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

    // Act
    var result = await _controller.UpdateAuction(auction.Id, updateDto);

    // Assert
    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    auction.Seller = "test";
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
    _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

    // Act
    var result = await _controller.DeleteAuction(auction.Id);

    // Assert
    Assert.IsType<OkResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    auction.Seller = "test";
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

    // Act
    var result = await _controller.DeleteAuction(auction.Id);

    // Assert
    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task DeleteAuction_WithInvalidUser_Returns403Response()
  {
    // Arrange
    var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
    _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: auction);

    // Act
    var result = await _controller.DeleteAuction(auction.Id);

    // Assert
    Assert.IsType<ForbidResult>(result);
  }
// END OF TESTS ==============================================================================================
}
