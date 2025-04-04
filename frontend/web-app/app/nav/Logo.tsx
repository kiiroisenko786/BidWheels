'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import { usePathname } from 'next/navigation'
import { useRouter } from 'next/navigation'
import React from 'react'
import { PiCarProfileFill } from 'react-icons/pi'

export default function Logo() {
    const router=  useRouter();
    const pathname = usePathname();
    const reset = useParamsStore(state => state.reset);

    function doReset() {
        if (pathname !== '/') router.push('/');

        reset();
    }

    return (
        <div onClick={doReset} className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500">
            <PiCarProfileFill size='34'/>
            <div>BidWheels Auctions</div>
        </div>
    )
}
