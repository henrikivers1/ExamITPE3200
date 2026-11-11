
import { useEffect, useState } from "react";
import type { Planet } from "./types/Planet";
import { getPlanets } from "./services/planetService";

function App() {
    const [planets, setPlanets] = useState<Planet[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        async function loadPlanets() {
            try {
                const data = await getPlanets();
                setPlanets(data);
            } catch {
                setError("Could not load planets.");
            } finally {
                setLoading(false);
            }
        }

        loadPlanets();
    }, []);

    if (loading) {
        return <p>Loading planets...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <main>
            <h1>Galactic Slicer</h1>
            <h2>Planets</h2>

            {planets.map(planet => (
                <div key={planet.planetId}>
                    <h3>{planet.name}</h3>
                    <p>{planet.description}</p>
                </div>
            ))}
        </main>
    );
}

export default App;
