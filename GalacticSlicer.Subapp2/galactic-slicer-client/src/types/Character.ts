// A character returned by the API for the Shop and active pilot badge.
export interface Character {
    characterId: number;
    name: string;
    description: string | null;
    price: number;
    imageUrl: string | null;
}

// Player data assembled from the progress and character ownership APIs.
export interface ShopPlayer {
    credits: number;
    ownedCharacterIds: number[];
    activeCharacterId: number | null;
}
