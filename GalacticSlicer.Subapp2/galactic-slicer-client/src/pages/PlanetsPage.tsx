import { useEffect, useState } from "react";
import type { Planet } from "../types/Planet";
import { getPlanets } from "../services/planetService";

// Moved here from App.tsx: loads the planets from GET /api/planets
function PlanetsPage() {
    const [planets, setPlanets] = useState<Planet[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadPlanets() {
            try {
                const data = await getPlanets();
                // the API doesn't sort, so show them in story order
                setPlanets([...data].sort((a, b) => a.order - b.order));
            } catch {
                setError("Could not load planets.");
            } finally {
                setLoading(false);
            }
        }

        loadPlanets();
    }, []);

    return (
        <>
            <section className="gs-page-intro">
                <h1>Galaxy</h1>
                <p>Every planet is a mission. Pick a world and start slicing.</p>
            </section>

            {loading && <p className="gs-page-intro">Loading planets...</p>}

            {error && <p className="gs-page-intro" role="alert">{error}</p>}

            {!loading && !error && planets.length === 0 && (
                <p className="gs-page-intro">No planets found.</p>
            )}

            {/* simple list for now; Step 3 turns this into the starchart with PlanetCard */}
            {planets.length > 0 && (
                <ul className="planet-list">
                    {planets.map(planet => (
                        <li key={planet.planetId} className="gs-feature-card planet-list-item">
                            <span className={`gs-planet gs-planet--${planet.cssClass}`} aria-hidden="true"></span>
                            <div>
                                <h2 className="gs-feature-title">{planet.name}</h2>
                                <p className="gs-feature-text">{planet.description}</p>
                            </div>
                        </li>
                    ))}
                </ul>
            )}
        </>
    );
}

export default PlanetsPage;
