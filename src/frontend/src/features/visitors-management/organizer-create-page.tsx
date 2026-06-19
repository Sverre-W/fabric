import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';

import { OrganizerForm, type OrganizerFormValues } from './organizer-form';

const organizersQueryKey = ['visitors-management', 'organizers'] as const;
const emptyOrganizer: OrganizerFormValues = { firstName: '', lastName: '', email: '' };

export default function OrganizerCreatePage() {
  const queryClient = useQueryClient();

  const createOrganizer = useMutation({
    mutationFn: async (values: OrganizerFormValues) => {
      const { error } = await api.POST('/api/visitors/organizers', {
        body: values,
      });

      if (error) {
        throw new Error('Could not create organizer.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: organizersQueryKey });
      toast.success('Organizer created.');
      window.history.back();
    },
  });

  function handleSubmit(values: OrganizerFormValues) {
    createOrganizer.mutate(values);
  }

  return (
    <div className="grid gap-6">
      <header className="flex items-start gap-4">
        <button
          type="button"
          className="inline-flex size-9 shrink-0 items-center justify-center rounded-interactive border border-border bg-content text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
          aria-label="Go back"
          onClick={() => window.history.back()}
        >
          <ArrowLeft className="size-4" aria-hidden="true" />
        </button>

        <div>
          <h2 className="text-[20px] font-semibold tracking-tight">Add organizer</h2>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Create an organizer who can host and coordinate visits.</p>
        </div>
      </header>

      <section className="rounded-structural border border-border bg-content p-6">
        {createOrganizer.isError ? (
          <p className="mb-6 rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">
            Could not create organizer.
          </p>
        ) : null}

        <OrganizerForm initialValues={emptyOrganizer} isSubmitting={createOrganizer.isPending} submitLabel="Create organizer" onSubmit={handleSubmit} />
      </section>
    </div>
  );
}
