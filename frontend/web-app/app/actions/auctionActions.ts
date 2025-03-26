'use server'

import { Auction, PagedResult } from "@/types";

// Promise just means what you're returning, so here we are returning a paged result of auction type so the listings function knows the data type is a paged result of auctions rather than any for type safety
export async function getData(query: string): Promise<PagedResult<Auction>> {
    const res = await fetch(`http://localhost:6001/search${query}`);

    if (!res.ok) throw new Error("Failed to fetch data");

    return res.json();
}