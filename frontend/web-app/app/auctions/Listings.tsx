'use client'

import React, { useEffect, useState } from 'react'
import AuctionCard from './AuctionCard';
import { Auction, PagedResult } from '@/types';
import AppPagination from '../components/AppPagination';
import { getData } from '../actions/auctionActions';
import Filters from './Filters';
import { useParamsStore } from '@/hooks/useParamsStore';
import qs from 'query-string';
import { useShallow } from 'zustand/react/shallow';


export default function Listings() { // Define and export the Listings component
    const [data, setData] = useState<PagedResult<Auction>>(); // useState hook to store auction data, initially undefined
    
    // Extract parameters from a state store using a shallow comparison
    const params = useParamsStore(useShallow(state => ({
        pageNumber: state.pageNumber, // Current page number
        pageSize: state.pageSize, // Number of items per page
        searchTerm: state.searchTerm // Search term for filtering
    })));

    const setParams = useParamsStore(state => state.setParams); // Get the setParams function to update query parameters
    const url = qs.stringifyUrl({url: '', query: params}); // Convert query parameters into a URL string

    // Function to update the page number in the state store
    function setPageNumber(pageNumber: number) {
        setParams({pageNumber});
    }

    // useEffect runs when the component mounts and when 'url' changes
    useEffect(() => {
        getData(url).then(data => { // Fetch auction data based on the current URL
            setData(data); // Update state with the fetched data
        });
    }, [url]); // Dependency array ensures useEffect runs when 'url' changes

    if (!data) return <h3>Loading...</h3>; // Display a loading message while data is being fetched

    return (
        // React fragment (<></>) allows returning multiple elements without an extra wrapper
        <>
            <Filters /> {/* Renders filter options for refining auction results */}
            <div className='grid grid-cols-4 gap-6'> {/* Grid layout with 4 columns and spacing */}
                {
                // Map over data results to create AuctionCard components
                }
                {data.results.map(auction => (
                    <AuctionCard auction={auction} key={auction.id} /> // Render an AuctionCard for each auction, using auction.id as a unique key
                ))}
            </div>
            <div className='flex justify-center mt-4'> {/* Center pagination controls */}
                <AppPagination 
                    pageChanged={setPageNumber} // Function to handle page changes
                    currentPage={params.pageNumber} // Pass current page number from state
                    pageCount={data.pageCount} // Pass total page count from data
                />
            </div>
        </>
    );
}

