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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
// ControllerBase doesn't provide view support because this is just an endpoint
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(IAuctionRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
    _repo = repo;
    _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    // Get all auctions but after a certain date
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _repo.GetAuctionsAsync(date);
    }

    // Get specific auction
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid Id)
    {
        var auction = await _repo.GetAuctionByIdAsync(Id);

        if (auction == null) return NotFound();

        return auction;
    }


    // Create auction
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        
        // This will give us username that we can use to populate seller
        auction.Seller = User.Identity.Name;

        _repo.AddAuction(auction);

        // Shape new auction as auctiondto
        var newAuction = _mapper.Map<AuctionDto>(auction);

        // Publish new auctiondto to consumer
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not save changes to db");

        return CreatedAtAction(nameof(GetAuctionById),
            new {auction.Id}, newAuction);
    }

    // Update auction
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _repo.GetAuctionEntityById(id);

        if (auction == null) return NotFound();
        
        // Check if the seller is the same as the user, if not, return forbidden
        if (auction.Seller != User.Identity.Name) return Forbid();

        // Auction is updated if there is a difference
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Colour = updateAuctionDto.Colour ?? auction.Item.Colour;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        // If service bus is down, auction is still saved to messageoutbox
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _repo.SaveChangesAsync();

        if (result) return Ok();

        return BadRequest("Problem saving changes");
    }

    // Wouldn't actually be included in live, but for the purpose of learning is included
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _repo.GetAuctionEntityById(id);

        if (auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Forbid();

        _repo.RemoveAuction(auction);

        // Auction id is GUID so it has to be converted to string
        await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});

        var result = await _repo.SaveChangesAsync();

        if (!result) return BadRequest("Could not delete auction");

        return Ok();
    }
}