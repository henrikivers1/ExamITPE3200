interface ComingSoonProps {
    title: string;
    text: string;
}

// Placeholder for pages that need API endpoints that don't exist yet.
// Shows no fake Credits, progress or login data.
function ComingSoon({ title, text }: ComingSoonProps) {
    return (
        <section className="page">
            <h1>{title}</h1>
            <p>{text}</p>
            <p className="muted">Coming soon.</p>
        </section>
    );
}

export default ComingSoon;
