
import type { Character } from "../types/Character";
import { apiRequest } from "./api";

// GET /api/characters
// Returns all available characters in the shop.
export async function getCharacters(): Promise<Character[]> {
    return apiRequest<Character[]>("/api/characters");
}

// GET /api/characters/owned
// Returns character IDs owned by the logged-in player.
export async function getOwnedCharacterIds(): Promise<number[]> {
    return apiRequest<number[]>("/api/characters/owned");
}

// POST /api/characters/{id}/buy
// The API checks Credits, ownership and purchase rules.
export async function buyCharacter(
    characterId: number
): Promise<void> {
    await apiRequest<unknown>(
        `/api/characters/${characterId}/buy`,
        {
            method: "POST",
        }
    );
}

// POST /api/characters/{id}/equip
// The API checks that the player owns the character.
export async function equipCharacter(
    characterId: number
): Promise<void> {
    await apiRequest<unknown>(
        `/api/characters/${characterId}/equip`,
        {
            method: "POST",
        }
    );
}

// POST /api/characters/unequip
export async function unequipCharacter(): Promise<void> {
    await apiRequest<unknown>(
        "/api/characters/unequip",
        {
            method: "POST",
        }
    );
}
