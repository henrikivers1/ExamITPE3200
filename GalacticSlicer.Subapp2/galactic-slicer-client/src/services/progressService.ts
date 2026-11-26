import type { PlayerProgress } from "../types/PlayerProgress.ts";
import { apiRequest } from "./api.ts";

// TODO: the API has no login or progress endpoint yet. Check the address and the JSON shape
// with Jonas when he adds it (GET /api/progress for the logged-in player is a guess).
export async function getProgress(): Promise<PlayerProgress> {
    return apiRequest<PlayerProgress>("/api/progress");
}
