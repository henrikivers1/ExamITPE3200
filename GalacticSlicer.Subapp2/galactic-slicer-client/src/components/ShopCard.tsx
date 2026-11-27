
import { Link } from "react-router-dom";
import CharacterBadge from "./CharacterBadge";
import type {
    Character,
    ShopPlayer,
} from "../types/Character";
import { getTier } from "../utils/characterTier";

interface ShopCardProps {
    character: Character;
    player: ShopPlayer | null;
    busy?: boolean;
    onBuy?: (characterId: number) => void;
    onEquip?: (characterId: number) => void;
    onUnequip?: () => void;
}

function ShopCard({
    character,
    player,
    busy = false,
    onBuy,
    onEquip,
    onUnequip,
}: ShopCardProps) {
    const tier = getTier(character.price);

    const isOwned =
        player?.ownedCharacterIds.includes(
            character.characterId
        ) ?? false;

    const isEquipped =
        player?.activeCharacterId ===
        character.characterId;

    const creditsMissing =
        character.price - (player?.credits ?? 0);

    return (
        <article
            className={
                `shop-card shop-card--${tier.key}` +
                (isEquipped
                    ? " shop-card--equipped"
                    : "")
            }
        >
            <div className="shop-portrait">
                {character.imageUrl ? (
                    <img
                        className="shop-portrait-img"
                        src={character.imageUrl}
                        alt={character.name}
                    />
                ) : (
                    <span className="shop-badge">
                        <CharacterBadge character={character} />
                    </span>
                )}

                <span className="shop-tier">
                    {tier.label}
                </span>

                {isEquipped && (
                    <span className="shop-status shop-status--equipped">
                        Equipped
                    </span>
                )}

                {!isEquipped && isOwned && (
                    <span className="shop-status">
                        Owned
                    </span>
                )}
            </div>

            <div className="shop-card-body">
                <h2 className="shop-card-title">
                    {character.name}
                </h2>

                <p className="shop-card-text">
                    {character.description}
                </p>
            </div>

            <div className="shop-card-footer">
                <span className="shop-price">
                    {character.price === 0 ? (
                        "Free"
                    ) : (
                        <>
                            <span
                                className="shop-coin"
                                aria-hidden="true"
                            >
                                &#9679;
                            </span>

                            {character.price}

                            <span className="gs-sr-only">
                                {" "}Credits
                            </span>
                        </>
                    )}
                </span>

                <ShopAction
                    characterId={character.characterId}
                    player={player}
                    isOwned={isOwned}
                    isEquipped={isEquipped}
                    creditsMissing={creditsMissing}
                    busy={busy}
                    onBuy={onBuy}
                    onEquip={onEquip}
                    onUnequip={onUnequip}
                />
            </div>
        </article>
    );
}

interface ShopActionProps {
    characterId: number;
    player: ShopPlayer | null;
    isOwned: boolean;
    isEquipped: boolean;
    creditsMissing: number;
    busy: boolean;
    onBuy?: (characterId: number) => void;
    onEquip?: (characterId: number) => void;
    onUnequip?: () => void;
}

function ShopAction({
    characterId,
    player,
    isOwned,
    isEquipped,
    creditsMissing,
    busy,
    onBuy,
    onEquip,
    onUnequip,
}: ShopActionProps) {
    if (player === null) {
        return (
            <Link
                to="/login"
                className="gs-btn gs-btn--outline gs-btn--sm"
            >
                Log in to buy
            </Link>
        );
    }

    if (isEquipped) {
        return (
            <button
                type="button"
                className="gs-btn gs-btn--ghost gs-btn--sm"
                disabled={busy || !onUnequip}
                onClick={onUnequip}
            >
                {busy ? "Processing..." : "Unequip"}
            </button>
        );
    }

    if (isOwned) {
        return (
            <button
                type="button"
                className="gs-btn gs-btn--outline gs-btn--sm"
                disabled={busy || !onEquip}
                onClick={() => onEquip?.(characterId)}
            >
                {busy ? "Processing..." : "Equip"}
            </button>
        );
    }

    if (creditsMissing > 0) {
        return (
            <span
                className="gs-btn gs-btn--sm shop-btn-locked"
                aria-disabled="true"
            >
                Need {creditsMissing} more
            </span>
        );
    }

    return (
        <button
            type="button"
            className="gs-btn gs-btn--primary gs-btn--sm"
            disabled={busy || !onBuy}
            onClick={() => onBuy?.(characterId)}
        >
            {busy ? "Processing..." : "Buy"}
        </button>
    );
}

export default ShopCard;
