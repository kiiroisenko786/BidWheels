import React from 'react'
import AuctionCard from './AuctionCard';

async function getData() {
    const res = await fetch('http://localhost:6001/search?pageSize=10');

    if (!res.ok) throw new Error("Failed to fetch data");

    return res.json();
}

export default async function Listings() {
    const data = await getData();
    
    return (
        <div className='grid grid-cols-4 gap-6'>
            {
            /* check if data is available, then map iterates over results array in data, each results element = auction object, map takes each auction and returns react component. auction object is currently of any type, then auctioncard component is rendered for each auction object, passing the auction prop. react needs a unique key when rendering a list of elements, so we can use the auction id as a key*/
            }
            {data && data.results.map((auction: any) => (
                <AuctionCard auction={auction} key={auction.id} />
            ))}
        </div>
    )
}
