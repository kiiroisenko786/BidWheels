import { create } from "zustand"

// Define State representing the shape of the state object.
type State = {
    pageNumber: number,  // Current page number
    pageSize: number,    // Number of items per page
    pageCount: number,   // Total number of pages
    searchTerm: string,   // Current search term
    searchValue: string,  // Current search value
    orderBy: string, // Current order by criteria
    filterBy: string // Current filter criteria
}

type Actions = {
    setParams: (params: Partial<State>) => void
    reset: () => void
    setSearchValue: (value: string) => void
}

// Define the initial state values.
const initialState: State = {
    pageNumber: 1,   // Start on the first page
    pageSize: 12,    // Default to 12 items per page
    pageCount: 1,    // Assume only one page initially
    searchTerm: '',   // No search term initially
    searchValue: '',   // No search value initially
    orderBy: 'make', // Default order by 'make'
    filterBy: 'live' // Default filter by 'live' 
}

// Create the Zustand store using the 'create' function. The store combines the state and actions.
export const useParamsStore = create<State & Actions>()((set) => ({
    // Spread the initial state into the store.
    ...initialState,

    // Define the 'setParams' action to update the state with new parameters.
    setParams: (newParams: Partial<State>) => {
        set((state) => {
            // If 'pageNumber' is provided in 'newParams', update only the 'pageNumber'.
            if (newParams.pageNumber !== undefined) {
                return { ...state, pageNumber: newParams.pageNumber };
            } else {
                // If 'pageNumber' is not provided, update the state with 'newParams' and reset 'pageNumber' to 1.
                return { ...state, ...newParams, pageNumber: 1 };
            }
        });
    },

    // Define the 'reset' action to revert the state back to the initial values.
    reset: () => set(initialState),

    setSearchValue: (value: string) => {
        set({searchValue: value})
    } 
}));