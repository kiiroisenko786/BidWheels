'use client'

import React, { useEffect } from 'react'
import { FieldValues, useForm } from 'react-hook-form'
import { Button } from 'flowbite-react';
import Input from '../components/Input';
import DateInput from '../components/DateInput';
import { createAuction, updateAuction } from '../actions/auctionActions';
import { usePathname, useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Auction } from '@/types';

type Props = {
    auction?: Auction
}

export default function AuctionForm({auction}: Props) {
    const router = useRouter();
    const pathname = usePathname();
    const {control, handleSubmit, setFocus, reset,
        formState: {isSubmitting, isValid}} = useForm({
            mode: 'onTouched'
        });

    useEffect(() => {
        if (auction) {
            const {make, model, colour, mileage, year} = auction;
            reset({make, model, colour, mileage, year});
        }
        setFocus('make');
    }, [setFocus, auction, reset])

    async function onSubmit(data: FieldValues) {
        try {
            let id = "";
            let res;
            if (pathname === '/auctions/create') {
                res = await createAuction(data);
                id = res.id;
            } else {
                if (auction) {
                    res = await updateAuction(data, auction.id);
                    id = auction.id;
                }
            }

            if (res.error) {
                throw res.error;
            }
            router.push(`/auctions/details/${id}`);
        } catch (error: any) {
            toast.error(error.status + ' ' + error.message);
        }
    }

    return (
        <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
            <Input label='Make' name='make' control={control} rules={{required: 'Make is required'}} />
            <Input label='Model' name='model' control={control} rules={{required: 'Model is required'}} />
            <Input label='Colour' name='colour' control={control} rules={{required: 'Colour is required'}} />
            
            <div className='grid grid-cols-2 gap-3'>
                <Input label='Year' name='year' control={control} type='number' rules={{required: 'Year is required'}} />

                <Input label='Mileage' name='mileage' control={control} type='number' rules={{required: 'Mileage is required'}} />
            </div>

            {pathname === '/auctions/create' &&
            <>
                <Input label='Image Url' name='imageUrl' control={control} rules={{required: 'Image url is required'}} />

                <div className='grid grid-cols-2 gap-3'>
                    <Input label='Reserve Price (0 if no reserve)' name='reservePrice' control={control} type='number' rules={{required: 'Reserve price is required'}} />

                    <DateInput label='Auction End Date/Time' name='auctionEnd' control={control} dateFormat='dd MMMM yyyy h:mm: a' showTimeSelect rules={{required: 'Auction end date is required'}} />
                </div>
            </>}

            <div className='flex justify-between'>
                <Button outline color="gray">Cancel</Button>
                <Button isProcessing={isSubmitting} type='submit' disabled={!isValid} outline color='success'>Submit</Button>
            </div>
        </form>
    )
}