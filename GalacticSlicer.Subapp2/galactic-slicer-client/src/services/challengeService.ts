import type { Question, Topic } from "../types/Question.ts";

// TODO: questions.json was removed from Subapp 2 (public/data/) to wait for Jonas's import into the API.
// Until then this URL doesn't exist, so the Challenges and Practice pages show "Could not load ...".
// When the API has a challenges endpoint, load the questions with apiRequest("/api/challenges...")
// instead of fetch(QUESTIONS_URL) below. Only this file needs to change; the pages stay the same.
const QUESTIONS_URL = "/data/questions.json";

export const QUESTIONS_PER_ROUND = 10;

// Icon and difficulty per subject, like ChallengeController.Topics in Subapp 1.
// order = the planet that has this subject as its mission (Tatooine 1 ... Hoth 5).
const subjectInfo: Record<string, { iconKey: string; order: number }> = {
    "Linux Command Line": { iconKey: "linux", order: 1 },
    "Network Security": { iconKey: "network", order: 2 },
    "Software Security": { iconKey: "web", order: 3 },
    "Access Control & Authentication": { iconKey: "access", order: 4 },
    "Cryptography Basics": { iconKey: "crypto", order: 5 },
};

// Checks one entry from the file before using it: a broken question is skipped
// instead of breaking the quiz (e.g. 3 options, or correctIndex pointing at no option)
function isValidQuestion(q: Question): boolean {
    return typeof q.id === "string"
        && typeof q.subject === "string" && q.subject.trim() !== ""
        && typeof q.question === "string" && q.question.trim() !== ""
        && Array.isArray(q.options) && q.options.length === 4
        && q.options.every(option => typeof option === "string" && option.trim() !== "")
        && Number.isInteger(q.correctIndex) && q.correctIndex >= 0 && q.correctIndex < q.options.length;
}

// all valid questions from the file
export async function getQuestions(): Promise<Question[]> {
    let response: Response;
    try {
        response = await fetch(QUESTIONS_URL);
    } catch {
        throw new Error("Could not load the questions.");
    }

    if (!response.ok) {
        throw new Error("Could not load the questions.");
    }

    const data: Question[] = await response.json();
    const valid = data.filter(isValidQuestion);

    if (valid.length < data.length) {
        console.warn(`Skipped ${data.length - valid.length} invalid question(s)`);
    }

    return valid;
}

// one card per subject with its number of questions, in planet order
export async function getTopics(): Promise<Topic[]> {
    const questions = await getQuestions();
    const counts = new Map<string, number>();

    for (const q of questions) {
        counts.set(q.subject, (counts.get(q.subject) ?? 0) + 1);
    }

    // subjects that aren't in subjectInfo go last, with the default icon
    const infoFor = (subject: string) => subjectInfo[subject] ?? { iconKey: "access", order: 99 };

    return [...counts.entries()]
        .sort(([a], [b]) => infoFor(a).order - infoFor(b).order)
        .map(([subject, questionCount]) => {
            const { iconKey, order } = infoFor(subject);
            return {
                subject,
                questionCount,
                iconKey,
                // same split as Subapp 1: chapter 1-2, 3-4, 5+
                difficulty: order <= 2 ? "Beginner" : order <= 4 ? "Intermediate" : "Advanced",
            };
        });
}

// Fisher-Yates shuffle: every order is equally likely (sort with Math.random is not)
function shuffle<T>(items: T[]): T[] {
    const copy = [...items];
    for (let i = copy.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
}

// 10 random questions from one subject, each with its options in a random order.
// correctIndex is moved along with the right answer, so it still points at it.
export async function getPracticeQuestions(subject: string): Promise<Question[]> {
    const questions = await getQuestions();
    const pool = questions.filter(q => q.subject === subject);

    return shuffle(pool)
        .slice(0, QUESTIONS_PER_ROUND)
        .map(q => {
            const order = shuffle(q.options.map((_, index) => index));
            return {
                ...q,
                options: order.map(index => q.options[index]),
                correctIndex: order.indexOf(q.correctIndex),
            };
        });
}
