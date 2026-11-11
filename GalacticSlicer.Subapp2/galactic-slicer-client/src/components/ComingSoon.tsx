interface ComingSoonProps {
    title: string;
    text: string;
}

// Placeholder for pages that need API endpoints that don't exist yet.
// Shows no fake Credits, progress or login data.
function ComingSoon({ title, text }: ComingSoonProps) {
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
                <p className="gs-feature-text">This part of the galaxy is still under construction.</p>
            </div>
        </>
    );
}

export default ComingSoon;
