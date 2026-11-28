import type { Character } from "../types/Character";
import { getTier, starburstPoints } from "../utils/characterTier";

interface CharacterBadgeProps {
    // null = nothing equipped: draws an empty circle instead
    character: Character | null;
}

// Space badge for a character, same as Views/Shared/_CharacterBadge.cshtml in Subapp 1.
// The outer shape shows the tier, the emblem in the middle shows the character.
// The size comes from the element around it (styles in theme.css under "character badge").
function CharacterBadge({ character }: CharacterBadgeProps) {
    if (character === null) {
        return (
            <svg className="gs-badge gs-badge--empty" viewBox="0 0 100 100" aria-hidden="true">
                <circle className="gs-badge-slot" cx="50" cy="50" r="40" />
            </svg>
        );
    }

    const tier = getTier(character.price).key;

    return (
        <svg className={`gs-badge gs-badge--${tier}`} viewBox="0 0 100 100" aria-hidden="true">
            {/* ribbon tails hanging behind the epic and legendary badges */}
            {(tier === "epic" || tier === "legendary") && (
                <>
                    <path className="gs-badge-ribbon" d="M30 62 L20 96 L32 90 L40 99 L46 70 Z" />
                    <path className="gs-badge-ribbon" d="M70 62 L80 96 L68 90 L60 99 L54 70 Z" />
                </>
            )}

            <BadgeShape tier={tier} />

            {/* emblem for the character, drawn on a 24x24 grid and scaled into the middle */}
            <g className="gs-badge-icon" transform="translate(32 31) scale(1.5)">
                <Emblem name={character.name} />
            </g>
        </svg>
    );
}

// outer shape and inner ring, one pair per tier
function BadgeShape({ tier }: { tier: string }) {
    switch (tier) {
        case "common":
            return (
                <>
                    <circle className="gs-badge-base" cx="50" cy="50" r="40" />
                    <circle className="gs-badge-ring" cx="50" cy="50" r="31" />
                </>
            );
        case "uncommon":
            return (
                <>
                    <polygon className="gs-badge-base" points="50,8 86,29 86,71 50,92 14,71 14,29" />
                    <polygon className="gs-badge-ring" points="50,18 77,34 77,66 50,82 23,66 23,34" />
                </>
            );
        case "rare":
            return (
                <>
                    <path className="gs-badge-base" d="M50 7 L85 19 V47 C85 70 69 85 50 93 C31 85 15 70 15 47 V19 Z" />
                    <path className="gs-badge-ring" d="M50 16 L76 25 V47 C76 64 64 76 50 83 C36 76 24 64 24 47 V25 Z" />
                </>
            );
        case "epic":
            return (
                <>
                    <polygon className="gs-badge-base" points="34,10 66,10 89,33 89,63 66,86 34,86 11,63 11,33" />
                    <polygon className="gs-badge-ring" points="38,19 62,19 80,37 80,59 62,77 38,77 20,59 20,37" />
                </>
            );
        default:
            return (
                <>
                    <polygon className="gs-badge-base" points={starburstPoints()} />
                    <circle className="gs-badge-ring" cx="50" cy="50" r="29" />
                </>
            );
    }
}

// the character's emblem; any new character without its own emblem gets a star
function Emblem({ name }: { name: string }) {
    switch (name) {
        case "Rookie Slicer":
            // terminal prompt
            return (
                <>
                    <rect x="3" y="5" width="18" height="14" rx="2" />
                    <path d="M7 10l3 2-3 2M12.5 15h4.5" />
                </>
            );
        case "Astromech Hacker":
            // astromech droid: dome, body and legs
            return (
                <>
                    <path d="M6.5 11a5.5 5.5 0 0 1 11 0" />
                    <rect x="6.5" y="11" width="11" height="8" rx="1" />
                    <circle cx="12" cy="8.5" r="1.2" />
                    <path d="M6.5 19l-2 2.5M17.5 19l2 2.5M10 14.5h4" />
                </>
            );
        case "Smuggler":
            // light freighter seen from above
            return (
                <>
                    <circle cx="11" cy="12" r="6" />
                    <path d="M15 8.5l6-1.5v3l-4 1M15 15.5l6 1.5v-3l-4-1" />
                    <circle cx="11" cy="12" r="1.5" />
                </>
            );
        case "Bounty Hunter":
            // targeting crosshair
            return (
                <>
                    <circle cx="12" cy="12" r="7" />
                    <circle cx="12" cy="12" r="1.5" />
                    <path d="M12 2.5v5M12 16.5v5M2.5 12h5M16.5 12h5" />
                </>
            );
        case "Shadow Operative":
            // watchful eye
            return (
                <>
                    <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6Z" />
                    <circle cx="12" cy="12" r="2.6" />
                </>
            );
        default:
            return <path d="M12 3l2.7 5.6 6.1.9-4.4 4.3 1 6.1L12 17l-5.4 2.9 1-6.1-4.4-4.3 6.1-.9Z" />;
    }
}

export default CharacterBadge;
