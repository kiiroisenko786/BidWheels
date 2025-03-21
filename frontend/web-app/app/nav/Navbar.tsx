import React from 'react'
import { PiCarProfileFill } from 'react-icons/pi'

export default function Navbar() {
  return (
    <header className='sticky top-0 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md'>
      <div className="flex items-center gap-2 text-3xl font-semibold text-red-500">
        <PiCarProfileFill size='34'/>
        <div>BidWheels Auctions</div>
      </div>
      <div>Search</div>
      <div>Login</div>
    </header>
  )
}
