import { useEffect } from "react";

// Sets the browser tab title, like ViewData["Title"] in Subapp 1.
// A SPA never loads a new page, so without this every page would have the same title.
export function usePageTitle(title: string) {
    useEffect(() => {
        document.title = `${title} - Galactic Slicer`;
    }, [title]);
}
