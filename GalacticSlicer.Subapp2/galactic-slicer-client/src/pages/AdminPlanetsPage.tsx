
import {
    useEffect,
    useRef,
    useState,
    type FormEvent,
} from "react";
import { Link } from "react-router-dom";

import { usePageTitle } from "../hooks/usePageTitle";
import { getToken } from "../services/api";
import {
    getPlanets,
    createPlanet,
    updatePlanet,
    deletePlanet,
    type PlanetInput,
} from "../services/planetService";

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

// Used to control the visibility of the admin interface.
// The API enforces the actual authorization.
function hasAdminRole(token: string | null): boolean {
    if (!token) return false;

    try {
        const parts = token.split(".");
        if (parts.length !== 3) return false;

        const base64 = parts[1]
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        const payload = JSON.parse(
            atob(base64)
        ) as Record<string, unknown>;

        const roleClaim =
            payload.role ??
            payload.roles ??
            payload[
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ];

        return (
            roleClaim === "Admin" ||
            (
                Array.isArray(roleClaim) &&
                roleClaim.includes("Admin")
            )
        );
    } catch {
        return false;
    }
}

function AdminPlanetsPage() {
    usePageTitle("Manage Planets");

    const [planets, setPlanets] =
        useState<Planet[]>([]);

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

    const token = getToken();
    const isLoggedIn = Boolean(token);
    const isAdmin = hasAdminRole(token);

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
                const data = await getPlanets();

                if (cancelled) return;

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

                <p>
                    Create, update and manage planets
                    in Galactic Slicer.
                </p>

                <div className="gs-button-row">
                    <button
                        type="button"
                        className="gs-btn gs-btn--primary"
                        onClick={openCreate}
                        disabled={saving}
                    >
                        Create New Planet
                    </button>

                    <Link
                        to="/planets"
                        className="gs-btn gs-btn--outline"
                    >
                        View Galaxy
                    </Link>
                </div>
            </section>

            {error && (
                <p
                    className="challenge-empty"
                    role="alert"
                >
                    {error}
                </p>
            )}

            {notice && (
                <p role="status">
                    {notice}
                </p>
            )}

            {/* CREATE / EDIT FORM */}

            {editor && (
                <section
                    ref={editorRef}
                    className="gs-page-intro"
                    style={{
                        scrollMarginTop: "90px",
                    }}
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

            {/* PLANET LIST */}

            <section>
                <h2>Existing Planets</h2>

                {loading ? (
                    <p role="status">
                        Loading planets...
                    </p>
                ) : planets.length === 0 ? (
                    <p className="challenge-empty">
                        No planets found.
                    </p>
                ) : (
                    <div className="gs-challenge-list">
                        {planets.map(planet => {
                            const official =
                                isOfficialPlanet(planet);

                            return (
                                <article
                                    key={planet.planetId}
                                    className="gs-page-intro"
                                >
                                    <h3>
                                        Chapter {planet.order}:{" "}
                                        {planet.name}
                                    </h3>

                                    {official && (
                                        <p>
                                            Official Planet
                                        </p>
                                    )}

                                    {planet.subject && (
                                        <p>
                                            <strong>
                                                Subject:
                                            </strong>{" "}
                                            {planet.subject}
                                        </p>
                                    )}

                                    {planet.description && (
                                        <p>
                                            {planet.description}
                                        </p>
                                    )}

                                    <p>
                                        <strong>
                                            CSS:
                                        </strong>{" "}
                                        {planet.cssClass}
                                    </p>

                                    <div className="gs-button-row">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline"
                                            disabled={saving}
                                            onClick={() =>
                                                openEdit(planet)
                                            }
                                        >
                                            Edit
                                        </button>

                                        {!official && (
                                            <button
                                                type="button"
                                                className="gs-btn gs-btn--outline"
                                                disabled={saving}
                                                onClick={() =>
                                                    void handleDelete(
                                                        planet
                                                    )
                                                }
                                            >
                                                Delete
                                            </button>
                                        )}
                                    </div>
                                </article>
                            );
                        })}
                    </div>
                )}
            </section>
        </>
    );
}

export default AdminPlanetsPage;
