import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import PilotPanel from "../components/PilotPanel";
import ProgressBars from "../components/ProgressBars";
import { usePageTitle } from "../hooks/usePageTitle";
import { getProgress } from "../services/progressService";
import type { PlayerProgress } from "../types/PlayerProgress";

// Progress page: the logged-in player's dashboard, like Progress/Index in Subapp 1.
// Subapp 1 sends visitors to Log in; here they get a card that explains why and links there.
function ProgressPage() {
    usePageTitle("Progress");

    // TODO: comes from the login state once the API has authentication
    const [isLoggedIn] = useState(false);

    const [progress, setProgress] = useState<PlayerProgress | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!isLoggedIn) {
            return;
        }

        async function loadProgress() {
            setLoading(true);
            try {
                setProgress(await getProgress());
            } catch {
                setError("Could not load your progress.");
            } finally {
                setLoading(false);
            }
        }

        loadProgress();
    }, [isLoggedIn]);

    if (!isLoggedIn) {
        return (
            <>
                <section className="gs-page-intro">
                    <h1>Progress</h1>
                    <p>Track your Credits, XP, rank and unlocked planets.</p>
                </section>

                <div className="gs-feature-card gs-coming-soon">
                    <div className="gs-feature-icon" aria-hidden="true">
                        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                            strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M4 20V10" />
                            <path d="M11 20V4" />
                            <path d="M18 20v-7" />
                        </svg>
                    </div>
                    <h2 className="gs-feature-title">Log in to track your progress</h2>
                    <p className="gs-feature-text">Your Credits, XP, rank and planets are saved to your account.</p>
                    <div className="gs-page-actions">
                        <Link to="/login" className="gs-btn gs-btn--primary">Log in</Link>
                        <Link to="/register" className="gs-btn gs-btn--outline">Sign up</Link>
                    </div>
                </div>
            </>
        );
    }

    return (
        <>
            {/* aria-live: screen readers hear when loading ends or an error appears */}
            <div aria-live="polite">
                {loading && <p className="gs-page-intro">Loading your progress...</p>}
                {error && <p className="gs-page-intro" role="alert">{error}</p>}
            </div>

            {progress && (
                <>
                    <PilotPanel progress={progress} />
                    <ProgressBars progress={progress} />
                </>
            )}
        </>
    );
}

export default ProgressPage;
