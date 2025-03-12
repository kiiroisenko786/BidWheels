using System;

namespace Contracts;

public class AuctionFinished
{
    // Did the bid meet the reserve price and the auction finished
    public bool ItemSold { get; set; }
    public string  AuctionId { get; set; }
    public string Winner { get; set; }
    public string Seller { get; set; }
    public int? Amount { get; set; }
}
