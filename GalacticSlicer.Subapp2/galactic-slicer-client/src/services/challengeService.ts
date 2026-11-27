
import { apiRequest } from "./api";
import type { Topic } from "../types/Question.ts";

// ============================================
// CHALLENGE TYPES
// ============================================

export interface Challenge {
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

    isOwnedByCurrentUser: boolean;

    correctAnswer?: string | null;
    explanation?: string | null;
}

export interface ChallengeInput {
    question: string;
    optionA: string;
    optionB: string;
    optionC: string;
    optionD: string;
    correctAnswer: string;
    subject: string | null;
    explanation: string | null;
}

// ============================================
// CHALLENGE CRUD
// ============================================

export async function getChallenges(): Promise<Challenge[]> {
    return apiRequest<Challenge[]>("/api/challenges");
}

export async function searchChallenges(
    searchString: string,
    subject: string
): Promise<Challenge[]> {
    const params = new URLSearchParams();

    if (searchString.trim()) {
        params.set("searchString", searchString.trim());
    }

    if (subject.trim()) {
        params.set("subject", subject.trim());
    }

    const query = params.toString();

    return apiRequest<Challenge[]>(
        `/api/challenges/search${query ? `?${query}` : ""}`
    );
}

export async function getChallengeById(
    id: number
): Promise<Challenge> {
    return apiRequest<Challenge>(
        `/api/challenges/${id}`
    );
}

export async function createChallenge(
    challenge: ChallengeInput
): Promise<Challenge> {
    return apiRequest<Challenge>("/api/challenges", {
        method: "POST",
        body: JSON.stringify(challenge),
    });
}

export async function updateChallenge(
    id: number,
    challenge: ChallengeInput
): Promise<void> {
    await apiRequest<unknown>(
        `/api/challenges/${id}`,
        {
            method: "PUT",
            body: JSON.stringify(challenge),
        }
    );
}

export async function deleteChallenge(
    id: number
): Promise<void> {
    await apiRequest<unknown>(
        `/api/challenges/${id}`,
        {
            method: "DELETE",
        }
    );
}

// ============================================
// TOPICS
// ============================================

export const QUESTIONS_PER_ROUND = 10;

const subjectInfo: Record<
    string,
    { iconKey: string; order: number }
> = {
    "Linux Command Line": {
        iconKey: "linux",
        order: 1,
    },
    "Network Security": {
        iconKey: "network",
        order: 2,
    },
    "Software Security": {
        iconKey: "web",
        order: 3,
    },
    "Access Control & Authentication": {
        iconKey: "access",
        order: 4,
    },
    "Cryptography Basics": {
        iconKey: "crypto",
        order: 5,
    },
};

export async function getTopics(): Promise<Topic[]> {
    const challenges = await getChallenges();
    const counts = new Map<string, number>();

    // Count only official questions assigned to planets.
    // User-created challenges have planetId = null.
    for (const challenge of challenges) {
        if (
            challenge.planetId === null ||
            !challenge.subject
        ) {
            continue;
        }

        counts.set(
            challenge.subject,
            (counts.get(challenge.subject) ?? 0) + 1
        );
    }

    const infoFor = (subject: string) =>
        subjectInfo[subject] ?? {
            iconKey: "access",
            order: 99,
        };

    return [...counts.entries()]
        .sort(
            ([a], [b]) =>
                infoFor(a).order - infoFor(b).order
        )
        .map(([subject, questionCount]) => {
            const { iconKey, order } = infoFor(subject);

            return {
                subject,
                questionCount,
                iconKey,
                difficulty:
                    order <= 2
                        ? "Beginner"
                        : order <= 4
                          ? "Intermediate"
                          : "Advanced",
            };
        });
}

// ============================================
// PRACTICE API
// ============================================

// Question sent by the API without a correct answer.
export interface PracticeQuestion {
    challengeId: number;
    question: string;
    subject: string | null;

    optionA: string;
    optionB: string;
    optionC: string;
    optionD: string;
}

// Result returned only after submitting an answer.
export interface PracticeAnswerResult {
    isCorrect: boolean;
    correctAnswer: string;
    explanation: string | null;
}

// Fetch up to 10 random official questions.
export async function getPracticeQuestions(
    subject: string
): Promise<PracticeQuestion[]> {
    const params = new URLSearchParams({
        subject,
    });

    return apiRequest<PracticeQuestion[]>(
        `/api/practice/questions?${params.toString()}`
    );
}

// Check one answer against the database.
export async function checkPracticeAnswer(
    challengeId: number,
    answer: string
): Promise<PracticeAnswerResult> {
    return apiRequest<PracticeAnswerResult>(
        "/api/practice/check",
        {
            method: "POST",
            body: JSON.stringify({
                challengeId,
                answer,
            }),
        }
    );
}
