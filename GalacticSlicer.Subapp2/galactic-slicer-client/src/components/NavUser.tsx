import { useEffect, useState } from "react";
import CharacterBadge from "./CharacterBadge";
import {
    getProgress,
    PROGRESS_CHANGED,
} from "../services/progressService";
import type { Character } from "../types/Character";

interface NavUserData {
    email: string;
    credits: number;
    activeCharacter: Character | null;
}

// Badge, name and Credits in the navbar, like NavUserViewComponent in Subapp 1.
// Only shown when logged in (Navbar decides). Reloads after the Shop or a mission
// changes the Credits or the badge (the "progress-changed" event).
function NavUser() {
    const [user, setUser] = useState<NavUserData | null>(null);

    useEffect(() => {
        let cancelled = false;

        async function load() {
            try {
                const progress = await getProgress();

                if (!cancelled) {
                    setUser({
                        email: progress.username,
                        credits: progress.credits,
                        activeCharacter: progress.activeCharacter,
                    });
                }
            } catch {
                // Like Subapp 1: hide the badge instead of breaking the navbar
                if (!cancelled) setUser(null);
            }
        }

        void load();
        window.addEventListener(PROGRESS_CHANGED, load);

        return () => {
            cancelled = true;
            window.removeEventListener(PROGRESS_CHANGED, load);
        };
    }, []);

    if (!user) return null;

    // Subapp 2 logs in with email, so show the part before the @ as the name
    const name = user.email.split("@")[0] || user.email;

    return (
        <div className="gs-nav-user">
            {/* the equipped character's badge, or an empty circle if nothing is equipped */}
            <span
                className="gs-user-badge"
                title={user.activeCharacter?.name ?? "No badge equipped"}
            >
                <CharacterBadge character={user.activeCharacter} />
            </span>
            <span className="gs-user-name" title={user.email}>{name}</span>
            <span className="gs-user-credits">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                     strokeWidth="2" aria-hidden="true">
                    <circle cx="12" cy="12" r="9" />
                    <path d="M12 7v10M9.5 9.5h4a1.8 1.8 0 0 1 0 3.6h-3a1.8 1.8 0 0 0 0 3.6h4.5" />
                </svg>
                {user.credits}
                <span className="gs-sr-only"> Credits</span>
            </span>
        </div>
    );
}

export default NavUser;
