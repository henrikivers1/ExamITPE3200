
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import ShopCard from "../components/ShopCard";
import { usePageTitle } from "../hooks/usePageTitle";
import { getToken } from "../services/api";
import {
    getCharacters,
    getOwnedCharacterIds,
    buyCharacter,
    equipCharacter,
    unequipCharacter,
} from "../services/characterService";
import { getProgress } from "../services/progressService";
import type {
    Character,
    ShopPlayer,
} from "../types/Character";

// Character Shop, like Character/Shop in Subapp 1.
function ShopPage() {
    usePageTitle("Character Shop");

    const [characters, setCharacters] =
        useState<Character[]>([]);

    const [player, setPlayer] =
        useState<ShopPlayer | null>(null);

    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);

    const [error, setError] =
        useState<string | null>(null);

    const [notice, setNotice] =
        useState<string | null>(null);

    const isLoggedIn = Boolean(getToken());

    // ============================================
    // LOAD SHOP DATA
    // ============================================

    useEffect(() => {
        let cancelled = false;

        async function loadShop() {
            setLoading(true);
            setError(null);

            try {
                // Character catalogue is public.
                const characterData = await getCharacters();

                if (cancelled) return;

                // Cheapest characters first.
                setCharacters(
                    [...characterData].sort(
                        (a, b) => a.price - b.price
                    )
                );

                if (isLoggedIn) {
                    // Fetch player balance and ownership.
                    const [progress, ownedIds] =
                        await Promise.all([
                            getProgress(),
                            getOwnedCharacterIds(),
                        ]);

                    if (cancelled) return;

                    setPlayer({
                        credits: progress.credits,
                        activeCharacterId:
                            progress.activeCharacterId,
                        ownedCharacterIds: ownedIds,
                    });
                } else {
                    setPlayer(null);
                }
            } catch (err) {
                if (cancelled) return;

                setError(
                    err instanceof Error
                        ? err.message
                        : "Could not load the shop."
                );
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        void loadShop();

        return () => {
            cancelled = true;
        };
    }, [isLoggedIn]);

    // ============================================
    // REFRESH PLAYER AFTER A SHOP ACTION
    // ============================================

    async function refreshPlayer() {
        const [progress, ownedIds] = await Promise.all([
            getProgress(),
            getOwnedCharacterIds(),
        ]);

        setPlayer({
            credits: progress.credits,
            activeCharacterId: progress.activeCharacterId,
            ownedCharacterIds: ownedIds,
        });
    }

    // ============================================
    // BUY CHARACTER
    // ============================================

    async function handleBuy(characterId: number) {
        if (!player || busy) return;

        const character = characters.find(
            c => c.characterId === characterId
        );

        if (!character) return;

        setBusy(true);
        setError(null);
        setNotice(null);

        try {
            await buyCharacter(characterId);

            await refreshPlayer();

            setNotice(
                `${character.name} purchased successfully!`
            );
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not purchase character."
            );

            // Refresh in case the balance changed on server.
            try {
                await refreshPlayer();
            } catch {
                // Keep the original error message.
            }
        } finally {
            setBusy(false);
        }
    }

    // ============================================
    // EQUIP CHARACTER
    // ============================================

    async function handleEquip(characterId: number) {
        if (!player || busy) return;

        const character = characters.find(
            c => c.characterId === characterId
        );

        if (!character) return;

        setBusy(true);
        setError(null);
        setNotice(null);

        try {
            await equipCharacter(characterId);

            await refreshPlayer();

            setNotice(
                `${character.name} equipped successfully!`
            );
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not equip character."
            );
        } finally {
            setBusy(false);
        }
    }

    // ============================================
    // UNEQUIP CHARACTER
    // ============================================

    async function handleUnequip() {
        if (!player || busy) return;

        setBusy(true);
        setError(null);
        setNotice(null);

        try {
            await unequipCharacter();

            await refreshPlayer();

            setNotice(
                "Character unequipped successfully."
            );
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Could not unequip character."
            );
        } finally {
            setBusy(false);
        }
    }

    // ============================================
    // RENDER
    // ============================================

    return (
        <>
            <section className="gs-page-intro">
                <h1>Character Shop</h1>

                <p>
                    Spend the Credits you earn on missions
                    to unlock new slicers. Characters are
                    cosmetic, so pick your favorite.
                </p>

                {!loading && player ? (
                    <div
                        className="shop-balance"
                        aria-live="polite"
                    >
                        <span
                            className="shop-coin"
                            aria-hidden="true"
                        >
                            &#9679;
                        </span>

                        {player.credits} Credits
                    </div>
                ) : !loading && !isLoggedIn ? (
                    <p className="shop-login-hint">
                        <Link to="/login">
                            Log in
                        </Link>{" "}
                        to buy and equip characters.
                    </p>
                ) : null}
            </section>

            <div aria-live="polite">
                {loading && (
                    <p className="gs-page-intro">
                        Loading characters...
                    </p>
                )}

                {error && (
                    <p
                        className="gs-page-intro"
                        role="alert"
                    >
                        {error}
                    </p>
                )}

                {notice && (
                    <p
                        className="gs-page-intro"
                        role="status"
                    >
                        {notice}
                    </p>
                )}

                {!loading &&
                    !error &&
                    characters.length === 0 && (
                    <p className="gs-page-intro">
                        No characters in the shop yet.
                    </p>
                )}
            </div>

            {!loading && characters.length > 0 && (
                <section
                    className="shop-grid"
                    aria-label="Characters"
                >
                    {characters.map(character => (
                        <ShopCard
                            key={character.characterId}
                            character={character}
                            player={player}
                            busy={busy}
                            onBuy={handleBuy}
                            onEquip={handleEquip}
                            onUnequip={handleUnequip}
                        />
                    ))}
                </section>
            )}
        </>
    );
}

export default ShopPage;
