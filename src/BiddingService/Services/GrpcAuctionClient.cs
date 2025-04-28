using System;
using System.Diagnostics;
using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
  private readonly ILogger<GrpcAuctionClient> _logger;
  private readonly IConfiguration _config;

  public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
  {
    _logger = logger;
    _config = config;
  }

  public Auction GetAuction(string id)
  {
    _logger.LogInformation("Calling Grpc service");
    Console.WriteLine("Calling Grpc service");
    // Get grpc channel
    var channel = GrpcChannel.ForAddress(_config["GrpcAuction"]);
    // Create a new client
    var client = new GrpcAuction.GrpcAuctionClient(channel);
    // Create a new request
    var request = new GetAuctionRequest{Id = id};

    try
    {
      var reply = client.GetAuction(request);
      var auction = new Auction
      {
        ID = reply.Auction.Id,
        AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
        Seller = reply.Auction.Seller,
        ReservePrice = reply.Auction.ReservePrice
      };
      
      return auction;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Could not call grpc server");
      return null;
    }
  }
}
