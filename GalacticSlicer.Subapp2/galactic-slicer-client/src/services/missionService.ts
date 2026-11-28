
import { apiRequest } from "./api";

// Same values as ChallengeCompletionService in the API:
// pass at 80%, 2000 XP per mission and 500 Credits per chapter.
// Shown in the mission briefing; the API decides the real rewards.
export const PASS_PERCENT = 80;
export const XP_PER_MISSION = 2000;

export function creditsForMission(chapter: number): number {
    return 500 * chapter;
}

// The API returns questions without correct answers.
export interface MissionQuestion {
    challengeId: number;
    title: string;
    question: string;
    optionA: string;
    optionB: string;
    optionC: string;
    optionD: string;
    subject: string | null;
    planetId: number | null;
    creditReward: number;
}

// Mission submission maps a challenge ID to the
// actual answer text selected by the player.
export interface MissionAnswers {
    [challengeId: number]: string;
}

// What POST /api/missions/{planetId}/submit returns.
export interface MissionResult {
    passed: boolean;
    correctCount: number;
    totalQuestions: number;

    // true when the mission was passed before (a replay)
    alreadyCompleted: boolean;

    // the real rewards: 0 on a failed mission or a replay
    awardedCredits: number;
    awardedXp: number;

    // the player's totals after the mission
    credits: number;
    experiencePoints: number;
    rank: string;
}

// GET /api/missions/{planetId}
export async function getMission(
    planetId: number
): Promise<MissionQuestion[]> {
    return apiRequest<MissionQuestion[]>(
        `/api/missions/${planetId}`
    );
}

// POST /api/missions/{planetId}/submit
export async function submitMission(
    planetId: number,
    answers: MissionAnswers
): Promise<MissionResult> {
    return apiRequest<MissionResult>(
        `/api/missions/${planetId}/submit`,
        {
            method: "POST",
            body: JSON.stringify({
                answers,
            }),
        }
    );
}
