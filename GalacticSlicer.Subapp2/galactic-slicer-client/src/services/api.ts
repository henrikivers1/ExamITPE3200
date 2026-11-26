
export const API_BASE_URL =
    import.meta.env.VITE_API_URL ?? "http://localhost:5190";

const TOKEN_KEY = "galacticSlicerToken";

export function saveToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    window.dispatchEvent(new Event("auth-changed"));
}

export function clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
    window.dispatchEvent(new Event("auth-changed"));
}

export function getToken(): string | null {
    const token = localStorage.getItem(TOKEN_KEY);

    if (!token) return null;

    try {
        const payload = JSON.parse(
            atob(
                token.split(".")[1]
                    .replace(/-/g, "+")
                    .replace(/_/g, "/")
            )
        );

        if (typeof payload.exp !== "number" ||
            payload.exp * 1000 <= Date.now()) {
            clearToken();
            return null;
        }

        return token;
    } catch {
        clearToken();
        return null;
    }
}

export class ApiError extends Error {
    status: number;
    errors: Record<string, string[]>;

    constructor(
        status: number,
        message: string,
        errors: Record<string, string[]> = {}
    ) {
        super(message);
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
    }
}

export async function apiRequest<T>(
    path: string,
    options: RequestInit = {}
): Promise<T> {
    const token = getToken();

    const headers = new Headers(options.headers);

    if (options.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
    }

    if (token) {
        headers.set("Authorization", `Bearer ${token}`);
    }

    let response: Response;

    try {
        response = await fetch(`${API_BASE_URL}${path}`, {
            ...options,
            headers,
        });
    } catch {
        throw new ApiError(
            0,
            "Could not reach the server. Is the API running?"
        );
    }

    if (!response.ok) {
        const problem: unknown = await response.json()
            .catch(() => null);

        let message =
            `Request failed with status ${response.status}`;

        let errors: Record<string, string[]> = {};

        if (Array.isArray(problem)) {
            const descriptions = problem
                .filter(
                    item =>
                        item &&
                        typeof item.description === "string"
                )
                .map(item => item.description as string);

            if (descriptions.length > 0) {
                message = descriptions.join(" ");
            }
        } else if (problem && typeof problem === "object") {
            const data = problem as {
                title?: string;
                message?: string;
                errors?: Record<string, string[]>;
            };

            message = data.message ?? data.title ?? message;
            errors = data.errors ?? {};
        }

        // Invalid/expired authentication must not remain cached.
        // Do not clear the session on a failed login attempt.
        if (response.status === 401 &&
            !path.startsWith("/api/auth/")) {
            clearToken();
        }

        throw new ApiError(response.status, message, errors);
    }

    if (response.status === 204) {
        return undefined as T;
    }

    return response.json() as Promise<T>;
}
