import { Link } from "react-router-dom";

function HomePage() {
    return (
        <section className="page">
            <h1>Galactic Slicer</h1>
            <p>
                Learn cybersecurity through hands-on missions across planets. Solve
                real-world inspired challenges, build your skills, and help make the
                galaxy a safer place.
            </p>
            <Link to="/planets" className="button">View galaxy</Link>
        </section>
    );
}

export default HomePage;
