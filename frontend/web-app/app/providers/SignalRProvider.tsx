'use client'

import { useAuctionStore } from '@/hooks/useAuctionStore'
import { useBidStore } from '@/hooks/useBidStore'
import { Bid } from '@/types'
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { useParams } from 'next/navigation'
import React, { ReactNode, useCallback, useEffect, useRef } from 'react'

type Props = {
  children: ReactNode
}

export default function SignalRProvider({ children }: Props) {
  const connection = useRef<HubConnection | null>(null);
  const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
  const addBid = useBidStore(state => state.addBid);
  const params = useParams<{id: string}>();

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

    return () => {
      connection.current?.off('BidPlaced', handleBidPlaced);
    }
  }, [setCurrentPrice, handleBidPlaced]);
  
  return (
    children
  )
}
