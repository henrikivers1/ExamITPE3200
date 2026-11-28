
import { Link } from "react-router-dom";

import type { Planet } from "../types/Planet";
import type {
    PlanetStatus,
} from "../types/PlayerProgress";

interface PlanetCardProps {
    planet: Planet;
    status?: PlanetStatus;
    isLoggedIn: boolean;
}

function PlanetCard({
    planet,
    status,
    isLoggedIn,
}: PlanetCardProps) {
    const isUnlocked = status?.isUnlocked ?? false;
    const isCompleted = status?.isCompleted ?? false;

    const canStart =
        isLoggedIn &&
        status !== undefined &&
        isUnlocked &&
        !isCompleted;

    const stateClass = !isLoggedIn || !status
        ? ""
        : isCompleted
          ? " gs-star-node--completed"
          : isUnlocked
            ? " gs-star-node--unlocked"
            : " gs-star-node--locked";

    const content = (
        <>
            <span
                className={
                    `gs-planet gs-planet--${planet.cssClass}`
                }
                aria-hidden="true"
            />

            <span className="gs-star-node-info">
                <span className="gs-star-node-step">
                    Chapter {planet.order}
                </span>

                <span className="gs-star-node-name">
                    {planet.name}
                </span>

                {planet.subject && (
                    <span className="gs-star-node-meta">
                        {planet.subject}
                    </span>
                )}

                {!isLoggedIn && (
                    <span className="gs-star-node-meta">
                        Log in to start missions
                    </span>
                )}

                {isLoggedIn && status && (
                    <span className="gs-star-node-meta">
                        {isCompleted ? (
                            <strong>✓ Completed</strong>
                        ) : !isUnlocked ? (
                            <strong>🔒 Locked</strong>
                        ) : (
                            <strong>
                                ▶ Start Mission
                            </strong>
                        )}
                    </span>
                )}

                {isLoggedIn && !status && (
                    <span className="gs-star-node-meta">
                        Progress unavailable
                    </span>
                )}
            </span>
        </>
    );

    return (
        <li className={`gs-star-node${stateClass}`}>
            {canStart ? (
                <Link
                    className="gs-star-node-link"
                    to={`/planets/${planet.planetId}/mission`}
                    aria-label={
                        `Start ${planet.name} mission`
                    }
                >
                    {content}
                </Link>
            ) : (
                <div className="gs-star-node-link">
                    {content}
                </div>
            )}
        </li>
    );
}

export default PlanetCard;
