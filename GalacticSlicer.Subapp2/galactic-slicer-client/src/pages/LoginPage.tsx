import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import { validateLogin, type LoginValues } from "../validation/authValidation";

const fields: (keyof LoginValues)[] = ["username", "password"];

// Log in form with client-side validation, like Account/Login in Subapp 1.
// Not connected to the API yet (no login endpoint), so a valid form only shows a notice.
function LoginPage() {
    usePageTitle("Log in");

    const [values, setValues] = useState<LoginValues>({ username: "", password: "" });
    // a field shows its error after the user has left it once, or after pressing Log in
    const [touched, setTouched] = useState<Partial<Record<keyof LoginValues, boolean>>>({});
    const [submitted, setSubmitted] = useState(false);
    const [notice, setNotice] = useState<string | null>(null);

    const errors = validateLogin(values);
    const visibleError = (field: keyof LoginValues) =>
        touched[field] || submitted ? errors[field] : undefined;

    function update(field: keyof LoginValues, value: string) {
        setValues(current => ({ ...current, [field]: value }));
        setNotice(null);
    }

    function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setSubmitted(true);

        // move focus to the first field with an error, so keyboard users land on it
        const firstInvalid = fields.find(field => errors[field]);
        if (firstInvalid) {
            document.getElementById(firstInvalid)?.focus();
            return;
        }

        // TODO: send to the API's login endpoint once it exists
        setNotice("Logging in isn't available yet: the API doesn't have login yet. Nothing was sent.");
    }

    return (
        <section className="gs-auth-wrapper">
            <div className="gs-auth-card">
                <h1 className="gs-auth-title">Log in</h1>
                <p className="gs-auth-subtitle">Welcome back, slicer.</p>

                {/* role="status": screen readers read the notice when it appears */}
                <div role="status">
                    {notice && <p className="gs-form-notice">{notice}</p>}
                </div>

                {/* noValidate: our own messages instead of the browser's pop-ups */}
                <form className="gs-form" onSubmit={handleSubmit} noValidate>
                    <FormField
                        id="username"
                        label="Username"
                        value={values.username}
                        onChange={value => update("username", value)}
                        onBlur={() => setTouched(current => ({ ...current, username: true }))}
                        error={visibleError("username")}
                        placeholder="Enter your username"
                        autoComplete="username"
                    />

                    <FormField
                        id="password"
                        label="Password"
                        type="password"
                        value={values.password}
                        onChange={value => update("password", value)}
                        onBlur={() => setTouched(current => ({ ...current, password: true }))}
                        error={visibleError("password")}
                        placeholder="Enter your password"
                        autoComplete="current-password"
                    />

                    <button type="submit" className="gs-btn gs-btn--primary gs-btn--lg gs-form-submit">
                        Log in
                    </button>
                </form>

                <p className="gs-form-footer">
                    Don't have an account? <Link to="/register">Sign up</Link>
                </p>
            </div>
        </section>
    );
}

export default LoginPage;
