
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import PilotPanel from "../components/PilotPanel";
import ProgressBars from "../components/ProgressBars";
import { usePageTitle } from "../hooks/usePageTitle";
import { getProgress } from "../services/progressService";
import { getToken, ApiError } from "../services/api";
import type { PlayerProgress } from "../types/PlayerProgress";

function ProgressPage() {
    usePageTitle("Progress");

    const [isLoggedIn, setIsLoggedIn] = useState(
        () => getToken() !== null
    );

    const [progress, setProgress] =
        useState<PlayerProgress | null>(null);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Update when the user logs in or out
    useEffect(() => {
        function updateAuth() {
            setIsLoggedIn(getToken() !== null);
        }

        window.addEventListener("auth-changed", updateAuth);
        window.addEventListener("storage", updateAuth);

        return () => {
            window.removeEventListener("auth-changed", updateAuth);
            window.removeEventListener("storage", updateAuth);
        };
    }, []);

    // Load real player progress from the API
    useEffect(() => {
        if (!isLoggedIn) {
            setProgress(null);
            setError(null);
            setLoading(false);
            return;
        }

        let cancelled = false;

        async function loadProgress() {
            setLoading(true);
            setError(null);

            try {
                const data = await getProgress();

                if (!cancelled) {
                    setProgress(data);
                }
            } catch (err) {
                if (!cancelled) {
                    setProgress(null);

                    if (err instanceof ApiError) {
                        setError(err.message);
                    } else {
                        setError("Could not load your progress.");
                    }
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        loadProgress();

        return () => {
            cancelled = true;
        };
    }, [isLoggedIn]);

    return (
        <>
            <section className="gs-page-intro">
                <h1>Progress</h1>
                <p>
                    Track your Credits, XP, rank
                    and unlocked planets.
                </p>
            </section>

            {!isLoggedIn ? (
                <div className="gs-feature-card gs-coming-soon">
                    <div
                        className="gs-feature-icon"
                        aria-hidden="true"
                    >
                        <svg
                            width="22"
                            height="22"
                            viewBox="0 0 24 24"
                            fill="none"
                            stroke="currentColor"
                            strokeWidth="1.8"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        >
                            <path d="M4 20V10" />
                            <path d="M11 20V4" />
                            <path d="M18 20v-7" />
                        </svg>
                    </div>

                    <h2 className="gs-feature-title">
                        Log in to track your progress
                    </h2>

                    <p className="gs-feature-text">
                        Your Credits, XP, rank and planets
                        are saved to your account.
                    </p>

                    <div className="gs-page-actions">
                        <Link
                            to="/login"
                            className="gs-btn gs-btn--primary"
                        >
                            Log in
                        </Link>

                        <Link
                            to="/register"
                            className="gs-btn gs-btn--outline"
                        >
                            Sign up
                        </Link>
                    </div>
                </div>
            ) : (
                <>
                    <div aria-live="polite">
                        {loading && (
                            <p className="gs-feature-text">
                                Loading your progress...
                            </p>
                        )}

                        {error && (
                            <p
                                className="gs-feature-text"
                                role="alert"
                            >
                                {error}
                            </p>
                        )}
                    </div>

                    {progress && !loading && (
                        <>
                            <PilotPanel progress={progress} />
                            <ProgressBars progress={progress} />
                        </>
                    )}
                </>
            )}
        </>
    );
}

export default ProgressPage;
