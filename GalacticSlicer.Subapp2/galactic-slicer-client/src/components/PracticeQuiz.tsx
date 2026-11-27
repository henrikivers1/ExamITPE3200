
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import {
    getPracticeQuestions,
    checkPracticeAnswer,
    type PracticeQuestion,
    type PracticeAnswerResult,
} from "../services/challengeService";

interface PracticeQuizProps {
    subject: string;
    focusOnStart: boolean;
    onRestart: () => void;
}

function PracticeQuiz({
    subject,
    focusOnStart,
    onRestart,
}: PracticeQuizProps) {
    const [questions, setQuestions] =
        useState<PracticeQuestion[]>([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [currentIndex, setCurrentIndex] = useState(0);
    const [selectedIndex, setSelectedIndex] =
        useState<number | null>(null);

    const [answerResult, setAnswerResult] =
        useState<PracticeAnswerResult | null>(null);

    const [checking, setChecking] = useState(false);
    const [correctCount, setCorrectCount] = useState(0);
    const [finished, setFinished] = useState(false);

    const submittingRef = useRef(false);
    const questionRef = useRef<HTMLHeadingElement>(null);
    const nextRef = useRef<HTMLButtonElement>(null);
    const completeRef = useRef<HTMLHeadingElement>(null);

    // ============================================
    // LOAD QUESTIONS FROM API
    // ============================================

    useEffect(() => {
        let cancelled = false;

        async function loadQuestions() {
            setLoading(true);
            setError(null);

            try {
                const result = await getPracticeQuestions(subject);

                if (!cancelled) {
                    setQuestions(result);
                }
            } catch (err) {
                if (!cancelled) {
                    setError(
                        err instanceof Error
                            ? err.message
                            : "Could not load the questions."
                    );
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        void loadQuestions();

        return () => {
            cancelled = true;
        };
    }, [subject]);

    // ============================================
    // ACCESSIBILITY
    // ============================================

    useEffect(() => {
        if (
            questions.length > 0 &&
            !finished &&
            (currentIndex > 0 || focusOnStart)
        ) {
            questionRef.current?.focus();
        }
    }, [currentIndex, questions, focusOnStart, finished]);

    useEffect(() => {
        if (answerResult !== null) {
            nextRef.current?.focus();
        }
    }, [answerResult]);

    useEffect(() => {
        if (finished) {
            completeRef.current?.focus();
        }
    }, [finished]);

    // ============================================
    // CHECK ANSWER ON SERVER
    // ============================================

    async function selectAnswer(index: number) {
        if (
            submittingRef.current ||
            answerResult !== null ||
            finished
        ) {
            return;
        }

        const question = questions[currentIndex];

        if (!question) return;

        const options = [
            question.optionA,
            question.optionB,
            question.optionC,
            question.optionD,
        ];

        const selectedAnswer = options[index];

        if (!selectedAnswer) return;

        submittingRef.current = true;
        setChecking(true);
        setSelectedIndex(index);
        setError(null);

        try {
            const result = await checkPracticeAnswer(
                question.challengeId,
                selectedAnswer
            );

            setAnswerResult(result);

            if (result.isCorrect) {
                setCorrectCount(count => count + 1);
            }
        } catch (err) {
            // Restore the option buttons when checking fails,
            // so the player can retry their answer.
            setSelectedIndex(null);

            setError(
                err instanceof Error
                    ? err.message
                    : "Could not check the answer."
            );
        } finally {
            submittingRef.current = false;
            setChecking(false);
        }
    }

    // ============================================
    // NEXT QUESTION
    // ============================================

    function goToNext() {
        if (answerResult === null) return;

        const isLast =
            currentIndex === questions.length - 1;

        if (isLast) {
            setFinished(true);
        } else {
            setCurrentIndex(index => index + 1);
            setSelectedIndex(null);
            setAnswerResult(null);
            setError(null);
        }
    }

    // ============================================
    // OPTION STYLING
    // ============================================

    function optionClass(
        index: number,
        options: string[]
    ) {
        if (answerResult === null) {
            return "gs-quiz-option";
        }

        const option = options[index];

        if (option === answerResult.correctAnswer) {
            return "gs-quiz-option gs-quiz-option--correct";
        }

        if (index === selectedIndex) {
            return "gs-quiz-option gs-quiz-option--incorrect";
        }

        return "gs-quiz-option";
    }

    function optionMark(
        index: number,
        options: string[]
    ) {
        if (answerResult === null) return "";

        if (options[index] === answerResult.correctAnswer) {
            return "✓ ";
        }

        return index === selectedIndex ? "✗ " : "";
    }

    // ============================================
    // LOADING AND EMPTY STATES
    // ============================================

    if (loading) {
        return (
            <p
                className="gs-page-intro"
                aria-live="polite"
            >
                Loading questions...
            </p>
        );
    }

    if (questions.length === 0) {
        return (
            <section className="gs-page-intro">
                {error ? (
                    <p role="alert">{error}</p>
                ) : (
                    <p>
                        There are no questions for this subject.
                    </p>
                )}

                <Link
                    to="/challenges/topics"
                    className="gs-btn gs-btn--primary"
                >
                    Back to challenges
                </Link>
            </section>
        );
    }

    const question = questions[currentIndex];

    const options = [
        question.optionA,
        question.optionB,
        question.optionC,
        question.optionD,
    ];

    const answered = answerResult !== null;

    const isLast =
        currentIndex === questions.length - 1;

    const progress = finished
        ? questions.length
        : currentIndex;

    // ============================================
    // QUIZ UI
    // ============================================

    return (
        <section className="gs-quiz-wrapper">
            <div className="gs-quiz-card">
                <div className="gs-quiz-progress-row">
                    <span aria-live="polite">
                        Question{" "}
                        {Math.min(
                            currentIndex + 1,
                            questions.length
                        )}{" "}
                        of {questions.length}
                    </span>

                    <div
                        className="gs-progress-bar gs-quiz-progress-bar"
                        role="progressbar"
                        aria-label="Practice progress"
                        aria-valuemin={0}
                        aria-valuemax={questions.length}
                        aria-valuenow={progress}
                    >
                        <div
                            className="gs-progress-bar-fill"
                            style={{
                                width: `${
                                    Math.round(
                                        (progress / questions.length) * 100
                                    )
                                }%`,
                            }}
                        />
                    </div>
                </div>

                {!finished ? (
                    <div>
                        <h2
                            className="gs-quiz-question"
                            ref={questionRef}
                            tabIndex={-1}
                        >
                            {question.question}
                        </h2>

                        <div className="gs-quiz-options">
                            {options.map((option, index) => (
                                <button
                                    key={`${currentIndex}-${index}`}
                                    type="button"
                                    className={optionClass(
                                        index,
                                        options
                                    )}
                                    disabled={checking || answered}
                                    onClick={() =>
                                        void selectAnswer(index)
                                    }
                                >
                                    {optionMark(index, options)}
                                    {option}
                                </button>
                            ))}
                        </div>

                        {checking && (
                            <p role="status">
                                Checking answer...
                            </p>
                        )}

                        {error && (
                            <p role="alert">
                                {error}
                            </p>
                        )}

                        {answered && (
                            <p
                                className="gs-sr-only"
                                aria-live="polite"
                            >
                                {answerResult.isCorrect
                                    ? "Correct!"
                                    : `Wrong. The correct answer is: ${answerResult.correctAnswer}`}
                            </p>
                        )}

                        {answered &&
                            answerResult.explanation && (
                            <p className="gs-quiz-explanation">
                                {answerResult.explanation}
                            </p>
                        )}

                        {answered && (
                            <button
                                type="button"
                                className="gs-btn gs-btn--primary gs-quiz-next"
                                ref={nextRef}
                                onClick={goToNext}
                            >
                                {isLast
                                    ? "See results"
                                    : "Next question"}
                            </button>
                        )}
                    </div>
                ) : (
                    <div className="gs-quiz-complete">
                        <h2
                            ref={completeRef}
                            tabIndex={-1}
                        >
                            Practice complete!
                        </h2>

                        <p>
                            You got {correctCount} out of{" "}
                            {questions.length} correct.
                        </p>

                        <div className="gs-quiz-complete-actions">
                            <button
                                type="button"
                                className="gs-btn gs-btn--primary"
                                onClick={onRestart}
                            >
                                Practice again
                            </button>

                            <Link
                                to="/challenges/topics"
                                className="gs-btn gs-btn--outline"
                            >
                                Back to challenges
                            </Link>
                        </div>
                    </div>
                )}
            </div>

            <div className="gs-quiz-nav">
                <Link
                    to="/challenges/topics"
                    className="gs-btn gs-btn--ghost"
                >
                    &larr; Back to challenges
                </Link>
            </div>
        </section>
    );
}

export default PracticeQuiz;
