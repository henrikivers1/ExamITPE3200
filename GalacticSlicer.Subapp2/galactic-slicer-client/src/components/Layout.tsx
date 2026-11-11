import { Outlet, useLocation } from "react-router-dom";
import Navbar from "./Navbar";
import Sky from "./Sky";

// Shared frame for every page, like _Layout.cshtml in Subapp 1
function Layout() {
    const location = useLocation();

    return (
        <>
            {/* first thing Tab reaches: lets keyboard users jump past the navbar */}
            <a className="gs-skip-link" href="#main">Skip to content</a>

            {/* the Galaxy page has its own big planets, so no drifting ones behind them */}
            <Sky showPlanets={location.pathname !== "/planets"} />

            <Navbar />

            {/* tabIndex={-1} lets the skip link move focus here */}
            <main id="main" className="gs-main" tabIndex={-1}>
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
