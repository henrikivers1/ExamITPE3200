import { useEffect, useState } from "react";
import { Link, NavLink } from "react-router-dom";

const links = [
    { to: "/", label: "Home" },
    { to: "/planets", label: "Galaxy" },
    { to: "/challenges", label: "Challenges" },
    { to: "/characters", label: "Shop" },
    { to: "/progress", label: "Progress" },
];

function Navbar() {
    // on small screens the links are hidden behind the menu button
    const [menuOpen, setMenuOpen] = useState(false);

    // Escape closes the open menu
    useEffect(() => {
        if (!menuOpen) {
            return;
        }

        function handleKeyDown(event: KeyboardEvent) {
            if (event.key === "Escape") {
                setMenuOpen(false);
            }
        }

        document.addEventListener("keydown", handleKeyDown);
        return () => document.removeEventListener("keydown", handleKeyDown);
    }, [menuOpen]);

    return (
        <header className="gs-header">
            <nav className="gs-navbar" aria-label="Main">
                <Link to="/" className="gs-brand" onClick={() => setMenuOpen(false)}>
                    <span className="gs-brand-icon" aria-hidden="true">
                        <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                            strokeWidth="1.6" strokeLinecap="round">
                            <circle cx="12" cy="12" r="5.2" />
                            <ellipse cx="12" cy="12" rx="10.4" ry="3.4" transform="rotate(-24 12 12)" />
                        </svg>
                    </span>
                    <span>Galactic Slicer</span>
                </Link>

                <button
                    className="gs-nav-toggle"
                    type="button"
                    aria-controls="primaryNav"
                    aria-expanded={menuOpen}
                    aria-label="Toggle navigation"
                    onClick={() => setMenuOpen(open => !open)}
                >
                    <span></span><span></span><span></span>
                </button>

                <div className={`gs-nav-collapse${menuOpen ? " is-open" : ""}`} id="primaryNav">
                    <ul className="gs-nav-links">
                        {links.map(link => (
                            <li key={link.to}>
                                {/* NavLink knows which page is open; "end" keeps Home from matching every page.
                                    It also sets aria-current="page" on the open page's link. */}
                                <NavLink
                                    to={link.to}
                                    end={link.to === "/"}
                                    className={({ isActive }) => `gs-nav-link${isActive ? " is-active" : ""}`}
                                    onClick={() => setMenuOpen(false)}
                                >
                                    {link.label}
                                </NavLink>
                            </li>
                        ))}
                    </ul>
                </div>
            </nav>
        </header>
    );
}

export default Navbar;
