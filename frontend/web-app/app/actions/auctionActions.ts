'use server'

import { fetchWrapper } from "@/lib/fetchWrapper";
import { Auction, PagedResult } from "@/types";
import { revalidatePath } from "next/cache";
import { FieldValues } from "react-hook-form";

// Promise just means what you're returning, so here we are returning a paged result of auction type so the listings function knows the data type is a paged result of auctions rather than any for type safety
export async function getData(query: string): Promise<PagedResult<Auction>> {
    return await fetchWrapper.get(`search${query}`);
}

export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 10000) + 1
    }

    return await fetchWrapper.put('auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c', data);
}

export async function createAuction(data: FieldValues) {
    return await fetchWrapper.post('auctions', data);
}

export async function getDetailsViewData(id: string): Promise<Auction> {
    return await fetchWrapper.get(`auctions/${id}`);
}

export async function updateAuction(data: FieldValues, id: string) {
    const res = await fetchWrapper.put(`auctions/${id}`, data);
    revalidatePath(`/auctions/${id}`);
    return res;
}