// Client-side validation for the Log in and Sign up forms.
// Same rules and messages as LoginViewModel and SignUpViewModel in Subapp 1,
// plus ASP.NET Identity's default password rules (the API uses Identity's defaults).
// The API must still check everything again: client-side checks only give faster feedback.

export interface LoginValues {
    username: string;
    password: string;
}

export interface SignUpValues {
    username: string;
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

export function validateLogin(values: LoginValues): FormErrors<LoginValues> {
    const errors: FormErrors<LoginValues> = {};

    if (!values.username.trim()) {
        errors.username = "Please enter your username.";
    }
    if (!values.password) {
        errors.password = "Please enter your password.";
    }

    return errors;
}

export function validateSignUp(values: SignUpValues): FormErrors<SignUpValues> {
    const errors: FormErrors<SignUpValues> = {};
    const username = values.username.trim();

    if (!username) {
        errors.username = "Please choose a username.";
    } else if (username.length < 3 || username.length > 30) {
        errors.username = "Username must be between 3 and 30 characters.";
    } else if (!/^[a-zA-Z0-9_.-]+$/.test(username)) {
        errors.username = "Username can only contain letters, numbers, _ . and -";
    }

    if (!values.password) {
        errors.password = "Please choose a password.";
    } else if (!passwordRules.every(rule => rule.test(values.password))) {
        errors.password = "The password doesn't meet all the rules below.";
    }

    if (values.confirmPassword !== values.password) {
        errors.confirmPassword = "The passwords do not match.";
    }

    return errors;
}
