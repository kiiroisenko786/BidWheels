using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParameters searchParams)
    {
        // create query which is a paged search
        var query = DB.PagedSearch<Item, Item>();

        // Check if search term is not empty, if not, then match it
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        // Ordering by make and newest auction
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };

        // Filters for finished and ending soon auctions
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        // Check if seller matches
        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        // Check if winner matches
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}