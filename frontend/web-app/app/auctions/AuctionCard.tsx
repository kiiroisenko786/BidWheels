import Image from 'next/image'
import React from 'react'
import CountdownTimer from './CountdownTimer'

// Needs to define auctions as prop
type Props = {
    auction: any
}

// Receiving auction object as a prop
export default function AuctionCard({auction}: Props) {
    return (
        // Accessing make property of auction object
        <a href='#'>
            <div className='relative w-full bg-gray-200 aspect-[16/10] rounded-lg overflow-hidden'>
                <Image src={auction.imageUrl} alt={`Image of ${auction.make} ${auction.model} in ${auction.color}`} fill className='object-cover' priority sizes='(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw'/>
                <div className='absolute bottom-2 left-2'>
                    <CountdownTimer auctionEnd={auction.auctionEnd} />
                </div>
            </div>
            <div className='flex justify-between items-center mt-4'>
                <h3 className='text-gray-700'>{auction.make} {auction.model}</h3>
                <p className='font-semibold text-sm'>{auction.year}</p>
            </div>
        </a>
    )
}
