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
        <section className="page">
            <h1>Galaxy</h1>

            {loading && <p>Loading planets...</p>}

            {error && <p role="alert">{error}</p>}

            {!loading && !error && planets.length === 0 && (
                <p>No planets found.</p>
            )}

            {planets.length > 0 && (
                <ul className="planet-list">
                    {planets.map(planet => (
                        <li key={planet.planetId}>
                            <h2>{planet.name}</h2>
                            <p>{planet.description}</p>
                        </li>
                    ))}
                </ul>
            )}
        </section>
    );
}

export default PlanetsPage;
