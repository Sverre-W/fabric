import { type FormEvent, useEffect, useState } from 'react';

export type RoomFormValues = {
  readonly name: string;
  readonly capacity: string;
  readonly wheelchairAccessible: boolean;
};

type RoomFormProps = {
  readonly initialValues: RoomFormValues;
  readonly isSubmitting: boolean;
  readonly submitLabel: string;
  readonly onSubmit: (values: RoomFormValues) => void;
};

export function RoomForm({ initialValues, isSubmitting, submitLabel, onSubmit }: RoomFormProps) {
  const [name, setName] = useState(initialValues.name);
  const [capacity, setCapacity] = useState(initialValues.capacity);
  const [wheelchairAccessible, setWheelchairAccessible] = useState(initialValues.wheelchairAccessible);

  useEffect(() => {
    setName(initialValues.name);
    setCapacity(initialValues.capacity);
    setWheelchairAccessible(initialValues.wheelchairAccessible);
  }, [initialValues]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit({ name, capacity, wheelchairAccessible });
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
        Capacity
        <input
          className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
          type="number"
          min="0"
          value={capacity}
          onChange={(event) => setCapacity(event.target.value)}
          required
        />
      </label>

      <label className="inline-flex items-center gap-2 text-[14px] font-medium">
        <input
          type="checkbox"
          className="size-4 accent-primary"
          checked={wheelchairAccessible}
          onChange={(event) => setWheelchairAccessible(event.target.checked)}
        />
        Wheelchair accessible
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
