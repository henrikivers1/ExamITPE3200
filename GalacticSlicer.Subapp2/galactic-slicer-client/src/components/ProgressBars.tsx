import type { PlayerProgress } from "../types/PlayerProgress";

interface ProgressBarsProps {
    progress: PlayerProgress;
}

// percentage for a bar, 0 when there is nothing to count
function percent(value: number, total: number) {
    return total === 0 ? 0 : Math.round((100 * value) / total);
}

// Planets unlocked, challenges completed and XP towards the next rank,
// like the bottom of Progress/Index.cshtml in Subapp 1
function ProgressBars({ progress }: ProgressBarsProps) {
    const planetsTotal = progress.planets.length;
    const planetsUnlocked = progress.planets.filter(p => p.isUnlocked).length;

    const { experiencePoints: xp, xpForCurrentRank: from, xpForNextRank: to } = progress;
    const isTopRank = to <= from;
    const xpPercent = isTopRank ? 100 : Math.min(100, Math.max(0, Math.floor(((xp - from) * 100) / (to - from))));

    return (
        <section className="gs-progress-section">
            <div className="gs-progress-block">
                <div className="gs-progress-block-header">
                    <span>Starchart progress</span>
                    <span>{planetsUnlocked} / {planetsTotal} planets unlocked</span>
                </div>
                {/* role="progressbar" + aria values: screen readers read the bar as "2 of 6" */}
                <div className="gs-progress-bar" role="progressbar" aria-label="Starchart progress"
                    aria-valuemin={0} aria-valuemax={planetsTotal} aria-valuenow={planetsUnlocked}>
                    <div className="gs-progress-bar-fill" style={{ width: `${percent(planetsUnlocked, planetsTotal)}%` }}></div>
                </div>
            </div>

            <div className="gs-progress-block">
                <div className="gs-progress-block-header">
                    <span>Challenges progress</span>
                    <span>{progress.challengesCompleted} / {progress.challengesTotal} challenges</span>
                </div>
                <div className="gs-progress-bar" role="progressbar" aria-label="Challenges progress"
                    aria-valuemin={0} aria-valuemax={progress.challengesTotal} aria-valuenow={progress.challengesCompleted}>
                    <div className="gs-progress-bar-fill gs-progress-bar-fill--accent2"
                        style={{ width: `${percent(progress.challengesCompleted, progress.challengesTotal)}%` }}></div>
                </div>
            </div>

            <div className="gs-progress-block">
                <div className="gs-progress-block-header">
                    <span>Experience progress – {progress.rank}</span>
                    <span>{isTopRank ? `${xp} XP · top rank reached` : `${xp} / ${to} XP`}</span>
                </div>
                <div className="gs-progress-bar" role="progressbar" aria-label="Experience towards the next rank"
                    aria-valuemin={from} aria-valuemax={to} aria-valuenow={Math.min(xp, to)}>
                    <div className="gs-progress-bar-fill" style={{ width: `${xpPercent}%` }}></div>
                </div>
            </div>
        </section>
    );
}

export default ProgressBars;
