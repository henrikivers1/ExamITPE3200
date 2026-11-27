
import type { Planet } from "../types/Planet";
import { apiRequest } from "./api";

export interface PlanetInput {
    name: string;
    description: string | null;
    subject: string | null;
    cssClass: string;
    order: number;
}

// Public: GET /api/planets
export async function getPlanets(): Promise<Planet[]> {
    return apiRequest<Planet[]>("/api/planets");
}

// Public: GET /api/planets/{id}
export async function getPlanet(
    id: number
): Promise<Planet> {
    return apiRequest<Planet>(`/api/planets/${id}`);
}

// Admin: POST /api/planets
export async function createPlanet(
    data: PlanetInput
): Promise<Planet> {
    return apiRequest<Planet>("/api/planets", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

// Admin: PUT /api/planets/{id}
export async function updatePlanet(
    id: number,
    data: PlanetInput
): Promise<void> {
    await apiRequest<unknown>(`/api/planets/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

// Admin: DELETE /api/planets/{id}
export async function deletePlanet(
    id: number
): Promise<void> {
    await apiRequest<unknown>(`/api/planets/${id}`, {
        method: "DELETE",
    });
}
