// Shared helper for every request to the Galactic Slicer API.
// The service files (planetService.ts, ...) use this instead of calling fetch themselves,
// so the API address and the error handling live in one place.

// Can be changed with VITE_API_URL in a .env file; defaults to the API's http launch profile
export const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5190";

// Thrown when the API answers with an error status (400, 404, 500, ...)
export class ApiError extends Error {
    status: number;
    // validation errors per field from ASP.NET (e.g. { Name: ["The Name field is required."] })
    errors: Record<string, string[]>;

    constructor(status: number, message: string, errors: Record<string, string[]> = {}) {
        super(message);
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
    }
}

// Sends a request to the API and returns the JSON response.
// path starts with /api, e.g. apiRequest<Planet[]>("/api/planets")
export async function apiRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
    let response: Response;

    try {
        response = await fetch(`${API_BASE_URL}${path}`, {
            ...options,
            headers: {
                "Content-Type": "application/json",
                ...options.headers,
            },
        });
    } catch {
        // fetch only throws when there is no answer at all (API not running, no network)
        throw new ApiError(0, "Could not reach the server. Is the API running?");
    }

    if (!response.ok) {
        // ASP.NET sends error details as JSON (ProblemDetails); use them when they are there
        const problem = await response.json().catch(() => null);
        throw new ApiError(
            response.status,
            problem?.title ?? `Request failed with status ${response.status}`,
            problem?.errors ?? {},
        );
    }

    // 204 No Content (e.g. after a delete) has no body to read
    if (response.status === 204) {
        return undefined as T;
    }

    return response.json();
}
