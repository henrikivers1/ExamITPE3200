import { Link } from "react-router-dom";

// Shown for any URL that doesn't match a route
function NotFoundPage() {
    return (
        <section className="page">
            <h1>Page not found</h1>
            <p>This part of the galaxy hasn't been charted yet.</p>
            <Link to="/" className="button">Back to Home</Link>
        </section>
    );
}

export default NotFoundPage;
