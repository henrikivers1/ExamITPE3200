
import type { Character } from "../types/Character";
import { apiRequest } from "./api";

// GET /api/characters
// Returns all available characters in the shop.
export async function getCharacters(): Promise<Character[]> {
    return apiRequest<Character[]>("/api/characters");
}

// What the admin form sends when creating or editing a character.
export interface CharacterInput {
    name: string;
    description: string | null;
    price: number;
    imageUrl: string | null;
}

// POST /api/characters (Admin)
export async function createCharacter(
    data: CharacterInput
): Promise<Character> {
    return apiRequest<Character>("/api/characters", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

// PUT /api/characters/{id} (Admin)
export async function updateCharacter(
    id: number,
    data: CharacterInput
): Promise<void> {
    await apiRequest<unknown>(`/api/characters/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

// DELETE /api/characters/{id} (Admin)
// The API blocks standard and owned characters.
export async function deleteCharacter(
    id: number
): Promise<void> {
    await apiRequest<unknown>(`/api/characters/${id}`, {
        method: "DELETE",
    });
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
