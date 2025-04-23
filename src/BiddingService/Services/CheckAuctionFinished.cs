using System;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MassTransit.Initializers.PropertyConverters;
using MongoDB.Entities;

namespace BiddingService.Services;

// This will be a singleton; will run when application starts and will only stops when it shuts down
public class CheckAuctionFinished : BackgroundService
{
  private readonly ILogger<CheckAuctionFinished> _logger;
  private readonly IServiceProvider _services;

  public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
  {
    _logger = logger;
    _services = services;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting check for finished auction");  

    stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

    while (!stoppingToken.IsCancellationRequested) {
      await CheckAuctions(stoppingToken);
    
      // Wait for 5 seconds before checking again
      await Task.Delay(5000, stoppingToken);
    }
  }

  private async Task CheckAuctions(CancellationToken stoppingToken)
  {
    var finishedAuctions = await DB.Find<Auction>()
      .Match(x => x.AuctionEnd <= DateTime.UtcNow)
      .Match(x => !x.Finished)
      .ExecuteAsync(stoppingToken);

    if (finishedAuctions.Count == 0) return;

    _logger.LogInformation("==> Found {count} finished auctions", finishedAuctions.Count);

    /* this background service is a singleton, masstransit's lifetime is scoped to the scope of the request
    *  so we can't inject something that has a different lifetime into the background service
    *  so we have to create a scope inside this to have access to the IPublish endpoint and publish the finished auction event
    */

    using var scope = _services.CreateScope();
    var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

    foreach (var auction in finishedAuctions)
    {
      auction.Finished = true;
      await auction.SaveAsync(null, stoppingToken);

      var winningBid = await DB.Find<Bid>()
        .Match(a => a.AuctionId == auction.ID)
        .Match(b => b.BidStatus == BidStatus.Accepted)
        .Sort(x => x.Descending(s => s.Amount))
        .ExecuteFirstAsync(stoppingToken);
    
      await endpoint.Publish(new AuctionFinished
      {
        ItemSold = winningBid != null,
        AuctionId = auction.ID,
        Winner = winningBid?.Bidder,
        Amount = winningBid?.Amount,
        Seller = auction.Seller,
      }, stoppingToken);
    }
  }
}
