'use client'

import { useAuctionStore } from '@/hooks/useAuctionStore'
import { useBidStore } from '@/hooks/useBidStore'
import { Auction, AuctionFinished, Bid } from '@/types'
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { User } from 'next-auth'
import { useParams } from 'next/navigation'
import React, { ReactNode, useCallback, useEffect, useRef } from 'react'
import toast from 'react-hot-toast'
import AuctionCreatedToast from '../components/AuctionCreatedToast'
import { getDetailedViewData } from '../actions/auctionActions'
import AuctionFinishedToast from '../components/AuctionFinishedToast'

type Props = {
  children: ReactNode
  // user can be removed later bc of auctioncreatedtoast
  user: User | null
}

export default function SignalRProvider({ children, user }: Props) {
  const connection = useRef<HubConnection | null>(null);
  const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
  const addBid = useBidStore(state => state.addBid);
  const params = useParams<{id: string}>();

  // this is also part of auctionfinishedtoast
  const handleAuctionFinished = useCallback((finishedAuction: AuctionFinished) => {
    const auction = getDetailedViewData(finishedAuction.auctionId);
    return toast.promise(auction, {
      loading: "Loading",
      success: (auction) => <AuctionFinishedToast auction={auction} finishedAuction={finishedAuction}/>,
      error: (error) => "Auction finished"
    }, {success: {duration: 10000, icon: null}})
  }, [])

  // this is also part of auctioncreatedtoast
  const handleAuctionCreated = useCallback((auction: Auction) => {
    if (user?.username !== auction.seller) {
      return toast(<AuctionCreatedToast auction={auction}/>, {
        duration: 10000
      });
    }
  }, [user?.username])

  // callbacks only get recreated when the dependencies change
  const handleBidPlaced = useCallback((bid: Bid) => {
    if (bid.bidStatus.includes('Accepted')) {
      setCurrentPrice(bid.auctionId, bid.amount);
    }

    if (params.id === bid.auctionId) {
      addBid(bid);

    }
  }, [setCurrentPrice, params.id]);

  useEffect(() => {
    // Check if the connection is already established
    if (!connection.current) {
      connection.current = new HubConnectionBuilder()
        .withUrl("http://localhost:6001/notifications")
        .withAutomaticReconnect()
        .build();    
      
        connection.current.start()
          .then(() => 'Connected to notifications hub')
          .catch(err => console.log(err));
        
    }

    connection.current.on('BidPlaced', handleBidPlaced);
    // this is also part of auctioncreatedtoast
    connection.current.on('AuctionCreated', handleAuctionCreated);
    // this is also part of auctionfinishedtoast
    connection.current.on('AuctionFinished', handleAuctionFinished);

    return () => {
      connection.current?.off('BidPlaced', handleBidPlaced);
      // this is also part of auctioncreatedtoast
      connection.current?.off('AuctionCreated', handleAuctionCreated);
      // this is also part of auctionfinishedtoast
      connection.current?.off('AuctionFinished', handleAuctionFinished);
    }
  }, [setCurrentPrice, handleBidPlaced, handleAuctionCreated, handleAuctionFinished]);
  
  return (
    children
  )
}
