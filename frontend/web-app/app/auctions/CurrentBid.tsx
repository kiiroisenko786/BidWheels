import React from 'react'

type Props = {
  amount?: number,
  reservePrice: number
}

export default function CurrentBid({amount, reservePrice}: Props) {
  const text = amount ? '£' + amount : 'No bids yet';
  const colour = amount ? amount > reservePrice ? 'bg-green-600' : 'bg-amber-600' : 'bg-red-600';

  return (
    <div className={`border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center ${colour}`}>{text}</div>
  )
}
