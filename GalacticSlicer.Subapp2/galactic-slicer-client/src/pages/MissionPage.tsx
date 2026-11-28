
import {
    useEffect,
    useRef,
    useState,
} from "react";
import { Link, useParams } from "react-router-dom";

import { usePageTitle } from "../hooks/usePageTitle";
import { getToken } from "../services/api";
import {
    getProgress,
    notifyProgressChanged,
} from "../services/progressService";

import {
    getMission,
    submitMission,
    type MissionQuestion,
    type MissionAnswers,
    type MissionResult,
    PASS_PERCENT,
    XP_PER_MISSION,
    creditsForMission,
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

    // false: the mission briefing is shown, true: the questions
    const [started, setStarted] = useState(false);

    const submittingRef = useRef(false);
    const questionRef = useRef<HTMLHeadingElement>(null);
    const resultRef = useRef<HTMLHeadingElement>(null);

    const isLoggedIn = Boolean(getToken());
    const isFinalMission =
        planet?.name.toLowerCase() === "endor";

    // ============================================
    // LOAD MISSION
    // ============================================

    useEffect(() => {
        if (
            !isLoggedIn ||
            !Number.isInteger(planetId) ||
            planetId <= 0
        ) {
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
            setStarted(false);
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

                // completed missions can be replayed, without new rewards
                setAlreadyCompleted(currentPlanet.isCompleted);

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
            started &&
            questions.length > 0 &&
            result === null
        ) {
            questionRef.current?.focus();
        }
    }, [started, currentIndex, questions.length, result]);

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

            // new Credits in the navbar after a first pass
            notifyProgressChanged();
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
    // TRY AGAIN / PLAY AGAIN
    // ============================================

    // A new set of 10 random questions, straight to question 1
    async function playAgain() {
        setAnswers({});
        setCurrentIndex(0);
        setResult(null);
        setError(null);

        try {
            setQuestions(await getMission(planetId));

            // after a pass the mission counts as completed
            if (result?.passed) setAlreadyCompleted(true);
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not load the mission."
            );
        }
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

    if (locked) {
        return (
            <section className="gs-page-intro">
                <h1>{planet?.name ?? "Mission"}</h1>

                <h2>🔒 Planet locked</h2>

                <p>
                    Pass the previous mission to unlock
                    this planet.
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

    // shown in the briefing (Endor: 12 of 15)
    const passCount = Math.ceil(
        (questions.length * PASS_PERCENT) / 100
    );
    const creditReward = planet
        ? creditsForMission(planet.order)
        : 0;

    // ============================================
    // FLASHCARD MISSION UI
    // ============================================

    return (
        <>
            {/* the planet itself, huge and dimmed, behind the mission */}
            {planet && (
                <span
                    className={`gs-planet gs-planet--${planet.cssClass} mission-planet`}
                    aria-hidden="true"
                />
            )}

            {/* chapter badge, mission name and the planet's description */}
            <section className="gs-page-intro">
                {planet && (
                    <span className="gs-rank-badge">
                        Chapter {planet.order}
                    </span>
                )}

                <h1 className="mission-title">
                    {planet?.name ?? "Planet"}{" "}
                    {isFinalMission
                        ? "Final Trial"
                        : "mission"}
                </h1>

                {planet?.description && (
                    <p>{planet.description}</p>
                )}
            </section>

            <section className="gs-quiz-wrapper">
                <div className="gs-quiz-card mission-card">
                    {!started ? (

                        /* MISSION BRIEFING: shown before the first question */

                        <div className="mission-briefing">
                            <h2>Mission briefing</h2>

                            {isFinalMission ? (
                                <p>
                                    The Final Trial combines all five
                                    subjects. Answer{" "}
                                    <strong>{questions.length}</strong>{" "}
                                    questions. You need at least{" "}
                                    <strong>{passCount}</strong> correct
                                    to complete it.
                                </p>
                            ) : (
                                <p>
                                    Answer{" "}
                                    <strong>{questions.length}</strong>{" "}
                                    questions about {planet?.name}.
                                    You need{" "}
                                    <strong>{PASS_PERCENT}%</strong>{" "}
                                    correct to complete the mission.
                                </p>
                            )}

                            {/* what the player earns if they pass (only the first time) */}
                            {alreadyCompleted ? (
                                <p className="mission-replay-note">
                                    ✓ You have already completed this
                                    mission. You can play it again, but
                                    the rewards are only given once.
                                </p>
                            ) : (
                                <div className="mission-rewards">
                                    <span className="mission-reward">
                                        +{creditReward} credits
                                    </span>
                                    <span className="mission-reward mission-reward--xp">
                                        +{XP_PER_MISSION} XP
                                    </span>
                                </div>
                            )}

                            <button
                                type="button"
                                className="gs-btn gs-btn--primary gs-btn--lg mission-start"
                                onClick={() => setStarted(true)}
                            >
                                {alreadyCompleted
                                    ? "Replay mission"
                                    : "Start mission"}
                            </button>
                        </div>
                    ) : (
                        <>

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
                                        {isFinalMission
                                            ? "✓ Final Trial completed!"
                                            : "✓ Mission completed!"}
                                    </h2>

                                    <p className="mission-score">
                                        You got {result.correctCount} of{" "}
                                        {result.totalQuestions} correct.
                                    </p>

                                    {result.alreadyCompleted ? (
                                        <p>
                                            You had already completed this
                                            mission, so there are no new
                                            rewards this time.
                                        </p>
                                    ) : (
                                        <>
                                            <p>
                                                {isFinalMission
                                                    ? "Congratulations! You conquered Endor and completed the Galactic Slicer journey."
                                                    : "You passed the mission and earned your rewards!"}
                                            </p>

                                            <div className="mission-rewards">
                                                <span className="mission-reward">
                                                    +{result.awardedCredits} credits
                                                </span>
                                                <span className="mission-reward mission-reward--xp">
                                                    +{result.awardedXp} XP
                                                </span>
                                            </div>
                                        </>
                                    )}

                                    <p>
                                        <strong>Credits:</strong>{" "}
                                        {result.credits.toLocaleString()}
                                        {" · "}
                                        <strong>XP:</strong>{" "}
                                        {result.experiencePoints.toLocaleString()}
                                        {" · "}
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

                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline"
                                            onClick={() => void playAgain()}
                                        >
                                            Play Again
                                        </button>

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
                                        {isFinalMission
                                            ? "Final Trial not passed"
                                            : "Mission not passed"}
                                    </h2>

                                    <p className="mission-score">
                                        You got {result.correctCount} of{" "}
                                        {result.totalQuestions} correct.
                                        You need at least {passCount} to
                                        pass.
                                    </p>

                                    <p>
                                        No rewards have been awarded.
                                    </p>

                                    <div className="gs-quiz-complete-actions">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--primary"
                                            onClick={() => void playAgain()}
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
                        </>
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
