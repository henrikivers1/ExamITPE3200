
// Same fields as PlanetDto in the API
export interface Planet {
    planetId: number;
    name: string;
    description: string | null;
    order: number;
    cssClass: string;
    subject: string | null;
}