import React from 'react'
import { PiCarProfileFill } from 'react-icons/pi'
import Search from './Search'
import Logo from './Logo'

export default function Navbar() {
  return (
    <header className='sticky top-0 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md'>
      <Logo/>
      <Search/>
      <div>Login</div>
    </header>
  )
}
