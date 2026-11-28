import { Link } from "react-router-dom";
import type { MissionAttempt } from "../types/PlayerProgress";

interface MissionHistoryProps {
    attempts: MissionAttempt[];
}

const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

// "28 Nov 14:05" in the player's local time, same format as Subapp 1
function formatDate(iso: string) {
    const date = new Date(iso);
    const time = date.toTimeString().slice(0, 5);
    return `${date.getDate()} ${months[date.getMonth()]} ${time}`;
}

// The latest graded missions, passed or failed, like the mission history
// at the bottom of Progress/Index.cshtml in Subapp 1.
function MissionHistory({ attempts }: MissionHistoryProps) {
    return (
        <section className="history" aria-labelledby="historyTitle">
            <h2 id="historyTitle" className="history-title">Mission history</h2>

            {attempts.length === 0 ? (
                <p className="history-empty">
                    No missions played yet. <Link to="/planets">Pick a planet in the Galaxy</Link> to start.
                </p>
            ) : (
                <ol className="history-list">
                    {attempts.map((attempt, index) => (
                        <li key={`${attempt.attemptedAt}-${index}`} className="history-item">
                            <span className="history-planet">{attempt.planetName}</span>
                            <span className="history-score">
                                {attempt.correctCount} / {attempt.totalQuestions} correct
                            </span>
                            {/* the word next to the colour, so passed/failed doesn't depend on seeing green and red */}
                            <span className={`history-result ${attempt.passed ? "history-result--passed" : "history-result--failed"}`}>
                                {attempt.passed ? "✓ Passed" : "✗ Failed"}
                            </span>
                            <time className="history-date" dateTime={attempt.attemptedAt}>
                                {formatDate(attempt.attemptedAt)}
                            </time>
                        </li>
                    ))}
                </ol>
            )}
        </section>
    );
}

export default MissionHistory;
