import { useParamsStore } from '@/hooks/useParamsStore';
import { Button, ButtonGroup } from 'flowbite-react';
import React from 'react'

const pageSizeButtons = [4, 8, 12]; // Array of available page size options for pagination

export default function Filters() { // Define and export the Filters component
    const pageSize = useParamsStore(state => state.pageSize); // Retrieve the current page size from the state store
    const setParams = useParamsStore(state => state.setParams); // Get the function to update query parameters

    return (
        <div className='flex justify-between items-center mb-4'> {/* Container for the filter controls */}
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Page Size</span> {/* Label for the page size selection */}
                <ButtonGroup> {/* Group of buttons allowing users to select a page size */}
                    {pageSizeButtons.map((value, i) => (
                        <Button key={i} // Unique key assigned using the index
                            onClick={() => setParams({pageSize: value})} // Update the page size in the state when clicked
                            color={`${pageSize === value ? 'red' : 'gray'}`} // Apply red color for the selected page size, gray otherwise
                            className='focus:ring-0' // Remove focus ring styling for a cleaner UI
                        >
                            {value} {/* Display the page size number inside the button */}
                        </Button>
                    ))}
                </ButtonGroup>
            </div>
        </div>
    );
}
