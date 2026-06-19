import { type FormEvent, useEffect, useState } from 'react';

export type OrganizerFormValues = {
  readonly firstName: string;
  readonly lastName: string;
  readonly email: string;
};

type OrganizerFormProps = {
  readonly initialValues: OrganizerFormValues;
  readonly isSubmitting: boolean;
  readonly submitLabel: string;
  readonly onSubmit: (values: OrganizerFormValues) => void;
};

export function OrganizerForm({ initialValues, isSubmitting, submitLabel, onSubmit }: OrganizerFormProps) {
  const [firstName, setFirstName] = useState(initialValues.firstName);
  const [lastName, setLastName] = useState(initialValues.lastName);
  const [email, setEmail] = useState(initialValues.email);

  useEffect(() => {
    setFirstName(initialValues.firstName);
    setLastName(initialValues.lastName);
    setEmail(initialValues.email);
  }, [initialValues]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({ firstName, lastName, email });
  }

  return (
    <form className="grid gap-5" onSubmit={handleSubmit}>
      <div className="grid gap-5 md:grid-cols-2">
        <label className="grid gap-2 text-[14px] font-medium">
          First name
          <input
            className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
            required
          />
        </label>

        <label className="grid gap-2 text-[14px] font-medium">
          Last name
          <input
            className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
            required
          />
        </label>
      </div>

      <label className="grid gap-2 text-[14px] font-medium">
        Email
        <input
          className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />
      </label>

      <div className="flex justify-end">
        <button
          type="submit"
          className="rounded-interactive bg-primary px-4 py-2 text-[14px] font-semibold text-white transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-60"
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Saving...' : submitLabel}
        </button>
      </div>
    </form>
  );
}
