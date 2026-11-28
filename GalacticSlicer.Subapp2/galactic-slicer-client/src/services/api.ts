
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

// Reads the role from the JWT, so the client can show admin links and pages.
// The API still checks the Admin role on every write.
export function isAdminUser(): boolean {
    const token = getToken();

    if (!token) return false;

    try {
        const payload = JSON.parse(
            atob(
                token.split(".")[1]
                    .replace(/-/g, "+")
                    .replace(/_/g, "/")
            )
        ) as Record<string, unknown>;

        const role =
            payload.role ??
            payload.roles ??
            payload[
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            ];

        return (
            role === "Admin" ||
            (Array.isArray(role) && role.includes("Admin"))
        );
    } catch {
        return false;
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

// The API's per-field errors for the given form fields, e.g. { email: "..." }.
// ASP.NET names the fields like the DTO ("Email", "$.email"), so the match ignores case.
export function fieldErrors<T extends string>(
    error: unknown,
    fields: readonly T[]
): Partial<Record<T, string>> {
    const result: Partial<Record<T, string>> = {};

    if (!(error instanceof ApiError)) return result;

    for (const [key, messages] of Object.entries(error.errors)) {
        const name = key.replace(/^\$\./, "").toLowerCase();
        const field = fields.find(f => f.toLowerCase() === name);

        if (field && messages.length > 0) {
            result[field] = messages.join(" ");
        }
    }

    return result;
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
        const responseText = await response.text()
            .catch(() => "");

        let problem: unknown = null;

        if (responseText.trim()) {
            try {
                problem = JSON.parse(responseText);
            } catch {
                problem = responseText.trim();
            }
        }

        let message =
            `Request failed with status ${response.status}`;

        let errors: Record<string, string[]> = {};

        if (Array.isArray(problem)) {
            // ASP.NET Identity errors: [{ code, description }]
            const items = problem.filter(
                item =>
                    item &&
                    typeof item.description === "string"
            ) as { code?: string; description: string }[];

            if (items.length > 0) {
                message = items
                    .map(item => item.description)
                    .join(" ");
            }

            for (const item of items) {
                const key = item.code ?? "General";
                errors[key] = [
                    ...(errors[key] ?? []),
                    item.description,
                ];
            }
        } else if (typeof problem === "string") {
            message = problem;
        } else if (problem && typeof problem === "object") {
            const data = problem as {
                title?: string;
                detail?: string;
                message?: string;
                error?: string;
                errors?: Record<string, string[]>;
            };

            errors = data.errors ?? {};

            const fieldMessages = Object.values(errors).flat();

            message = data.message
                ?? data.detail
                ?? (fieldMessages.length > 0
                    ? fieldMessages.join(" ")
                    : undefined)
                ?? data.title
                ?? data.error
                ?? message;
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
