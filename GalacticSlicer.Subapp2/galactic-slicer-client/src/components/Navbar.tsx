import { Link, NavLink } from "react-router-dom";

const links = [
    { to: "/", label: "Home" },
    { to: "/planets", label: "Galaxy" },
    { to: "/challenges", label: "Challenges" },
    { to: "/characters", label: "Shop" },
    { to: "/progress", label: "Progress" },
];

// NavLink adds the "active" class and aria-current="page" to the current page's link
function Navbar() {
    return (
        <header className="header">
            <nav className="navbar" aria-label="Main">
                <Link to="/" className="brand">Galactic Slicer</Link>
                <ul className="nav-links">
                    {links.map(link => (
                        <li key={link.to}>
                            {/* end: "/" is only active on the Home page, not on every page */}
                            <NavLink to={link.to} end={link.to === "/"} className="nav-link">
                                {link.label}
                            </NavLink>
                        </li>
                    ))}
                </ul>
            </nav>
        </header>
    );
}

export default Navbar;
