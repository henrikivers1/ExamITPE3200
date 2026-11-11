import { Link } from "react-router-dom";
import { usePageTitle } from "../hooks/usePageTitle";

// Same hero and feature cards as the Subapp 1 Home page (visitor version: there is no login yet)
function HomePage() {
    usePageTitle("Home");

    return (
        <>
            <section className="gs-hero">
                <div className="gs-hero-inner">
                    <h1 className="gs-hero-title">Galactic <span className="gs-accent">Slicer</span></h1>
                    <p className="gs-hero-subtitle">
                        Learn cybersecurity through hands-on missions across planets. Solve
                        real-world inspired challenges, build your skills, and help make the
                        galaxy a safer place.
                    </p>
                    <div className="gs-hero-actions">
                        <Link to="/planets" className="gs-btn gs-btn--primary gs-btn--lg">
                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                                <path d="M9 4 3 6.5v14L9 18l6 2.5 6-2.5v-14L15 6.5 9 4Z" />
                                <path d="M9 4v14" />
                                <path d="M15 6.5v14" />
                            </svg>
                            View galaxy
                        </Link>
                        <Link to="/challenges" className="gs-btn gs-btn--outline gs-btn--lg">
                            Browse challenges
                        </Link>
                    </div>
                </div>
            </section>

            <section className="gs-features">
                <div className="gs-feature-card">
                    <div className="gs-feature-icon" aria-hidden="true">
                        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                            strokeWidth="1.8" strokeLinecap="round">
                            <circle cx="12" cy="12" r="9" />
                            <path d="M3.5 9.5h17" />
                            <path d="M3.5 14.5h17" />
                            <path d="M12 3c2.5 2.5 3.6 5.7 3.6 9s-1.1 6.5-3.6 9c-2.5-2.5-3.6-5.7-3.6-9S9.5 5.5 12 3Z" />
                        </svg>
                    </div>
                    <h2 className="gs-feature-title">Galaxy</h2>
                    <p className="gs-feature-text">Explore unique worlds, each with its own mission and story.</p>
                    <Link to="/planets" className="gs-feature-link">Explore the planets →</Link>
                </div>

                <div className="gs-feature-card">
                    <div className="gs-feature-icon" aria-hidden="true">
                        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                            strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M6 3v18" />
                            <path d="M6 4h11l-2.5 3.5L17 11H6" />
                        </svg>
                    </div>
                    <h2 className="gs-feature-title">Challenges</h2>
                    <p className="gs-feature-text">Solve cybersecurity challenges and apply real-world skills.</p>
                    <Link to="/challenges" className="gs-feature-link">See the challenges →</Link>
                </div>

                <div className="gs-feature-card">
                    <div className="gs-feature-icon" aria-hidden="true">
                        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                            strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M4 20V10" />
                            <path d="M11 20V4" />
                            <path d="M18 20v-7" />
                        </svg>
                    </div>
                    <h2 className="gs-feature-title">Progress</h2>
                    <p className="gs-feature-text">Track your progress, earn rewards, and become a legendary slicer.</p>
                    <Link to="/progress" className="gs-feature-link">See your progress →</Link>
                </div>
            </section>
        </>
    );
}

export default HomePage;
