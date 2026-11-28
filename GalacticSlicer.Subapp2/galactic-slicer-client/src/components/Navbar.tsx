
import { useEffect, useState } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";
import { getToken } from "../services/api";
import { logout } from "../services/authService";
import NavUser from "./NavUser";

const links = [
    { to: "/", label: "Home" },
    { to: "/planets", label: "Galaxy" },
    { to: "/challenges", label: "Challenges" },
    { to: "/characters", label: "Shop" },
    { to: "/progress", label: "Progress" },
];

function Navbar() {
    const navigate = useNavigate();

    // Mobile navigation
    const [menuOpen, setMenuOpen] = useState(false);

    // Current authentication state
    const [isLoggedIn, setIsLoggedIn] = useState(
        () => getToken() !== null
    );

    // Update navigation whenever authentication changes
    useEffect(() => {
        function updateAuth() {
            setIsLoggedIn(getToken() !== null);
        }

        window.addEventListener("auth-changed", updateAuth);
        window.addEventListener("storage", updateAuth);

        return () => {
            window.removeEventListener("auth-changed", updateAuth);
            window.removeEventListener("storage", updateAuth);
        };
    }, []);

    // Escape closes the mobile menu
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

        return () =>
            document.removeEventListener("keydown", handleKeyDown);
    }, [menuOpen]);

    // Remove JWT and return to the home page
    function handleLogout() {
        logout();
        setIsLoggedIn(false);
        setMenuOpen(false);
        navigate("/", { replace: true });
    }

    return (
        <header className="gs-header">
            <nav className="gs-navbar" aria-label="Main">
                <Link
                    to="/"
                    className="gs-brand"
                    onClick={() => setMenuOpen(false)}
                >
                    <span
                        className="gs-brand-icon"
                        aria-hidden="true"
                    >
                        <svg
                            width="26"
                            height="26"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="1.6"
                            strokeLinecap="round"
                        >
                            <circle cx="12" cy="12" r="5.2" />
                            <ellipse
                                cx="12"
                                cy="12"
                                rx="10.4"
                                ry="3.4"
                                transform="rotate(-24 12 12)"
                            />
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
                    onClick={() =>
                        setMenuOpen(open => !open)
                    }
                >
                    <span></span>
                    <span></span>
                    <span></span>
                </button>

                <div
                    className={`gs-nav-collapse${menuOpen ? " is-open" : ""}`}
                    id="primaryNav"
                >
                    <ul className="gs-nav-links">
                        {links.map(link => (
                            <li key={link.to}>
                                <NavLink
                                    to={link.to}
                                    end={link.to === "/"}
                                    className={({ isActive }) =>
                                        `gs-nav-link${isActive ? " is-active" : ""}`
                                    }
                                    onClick={() =>
                                        setMenuOpen(false)
                                    }
                                >
                                    {link.label}
                                </NavLink>
                            </li>
                        ))}
                    </ul>

                    <div className="gs-nav-auth">
                        {isLoggedIn ? (
                            <>
                                {/* badge, name and Credits, like NavUserViewComponent in Subapp 1 */}
                                <NavUser />

                                <button
                                    type="button"
                                    className="gs-btn gs-btn--ghost gs-btn--sm"
                                    onClick={handleLogout}
                                >
                                    Log out
                                </button>
                            </>
                        ) : (
                            <>
                                <Link
                                    to="/login"
                                    className="gs-btn gs-btn--ghost"
                                    onClick={() =>
                                        setMenuOpen(false)
                                    }
                                >
                                    Log in
                                </Link>

                                <Link
                                    to="/register"
                                    className="gs-btn gs-btn--primary gs-btn--sm"
                                    onClick={() =>
                                        setMenuOpen(false)
                                    }
                                >
                                    Sign up
                                </Link>
                            </>
                        )}
                    </div>
                </div>
            </nav>
        </header>
    );
}

export default Navbar;
