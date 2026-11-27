
import type { Character } from "./Character";
import type { Planet } from "./Planet";

// A planet with the player's status,
// like PlanetViewModel in Subapp 1.
export interface PlanetStatus extends Planet {
    isUnlocked: boolean;
    isCompleted: boolean;

    // XP needed to unlock this planet
    requiredXp: number;
}

// Everything the Progress page shows
// for the logged-in player.
export interface PlayerProgress {
    username: string;
    rank: string;
    credits: number;
    experiencePoints: number;

    // XP where the current rank starts
    // and where the next one starts.
    xpForCurrentRank: number;
    xpForNextRank: number;

    challengesCompleted: number;
    challengesTotal: number;

    // ID of the equipped character.
    // null means no character is equipped.
    activeCharacterId: number | null;

    // Full information about the equipped character.
    activeCharacter: Character | null;

    // Every planet in chapter order.
    planets: PlanetStatus[];
}
