
import { apiRequest } from "./api";

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

export type MissionResult =
    | {
          passed: false;
          message: string;
      }
    | {
          passed: true;
          credits: number;
          experiencePoints: number;
          rank: string;
      };

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
