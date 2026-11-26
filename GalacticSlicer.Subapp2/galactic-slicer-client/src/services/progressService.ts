
import { apiRequest } from "./api";
import type { PlayerProgress } from "../types/PlayerProgress";

export async function getProgress(): Promise<PlayerProgress> {
    return apiRequest<PlayerProgress>("/api/progress/me");
}
