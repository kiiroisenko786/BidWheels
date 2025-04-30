'use client'

import { getBidsForAuction } from '@/app/actions/auctionActions'
import Heading from '@/app/components/Heading'
import { useBidStore } from '@/hooks/useBidStore'
import { Auction, Bid } from '@/types'
import { User } from 'next-auth'
import React, { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import BidItem from './BidItem'
import { numberWithCommas } from '@/app/lib/numberWithComma'
import EmptyFilter from '@/app/components/EmptyFilter'
import BidForm from './BidForm'

type Props = {
  user: User | null
  auction: Auction
}

export default function BidList({user, auction}: Props) {
  const [loading, setLoading] = useState(true);
  const bids = useBidStore(state => state.bids);
  const setBids = useBidStore(state => state.setBids);

  /* Use the reduce() method to find the highest bid amount in the 'bids' array.
  'reduce' iterates through each bid object in the array.
  'prev' holds the accumulator (either the previous highest amount or 0 initially).
  'current' is the current bid object being processed.
  The ternary operator compares 'prev' with 'current.amount':
  - If 'prev' is greater, keep it.
  - Otherwise, update to 'current.amount'.
  The initial value of the accumulator is set to 0.
  The final result, 'highBid', will be the highest bid amount in the array. */
  const highBid =  bids.reduce((prev, current) => prev > current.amount ? prev : current.amount, 0);

  useEffect(() => {
    getBidsForAuction(auction.id)
      .then((res: any) => {
        if (res.error) {
          throw res.error
        }
        setBids(res as Bid[]);
      }).catch(err => {
        toast.error(err.message);
      }).finally(() => setLoading(false))
  }, [auction.id, setLoading, setBids])

  if (loading) return <span>Loading bids...</span>

  return (
    <div className='rounded-lg shadow-md'>
      <div className='py-2 px-4 bg-white'>
        <div className='sticky top-0 bg-white p-2'>
          <Heading title={`Current high bid is £${numberWithCommas(highBid)}`}/>
        </div>
      </div>
      
      <div className='overflow-auto h-[400px] flex flex-col-reverse px-2'>
        {bids.length === 0 ? (
          <EmptyFilter title='No bids yet' subtitle='Be the first to place a bid!'/>
        ) : (
          <>
            {bids.map(bid => (
              <BidItem key={bid.id} bid={bid}/>
            ))}
          </>
        )}
      </div>
      <div className='px-2 pb-2 text-gray-500'>
        {!user ? (
          <div className='flex items-center justify-center p-2 text-lg font-semibold'>
            Please log in to make a bid
          </div>
        ): user && user.username === auction.seller ? (
          <div className='flex items-center justify-center p-2 text-lg font-semibold'>
            You cannot bid on your own auction
          </div>
        ) : (
          <BidForm auctionId={auction.id} highBid={highBid}/>
        )}  
      </div>
    </div>
  )
}
