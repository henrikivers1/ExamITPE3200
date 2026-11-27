
import {
    useEffect,
    useRef,
    useState,
} from "react";
import { Link, useParams } from "react-router-dom";

import { usePageTitle } from "../hooks/usePageTitle";
import { getToken } from "../services/api";
import { getProgress } from "../services/progressService";

import {
    getMission,
    submitMission,
    type MissionQuestion,
    type MissionAnswers,
    type MissionResult,
} from "../services/missionService";

import type { PlanetStatus } from "../types/PlayerProgress";

function MissionPage() {
    const { planetId: planetIdParam } = useParams();
    const planetId = Number(planetIdParam);

    usePageTitle("Mission");

    const [planet, setPlanet] =
        useState<PlanetStatus | null>(null);

    const [questions, setQuestions] =
        useState<MissionQuestion[]>([]);

    const [answers, setAnswers] =
        useState<MissionAnswers>({});

    const [currentIndex, setCurrentIndex] = useState(0);

    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

    const [error, setError] =
        useState<string | null>(null);

    const [result, setResult] =
        useState<MissionResult | null>(null);

    const [alreadyCompleted, setAlreadyCompleted] =
        useState(false);

    const [locked, setLocked] = useState(false);

    const submittingRef = useRef(false);
    const questionRef = useRef<HTMLHeadingElement>(null);
    const resultRef = useRef<HTMLHeadingElement>(null);

    const isLoggedIn = Boolean(getToken());

    // ============================================
    // LOAD MISSION
    // ============================================

    useEffect(() => {
        if (
            !isLoggedIn ||
            !Number.isInteger(planetId) ||
            planetId <= 0
        ) {
            setLoading(false);
            return;
        }

        let cancelled = false;

        async function loadMission() {
            setLoading(true);
            setError(null);
            setResult(null);
            setAnswers({});
            setCurrentIndex(0);
            setQuestions([]);
            setAlreadyCompleted(false);
            setLocked(false);
            setPlanet(null);

            try {
                const progress = await getProgress();

                if (cancelled) return;

                const currentPlanet = progress.planets.find(
                    p => p.planetId === planetId
                );

                if (!currentPlanet) {
                    setError("Planet not found.");
                    return;
                }

                setPlanet(currentPlanet);

                if (currentPlanet.isCompleted) {
                    setAlreadyCompleted(true);
                    return;
                }

                if (!currentPlanet.isUnlocked) {
                    setLocked(true);
                    return;
                }

                const missionQuestions =
                    await getMission(planetId);

                if (!cancelled) {
                    setQuestions(missionQuestions);
                }
            } catch (err) {
                if (!cancelled) {
                    setError(
                        err instanceof Error
                            ? err.message
                            : "Could not load the mission."
                    );
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        void loadMission();

        return () => {
            cancelled = true;
        };
    }, [planetId, isLoggedIn]);

    // ============================================
    // ACCESSIBILITY
    // ============================================

    useEffect(() => {
        if (
            questions.length > 0 &&
            currentIndex > 0 &&
            result === null
        ) {
            questionRef.current?.focus();
        }
    }, [currentIndex, questions.length, result]);

    useEffect(() => {
        if (result !== null) {
            resultRef.current?.focus();
        }
    }, [result]);

    // ============================================
    // QUESTION NAVIGATION
    // ============================================

    const question = questions[currentIndex];

    const answeredCount = questions.filter(
        q => Boolean(answers[q.challengeId])
    ).length;

    const allAnswered =
        questions.length > 0 &&
        answeredCount === questions.length;

    const isLast =
        currentIndex === questions.length - 1;

    const progressPercent =
        questions.length > 0
            ? Math.round(
                (answeredCount / questions.length) * 100
            )
            : 0;

    function selectAnswer(
        challengeId: number,
        answer: string
    ) {
        if (submitting || result !== null) return;

        setAnswers(previous => ({
            ...previous,
            [challengeId]: answer,
        }));

        setError(null);
    }

    function nextQuestion() {
        if (!question) return;

        if (!answers[question.challengeId]) {
            setError(
                "Choose an answer before continuing."
            );
            return;
        }

        if (!isLast) {
            setCurrentIndex(index => index + 1);
            setError(null);
        }
    }

    function previousQuestion() {
        if (currentIndex > 0) {
            setCurrentIndex(index => index - 1);
            setError(null);
        }
    }

    // ============================================
    // SUBMIT ALL ANSWERS TO BACKEND
    // ============================================

    async function handleSubmit() {
        if (
            submittingRef.current ||
            !allAnswered ||
            result !== null
        ) {
            return;
        }

        submittingRef.current = true;
        setSubmitting(true);
        setError(null);

        try {
            const response = await submitMission(
                planetId,
                answers
            );

            setResult(response);
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not submit the mission."
            );
        } finally {
            submittingRef.current = false;
            setSubmitting(false);
        }
    }

    // ============================================
    // TRY AGAIN AFTER FAILED MISSION
    // ============================================

    function tryAgain() {
        setAnswers({});
        setCurrentIndex(0);
        setResult(null);
        setError(null);
    }

    // ============================================
    // PAGE STATES
    // ============================================

    if (!isLoggedIn) {
        return (
            <section className="gs-page-intro">
                <h1>Mission</h1>

                <p>
                    Log in to start missions and
                    earn Credits and XP.
                </p>

                <Link
                    to="/login"
                    className="gs-btn gs-btn--primary"
                >
                    Log in
                </Link>
            </section>
        );
    }

    if (
        !Number.isInteger(planetId) ||
        planetId <= 0
    ) {
        return (
            <section className="gs-page-intro">
                <h1>Invalid planet</h1>

                <Link
                    to="/planets"
                    className="gs-btn gs-btn--outline"
                >
                    Back to Galaxy
                </Link>
            </section>
        );
    }

    if (loading) {
        return (
            <p className="gs-page-intro" role="status">
                Loading mission...
            </p>
        );
    }

    if (alreadyCompleted) {
        return (
            <section className="gs-page-intro">
                <h1>{planet?.name ?? "Mission"}</h1>

                <h2>✓ Mission completed!</h2>

                <p>
                    You have already completed this mission.
                    Rewards can only be earned once.
                </p>

                <div className="gs-button-row">
                    <Link
                        to="/progress"
                        className="gs-btn gs-btn--primary"
                    >
                        View Progress
                    </Link>

                    <Link
                        to="/planets"
                        className="gs-btn gs-btn--outline"
                    >
                        Back to Galaxy
                    </Link>
                </div>
            </section>
        );
    }

    if (locked) {
        return (
            <section className="gs-page-intro">
                <h1>{planet?.name ?? "Mission"}</h1>

                <h2>🔒 Planet locked</h2>

                <p>
                    You need{" "}
                    {planet?.requiredXp.toLocaleString() ?? "more"}{" "}
                    total XP to unlock this planet.
                </p>

                <Link
                    to="/planets"
                    className="gs-btn gs-btn--primary"
                >
                    Back to Galaxy
                </Link>
            </section>
        );
    }

    if (questions.length === 0) {
        return (
            <section className="gs-page-intro">
                <h1>{planet?.name ?? "Mission"}</h1>

                <p role={error ? "alert" : "status"}>
                    {error ??
                        "No questions are available for this mission."}
                </p>

                <Link
                    to="/planets"
                    className="gs-btn gs-btn--outline"
                >
                    Back to Galaxy
                </Link>
            </section>
        );
    }

    const options = question
        ? [
            question.optionA,
            question.optionB,
            question.optionC,
            question.optionD,
        ]
        : [];

    const selectedAnswer = question
        ? answers[question.challengeId]
        : undefined;

    // ============================================
    // FLASHCARD MISSION UI
    // ============================================

    return (
        <>
            <section className="gs-page-intro">
                <h1>
                    {planet?.name ?? "Planet"} Mission
                </h1>

                {planet?.subject && (
                    <p>{planet.subject}</p>
                )}

                <p>
                    Answer all questions to complete
                    this mission. You need at least
                    80% correct to pass.
                </p>
            </section>

            <section className="gs-quiz-wrapper">
                <div className="gs-quiz-card">

                    {/* PROGRESS BAR */}

                    <div className="gs-quiz-progress-row">
                        <span aria-live="polite">
                            {result === null
                                ? `Question ${currentIndex + 1} of ${questions.length}`
                                : "Mission complete"}
                        </span>

                        <div
                            className="gs-progress-bar gs-quiz-progress-bar"
                            role="progressbar"
                            aria-label="Mission progress"
                            aria-valuemin={0}
                            aria-valuemax={questions.length}
                            aria-valuenow={answeredCount}
                        >
                            <div
                                className="gs-progress-bar-fill"
                                style={{
                                    width: `${progressPercent}%`,
                                }}
                            />
                        </div>
                    </div>

                    {/* RESULTS */}

                    {result?.passed ? (
                        <div className="gs-quiz-complete">
                            <h2
                                ref={resultRef}
                                tabIndex={-1}
                            >
                                ✓ Mission completed!
                            </h2>

                            <p>
                                You passed the mission
                                and earned your rewards!
                            </p>

                            <p>
                                <strong>Current Credits:</strong>{" "}
                                {result.credits.toLocaleString()}
                            </p>

                            <p>
                                <strong>Total XP:</strong>{" "}
                                {result.experiencePoints.toLocaleString()}
                            </p>

                            <p>
                                <strong>Rank:</strong>{" "}
                                {result.rank}
                            </p>

                            <div className="gs-quiz-complete-actions">
                                <Link
                                    to="/progress"
                                    className="gs-btn gs-btn--primary"
                                >
                                    View Progress
                                </Link>

                                <Link
                                    to="/planets"
                                    className="gs-btn gs-btn--outline"
                                >
                                    Back to Galaxy
                                </Link>
                            </div>
                        </div>
                    ) : result && !result.passed ? (
                        <div className="gs-quiz-complete">
                            <h2
                                ref={resultRef}
                                tabIndex={-1}
                            >
                                Mission not passed
                            </h2>

                            <p>
                                You need at least 80% correct
                                answers to pass.
                            </p>

                            <p>
                                No rewards have been awarded.
                            </p>

                            <div className="gs-quiz-complete-actions">
                                <button
                                    type="button"
                                    className="gs-btn gs-btn--primary"
                                    onClick={tryAgain}
                                >
                                    Try Again
                                </button>

                                <Link
                                    to="/planets"
                                    className="gs-btn gs-btn--outline"
                                >
                                    Back to Galaxy
                                </Link>
                            </div>
                        </div>
                    ) : (

                        /* SINGLE QUESTION FLASHCARD */

                        <div>
                            <h2
                                className="gs-quiz-question"
                                ref={questionRef}
                                tabIndex={-1}
                            >
                                Question {currentIndex + 1}
                                {" of "}
                                {questions.length}
                            </h2>

                            <p
                                className="gs-quiz-question"
                                style={{
                                    marginTop: "1rem",
                                }}
                            >
                                {question.question}
                            </p>

                            <div className="gs-quiz-options">
                                {options.map((option, index) => {
                                    const selected =
                                        selectedAnswer === option;

                                    return (
                                        <button
                                            key={`${question.challengeId}-${index}`}
                                            type="button"
                                            className={
                                                "gs-quiz-option" +
                                                (selected
                                                    ? " gs-quiz-option--selected"
                                                    : "")
                                            }
                                            aria-pressed={selected}
                                            disabled={submitting}
                                            style={
                                                selected
                                                    ? {
                                                        borderColor: "#d4a33e",
                                                        backgroundColor:
                                                            "rgba(212,163,62,0.12)",
                                                    }
                                                    : undefined
                                            }
                                            onClick={() =>
                                                selectAnswer(
                                                    question.challengeId,
                                                    option
                                                )
                                            }
                                        >
                                            <strong>
                                                {String.fromCharCode(65 + index)}.
                                            </strong>
                                            {" "}
                                            {option}
                                        </button>
                                    );
                                })}
                            </div>

                            {error && (
                                <p role="alert">
                                    {error}
                                </p>
                            )}

                            {/* NAVIGATION */}

                            <div
                                className="gs-button-row"
                                style={{
                                    marginTop: "1.5rem",
                                    justifyContent: "space-between",
                                }}
                            >
                                <button
                                    type="button"
                                    className="gs-btn gs-btn--outline"
                                    disabled={
                                        currentIndex === 0 ||
                                        submitting
                                    }
                                    onClick={previousQuestion}
                                >
                                    ← Previous
                                </button>

                                {!isLast ? (
                                    <button
                                        type="button"
                                        className="gs-btn gs-btn--primary"
                                        disabled={
                                            !selectedAnswer ||
                                            submitting
                                        }
                                        onClick={nextQuestion}
                                    >
                                        Next Question →
                                    </button>
                                ) : (
                                    <button
                                        type="button"
                                        className="gs-btn gs-btn--primary"
                                        disabled={
                                            !allAnswered ||
                                            submitting
                                        }
                                        onClick={() =>
                                            void handleSubmit()
                                        }
                                    >
                                        {submitting
                                            ? "Submitting..."
                                            : "Submit Mission"}
                                    </button>
                                )}
                            </div>

                            <p
                                style={{
                                    textAlign: "center",
                                    marginTop: "1rem",
                                }}
                                aria-live="polite"
                            >
                                {answeredCount} of{" "}
                                {questions.length} answered
                            </p>
                        </div>
                    )}
                </div>
            </section>

            <div
                className="gs-quiz-nav"
                style={{
                    textAlign: "center",
                    marginTop: "1rem",
                }}
            >
                <Link
                    to="/planets"
                    className="gs-btn gs-btn--ghost"
                >
                    ← Back to Galaxy
                </Link>
            </div>
        </>
    );
}

export default MissionPage;
