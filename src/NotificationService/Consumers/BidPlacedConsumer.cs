using System;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
  private readonly IHubContext<NotificationHub> _hubContext;

  public BidPlacedConsumer(IHubContext<NotificationHub> hubContext)
  {
    _hubContext = hubContext;
  }
  public async Task Consume(ConsumeContext<BidPlaced> context)
  {
    Console.WriteLine("--> bid placed message received");

    // Send to all connected clients because we are not authenticating so we can't choose who to send to
    // also if this service is scaled, conntection tracking becomes complicated, so this keeps it simple
    await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
  }
}