import React from 'react'
import AuctionCard from './AuctionCard';
import { Auction, PagedResult } from '@/types';
import AppPagination from '../components/AppPagination';

// Promise just means what you're returning, so here we are returning a paged result of auction type so the listings function knows the data type is a paged result of auctions rather than any for type safety
async function getData(): Promise<PagedResult<Auction>> {
    const res = await fetch('http://localhost:6001/search?pageSize=4');

    if (!res.ok) throw new Error("Failed to fetch data");

    return res.json();
}

export default async function Listings() {
    const data = await getData();
    
    return (
        // React fragment, allows us to return multiple because react components can only return one element
        <>
            <div className='grid grid-cols-4 gap-6'>
                {
                /* check if data is available, then map iterates over results array in data, each results element = auction object, map takes each auction and returns react component. auction object is of specific auction type, then auctioncard component is rendered for each auction object, passing the auction prop. react needs a unique key when rendering a list of elements, so we can use the auction id as a key*/
                }
                {data && data.results.map(auction => (
                    <AuctionCard auction={auction} key={auction.id} />
                ))}
            </div>
            <div className='flex justify-center mt-4'>
                <AppPagination currentPage={1} pageCount={data.pageCount} />
            </div>
        </>
        
    )
}
