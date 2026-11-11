import type { Planet } from "../types/Planet";

interface PlanetCardProps {
    planet: Planet;
}

// One planet on the starchart: the CSS planet on top, chapter, name, description and subject underneath.
// Same markup as the Subapp 1 Galaxy page, but without Locked/Unlocked:
// the API doesn't know who the player is yet, so we don't guess.
function PlanetCard({ planet }: PlanetCardProps) {
    return (
        <li className="gs-star-node">
            <div className="gs-star-node-link">
                <span className={`gs-planet gs-planet--${planet.cssClass}`} aria-hidden="true"></span>
                <span className="gs-star-node-info">
                    <span className="gs-star-node-step">Chapter {planet.order}</span>
                    <span className="gs-star-node-name">{planet.name}</span>
                    {planet.description && (
                        <span className="gs-star-node-meta">{planet.description}</span>
                    )}
                    {/* only shown when the API sends one (the current seed data has none yet) */}
                    {planet.subject && (
                        <span className="gs-star-node-meta">{planet.subject}</span>
                    )}
                </span>
            </div>
        </li>
    );
}

export default PlanetCard;
