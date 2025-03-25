import { create } from "zustand"

type State = {
    pageNumber: number,
    pageSize: number,
    pageCount: number,
    searchTerm: string
}

type Actions = {
    setParams: (params: Partial<State>) => void
    reset: () => void
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: ''
}

export const useParamsStore = create<State & Actions>()((set) => ({
    ...initialState,

    setParams: (newParams: Partial<State>) => {
        set((state) => {
            // check if newparams is a page number -> are we updating the page?
            if (newParams.pageNumber) {
                // if so, we want to keep our existing state, and just update the page number because that means the user is just switching from one page to another
                return {...state, pageNumber: newParams.pageNumber}
            } else {
                return {...state, ...newParams, pageNumber: 1}
            }
        })
    },

    reset: () => set(initialState)
}))