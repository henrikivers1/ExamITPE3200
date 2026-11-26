import { Link } from "react-router-dom";
import CharacterBadge from "./CharacterBadge";
import type { Character, ShopPlayer } from "../types/Character";
import { getTier } from "../utils/characterTier";

interface ShopCardProps {
    character: Character;
    // null = visitor (not logged in)
    player: ShopPlayer | null;
}

// One character card in the Shop: portrait on top, info in the middle, price and action at the bottom.
// Same card as Character/Shop.cshtml in Subapp 1.
function ShopCard({ character, player }: ShopCardProps) {
    const tier = getTier(character.price);
    const isOwned = player?.ownedCharacterIds.includes(character.characterId) ?? false;
    const isEquipped = player?.activeCharacterId === character.characterId;
    const creditsMissing = character.price - (player?.credits ?? 0);

    return (
        <article className={`shop-card shop-card--${tier.key}${isEquipped ? " shop-card--equipped" : ""}`}>
            {/* portrait: the real image if there is one, otherwise the space badge */}
            <div className="shop-portrait">
                {character.imageUrl ? (
                    <img className="shop-portrait-img" src={character.imageUrl} alt={character.name} />
                ) : (
                    <span className="shop-badge">
                        <CharacterBadge character={character} />
                    </span>
                )}

                <span className="shop-tier">{tier.label}</span>

                {/* ownership tag in the corner */}
                {isEquipped && <span className="shop-status shop-status--equipped">Equipped</span>}
                {!isEquipped && isOwned && <span className="shop-status">Owned</span>}
            </div>

            <div className="shop-card-body">
                <h2 className="shop-card-title">{character.name}</h2>
                <p className="shop-card-text">{character.description}</p>
            </div>

            <div className="shop-card-footer">
                <span className="shop-price">
                    {character.price === 0 ? "Free" : (
                        <>
                            <span className="shop-coin" aria-hidden="true">&#9679;</span>
                            {character.price}
                            <span className="gs-sr-only"> Credits</span>
                        </>
                    )}
                </span>

                <ShopAction
                    player={player}
                    isOwned={isOwned}
                    isEquipped={isEquipped}
                    creditsMissing={creditsMissing}
                />
            </div>
        </article>
    );
}

interface ShopActionProps {
    player: ShopPlayer | null;
    isOwned: boolean;
    isEquipped: boolean;
    creditsMissing: number;
}

// Conditional rendering: visitors get a login link, players get Unequip / Equip / Buy,
// or "Need X more" when they can't afford it (the API must check this too)
function ShopAction({ player, isOwned, isEquipped, creditsMissing }: ShopActionProps) {
    if (player === null) {
        return <Link to="/login" className="gs-btn gs-btn--outline gs-btn--sm">Log in to buy</Link>;
    }

    // TODO: connect these buttons to characterService once the API has Buy / Equip / Unequip
    if (isEquipped) {
        return <button type="button" className="gs-btn gs-btn--ghost gs-btn--sm">Unequip</button>;
    }
    if (isOwned) {
        return <button type="button" className="gs-btn gs-btn--outline gs-btn--sm">Equip</button>;
    }
    if (creditsMissing > 0) {
        return <span className="gs-btn gs-btn--sm shop-btn-locked" aria-disabled="true">Need {creditsMissing} more</span>;
    }
    return <button type="button" className="gs-btn gs-btn--primary gs-btn--sm">Buy</button>;
}

export default ShopCard;
