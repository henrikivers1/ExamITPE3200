import type { Planet } from "../types/Planet.ts";
import { apiRequest } from "./api.ts";

// GET /api/planets
export async function getPlanets(): Promise<Planet[]> {
    return apiRequest<Planet[]>("/api/planets");
}
