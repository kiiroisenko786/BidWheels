using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    // This is just a very simple example of a unit test
    // Testing naming convention generally follows the pattern of [MethodName]_[Scenario]_[ExpectedResult]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        // Arrange test and set it up in the way we need it to test the functionality
        var auction = new Auction{Id = Guid.NewGuid(), ReservePrice = 10};

        // Act - execute the method that we want to test
        var result = auction.HasReservePrice();

        // Assert - assert that something is true, false, whatever we are testing
        // In this case we are asserting that the result is true
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceIsZero_True()
    {
        var auction = new Auction{Id = Guid.NewGuid(), ReservePrice = 0};

        var result = auction.HasReservePrice();

        Assert.False(result);
    }
}
