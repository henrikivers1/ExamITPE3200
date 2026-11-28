
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
    getCharacters,
    createCharacter,
    updateCharacter,
    deleteCharacter,
    type CharacterInput,
} from "../services/characterService";
import type { Character } from "../types/Character";
import CharacterBadge from "../components/CharacterBadge";
import { getTier } from "../utils/characterTier";

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

    const isLoggedIn = Boolean(getToken());
    const isAdmin = isAdminUser();

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

                <p>Add, edit and remove the characters in the Shop.</p>
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
                    Add Character
                </button>

                <Link
                    to="/characters"
                    className="gs-btn gs-btn--ghost"
                >
                    Back to the Shop
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

            {/* CHARACTER LIST: same table as Character/Manage in Subapp 1 */}

            {loading ? (
                <p className="gs-page-intro" role="status">
                    Loading characters...
                </p>
            ) : characters.length === 0 ? (
                <p className="gs-page-intro">
                    No characters yet. Add the first one above.
                </p>
            ) : (
                <section
                    className="gs-admin-table-wrap"
                    aria-label="Characters"
                >
                    <table className="gs-admin-table">
                        <thead>
                            <tr>
                                <th>
                                    <span className="gs-sr-only">
                                        Badge
                                    </span>
                                </th>
                                <th>Name</th>
                                <th className="gs-admin-hide-sm">Tier</th>
                                <th>Price</th>
                                <th className="gs-admin-actions-cell">
                                    Actions
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {characters.map(character => (
                                <tr key={character.characterId}>
                                    <td>
                                        <span className="gs-admin-badge">
                                            <CharacterBadge
                                                character={character}
                                            />
                                        </span>
                                    </td>
                                    <td>{character.name}</td>
                                    <td className="gs-admin-hide-sm">
                                        {getTier(character.price).label}
                                    </td>
                                    <td>
                                        {character.price === 0
                                            ? "Free"
                                            : `${character.price} Credits`}
                                    </td>
                                    <td className="gs-admin-actions-cell">
                                        <button
                                            type="button"
                                            className="gs-btn gs-btn--outline gs-btn--sm"
                                            disabled={saving}
                                            aria-label={`Edit ${character.name}`}
                                            onClick={() =>
                                                openEdit(character)
                                            }
                                        >
                                            Edit
                                        </button>

                                        {isOfficialCharacter(character) ? (
                                            <span
                                                className="gs-btn gs-btn--sm gs-btn--ghost"
                                                title="Standard characters can't be deleted"
                                            >
                                                Locked
                                            </span>
                                        ) : (
                                            <button
                                                type="button"
                                                className="gs-btn gs-btn--sm gs-btn--danger"
                                                disabled={saving}
                                                aria-label={`Delete ${character.name}`}
                                                onClick={() =>
                                                    void handleDelete(
                                                        character
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

export default AdminCharactersPage;
