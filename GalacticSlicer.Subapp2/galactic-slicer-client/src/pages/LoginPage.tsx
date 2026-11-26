
import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import { apiRequest, ApiError, saveToken } from "../services/api";

type LoginValues = {
    email: string;
    password: string;
};

type LoginResponse = {
    token: string;
};

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
    const [notice, setNotice] = useState<string | null>(null);

    function validate() {
        const errors: Partial<Record<keyof LoginValues, string>> = {};

        if (!values.email.trim()) {
            errors.email = "Email is required.";
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email.trim())) {
            errors.email = "Enter a valid email address.";
        }

        if (!values.password) {
            errors.password = "Password is required.";
        }

        return errors;
    }

    const errors = validate();

    const visibleError = (field: keyof LoginValues) =>
        touched[field] || submitted ? errors[field] : undefined;

    function update(field: keyof LoginValues, value: string) {
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
            const response = await apiRequest<LoginResponse>(
                "/api/auth/login",
                {
                    method: "POST",
                    body: JSON.stringify({
                        email: values.email.trim(),
                        password: values.password,
                    }),
                }
            );

            if (!response.token) {
                throw new Error("The API did not return a token.");
            }

            saveToken(response.token);

            navigate("/progress", { replace: true });
        } catch (error) {
            if (error instanceof ApiError) {
                setNotice(error.message);
            } else {
                setNotice(
                    "Login failed. Please try again."
                );
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
