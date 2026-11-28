import { ApiError, apiRequest, clearToken, saveToken } from "./api";

// What POST /api/auth/login returns.
interface LoginResponse {
    token: string;
    tokenType?: string;
    expiresIn?: number;
}

// POST /api/auth/login
// Saves the JWT, so apiRequest sends it with every later request.
export async function login(
    email: string,
    password: string
): Promise<void> {
    const response = await apiRequest<LoginResponse>(
        "/api/auth/login",
        {
            method: "POST",
            body: JSON.stringify({
                email: email.trim(),
                password,
            }),
        }
    );

    if (!response.token) {
        throw new Error("The API did not return a token.");
    }

    saveToken(response.token);
}

// Identity sends its errors by code (DuplicateUserName, PasswordTooShort ...).
// Move each one to the form field it belongs to, so the page can show it there.
function fieldForIdentityCode(code: string): string {
    if (code.startsWith("Password")) return "Password";

    if (
        code === "DuplicateUserName" ||
        code === "DuplicateEmail" ||
        code === "InvalidUserName" ||
        code === "InvalidEmail"
    ) {
        return "Email";
    }

    return code;
}

// POST /api/auth/register
// The player logs in afterwards on the Log in page.
export async function register(
    email: string,
    password: string
): Promise<void> {
    try {
        await apiRequest<unknown>("/api/auth/register", {
            method: "POST",
            body: JSON.stringify({
                email: email.trim(),
                password,
            }),
        });
    } catch (error) {
        if (!(error instanceof ApiError)) throw error;

        const errors: Record<string, string[]> = {};

        for (const [code, messages] of Object.entries(error.errors)) {
            const field = fieldForIdentityCode(code);
            errors[field] = [...(errors[field] ?? []), ...messages];
        }

        throw new ApiError(error.status, error.message, errors);
    }
}

// Removes the JWT. The API is stateless, so there is nothing to call.
export function logout(): void {
    clearToken();
}
