using System;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using ZstdSharp.Unsafe;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> CONSUMING BID PLACED EVENT");

        // Get auction but using mongodb 
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}
