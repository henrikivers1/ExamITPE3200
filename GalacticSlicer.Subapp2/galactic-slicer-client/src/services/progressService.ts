
import { apiRequest } from "./api";
import type { PlayerProgress } from "../types/PlayerProgress";

export async function getProgress(): Promise<PlayerProgress> {
    return apiRequest<PlayerProgress>("/api/progress/me");
}

// Sent after something changes the Credits or the badge (Shop, missions),
// so the navbar (NavUser) loads the new values.
export const PROGRESS_CHANGED = "progress-changed";

export function notifyProgressChanged(): void {
    window.dispatchEvent(new Event(PROGRESS_CHANGED));
}
