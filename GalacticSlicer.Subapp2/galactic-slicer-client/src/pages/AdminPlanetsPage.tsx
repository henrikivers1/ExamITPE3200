
import {
    useEffect,
    useRef,
    useState,
    type FormEvent,
} from "react";
import { Link } from "react-router-dom";

import { usePageTitle } from "../hooks/usePageTitle";
import { getToken, isAdminUser } from "../services/api";
import {
    getPlanets,
    createPlanet,
    updatePlanet,
    deletePlanet,
    type PlanetInput,
} from "../services/planetService";
import { getChallenges } from "../services/challengeService";

import type { Planet } from "../types/Planet";

type EditorState =
    | { mode: "create" }
    | { mode: "edit"; planet: Planet }
    | null;

const emptyPlanet: PlanetInput = {
    name: "",
    description: null,
    subject: null,
    cssClass: "",
    order: 7,
};

const officialPlanetNames = new Set([
    "tatooine",
    "naboo",
    "coruscant",
    "kamino",
    "hoth",
    "endor",
]);

function isOfficialPlanet(planet: Planet): boolean {
    return officialPlanetNames.has(
        planet.name.trim().toLowerCase()
    );
}

function AdminPlanetsPage() {
    usePageTitle("Manage Planets");

    const [planets, setPlanets] =
        useState<Planet[]>([]);

    // Official questions per planet (PlanetId -> count)
    const [questionCounts, setQuestionCounts] =
        useState<Record<number, number>>({});

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [error, setError] = useState("");
    const [notice, setNotice] = useState("");

    const [editor, setEditor] =
        useState<EditorState>(null);

    const [form, setForm] =
        useState<PlanetInput>(emptyPlanet);

    const [refreshKey, setRefreshKey] = useState(0);

    const editorRef = useRef<HTMLElement>(null);

    const isLoggedIn = Boolean(getToken());
    const isAdmin = isAdminUser();

    const editingOfficialPlanet =
        editor?.mode === "edit" &&
        isOfficialPlanet(editor.planet);

    // ============================================
    // SCROLL TO CREATE / EDIT FORM
    // ============================================

    useEffect(() => {
        if (editor) {
            editorRef.current?.scrollIntoView({
                behavior: "smooth",
                block: "start",
            });
        }
    }, [editor]);

    // ============================================
    // LOAD PLANETS
    // ============================================

    useEffect(() => {
        if (!isAdmin) return;

        let cancelled = false;

        async function load() {
            try {
                const [data, challenges] = await Promise.all([
                    getPlanets(),
                    getChallenges(),
                ]);

                if (cancelled) return;

                // Count the questions that belong to each planet.
                // Players' own questions have no planet.
                const counts: Record<number, number> = {};

                for (const challenge of challenges) {
                    if (challenge.planetId !== null) {
                        counts[challenge.planetId] =
                            (counts[challenge.planetId] ?? 0) + 1;
                    }
                }

                setQuestionCounts(counts);

                setPlanets(
                    [...data].sort(
                        (a, b) => a.order - b.order
                    )
                );

                setError("");
            } catch (err) {
                if (cancelled) return;

                setError(
                    err instanceof Error
                        ? err.message
                        : "Could not load planets."
                );
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        void load();

        return () => {
            cancelled = true;
        };
    }, [isAdmin, refreshKey]);

    function refresh() {
        setRefreshKey(value => value + 1);
    }

    // ============================================
    // OPEN EDITOR
    // ============================================

    function openCreate() {
        const nextOrder =
            Math.max(0, ...planets.map(p => p.order)) + 1;

        setForm({
            ...emptyPlanet,
            order: Math.min(nextOrder, 100),
        });

        setEditor({ mode: "create" });
        setError("");
        setNotice("");
    }

    function openEdit(planet: Planet) {
        setForm({
            name: planet.name,
            description: planet.description ?? null,
            subject: planet.subject ?? null,
            cssClass: planet.cssClass,
            order: planet.order,
        });

        setEditor({
            mode: "edit",
            planet,
        });

        setError("");
        setNotice("");
    }

    function closeEditor() {
        setEditor(null);
        setError("");
    }

    // ============================================
    // SAVE PLANET
    // ============================================

    async function handleSave(
        event: FormEvent<HTMLFormElement>
    ) {
        event.preventDefault();

        if (!editor || !isAdmin || saving) return;

        const data: PlanetInput = {
            name: form.name.trim(),
            description:
                form.description?.trim() || null,
            subject:
                form.subject?.trim() || null,
            cssClass: form.cssClass.trim(),
            order: Number(form.order),
        };

        if (
            !data.name ||
            !data.cssClass ||
            data.name.length > 50 ||
            data.cssClass.length > 30 ||
            (data.description?.length ?? 0) > 300 ||
            (data.subject?.length ?? 0) > 100 ||
            !Number.isInteger(data.order) ||
            data.order < 1 ||
            data.order > 100
        ) {
            setError(
                "Please check the planet fields. " +
                "Name: max 50 characters, " +
                "CSS class: max 30, " +
                "description: max 300, " +
                "subject: max 100, " +
                "chapter order: 1–100."
            );

            return;
        }

        setSaving(true);
        setError("");
        setNotice("");

        try {
            if (editor.mode === "create") {
                await createPlanet(data);
                setNotice(
                    "Planet created successfully."
                );
            } else {
                await updatePlanet(
                    editor.planet.planetId,
                    data
                );

                setNotice(
                    "Planet updated successfully."
                );
            }

            setEditor(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not save planet."
            );
        } finally {
            setSaving(false);
        }
    }

    // ============================================
    // DELETE PLANET
    // ============================================

    async function handleDelete(planet: Planet) {
        if (!isAdmin || saving) return;

        if (isOfficialPlanet(planet)) {
            setError(
                "Official planets cannot be deleted."
            );
            return;
        }

        const confirmed = window.confirm(
            `Delete ${planet.name}?\n\n` +
            "This action cannot be undone."
        );

        if (!confirmed) return;

        setSaving(true);
        setError("");
        setNotice("");

        try {
            await deletePlanet(planet.planetId);

            setNotice(
                `${planet.name} deleted successfully.`
            );

            setEditor(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not delete planet."
            );
        } finally {
            setSaving(false);
        }
    }

    // ============================================
    // ACCESS CONTROL
    // ============================================

    if (!isLoggedIn) {
        return (
            <section className="gs-page-intro">
                <h1>Manage Planets</h1>

                <p>
                    Log in as an administrator.
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

    if (!isAdmin) {
        return (
            <section className="gs-page-intro">
                <h1>Access denied</h1>

                <p>
                    Only administrators can manage planets.
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

    // ============================================
    // ADMIN PAGE
    // ============================================

    return (
        <>
            <section className="gs-page-intro">
                <h1>Manage Planets</h1>

                <p>Add, edit and remove the planets on the Starchart.</p>
            </section>

            {/* Message after saving or deleting, like TempData in Subapp 1 */}
            {notice && (
                <div
                    className="gs-admin-message gs-admin-message--success"
                    role="status"
                >
                    {notice}
                </div>
            )}

            {error && (
                <div
                    className="gs-admin-message gs-admin-message--error"
                    role="alert"
                >
                    {error}
                </div>
            )}

            <div className="gs-admin-actions">
                <button
                    type="button"
                    className="gs-btn gs-btn--primary"
                    onClick={openCreate}
                    disabled={saving}
                >
                    Add Planet
                </button>

                <Link
                    to="/planets"
                    className="gs-btn gs-btn--ghost"
                >
                    Back to the Starchart
                </Link>
            </div>

            {/* CREATE / EDIT FORM */}

            {editor && (
                <section
                    ref={editorRef}
                    className="gs-auth-card gs-admin-editor"
                >
                    <h2>
                        {editor.mode === "create"
                            ? "Create Planet"
                            : `Edit ${editor.planet.name}`}
                    </h2>

                    {editingOfficialPlanet && (
                        <p>
                            This is an official planet.
                            Only its description can be changed
                            to protect missions and progression.
                        </p>
                    )}

                    <form onSubmit={handleSave}>
                        <p>
                            <label htmlFor="planetName">
                                Planet Name
                            </label>
                        </p>

                        <input
                            id="planetName"
                            className="gs-input"
                            type="text"
                            maxLength={50}
                            required
                            readOnly={editingOfficialPlanet}
                            value={form.name}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    name: event.target.value,
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="planetOrder">
                                Chapter Order
                            </label>
                        </p>

                        <input
                            id="planetOrder"
                            className="gs-input"
                            type="number"
                            min={1}
                            max={100}
                            step={1}
                            required
                            readOnly={editingOfficialPlanet}
                            value={form.order}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    order: Number(
                                        event.target.value
                                    ),
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="planetSubject">
                                Subject (optional)
                            </label>
                        </p>

                        <input
                            id="planetSubject"
                            className="gs-input"
                            type="text"
                            maxLength={100}
                            placeholder="e.g. Network Security"
                            readOnly={editingOfficialPlanet}
                            value={form.subject ?? ""}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    subject: event.target.value,
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="planetCss">
                                CSS Class
                            </label>
                        </p>

                        <input
                            id="planetCss"
                            className="gs-input"
                            type="text"
                            maxLength={30}
                            required
                            readOnly={editingOfficialPlanet}
                            placeholder="e.g. tatooine"
                            value={form.cssClass}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    cssClass: event.target.value,
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="planetDescription">
                                Description
                            </label>
                        </p>

                        <textarea
                            id="planetDescription"
                            className="gs-input"
                            rows={3}
                            maxLength={300}
                            value={form.description ?? ""}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    description: event.target.value,
                                }))
                            }
                        />

                        <div className="gs-button-row">
                            <button
                                type="submit"
                                className="gs-btn gs-btn--primary"
                                disabled={saving}
                            >
                                {saving
                                    ? "Saving..."
                                    : editor.mode === "create"
                                      ? "Create Planet"
                                      : "Save Changes"}
                            </button>

                            <button
                                type="button"
                                className="gs-btn gs-btn--outline"
                                onClick={closeEditor}
                                disabled={saving}
                            >
                                Cancel
                            </button>
                        </div>
                    </form>
                </section>
            )}

            {/* PLANET LIST: same table as Planet/Manage in Subapp 1 */}

            {loading ? (
                <p className="gs-page-intro" role="status">
                    Loading planets...
                </p>
            ) : planets.length === 0 ? (
                <p className="gs-page-intro">
                    No planets yet. Add the first one above.
                </p>
            ) : (
                <section
                    className="gs-admin-table-wrap"
                    aria-label="Planets"
                >
                    <table className="gs-admin-table">
                        <thead>
                            <tr>
                                <th>
                                    <span className="gs-sr-only">
                                        Planet
                                    </span>
                                </th>
                                <th>Chapter</th>
                                <th>Name</th>
                                <th>Mission subject</th>
                                <th>Questions</th>
                                <th className="gs-admin-actions-cell">
                                    Actions
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {planets.map(planet => (
                                <tr key={planet.planetId}>
                                    <td>
                                        <span
                                            className={`gs-planet gs-planet--${planet.cssClass}`}
                                            aria-hidden="true"
                                        />
                                    </td>
                                    <td>{planet.order}</td>
                                    <td>{planet.name}</td>
                                    <td>
                                        {planet.subject ??
                                            "Mixed (every subject)"}
                                    </td>
                                    <td>
                                        {questionCounts[planet.planetId] ?? 0}
                                    </td>
                                    <td className="gs-admin-actions-cell">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline gs-btn--sm"
                                            disabled={saving}
                                            aria-label={`Edit ${planet.name}`}
                                            onClick={() =>
                                                openEdit(planet)
                                            }
                                        >
                                            Edit
                                        </button>

                                        {isOfficialPlanet(planet) ? (
                                            <span
                                                className="gs-btn gs-btn--sm gs-btn--ghost"
                                                title="Official planets can't be deleted"
                                            >
                                                Locked
                                            </span>
                                        ) : (
                                            <button
                                                type="button"
                                                className="gs-btn gs-btn--sm gs-btn--danger"
                                                disabled={saving}
                                                aria-label={`Delete ${planet.name}`}
                                                onClick={() =>
                                                    void handleDelete(
                                                        planet
                                                    )
                                                }
                                            >
                                                Delete
                                            </button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </section>
            )}
        </>
    );
}

export default AdminPlanetsPage;
