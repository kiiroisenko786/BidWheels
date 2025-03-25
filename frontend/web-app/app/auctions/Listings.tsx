'use client'

import React, { useEffect, useState } from 'react'
import AuctionCard from './AuctionCard';
import { Auction } from '@/types';
import AppPagination from '../components/AppPagination';
import { getData } from '../actions/auctionActions';
import Filters from './Filters';


export default function Listings() {
    // array of auctions, state updater function, usestate hook with type of auction array, initial state is empty array
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [pageCount, setPageCount] = useState(0);
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(4);

    // useEffect is like a side effect when the listing component first loads, and then depending on what happens, it may cause the component to re-render
    useEffect(() => {
        getData(pageNumber, pageSize).then(data=> {
            setAuctions(data.results);
            setPageCount(data.pageCount);

        })
    // useEffect will run whenever pageNumber changes as this second array is what useEffect is dependent on, i.e if pageNumber changes, the useEffect will run again
    }, [pageNumber, pageSize]);

    if (auctions.length === 0) return <h3>Loading...</h3>

    return (
        // React fragment, allows us to return multiple because react components can only return one element by putting them in empty tags
        <>
            <Filters pageSize={pageSize} setPageSize={setPageSize}/>
            <div className='grid grid-cols-4 gap-6'>
                {
                /* check if auctions array is populated, then map iterates over results array in auctions, each results element = auction object, map takes each auction and returns react component. auction object is of specific auction type, then auctioncard component is rendered for each auction object, passing the auction prop. react needs a unique key when rendering a list of elements, so we can use the auction id as a key*/
                }
                {auctions.map(auction => (
                    <AuctionCard auction={auction} key={auction.id} />
                ))}
            </div>
            <div className='flex justify-center mt-4'>
                <AppPagination pageChanged={setPageNumber} currentPage={pageNumber} pageCount={pageCount} />
            </div>
        </>
        
    )
}
