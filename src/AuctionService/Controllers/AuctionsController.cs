using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
// ControllerBase doesn't provide view support because this is just an endpoint
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    // Get all auctions but after a certain date
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        // Use AsQueryable to make sure the type is IQueryable
        // This is because ordering changes the type of the query to IOrderedQueryable
        // so in order to make further queries, it must be IQueryable
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        // Replaced previous code to use automapper instead to project query to auctiondto
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    // Get specific auction
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid Id)
    {
        // Get auctions with item included, and get the first one that matches the Id
        var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == Id);

        if (auction == null) return NotFound();

        return _mapper.Map<AuctionDto>(auction);
    }


    // Create auction
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // add current user as seller
        auction.Seller = "test";

        _context.Add(auction);

        // Shape new auction as auctiondto
        var newAuction = _mapper.Map<AuctionDto>(auction);

        // Publish new auctiondto to consumer
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to db");

        return CreatedAtAction(nameof(GetAuctionById),
            new {auction.Id}, newAuction);
    }

    // Update auction
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();
        
        // check seller == username

        // Auction is updated if there is a difference
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Colour = updateAuctionDto.Colour ?? auction.Item.Colour;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok();

        return BadRequest("Problem saving changes");
    }

    // Wouldn't actually be included in live, but for the purpose of learning is included
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();

        // check seller == username

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not delete auction");

        return Ok();
    }
}