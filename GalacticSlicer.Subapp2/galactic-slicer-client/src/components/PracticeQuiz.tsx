import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { getPracticeQuestions } from "../services/challengeService";
import type { Question } from "../types/Question";

interface PracticeQuizProps {
    subject: string;
    // true on "Practice again": focus the first question, since the clicked button is gone
    focusOnStart: boolean;
    onRestart: () => void;
}

// One practice round: answer a question, see right/wrong and the explanation at once,
// then the score at the end. Same flow as the Practice page script in Subapp 1.
// TODO: shows "Could not load the questions." until challengeService gets the questions from the API
function PracticeQuiz({ subject, focusOnStart, onRestart }: PracticeQuizProps) {
    const [questions, setQuestions] = useState<Question[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [currentIndex, setCurrentIndex] = useState(0);
    // the option the player picked for the current question (null = not answered yet)
    const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
    const [correctCount, setCorrectCount] = useState(0);
    const [finished, setFinished] = useState(false);

    const questionRef = useRef<HTMLHeadingElement>(null);
    const nextRef = useRef<HTMLButtonElement>(null);
    const completeRef = useRef<HTMLHeadingElement>(null);

    useEffect(() => {
        async function loadQuestions() {
            try {
                setQuestions(await getPracticeQuestions(subject));
            } catch {
                setError("Could not load the questions.");
            } finally {
                setLoading(false);
            }
        }

        loadQuestions();
    }, [subject]);

    // keyboard and screen reader users start at each new question
    // (not on the first one when the page opens, so the page doesn't jump)
    useEffect(() => {
        if (questions.length > 0 && (currentIndex > 0 || focusOnStart)) {
            questionRef.current?.focus();
        }
    }, [currentIndex, questions, focusOnStart]);

    // the clicked option is disabled after answering, so move focus to the button that continues
    useEffect(() => {
        if (selectedIndex !== null) {
            nextRef.current?.focus();
        }
    }, [selectedIndex]);

    // move focus to the result, so it's read out and Tab continues to its buttons
    useEffect(() => {
        if (finished) {
            completeRef.current?.focus();
        }
    }, [finished]);

    if (loading) {
        return <p className="gs-page-intro" aria-live="polite">Loading questions...</p>;
    }

    if (error) {
        return <p className="gs-page-intro" role="alert">{error}</p>;
    }

    if (questions.length === 0) {
        return (
            <section className="gs-page-intro">
                <p>There are no questions for this subject.</p>
                <Link to="/challenges/topics" className="gs-btn gs-btn--primary">Back to challenges</Link>
            </section>
        );
    }

    const question = questions[currentIndex];
    const answered = selectedIndex !== null;
    const isLast = currentIndex === questions.length - 1;
    const progress = finished ? questions.length : currentIndex;

    function selectAnswer(index: number) {
        if (answered) {
            return;
        }
        setSelectedIndex(index);
        if (index === question.correctIndex) {
            setCorrectCount(count => count + 1);
        }
    }

    function goToNext() {
        if (isLast) {
            setFinished(true);
        } else {
            setCurrentIndex(index => index + 1);
            setSelectedIndex(null);
        }
    }

    // green ✓ on the right answer and red ✗ on a wrong pick, after answering.
    // The symbols mean right and wrong don't depend on seeing green and red.
    function optionClass(index: number) {
        if (!answered) {
            return "gs-quiz-option";
        }
        if (index === question.correctIndex) {
            return "gs-quiz-option gs-quiz-option--correct";
        }
        if (index === selectedIndex) {
            return "gs-quiz-option gs-quiz-option--incorrect";
        }
        return "gs-quiz-option";
    }

    function optionMark(index: number) {
        if (!answered) {
            return "";
        }
        if (index === question.correctIndex) {
            return "✓ ";
        }
        return index === selectedIndex ? "✗ " : "";
    }

    return (
        <section className="gs-quiz-wrapper">
            <div className="gs-quiz-card">
                <div className="gs-quiz-progress-row">
                    {/* aria-live: screen readers read "Question X of Y" when it changes */}
                    <span aria-live="polite">Question {Math.min(currentIndex + 1, questions.length)} of {questions.length}</span>
                    <div className="gs-progress-bar gs-quiz-progress-bar" role="progressbar"
                        aria-label="Practice progress" aria-valuemin={0} aria-valuemax={questions.length} aria-valuenow={progress}>
                        <div className="gs-progress-bar-fill" style={{ width: `${Math.round((progress / questions.length) * 100)}%` }}></div>
                    </div>
                </div>

                {!finished ? (
                    <div>
                        {/* tabIndex -1 so focus can move here when a new question appears */}
                        <h2 className="gs-quiz-question" ref={questionRef} tabIndex={-1}>{question.question}</h2>

                        <div className="gs-quiz-options">
                            {question.options.map((option, index) => (
                                <button
                                    key={`${currentIndex}-${index}`}
                                    type="button"
                                    className={optionClass(index)}
                                    disabled={answered}
                                    onClick={() => selectAnswer(index)}
                                >
                                    {optionMark(index)}{option}
                                </button>
                            ))}
                        </div>

                        {/* read out after answering, hidden on screen (the colours and marks show it) */}
                        <p className="gs-sr-only" aria-live="polite">
                            {answered && (selectedIndex === question.correctIndex
                                ? "Correct!"
                                : `Wrong. The correct answer is: ${question.options[question.correctIndex]}`)}
                        </p>

                        {answered && question.explanation && (
                            <p className="gs-quiz-explanation">{question.explanation}</p>
                        )}

                        {answered && (
                            <button type="button" className="gs-btn gs-btn--primary gs-quiz-next" ref={nextRef} onClick={goToNext}>
                                {isLast ? "See results" : "Next question"}
                            </button>
                        )}
                    </div>
                ) : (
                    <div className="gs-quiz-complete">
                        <h2 ref={completeRef} tabIndex={-1}>Practice complete!</h2>
                        <p>You got {correctCount} out of {questions.length} correct.</p>
                        <div className="gs-quiz-complete-actions">
                            <button type="button" className="gs-btn gs-btn--primary" onClick={onRestart}>
                                Practice again
                            </button>
                            <Link to="/challenges/topics" className="gs-btn gs-btn--outline">Back to challenges</Link>
                        </div>
                    </div>
                )}
            </div>

            <div className="gs-quiz-nav">
                <Link to="/challenges/topics" className="gs-btn gs-btn--ghost">&larr; Back to challenges</Link>
            </div>
        </section>
    );
}

export default PracticeQuiz;
