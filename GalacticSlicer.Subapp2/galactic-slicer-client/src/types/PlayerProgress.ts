import type { Character } from "./Character";
import type { Planet } from "./Planet";

// A planet with the player's status, like PlanetViewModel in Subapp 1
export interface PlanetStatus extends Planet {
    isUnlocked: boolean;
    isCompleted: boolean;
    // XP needed to unlock this planet
    requiredXp: number;
}

// Everything the Progress page shows for the logged-in player, like PlayerDashboardViewModel in Subapp 1.
// TODO: match this to the API's progress DTO once Jonas has the endpoint.
export interface PlayerProgress {
    username: string;
    rank: string;
    credits: number;
    experiencePoints: number;
    // XP where the current rank starts and where the next one starts (equal at the top rank)
    xpForCurrentRank: number;
    xpForNextRank: number;
    challengesCompleted: number;
    challengesTotal: number;
    // equipped character for the badge (null = nothing equipped)
    activeCharacter: Character | null;
    // every planet in chapter order
    planets: PlanetStatus[];
}
