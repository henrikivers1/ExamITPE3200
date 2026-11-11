import { Link } from "react-router-dom";
import { usePageTitle } from "../hooks/usePageTitle";

interface ComingSoonProps {
    title: string;
    text: string;
    // what the page will have once the API endpoints exist
    features: string[];
}

// Placeholder for pages that need API endpoints that don't exist yet.
// Shows no fake Credits, progress or login data.
function ComingSoon({ title, text, features }: ComingSoonProps) {
    usePageTitle(title);

    return (
        <>
            <section className="gs-page-intro">
                <h1>{title}</h1>
                <p>{text}</p>
            </section>

            <div className="gs-feature-card gs-coming-soon">
                <div className="gs-feature-icon" aria-hidden="true">
                    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                        strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                        <circle cx="12" cy="12" r="9" />
                        <path d="M12 7v5l3 2" />
                    </svg>
                </div>
                <h2 className="gs-feature-title">Coming soon</h2>
                <p className="gs-feature-text">This part of the galaxy is still under construction. It will let you:</p>

                <ul className="gs-coming-soon-list">
                    {features.map(feature => (
                        <li key={feature}>{feature}</li>
                    ))}
                </ul>

                <div className="gs-page-actions">
                    <Link to="/planets" className="gs-btn gs-btn--primary">View galaxy</Link>
                    <Link to="/" className="gs-btn gs-btn--outline">Back to Home</Link>
                </div>
            </div>
        </>
    );
}

export default ComingSoon;
