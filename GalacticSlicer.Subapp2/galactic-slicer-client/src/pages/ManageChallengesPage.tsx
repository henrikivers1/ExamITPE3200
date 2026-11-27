
import {
    useEffect,
    useState,
    type FormEvent,
} from "react";
import { Link, useSearchParams } from "react-router-dom";
import { usePageTitle } from "../hooks/usePageTitle";
import { getToken } from "../services/api";
import {
    getTopics,
    searchChallenges,
    createChallenge,
    updateChallenge,
    deleteChallenge,
    type Challenge,
    type ChallengeInput,
} from "../services/challengeService";

type EditorState =
    | { mode: "create" }
    | { mode: "edit"; challenge: Challenge }
    | null;

function ManageChallengesPage() {
    usePageTitle("Manage Challenges");

    const [searchParams, setSearchParams] =
        useSearchParams();

    const searchString =
        searchParams.get("searchString") ?? "";

    const subject =
        searchParams.get("subject") ?? "";

    const [subjects, setSubjects] = useState<string[]>([]);
    const [challenges, setChallenges] = useState<Challenge[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [notice, setNotice] = useState("");
    const [editor, setEditor] = useState<EditorState>(null);
    const [selected, setSelected] = useState<Challenge | null>(null);
    const [refreshKey, setRefreshKey] = useState(0);

    const isLoggedIn = Boolean(getToken());

    useEffect(() => {
        if (!isLoggedIn) {
            setLoading(false);
            setChallenges([]);
            return;
        }

        let cancelled = false;

        async function load() {
            setLoading(true);
            setError("");

            try {
                const [results, topics] = await Promise.all([
                    searchChallenges(searchString, subject),
                    getTopics(),
                ]);

                if (cancelled) return;

                setChallenges(results);
                setSubjects(topics.map(t => t.subject));
            } catch (err) {
                if (cancelled) return;

                setError(
                    err instanceof Error
                        ? err.message
                        : "Could not load challenges."
                );
            } finally {
                if (!cancelled) setLoading(false);
            }
        }

        void load();

        return () => {
            cancelled = true;
        };
    }, [isLoggedIn, searchString, subject, refreshKey]);

    function refresh() {
        setRefreshKey(value => value + 1);
    }

    function handleSearch(
        event: FormEvent<HTMLFormElement>
    ) {
        event.preventDefault();

        const form = new FormData(event.currentTarget);
        const newSearch = String(
            form.get("searchString") ?? ""
        ).trim();

        const newSubject = String(
            form.get("subject") ?? ""
        );

        const params: Record<string, string> = {};

        if (newSearch) params.searchString = newSearch;
        if (newSubject) params.subject = newSubject;

        setSearchParams(params);
    }

    async function handleSave(
        event: FormEvent<HTMLFormElement>
    ) {
        event.preventDefault();

        if (!editor) return;

        if (
            editor.mode === "edit" &&
            !editor.challenge.isOwnedByCurrentUser
        ) {
            setError("You cannot edit this challenge.");
            return;
        }

        const form = new FormData(event.currentTarget);

        const question = String(
            form.get("question") ?? ""
        ).trim();

        const optionA = String(
            form.get("optionA") ?? ""
        ).trim();

        const optionB = String(
            form.get("optionB") ?? ""
        ).trim();

        const optionC = String(
            form.get("optionC") ?? ""
        ).trim();

        const optionD = String(
            form.get("optionD") ?? ""
        ).trim();

        const correctOption = String(
            form.get("correctOption") ?? ""
        );

        const options: Record<string, string> = {
            A: optionA,
            B: optionB,
            C: optionC,
            D: optionD,
        };

        const correctAnswer = options[correctOption];

        if (!correctAnswer) {
            setError("Choose the correct answer.");
            return;
        }

        const uniqueOptions = new Set(
            Object.values(options).map(value =>
                value.toLowerCase()
            )
        );

        if (uniqueOptions.size !== 4) {
            setError(
                "All four answer options must be different."
            );
            return;
        }

        const data: ChallengeInput = {
            question,
            optionA,
            optionB,
            optionC,
            optionD,
            correctAnswer,
            subject:
                String(form.get("subject") ?? "").trim()
                || null,
            explanation:
                String(
                    form.get("explanation") ?? ""
                ).trim() || null,
        };

        setSaving(true);
        setError("");
        setNotice("");

        try {
            if (editor.mode === "edit") {
                await updateChallenge(
                    editor.challenge.challengeId,
                    data
                );

                setNotice("Challenge updated successfully.");
            } else {
                await createChallenge(data);

                setNotice("Challenge created successfully.");
            }

            setEditor(null);
            setSelected(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not save challenge."
            );
        } finally {
            setSaving(false);
        }
    }

    async function handleDelete(challenge: Challenge) {
        if (!challenge.isOwnedByCurrentUser) {
            setError("You cannot delete this challenge.");
            return;
        }

        const confirmed = window.confirm(
            `Delete this challenge?\n\n${challenge.question}`
        );

        if (!confirmed) return;

        setError("");
        setNotice("");

        try {
            await deleteChallenge(challenge.challengeId);
            setNotice("Challenge deleted successfully.");
            setSelected(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not delete challenge."
            );
        }
    }

    function openCreate() {
        setSelected(null);
        setError("");
        setNotice("");
        setEditor({ mode: "create" });
    }

    function openEdit(challenge: Challenge) {
        if (!challenge.isOwnedByCurrentUser) {
            setError("You cannot edit this challenge.");
            return;
        }

        setSelected(null);
        setError("");
        setNotice("");
        setEditor({ mode: "edit", challenge });
    }

    return (
        <>
            <section className="gs-page-intro">
                <h1>Manage Challenges</h1>
                <p>
                    Create your own challenges, search questions,
                    and practice cybersecurity topics.
                </p>
            </section>

            <form
                key={`${searchParams.toString()}|${subjects.length}`}
                className="gs-search"
                role="search"
                onSubmit={handleSearch}
            >
                <label
                    className="gs-sr-only"
                    htmlFor="searchString"
                >
                    Search question
                </label>

                <input
                    id="searchString"
                    name="searchString"
                    className="gs-input gs-search-text"
                    type="search"
                    placeholder="Search question"
                    defaultValue={searchString}
                />

                <label
                    className="gs-sr-only"
                    htmlFor="subject"
                >
                    Subject
                </label>

                <select
                    id="subject"
                    name="subject"
                    className="gs-input gs-search-subject"
                    defaultValue={subject}
                >
                    <option value="">All subjects</option>
                    {subjects.map(name => (
                        <option key={name} value={name}>
                            {name}
                        </option>
                    ))}
                </select>

                <button
                    type="submit"
                    className="gs-btn gs-btn--primary"
                >
                    Search
                </button>

                <button
                    type="button"
                    className="gs-btn gs-btn--outline"
                    onClick={() => setSearchParams({})}
                >
                    Clear
                </button>
            </form>

            <div className="gs-button-row gs-button-row--large">
                {isLoggedIn ? (
                    <button
                        type="button"
                        className="gs-btn gs-btn--primary"
                        onClick={openCreate}
                    >
                        Create New Challenge
                    </button>
                ) : (
                    <Link
                        to="/login"
                        className="gs-btn gs-btn--primary"
                    >
                        Log in to Create
                    </Link>
                )}

                <Link
                    to="/challenges/topics"
                    className="gs-btn gs-btn--practice"
                >
                    Practice Challenges
                </Link>
            </div>

            {error && (
                <p className="challenge-empty" role="alert">
                    {error}
                </p>
            )}

            {notice && (
                <p role="status">{notice}</p>
            )}

            {!isLoggedIn && (
                <p className="challenge-empty">
                    <Link to="/login">Log in</Link> to view
                    and manage challenges.
                </p>
            )}

            {isLoggedIn && editor && (
                <section className="gs-page-intro">
                    <h2>
                        {editor.mode === "create"
                            ? "Create Challenge"
                            : "Edit Challenge"}
                    </h2>

                    <form
                        key={
                            editor.mode === "edit"
                                ? `edit-${editor.challenge.challengeId}`
                                : "create"
                        }
                        onSubmit={handleSave}
                    >
                        <p>
                            <label htmlFor="editQuestion">
                                Question
                            </label>
                        </p>

                        <textarea
                            id="editQuestion"
                            name="question"
                            className="gs-input"
                            rows={3}
                            maxLength={500}
                            required
                            defaultValue={
                                editor.mode === "edit"
                                    ? editor.challenge.question
                                    : ""
                            }
                        />

                        {(["A", "B", "C", "D"] as const).map(
                            letter => {
                                const field =
                                    `option${letter}` as
                                        | "optionA"
                                        | "optionB"
                                        | "optionC"
                                        | "optionD";

                                return (
                                    <div key={letter}>
                                        <p>
                                            <label htmlFor={field}>
                                                Option {letter}
                                            </label>
                                        </p>

                                        <input
                                            id={field}
                                            name={field}
                                            type="text"
                                            className="gs-input"
                                            maxLength={400}
                                            required
                                            defaultValue={
                                                editor.mode === "edit"
                                                    ? editor.challenge[field]
                                                    : ""
                                            }
                                        />
                                    </div>
                                );
                            }
                        )}

                        <p>
                            <label htmlFor="correctOption">
                                Correct Answer
                            </label>
                        </p>

                        <select
                            id="correctOption"
                            name="correctOption"
                            className="gs-input"
                            required
                            defaultValue={
                                editor.mode === "edit"
                                    ? (
                                        ["A", "B", "C", "D"] as const
                                    ).find(letter => {
                                        const field =
                                            `option${letter}` as
                                                | "optionA"
                                                | "optionB"
                                                | "optionC"
                                                | "optionD";

                                        return (
                                            editor.challenge[field] ===
                                            editor.challenge.correctAnswer
                                        );
                                    }) ?? ""
                                    : ""
                            }
                        >
                            <option value="" disabled>
                                Select correct option
                            </option>
                            <option value="A">Option A</option>
                            <option value="B">Option B</option>
                            <option value="C">Option C</option>
                            <option value="D">Option D</option>
                        </select>

                        <p>
                            <label htmlFor="editSubject">
                                Subject
                            </label>
                        </p>

                        <input
                            id="editSubject"
                            name="subject"
                            className="gs-input"
                            list="challenge-subjects"
                            maxLength={100}
                            placeholder="e.g. Network Security"
                            defaultValue={
                                editor.mode === "edit"
                                    ? editor.challenge.subject ?? ""
                                    : ""
                            }
                        />

                        <datalist id="challenge-subjects">
                            {subjects.map(name => (
                                <option key={name} value={name} />
                            ))}
                        </datalist>

                        <p>
                            <label htmlFor="editExplanation">
                                Explanation (optional)
                            </label>
                        </p>

                        <textarea
                            id="editExplanation"
                            name="explanation"
                            className="gs-input"
                            rows={3}
                            maxLength={500}
                            defaultValue={
                                editor.mode === "edit"
                                    ? editor.challenge.explanation ?? ""
                                    : ""
                            }
                        />

                        <div className="gs-button-row">
                            <button
                                type="submit"
                                disabled={saving}
                                className="gs-btn gs-btn--primary"
                            >
                                {saving
                                    ? "Saving..."
                                    : editor.mode === "create"
                                      ? "Create Challenge"
                                      : "Save Changes"}
                            </button>

                            <button
                                type="button"
                                className="gs-btn gs-btn--outline"
                                onClick={() => setEditor(null)}
                                disabled={saving}
                            >
                                Cancel
                            </button>
                        </div>
                    </form>
                </section>
            )}

            {isLoggedIn && (
                <section>
                    <h2>Challenges</h2>

                    <p>
                        Official questions and your own questions
                        are available here. Only questions you
                        created can be edited or deleted.
                    </p>

                    {loading ? (
                        <p role="status">
                            Loading challenges...
                        </p>
                    ) : challenges.length === 0 ? (
                        <p
                            className="challenge-empty"
                            role="status"
                        >
                            No challenges match your search.
                        </p>
                    ) : (
                        <div className="gs-challenge-list">
                            {challenges.map(challenge => (
                                <article
                                    key={challenge.challengeId}
                                    className="gs-page-intro"
                                >
                                    <h3>{challenge.question}</h3>

                                    {challenge.subject && (
                                        <p>{challenge.subject}</p>
                                    )}

                                    {selected?.challengeId ===
                                        challenge.challengeId && (
                                        <div>
                                            <p>
                                                A. {challenge.optionA}
                                            </p>
                                            <p>
                                                B. {challenge.optionB}
                                            </p>
                                            <p>
                                                C. {challenge.optionC}
                                            </p>
                                            <p>
                                                D. {challenge.optionD}
                                            </p>
                                        </div>
                                    )}

                                    <div className="gs-button-row">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline"
                                            onClick={() =>
                                                setSelected(
                                                    selected?.challengeId ===
                                                        challenge.challengeId
                                                        ? null
                                                        : challenge
                                                )
                                            }
                                        >
                                            {selected?.challengeId ===
                                            challenge.challengeId
                                                ? "Hide Details"
                                                : "Details"}
                                        </button>

                                        {challenge.isOwnedByCurrentUser && (
                                            <>
                                                <button
                                                    type="button"
                                                    className="gs-btn gs-btn--outline"
                                                    onClick={() =>
                                                        openEdit(challenge)
                                                    }
                                                >
                                                    Edit
                                                </button>

                                                <button
                                                    type="button"
                                                    className="gs-btn gs-btn--outline"
                                                    onClick={() =>
                                                        void handleDelete(
                                                            challenge
                                                        )
                                                    }
                                                >
                                                    Delete
                                                </button>
                                            </>
                                        )}
                                    </div>
                                </article>
                            ))}
                        </div>
                    )}
                </section>
            )}
        </>
    );
}

export default ManageChallengesPage;
