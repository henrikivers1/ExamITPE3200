import { useEffect, useRef } from "react";
import { Outlet, useLocation } from "react-router-dom";
import Navbar from "./Navbar";
import Sky from "./Sky";

// Shared frame for every page, like _Layout.cshtml in Subapp 1
function Layout() {
    const location = useLocation();
    const mainRef = useRef<HTMLElement>(null);
    // the page we were on last time, so the effect below only acts when the page really changed
    const previousPath = useRef(location.pathname);

    // A SPA doesn't load a new page when a link is clicked, so do what the browser
    // would normally do: start at the top, and move focus to the new page's heading
    // so screen readers read the new page instead of staying on the clicked link.
    useEffect(() => {
        // on the first load the browser already starts at the top with focus on the page
        if (previousPath.current === location.pathname) {
            return;
        }
        previousPath.current = location.pathname;

        window.scrollTo(0, 0);

        const heading = mainRef.current?.querySelector("h1");
        const target = heading ?? mainRef.current;
        if (target) {
            // tabIndex -1 lets the heading take focus without adding it to the Tab order
            target.setAttribute("tabindex", "-1");
            target.focus();
        }
    }, [location.pathname]);

    return (
        <>
            {/* first thing Tab reaches: lets keyboard users jump past the navbar */}
            <a className="gs-skip-link" href="#main">Skip to content</a>

            {/* the Galaxy page has its own big planets, so no drifting ones behind them */}
            <Sky showPlanets={location.pathname !== "/planets"} />

            <Navbar />

            {/* tabIndex={-1} lets the skip link move focus here */}
            <main id="main" className="gs-main" tabIndex={-1} ref={mainRef}>
                <Outlet />
            </main>

            <footer className="gs-footer">
                <div className="gs-footer-inner">
                    <div className="gs-footer-brand">
                        <span className="gs-footer-title">Galactic Slicer</span>
                        <span>Learn. Practice. A safer galaxy.</span>
                    </div>
                </div>
            </footer>
        </>
    );
}

export default Layout;
