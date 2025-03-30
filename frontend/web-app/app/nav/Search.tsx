'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import React, { useState } from 'react'
import { FaSearch } from 'react-icons/fa'

export default function Search() {
    const setParams = useParamsStore(state => state.setParams);
    const [value, setValue] = useState('');

    function onChange(event: any) {
        // Character the user is typing in to the search bar
        setValue(event.target.value);
    }

    function search() {
        // Set the search term to whatever the value is that was typed into the search bar
        setParams({searchTerm: value});
    }

    return (
        <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
            <input onKeyDown={(e: any) => {
                    if (e.key === 'Enter') search();
                }}
                onChange={onChange} type='text' placeholder='Search for Cars...' className='flex-grow pl-5 bg-transparent focus:outline-none border-transparent focus:border-transparent focus:ring-0 text-sm text-gray-600'/>
            <button onClick={search}>
                <FaSearch size={34} className='bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2'/>
            </button>
        </div>
    )
}
