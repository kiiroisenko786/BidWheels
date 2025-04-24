using System;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
  private readonly IHubContext<NotificationHub> _hubContext;

  public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext)
  {
    _hubContext = hubContext;
  }
  public async Task Consume(ConsumeContext<AuctionCreated> context)
  {
    Console.WriteLine("--> auction created message received");

    // Send to all connected clients because we are not authenticating so we can't choose who to send to
    // also if this service is scaled, conntection tracking becomes complicated, so this keeps it simple
    await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
  }}