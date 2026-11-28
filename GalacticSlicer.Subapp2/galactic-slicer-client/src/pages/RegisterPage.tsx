
import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import {
    passwordRules,
    validateSignUp,
    type FormErrors,
    type SignUpValues,
} from "../validation/authValidation";
import { ApiError, fieldErrors } from "../services/api";
import { register } from "../services/authService";

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

    // errors the API sent back for a field (e.g. "email is already taken")
    const [serverErrors, setServerErrors] =
        useState<FormErrors<SignUpValues>>({});

    const errors = validateSignUp(values);

    // the client-side message first, otherwise the API's message for the field
    const visibleError = (field: keyof SignUpValues) =>
        (touched[field] || submitted ? errors[field] : undefined)
        ?? serverErrors[field];

    function update(field: keyof SignUpValues, value: string) {
        setValues(current => ({
            ...current,
            [field]: value,
        }));

        setNotice(null);

        // the API's message is about the old value, so drop it while typing
        setServerErrors(current => ({
            ...current,
            [field]: undefined,
        }));
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
            await register(values.email, values.password);

            navigate("/login", {
                replace: true,
                state: {
                    message:
                        "Account created successfully. Please log in.",
                },
            });
        } catch (error) {
            // show the API's field errors under the fields, the rest above the form
            const apiErrors = fieldErrors(error, fields);
            const firstField = fields.find(field => apiErrors[field]);

            setServerErrors(apiErrors);

            if (firstField) {
                setNotice("Please fix the highlighted fields.");
                document.getElementById(firstField)?.focus();
            } else if (error instanceof ApiError) {
                setNotice(error.message);
            } else {
                setNotice("Registration failed. Please try again.");
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
