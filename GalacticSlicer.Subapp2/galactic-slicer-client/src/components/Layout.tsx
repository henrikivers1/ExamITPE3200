import { Outlet } from "react-router-dom";
import Navbar from "./Navbar";

// Shared frame for every page, like _Layout.cshtml in Subapp 1
function Layout() {
    return (
        <>
            {/* first thing Tab reaches: lets keyboard users jump past the navbar */}
            <a className="skip-link" href="#main">Skip to content</a>

            <Navbar />

            {/* tabIndex={-1} lets the skip link move focus here */}
            <main id="main" className="main" tabIndex={-1}>
                <Outlet />
            </main>

            <footer className="footer">
                <p>Galactic Slicer · Learn. Practice. A safer galaxy.</p>
            </footer>
        </>
    );
}

export default Layout;
