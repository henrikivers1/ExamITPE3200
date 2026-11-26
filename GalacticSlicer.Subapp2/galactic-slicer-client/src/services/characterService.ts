import type { Character } from "../types/Character.ts";
import { apiRequest } from "./api.ts";

// TODO: the API has no characters endpoint yet, so this request fails (404) and the Shop
// shows "Could not load the characters.". Check the address and the JSON shape with Jonas
// when he adds it (GET /api/characters is a guess that follows /api/planets).
export async function getCharacters(): Promise<Character[]> {
    return apiRequest<Character[]>("/api/characters");
}

// TODO: Buy, Equip and Unequip need login and endpoints in the API, e.g.
// POST /api/characters/{id}/buy, POST /api/characters/{id}/equip, POST /api/characters/unequip.
// Add them here when they exist, so the Shop page only calls this service.
