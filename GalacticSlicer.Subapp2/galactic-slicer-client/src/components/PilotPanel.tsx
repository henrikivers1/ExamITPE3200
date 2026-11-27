import { Link } from "react-router-dom";
import CharacterBadge from "./CharacterBadge";
import type { PlayerProgress } from "../types/PlayerProgress";

interface PilotPanelProps {
    progress: PlayerProgress;
}

// Pilot panel: badge, name, rank, Credits, the next mission and a mini starchart.
// Same panel as the top of Progress/Index.cshtml in Subapp 1.
// Mission shortcuts use the Galaxy page, which shows lock and completion status.
function PilotPanel({ progress }: PilotPanelProps) {
    const { planets, activeCharacter } = progress;

    // first unlocked planet that isn't completed: the "Continue" button
    const nextPlanet = planets.find(p => p.isUnlocked && !p.isCompleted) ?? null;
    // last unlocked planet, for "Replay" when every unlocked mission is completed
    const lastUnlocked = [...planets].reverse().find(p => p.isUnlocked) ?? null;
    // next locked planet, for "Kamino unlocks at 3000 XP"
    const nextLocked = planets.find(p => !p.isUnlocked) ?? null;

    return (
        <section className="pilot-wrapper">
            <div className="pilot">
                {/* badge, name, rank and Credits */}
                <div className="pilot-head">
                    {/* the badge picture is hidden from screen readers, so aria-label gives the link its name */}
                    <Link
                        className="pilot-badge"
                        to="/characters"
                        title={activeCharacter?.name ?? "No badge equipped - pick one in the Shop"}
                        aria-label={activeCharacter === null
                            ? "No badge equipped, open the Shop"
                            : `${activeCharacter.name} badge, open the Shop`}
                    >
                        <CharacterBadge character={activeCharacter} />
                    </Link>
                    <div className="pilot-info">
                        <h1 className="pilot-name">{progress.username}</h1>
                        <div className="pilot-stats">
                            <span className="gs-rank-badge">{progress.rank}</span>
                            <span className="pilot-credits"><span aria-hidden="true">&#9679;</span> {progress.credits} Credits</span>
                        </div>
                        <Link className="pilot-badge-link" to="/characters">
                            {activeCharacter === null ? "Equip a badge in the Shop" : "Change badge"}
                        </Link>
                    </div>
                </div>

                {/* the next mission to play, or a replay once every unlocked mission is completed */}
                <div className="pilot-actions">
                    {nextPlanet && (
                        <Link className="gs-btn gs-btn--primary" to="/planets">&#9654; Continue: {nextPlanet.name} mission</Link>
                    )}
                    {!nextPlanet && lastUnlocked && (
                        <Link className="gs-btn gs-btn--primary" to="/planets">&#8635; Replay {lastUnlocked.name}</Link>
                    )}
                    <Link className="gs-btn gs-btn--outline" to="/challenges/topics">Practice</Link>
                </div>
                {nextLocked && (
                    <p className="pilot-hint">{nextLocked.name} unlocks at {nextLocked.requiredXp} XP</p>
                )}

                {/* mini starchart: every planet with its status */}
                <ol className="pilot-chart">
                    {planets.map(planet => {
                        const isNext = planet.planetId === nextPlanet?.planetId;
                        const stateClass = planet.isCompleted ? "pilot-chart-item--completed"
                            : isNext ? "pilot-chart-item--next"
                            : planet.isUnlocked ? "" : "pilot-chart-item--locked";
                        const status = planet.isCompleted ? "Completed"
                            : isNext ? "Next mission"
                            : planet.isUnlocked ? "Unlocked" : "Locked";
                        const mark = planet.isCompleted ? "✓" : isNext ? "▶" : planet.isUnlocked ? "" : "\u{1F512}";

                        const content = (
                            <>
                                <span className={`gs-planet gs-planet--${planet.cssClass} pilot-chart-planet`} aria-hidden="true"></span>
                                <span className="pilot-chart-name">
                                    {planet.name}<span className="gs-sr-only">, {status}</span>
                                </span>
                                <span className="pilot-chart-status" aria-hidden="true">{mark}</span>
                            </>
                        );

                        return (
                            <li key={planet.planetId} className={`pilot-chart-item ${stateClass}`}>
                                {planet.isUnlocked ? (
                                    <Link className="pilot-chart-link" to="/planets" title={`${planet.name}: ${status}`}>{content}</Link>
                                ) : (
                                    <span className="pilot-chart-link" aria-disabled="true" title={`${planet.name}: Locked`}>{content}</span>
                                )}
                            </li>
                        );
                    })}
                </ol>
            </div>
        </section>
    );
}

export default PilotPanel;
