using System;
using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
  public static void RemoveDbContext<T>(this IServiceCollection services)
  {
    // Gives us the auctiondbcontext
    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

    // if we have the dbcontext, remove it (so we can replace it with the test container)
    if (descriptor != null) services.Remove(descriptor);
  }

  public static void EnsureCreated<T>(this IServiceCollection services)
  {
    // Set up test database with the schema that we use
    var sp = services.BuildServiceProvider();

    using var scope = sp.CreateScope();
    var scopedServices = scope.ServiceProvider;
    var db = scopedServices.GetRequiredService<AuctionDbContext>();

    db.Database.Migrate();

    // Initialize the database with test data
    DbHelper.InitDbForTests(db);
  }
}
