import { useEffect, useState, type CSSProperties } from "react";
import type { Planet } from "../types/Planet";
import { getPlanets } from "../services/planetService";
import PlanetCard from "../components/PlanetCard";
import { usePageTitle } from "../hooks/usePageTitle";

// Loads the planets from GET /api/planets and shows them as a starchart
function PlanetsPage() {
    usePageTitle("Galaxy");

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
                <h1>The Starchart</h1>
                <p>Follow the path from world to world. Every planet is a mission.</p>
                {planets.length > 0 && (
                    <p className="gs-starchart-progress">
                        {planets.length} {planets.length === 1 ? "planet" : "planets"} charted
                    </p>
                )}
            </section>

            {/* aria-live: screen readers hear when loading ends or an error appears */}
            <div aria-live="polite">
                {loading && <p className="gs-page-intro">Loading planets...</p>}

                {error && <p className="gs-page-intro" role="alert">{error}</p>}

                {!loading && !error && planets.length === 0 && (
                    <p className="gs-page-intro">No planets found.</p>
                )}
            </div>

            {planets.length > 0 && (
                <section className="gs-starchart" aria-label="Planets">
                    {/* --gs-count lets the CSS use fewer columns when there are fewer than 6 planets */}
                    <ol className="gs-starchart-path" style={{ "--gs-count": planets.length } as CSSProperties}>
                        {planets.map(planet => (
                            <PlanetCard key={planet.planetId} planet={planet} />
                        ))}
                    </ol>
                </section>
            )}
        </>
    );
}

export default PlanetsPage;
