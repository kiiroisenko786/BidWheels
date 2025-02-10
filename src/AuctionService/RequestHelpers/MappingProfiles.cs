using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Map from Auction to AuctionDto, and include the Item property
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        // Map from Item to AuctionDto
        CreateMap<Item, AuctionDto>();
        // Map from CreateAuctionDto to Auction, because there are item properties
        // d = destination which is the item, o = option, s = source item
        CreateMap<CreateAuctionDto, Auction>()
        .ForMember(d => d.Item, o => o.MapFrom(s => s));
        // Map from CreateAuctionDto to related Item
        CreateMap<CreateAuctionDto, Item>();
        /*  Map from auctiondto to auctioncreated
            auctiondto cannot be known from the searchservice, so we need to have something
            in between and that is the contract which is auctioncreated as it is known
            by both auctionservice and searchservice */
        CreateMap<AuctionDto, AuctionCreated>();
    }
}