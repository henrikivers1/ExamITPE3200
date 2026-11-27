// One question, same shape as an entry in Subapp 1's questions.json.
export interface Question {
    id: string;
    subject: string;
    type: string;
    question: string;
    options: string[];
    // position of the right answer in options (0-3)
    correctIndex: number;
    explanation: string | null;
}

// One subject card on the Challenges page
export interface Topic {
    subject: string;
    questionCount: number;
    iconKey: string;
    difficulty: string;
}
