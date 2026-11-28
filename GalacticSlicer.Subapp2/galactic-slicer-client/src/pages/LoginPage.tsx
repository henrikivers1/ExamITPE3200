
import { useState, type FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import { ApiError, fieldErrors } from "../services/api";
import { login } from "../services/authService";
import {
    validateLogin,
    type FormErrors,
    type LoginValues,
} from "../validation/authValidation";

const fields: (keyof LoginValues)[] = ["email", "password"];

function LoginPage() {
    usePageTitle("Log in");

    const navigate = useNavigate();

    const [values, setValues] = useState<LoginValues>({
        email: "",
        password: "",
    });

    const [touched, setTouched] = useState<
        Partial<Record<keyof LoginValues, boolean>>
    >({});

    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(false);
    // "Account created ..." when the player comes here from Sign up
    const location = useLocation();
    const [notice, setNotice] = useState<string | null>(
        (location.state as { message?: string } | null)?.message ?? null
    );

    // errors the API sent back for a field (e.g. "email is already taken")
    const [serverErrors, setServerErrors] =
        useState<FormErrors<LoginValues>>({});

    const errors = validateLogin(values);

    // the client-side message first, otherwise the API's message for the field
    const visibleError = (field: keyof LoginValues) =>
        (touched[field] || submitted ? errors[field] : undefined)
        ?? serverErrors[field];

    function update(field: keyof LoginValues, value: string) {
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
            await login(values.email, values.password);

            navigate("/progress", { replace: true });
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
                setNotice("Login failed. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    }

    return (
        <section className="gs-auth-wrapper">
            <div className="gs-auth-card">
                <h1 className="gs-auth-title">Log in</h1>

                <p className="gs-auth-subtitle">
                    Welcome back, slicer.
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
                        placeholder="Enter your password"
                        autoComplete="current-password"
                    />

                    <button
                        type="submit"
                        className="gs-btn gs-btn--primary gs-btn--lg gs-form-submit"
                        disabled={loading}
                    >
                        {loading ? "Logging in..." : "Log in"}
                    </button>
                </form>

                <p className="gs-form-footer">
                    Don't have an account?{" "}
                    <Link to="/register">Sign up</Link>
                </p>
            </div>
        </section>
    );
}

export default LoginPage;
