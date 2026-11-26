import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import FormField from "../components/FormField";
import { usePageTitle } from "../hooks/usePageTitle";
import { passwordRules, validateSignUp, type SignUpValues } from "../validation/authValidation";

const fields: (keyof SignUpValues)[] = ["username", "password", "confirmPassword"];

// Sign up form with client-side validation, like Account/SignUp in Subapp 1.
// Not connected to the API yet (no register endpoint), so a valid form only shows a notice.
function RegisterPage() {
    usePageTitle("Sign up");

    const [values, setValues] = useState<SignUpValues>({ username: "", password: "", confirmPassword: "" });
    // a field shows its error after the user has left it once, or after pressing Sign up
    const [touched, setTouched] = useState<Partial<Record<keyof SignUpValues, boolean>>>({});
    const [submitted, setSubmitted] = useState(false);
    const [notice, setNotice] = useState<string | null>(null);

    const errors = validateSignUp(values);
    const visibleError = (field: keyof SignUpValues) =>
        touched[field] || submitted ? errors[field] : undefined;

    function update(field: keyof SignUpValues, value: string) {
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

        // TODO: send to the API's register endpoint once it exists
        setNotice("Signing up isn't available yet: the API doesn't have accounts yet. Nothing was sent.");
    }

    return (
        <section className="gs-auth-wrapper">
            <div className="gs-auth-card">
                <h1 className="gs-auth-title">Sign up</h1>
                <p className="gs-auth-subtitle">Join the fight for a safer galaxy.</p>

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
                        placeholder="Choose a username"
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
                        placeholder="Choose a password"
                        autoComplete="new-password"
                    >
                        {/* checklist that ticks off each rule while typing */}
                        <ul className="gs-password-rules" aria-label="Password rules">
                            {passwordRules.map(rule => {
                                const met = rule.test(values.password);
                                return (
                                    <li key={rule.label} className={met ? "is-met" : ""}>
                                        <span aria-hidden="true">{met ? "✓" : "○"}</span> {rule.label}
                                        <span className="gs-sr-only">{met ? " (done)" : " (missing)"}</span>
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
                        onChange={value => update("confirmPassword", value)}
                        onBlur={() => setTouched(current => ({ ...current, confirmPassword: true }))}
                        error={visibleError("confirmPassword")}
                        placeholder="Repeat your password"
                        autoComplete="new-password"
                    />

                    <button type="submit" className="gs-btn gs-btn--primary gs-btn--lg gs-form-submit">
                        Sign up
                    </button>
                </form>

                <p className="gs-form-footer">
                    Already have an account? <Link to="/login">Log in</Link>
                </p>
            </div>
        </section>
    );
}

export default RegisterPage;
