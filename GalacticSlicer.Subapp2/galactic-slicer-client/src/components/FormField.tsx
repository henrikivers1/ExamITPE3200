import type { ReactNode } from "react";

interface FormFieldProps {
    id: string;
    label: string;
    value: string;
    onChange: (value: string) => void;
    onBlur?: () => void;
    error?: string;
    type?: string;
    placeholder?: string;
    autoComplete?: string;
    // extra content under the input, e.g. the password rules
    children?: ReactNode;
}

// A label, an input and its error message, like a gs-form-group in Subapp 1.
// aria-invalid and aria-describedby make screen readers read the error with the field.
function FormField({ id, label, value, onChange, onBlur, error, type = "text", placeholder, autoComplete, children }: FormFieldProps) {
    const errorId = `${id}-error`;

    return (
        <div className="gs-form-group">
            <label className="gs-label" htmlFor={id}>{label}</label>
            <input
                id={id}
                name={id}
                className="gs-input"
                type={type}
                value={value}
                placeholder={placeholder}
                autoComplete={autoComplete}
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? errorId : undefined}
                onChange={event => onChange(event.target.value)}
                onBlur={onBlur}
            />
            {error && <span id={errorId} className="gs-field-error">{error}</span>}
            {children}
        </div>
    );
}

export default FormField;
