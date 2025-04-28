using System;
using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using ZstdSharp.Unsafe;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IPublishEndpoint _publishEndpoint;
  private readonly GrpcAuctionClient _grpcClient;

  public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcClient)
  {
    _mapper = mapper;
    _publishEndpoint = publishEndpoint;
    _grpcClient = grpcClient;
  }

  [Authorize]
  [HttpPost]
  public async Task<ActionResult<BidDTO>> PlaceBid(string auctionId, int amount)
  {
    var auction = await DB.Find<Auction>().OneAsync(auctionId);

    if (auction == null)
    {
      Console.WriteLine("trying to get auction from grpc service");
      auction = _grpcClient.GetAuction(auctionId);

      if (auction == null) return BadRequest("Cannot accept bids on this auction at this time");
    }

    // check if seller is auction owner
    if (auction.Seller == User.Identity.Name)
    {
      Console.WriteLine(auction.Seller + " " + User.Identity.Name);
      return BadRequest("You cannot bid on your own auction.");
    }

    // set new bid
    var bid = new Bid
    {
      Amount = amount,
      AuctionId = auctionId,
      Bidder = User.Identity.Name
    };

    // check if auction is finished
    if (auction.AuctionEnd < DateTime.UtcNow)
    {
      bid.BidStatus = BidStatus.Finished;
    // otherwise find the current highest bid for the auction
    } else {
      var highBid = await DB.Find<Bid>()
        .Match(a => a.AuctionId == auctionId)
        .Sort(b => b.Descending(x => x.Amount))
        .ExecuteFirstAsync();

      // check if there is no existing high bid or if new amount is greater than current high bid
      if (highBid != null && amount > highBid.Amount || highBid == null)
      {
        // if bid is bigger than auction reserve price, accept it, otherwise accept it but mark it as below reserve
        bid.BidStatus = amount > auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
      }

      // if there is a high bid and the new bid is not higher, mark it as too low
      if (highBid != null && bid.Amount <= highBid.Amount)
      {
        bid.BidStatus = BidStatus.TooLow;
      }
    }

    // save bid to db
    await DB.SaveAsync(bid);

    // make sure auctionservice and searchservice get updated with new highest bid
    await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

    return Ok(_mapper.Map<BidDTO>(bid));
  }

  [HttpGet("{auctionId}")]
  public async Task<ActionResult<List<BidDTO>>> GetBidsForAuction(string auctionId) {
    var bids = await DB.Find<Bid>()
      .Match(a => a.AuctionId == auctionId)
      .Sort(b => b.Descending(x => x.BidTime))
      .ExecuteAsync();

      return bids.Select(_mapper.Map<BidDTO>).ToList();
  }
}
