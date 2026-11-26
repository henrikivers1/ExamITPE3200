
import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import { passwordRules } from "../validation/authValidation";
import { apiRequest, ApiError } from "../services/api";

type SignUpValues = {
    email: string;
    password: string;
    confirmPassword: string;
};

const fields: (keyof SignUpValues)[] = [
    "email",
    "password",
    "confirmPassword",
];

function RegisterPage() {
    usePageTitle("Sign up");

    const navigate = useNavigate();

    const [values, setValues] = useState<SignUpValues>({
        email: "",
        password: "",
        confirmPassword: "",
    });

    const [touched, setTouched] = useState<
        Partial<Record<keyof SignUpValues, boolean>>
    >({});

    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [notice, setNotice] = useState<string | null>(null);

    function validate() {
        const errors: Partial<Record<keyof SignUpValues, string>> = {};

        if (!values.email.trim()) {
            errors.email = "Email is required.";
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email.trim())) {
            errors.email = "Enter a valid email address.";
        }

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

    const errors = validate();

    const visibleError = (field: keyof SignUpValues) =>
        touched[field] || submitted ? errors[field] : undefined;

    function update(field: keyof SignUpValues, value: string) {
        setValues(current => ({
            ...current,
            [field]: value,
        }));

        setNotice(null);
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (loading) return;

        setSubmitted(true);
        setNotice(null);

        const firstInvalid = fields.find(field => errors[field]);

        if (firstInvalid) {
            document.getElementById(firstInvalid)?.focus();
            return;
        }

        setLoading(true);

        try {
            await apiRequest<unknown>(
                "/api/auth/register",
                {
                    method: "POST",
                    body: JSON.stringify({
                        email: values.email.trim(),
                        password: values.password,
                    }),
                }
            );

            navigate("/login", {
                replace: true,
                state: {
                    message:
                        "Account created successfully. Please log in.",
                },
            });
        } catch (error) {
            if (error instanceof ApiError) {
                setNotice(error.message);
            } else {
                setNotice(
                    "Registration failed. Please try again."
                );
            }
        } finally {
            setLoading(false);
        }
    }

    return (
        <section className="gs-auth-wrapper">
            <div className="gs-auth-card">
                <h1 className="gs-auth-title">Sign up</h1>

                <p className="gs-auth-subtitle">
                    Join the fight for a safer galaxy.
                </p>

                <div role="status" aria-live="polite">
                    {notice && (
                        <p className="gs-form-notice">
                            {notice}
                        </p>
                    )}
                </div>

                <form
                    className="gs-form"
                    onSubmit={handleSubmit}
                    noValidate
                >
                    <FormField
                        id="email"
                        label="Email"
                        type="email"
                        value={values.email}
                        onChange={value =>
                            update("email", value)
                        }
                        onBlur={() =>
                            setTouched(current => ({
                                ...current,
                                email: true,
                            }))
                        }
                        error={visibleError("email")}
                        placeholder="Enter your email"
                        autoComplete="email"
                    />

                    <FormField
                        id="password"
                        label="Password"
                        type="password"
                        value={values.password}
                        onChange={value =>
                            update("password", value)
                        }
                        onBlur={() =>
                            setTouched(current => ({
                                ...current,
                                password: true,
                            }))
                        }
                        error={visibleError("password")}
                        placeholder="Choose a password"
                        autoComplete="new-password"
                    >
                        <ul
                            className="gs-password-rules"
                            aria-label="Password rules"
                        >
                            {passwordRules.map(rule => {
                                const met = rule.test(values.password);

                                return (
                                    <li
                                        key={rule.label}
                                        className={met ? "is-met" : ""}
                                    >
                                        <span aria-hidden="true">
                                            {met ? "✓" : "○"}
                                        </span>{" "}
                                        {rule.label}
                                        <span className="gs-sr-only">
                                            {met
                                                ? " (done)"
                                                : " (missing)"}
                                        </span>
                                    </li>
                                );
                            })}
                        </ul>
                    </FormField>

                    <FormField
                        id="confirmPassword"
                        label="Confirm password"
                        type="password"
                        value={values.confirmPassword}
                        onChange={value =>
                            update("confirmPassword", value)
                        }
                        onBlur={() =>
                            setTouched(current => ({
                                ...current,
                                confirmPassword: true,
                            }))
                        }
                        error={visibleError("confirmPassword")}
                        placeholder="Repeat your password"
                        autoComplete="new-password"
                    />

                    <button
                        type="submit"
                        className="gs-btn gs-btn--primary gs-btn--lg gs-form-submit"
                        disabled={loading}
                    >
                        {loading ? "Creating account..." : "Sign up"}
                    </button>
                </form>

                <p className="gs-form-footer">
                    Already have an account?{" "}
                    <Link to="/login">Log in</Link>
                </p>
            </div>
        </section>
    );
}

export default RegisterPage;
