using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

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
    }
}