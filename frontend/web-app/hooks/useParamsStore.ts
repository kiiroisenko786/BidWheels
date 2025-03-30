import { create } from "zustand"

// Define State representing the shape of the state object.
type State = {
    pageNumber: number,  // Current page number
    pageSize: number,    // Number of items per page
    pageCount: number,   // Total number of pages
    searchTerm: string,   // Current search term
    searchValue: string  // Current search value
}

// Define 'Actions' representing the shape of the actions that can modify the state.
type Actions = {
    // Function to update the state with new parameters; accepts a partial State object.
    setParams: (params: Partial<State>) => void
    // Function to reset the state to its initial values.
    reset: () => void
    // Set Search Value
    setSearchValue: (value: string) => void
}

// Define the initial state values.
const initialState: State = {
    pageNumber: 1,   // Start on the first page
    pageSize: 12,    // Default to 12 items per page
    pageCount: 1,    // Assume only one page initially
    searchTerm: '',   // No search term initially
    searchValue: ''   // No search value initially
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