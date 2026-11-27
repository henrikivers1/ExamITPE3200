
import {
    useEffect,
    useRef,
    useState,
    type FormEvent,
} from "react";
import { Link } from "react-router-dom";

import { usePageTitle } from "../hooks/usePageTitle";
import { apiRequest, getToken } from "../services/api";
import type { Character } from "../types/Character";

interface CharacterInput {
    name: string;
    description: string | null;
    price: number;
    imageUrl: string | null;
}

type EditorState =
    | { mode: "create" }
    | { mode: "edit"; character: Character }
    | null;

const emptyCharacter: CharacterInput = {
    name: "",
    description: null,
    price: 0,
    imageUrl: null,
};

const officialCharacterNames = new Set([
    "rookie slicer",
    "astromech hacker",
    "smuggler",
    "bounty hunter",
    "shadow operative",
]);

function isOfficialCharacter(character: Character): boolean {
    return officialCharacterNames.has(
        character.name.trim().toLowerCase()
    );
}

// Client-side check for controlling the admin UI.
// Actual authorization is enforced by ASP.NET Core.
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

        const role =
            payload.role ??
            payload.roles ??
            payload[
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ];

        return (
            role === "Admin" ||
            (Array.isArray(role) && role.includes("Admin"))
        );
    } catch {
        return false;
    }
}

// ============================================
// ADMIN API
// ============================================

function getCharacters(): Promise<Character[]> {
    return apiRequest<Character[]>("/api/characters");
}

function createCharacter(
    data: CharacterInput
): Promise<Character> {
    return apiRequest<Character>("/api/characters", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

async function updateCharacter(
    id: number,
    data: CharacterInput
): Promise<void> {
    await apiRequest<unknown>(`/api/characters/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

async function deleteCharacter(id: number): Promise<void> {
    await apiRequest<unknown>(`/api/characters/${id}`, {
        method: "DELETE",
    });
}

// ============================================
// PAGE
// ============================================

function AdminCharactersPage() {
    usePageTitle("Manage Characters");

    const [characters, setCharacters] =
        useState<Character[]>([]);

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [error, setError] = useState("");
    const [notice, setNotice] = useState("");

    const [editor, setEditor] =
        useState<EditorState>(null);

    const [form, setForm] =
        useState<CharacterInput>(emptyCharacter);

    const [refreshKey, setRefreshKey] = useState(0);

    const editorRef = useRef<HTMLElement>(null);

    const token = getToken();
    const isLoggedIn = Boolean(token);
    const isAdmin = hasAdminRole(token);

    const editingOfficialCharacter =
        editor?.mode === "edit" &&
        isOfficialCharacter(editor.character);

    // ============================================
    // SCROLL TO EDITOR
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
    // LOAD CHARACTERS
    // ============================================

    useEffect(() => {
        if (!isAdmin) return;

        let cancelled = false;

        async function load() {
            try {
                const data = await getCharacters();

                if (cancelled) return;

                setCharacters(
                    [...data].sort(
                        (a, b) => a.price - b.price
                    )
                );

                setError("");
            } catch (err) {
                if (cancelled) return;

                setError(
                    err instanceof Error
                        ? err.message
                        : "Could not load characters."
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
        setForm({ ...emptyCharacter });
        setEditor({ mode: "create" });

        setError("");
        setNotice("");
    }

    function openEdit(character: Character) {
        setForm({
            name: character.name,
            description: character.description ?? null,
            price: character.price,
            imageUrl: character.imageUrl ?? null,
        });

        setEditor({
            mode: "edit",
            character,
        });

        setError("");
        setNotice("");
    }

    function closeEditor() {
        setEditor(null);
        setError("");
    }

    // ============================================
    // SAVE CHARACTER
    // ============================================

    async function handleSave(
        event: FormEvent<HTMLFormElement>
    ) {
        event.preventDefault();

        if (!editor || !isAdmin || saving) return;

        const data: CharacterInput = {
            name: form.name.trim(),
            description:
                form.description?.trim() || null,
            price: Number(form.price),
            imageUrl:
                form.imageUrl?.trim() || null,
        };

        if (
            data.name.length < 2 ||
            data.name.length > 50 ||
            (data.description?.length ?? 0) > 300 ||
            !Number.isInteger(data.price) ||
            data.price < 0 ||
            data.price > 100000
        ) {
            setError(
                "Name must contain 2–50 characters. " +
                "Description: max 300 characters. " +
                "Price must be between 0 and 100000."
            );
            return;
        }

        if (data.imageUrl) {
            try {
                const url = new URL(data.imageUrl);

                if (
                    url.protocol !== "https:" &&
                    url.protocol !== "http:"
                ) {
                    setError(
                        "Image URL must use HTTP or HTTPS."
                    );
                    return;
                }
            } catch {
                setError("Please enter a valid image URL.");
                return;
            }
        }

        setSaving(true);
        setError("");
        setNotice("");

        try {
            if (editor.mode === "create") {
                await createCharacter(data);

                setNotice(
                    "Character created successfully."
                );
            } else {
                await updateCharacter(
                    editor.character.characterId,
                    data
                );

                setNotice(
                    "Character updated successfully."
                );
            }

            setEditor(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not save character."
            );
        } finally {
            setSaving(false);
        }
    }

    // ============================================
    // DELETE CHARACTER
    // ============================================

    async function handleDelete(character: Character) {
        if (!isAdmin || saving) return;

        if (isOfficialCharacter(character)) {
            setError(
                "Official characters cannot be deleted " +
                "from this admin interface."
            );
            return;
        }

        const confirmed = window.confirm(
            `Delete ${character.name}?\n\n` +
            "Characters owned by players cannot be deleted. " +
            "This action cannot be undone."
        );

        if (!confirmed) return;

        setSaving(true);
        setError("");
        setNotice("");

        try {
            await deleteCharacter(character.characterId);

            setNotice(
                `${character.name} deleted successfully.`
            );

            setEditor(null);
            refresh();
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not delete character."
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
                <h1>Manage Characters</h1>

                <p>Log in as an administrator.</p>

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
                    Only administrators can manage
                    characters.
                </p>

                <Link
                    to="/characters"
                    className="gs-btn gs-btn--outline"
                >
                    Back to Shop
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
                <h1>Manage Characters</h1>

                <p>
                    Create, edit and manage characters
                    available in Galactic Slicer.
                </p>

                <div className="gs-button-row">
                    <button
                        type="button"
                        className="gs-btn gs-btn--primary"
                        onClick={openCreate}
                        disabled={saving}
                    >
                        Create New Character
                    </button>

                    <Link
                        to="/characters"
                        className="gs-btn gs-btn--outline"
                    >
                        View Shop
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
                <p role="status">{notice}</p>
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
                            ? "Create Character"
                            : `Edit ${editor.character.name}`}
                    </h2>

                    {editingOfficialCharacter && (
                        <p>
                            This is an official character.
                            Its name is protected to prevent
                            duplicate seeded characters.
                        </p>
                    )}

                    <form onSubmit={handleSave}>
                        <p>
                            <label htmlFor="characterName">
                                Character Name
                            </label>
                        </p>

                        <input
                            id="characterName"
                            className="gs-input"
                            type="text"
                            minLength={2}
                            maxLength={50}
                            required
                            readOnly={editingOfficialCharacter}
                            value={form.name}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    name: event.target.value,
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="characterPrice">
                                Price (Credits)
                            </label>
                        </p>

                        <input
                            id="characterPrice"
                            className="gs-input"
                            type="number"
                            min={0}
                            max={100000}
                            step={1}
                            required
                            value={form.price}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    price: Number(
                                        event.target.value
                                    ),
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="characterDescription">
                                Description
                            </label>
                        </p>

                        <textarea
                            id="characterDescription"
                            className="gs-input"
                            rows={3}
                            maxLength={300}
                            value={form.description ?? ""}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    description:
                                        event.target.value,
                                }))
                            }
                        />

                        <p>
                            <label htmlFor="characterImage">
                                Image URL (optional)
                            </label>
                        </p>

                        <input
                            id="characterImage"
                            className="gs-input"
                            type="url"
                            placeholder="https://example.com/character.png"
                            value={form.imageUrl ?? ""}
                            onChange={event =>
                                setForm(previous => ({
                                    ...previous,
                                    imageUrl:
                                        event.target.value,
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
                                      ? "Create Character"
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

            {/* CHARACTER LIST */}

            <section>
                <h2>Existing Characters</h2>

                {loading ? (
                    <p role="status">
                        Loading characters...
                    </p>
                ) : characters.length === 0 ? (
                    <p className="challenge-empty">
                        No characters found.
                    </p>
                ) : (
                    <div className="gs-challenge-list">
                        {characters.map(character => {
                            const official =
                                isOfficialCharacter(character);

                            return (
                                <article
                                    key={character.characterId}
                                    className="gs-page-intro"
                                >
                                    <h3>{character.name}</h3>

                                    {official && (
                                        <p>Official Character</p>
                                    )}

                                    <p>
                                        <strong>Price:</strong>{" "}
                                        {character.price} Credits
                                    </p>

                                    {character.description && (
                                        <p>
                                            {character.description}
                                        </p>
                                    )}

                                    {character.imageUrl && (
                                        <p>
                                            <a
                                                href={character.imageUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                            >
                                                View Character Image
                                            </a>
                                        </p>
                                    )}

                                    <div className="gs-button-row">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline"
                                            disabled={saving}
                                            onClick={() =>
                                                openEdit(character)
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
                                                        character
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

export default AdminCharactersPage;
