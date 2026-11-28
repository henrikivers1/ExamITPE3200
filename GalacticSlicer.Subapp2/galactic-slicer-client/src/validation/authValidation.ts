// Client-side validation for the Log in and Sign up forms.
// The API logs in with email (LoginDto and RegisterDto have [Required] and [EmailAddress])
// and uses ASP.NET Identity's default password rules.
// The API still checks everything again: client-side checks only give faster feedback.

export interface LoginValues {
    email: string;
    password: string;
}

export interface SignUpValues {
    email: string;
    password: string;
    confirmPassword: string;
}

// one message per field; a field that is missing here is valid
export type FormErrors<T> = Partial<Record<keyof T, string>>;

// Identity's default password rules, shown as a checklist on the Sign up page
export const passwordRules = [
    { label: "At least 6 characters", test: (p: string) => p.length >= 6 },
    { label: "An uppercase letter (A-Z)", test: (p: string) => /[A-Z]/.test(p) },
    { label: "A lowercase letter (a-z)", test: (p: string) => /[a-z]/.test(p) },
    { label: "A number (0-9)", test: (p: string) => /[0-9]/.test(p) },
    { label: "A symbol, like ! or #", test: (p: string) => /[^a-zA-Z0-9]/.test(p) },
];

function validateEmail(email: string): string | undefined {
    if (!email.trim()) {
        return "Email is required.";
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
        return "Enter a valid email address.";
    }
    return undefined;
}

export function validateLogin(values: LoginValues): FormErrors<LoginValues> {
    const errors: FormErrors<LoginValues> = {};

    const email = validateEmail(values.email);
    if (email) errors.email = email;

    if (!values.password) {
        errors.password = "Password is required.";
    }

    return errors;
}

export function validateSignUp(values: SignUpValues): FormErrors<SignUpValues> {
    const errors: FormErrors<SignUpValues> = {};

    const email = validateEmail(values.email);
    if (email) errors.email = email;

    if (!values.password) {
        errors.password = "Password is required.";
    } else if (!passwordRules.every(rule => rule.test(values.password))) {
        errors.password = "Password does not meet all requirements.";
    }

    if (!values.confirmPassword) {
        errors.confirmPassword = "Please confirm your password.";
    } else if (values.password !== values.confirmPassword) {
        errors.confirmPassword = "Passwords do not match.";
    }

    return errors;
}
