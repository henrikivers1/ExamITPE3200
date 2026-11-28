// Rarity tier of a character, decided by its price. Same limits as Helpers/CharacterTier.cs in Subapp 1.
// Used by the Shop cards and the badge, so the price limits only live here.

export interface Tier {
    // used in CSS class names (gs-badge--rare, shop-card--rare)
    key: "common" | "uncommon" | "rare" | "epic" | "legendary";
    // shown to the player
    label: string;
}

export function getTier(price: number): Tier {
    // one tier per seeded badge: 0, 500, 1000, 1500, 2000
    if (price === 0) return { key: "common", label: "Common" };
    if (price < 1000) return { key: "uncommon", label: "Uncommon" };
    if (price < 1500) return { key: "rare", label: "Rare" };
    if (price < 2000) return { key: "epic", label: "Epic" };
    return { key: "legendary", label: "Legendary" };
}

// Points for the legendary badge's starburst edge (12 spikes around the center).
// (Subapp 1 needed InvariantCulture here because of the Norwegian decimal comma;
// JavaScript always writes numbers with a ".")
export function starburstPoints(): string {
    const points: string[] = [];
    for (let i = 0; i < 24; i++) {
        const radius = i % 2 === 0 ? 44 : 37;
        const angle = (Math.PI * i) / 12 - Math.PI / 2;
        const x = Math.round((50 + radius * Math.cos(angle)) * 10) / 10;
        const y = Math.round((50 + radius * Math.sin(angle)) * 10) / 10;
        points.push(`${x},${y}`);
    }
    return points.join(" ");
}
