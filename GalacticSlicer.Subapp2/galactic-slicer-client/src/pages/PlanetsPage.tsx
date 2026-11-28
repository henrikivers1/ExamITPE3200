
import {
    useEffect,
    useState,
    type CSSProperties,
} from "react";
import { Link } from "react-router-dom";

import type { Planet } from "../types/Planet";
import type {
    PlayerProgress,
    PlanetStatus,
} from "../types/PlayerProgress";

import { getPlanets } from "../services/planetService";
import { getProgress } from "../services/progressService";
import { getToken, isAdminUser } from "../services/api";

import PlanetCard from "../components/PlanetCard";
import { usePageTitle } from "../hooks/usePageTitle";

// Galaxy / Starchart
function PlanetsPage() {
    usePageTitle("Galaxy");

    const [planets, setPlanets] = useState<Planet[]>([]);

    const [progress, setProgress] =
        useState<PlayerProgress | null>(null);

    const [loading, setLoading] = useState(true);

    const [error, setError] =
        useState<string | null>(null);

    const [progressError, setProgressError] =
        useState<string | null>(null);

    const isLoggedIn = Boolean(getToken());
    const isAdmin = isAdminUser();

    // ============================================
    // LOAD PLANETS AND PLAYER PROGRESS
    // ============================================

    useEffect(() => {
        let cancelled = false;

        async function loadGalaxy() {
            setLoading(true);
            setError(null);
            setProgressError(null);
            setProgress(null);

            try {
                // Public planet catalogue.
                const planetData = await getPlanets();

                if (cancelled) return;

                setPlanets(
                    [...planetData].sort(
                        (a, b) => a.order - b.order
                    )
                );

                // Only authenticated players have progress.
                if (isLoggedIn) {
                    try {
                        const playerProgress =
                            await getProgress();

                        if (!cancelled) {
                            setProgress(playerProgress);
                        }
                    } catch (err) {
                        if (!cancelled) {
                            setProgressError(
                                err instanceof Error
                                    ? err.message
                                    : "Could not load player progress."
                            );
                        }
                    }
                }
            } catch (err) {
                if (!cancelled) {
                    setError(
                        err instanceof Error
                            ? err.message
                            : "Could not load planets."
                    );
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        void loadGalaxy();

        return () => {
            cancelled = true;
        };
    }, [isLoggedIn]);

    // ============================================
    // MATCH PLANETS WITH PLAYER STATUS
    // ============================================

    function getPlanetStatus(
        planetId: number
    ): PlanetStatus | undefined {
        return progress?.planets.find(
            planet => planet.planetId === planetId
        );
    }

    const completedCount =
        progress?.planets.filter(
            planet => planet.isCompleted
        ).length ?? 0;

    const unlockedCount =
        progress?.planets.filter(
            planet => planet.isUnlocked
        ).length ?? 0;

    // ============================================
    // RENDER
    // ============================================

    return (
        <>
            <section className="gs-page-intro">
                <h1>The Starchart</h1>

                <p>
                    Follow the path from world to world.
                    Every planet is a mission.
                </p>

                {planets.length > 0 && (
                    <p className="gs-starchart-progress">
                        {planets.length}{" "}
                        {planets.length === 1
                            ? "planet"
                            : "planets"}{" "}
                        charted
                    </p>
                )}

                {progress && (
                    <>
                        <p className="gs-starchart-progress">
                            {completedCount} of{" "}
                            {planets.length} planets completed
                        </p>

                        <p>
                            <strong>
                                {unlockedCount} unlocked
                            </strong>
                            {" · "}
                            {progress.experiencePoints.toLocaleString()} XP
                            {" · "}
                            {progress.rank}
                        </p>
                    </>
                )}

                {!isLoggedIn && (
                    <p>
                        <Link to="/login">
                            Log in
                        </Link>{" "}
                        to unlock planets and track
                        your progress.
                    </p>
                )}
            </section>

            <div aria-live="polite">
                {loading && (
                    <p className="gs-page-intro">
                        Loading planets...
                    </p>
                )}

                {error && (
                    <p
                        className="gs-page-intro"
                        role="alert"
                    >
                        {error}
                    </p>
                )}

                {progressError && (
                    <p
                        className="gs-page-intro"
                        role="alert"
                    >
                        Planet progress is temporarily
                        unavailable: {progressError}
                    </p>
                )}

                {!loading &&
                    !error &&
                    planets.length === 0 && (
                        <p className="gs-page-intro">
                            No planets found.
                        </p>
                    )}
            </div>

            {!loading && planets.length > 0 && (
                <section
                    className="gs-starchart"
                    aria-label="Planets"
                >
                    <ol
                        className="gs-starchart-path"
                        style={
                            {
                                "--gs-count": planets.length,
                            } as CSSProperties
                        }
                    >
                        {planets.map(planet => (
                            <PlanetCard
                                key={planet.planetId}
                                planet={planet}
                                status={getPlanetStatus(
                                    planet.planetId
                                )}
                                isLoggedIn={isLoggedIn}
                            />
                        ))}
                    </ol>
                </section>
            )}

            {/* Only admins can manage planets */}
            {isAdmin && (
                <div className="gs-admin-link">
                    <Link
                        to="/admin/planets"
                        className="gs-btn gs-btn--outline gs-btn--sm"
                    >
                        Manage planets
                    </Link>
                </div>
            )}
        </>
    );
}

export default PlanetsPage;
