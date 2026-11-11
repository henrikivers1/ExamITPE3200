import { Link } from "react-router-dom";
import { usePageTitle } from "../hooks/usePageTitle";

// Shown for any URL that doesn't match a route
function NotFoundPage() {
    usePageTitle("Page not found");

    return (
        <section className="gs-page-intro">
            <h1>Page not found</h1>
            <p>This part of the galaxy hasn't been charted yet.</p>
            <div className="gs-page-actions">
                <Link to="/" className="gs-btn gs-btn--primary">Back to Home</Link>
                <Link to="/planets" className="gs-btn gs-btn--outline">Galaxy map</Link>
            </div>
        </section>
    );
}

export default NotFoundPage;
