// A character in the Shop, same fields as the Character model in Subapp 1.
// TODO: match this to the API's character DTO once Jonas has the endpoint.
export interface Character {
    characterId: number;
    name: string;
    description: string | null;
    price: number;
    imageUrl: string | null;
}

// What the Shop needs to know about the logged-in player.
// TODO: comes from the API once login and player progress exist.
export interface ShopPlayer {
    credits: number;
    ownedCharacterIds: number[];
    activeCharacterId: number | null;
}
