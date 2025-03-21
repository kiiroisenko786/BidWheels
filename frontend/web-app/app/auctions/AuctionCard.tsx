import React from 'react'

// Needs to define auctions as prop
type Props = {
    auction: any
}

// Receiving auction object as a prop
export default function AuctionCard({auction}: Props) {
    return (
        <div>{auction.make}</div>
    )
}
