import { type FormEvent, useEffect, useState } from 'react';

export type BuildingFormValues = {
  readonly name: string;
  readonly address: string;
};

type BuildingFormProps = {
  readonly initialValues: BuildingFormValues;
  readonly isSubmitting: boolean;
  readonly submitLabel: string;
  readonly onSubmit: (values: BuildingFormValues) => void;
};

export function BuildingForm({ initialValues, isSubmitting, submitLabel, onSubmit }: BuildingFormProps) {
  const [name, setName] = useState(initialValues.name);
  const [address, setAddress] = useState(initialValues.address);

  useEffect(() => {
    setName(initialValues.name);
    setAddress(initialValues.address);
  }, [initialValues]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({ name, address });
  }

  return (
    <form className="grid gap-5" onSubmit={handleSubmit}>
      <label className="grid gap-2 text-[14px] font-medium">
        Name
        <input
          className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
          value={name}
          onChange={(event) => setName(event.target.value)}
          required
        />
      </label>

      <label className="grid gap-2 text-[14px] font-medium">
        Address
        <input
          className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
          value={address}
          onChange={(event) => setAddress(event.target.value)}
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
