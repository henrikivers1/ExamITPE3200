import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import ShopCard from "../components/ShopCard";
import { usePageTitle } from "../hooks/usePageTitle";
import { getCharacters } from "../services/characterService";
import type { Character, ShopPlayer } from "../types/Character";

// Character Shop, like Character/Shop in Subapp 1
function ShopPage() {
    usePageTitle("Character Shop");

    const [characters, setCharacters] = useState<Character[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // TODO: the logged-in player's Credits, owned characters and equipped character,
    // from the API once login and player progress exist. null = visitor.
    const [player] = useState<ShopPlayer | null>(null);

    useEffect(() => {
        async function loadCharacters() {
            try {
                // cheapest first, so the Starter character comes first
                const data = await getCharacters();
                setCharacters([...data].sort((a, b) => a.price - b.price));
            } catch {
                setError("Could not load the characters.");
            } finally {
                setLoading(false);
            }
        }

        loadCharacters();
    }, []);

    return (
        <>
            <section className="gs-page-intro">
                <h1>Character Shop</h1>
                <p>Spend the Credits you earn on missions to unlock new slicers. Characters are cosmetic, so pick your favorite.</p>

                {/* the player's balance, or a hint to log in */}
                {player ? (
                    <div className="shop-balance">
                        <span className="shop-coin" aria-hidden="true">&#9679;</span>
                        {player.credits} Credits
                    </div>
                ) : (
                    <p className="shop-login-hint">
                        <Link to="/login">Log in</Link> to buy and equip characters.
                    </p>
                )}
            </section>

            {/* aria-live: screen readers hear when loading ends or an error appears */}
            <div aria-live="polite">
                {loading && <p className="gs-page-intro">Loading characters...</p>}
                {error && <p className="gs-page-intro" role="alert">{error}</p>}
                {!loading && !error && characters.length === 0 && (
                    <p className="gs-page-intro">No characters in the shop yet.</p>
                )}
            </div>

            {characters.length > 0 && (
                <section className="shop-grid" aria-label="Characters">
                    {characters.map(character => (
                        <ShopCard key={character.characterId} character={character} player={player} />
                    ))}
                </section>
            )}
        </>
    );
}

export default ShopPage;
