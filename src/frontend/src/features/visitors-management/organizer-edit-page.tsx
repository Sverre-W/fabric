import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useParams } from '@tanstack/react-router';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';

import { OrganizerForm, type OrganizerFormValues } from './organizer-form';

const organizersQueryKey = ['visitors-management', 'organizers'] as const;
const emptyOrganizer: OrganizerFormValues = { firstName: '', lastName: '', email: '' };

export default function OrganizerEditPage() {
  const { organizerId } = useParams({ from: '/main/visitors-management/organizers/$organizerId/edit' });
  const queryClient = useQueryClient();

  const organizerQuery = useQuery({
    queryKey: [...organizersQueryKey, organizerId],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/visitors/organizers/{organizerId}', {
        params: { path: { organizerId } },
      });

      if (error) {
        throw new Error('Could not load organizer.');
      }

      return data;
    },
  });

  const updateOrganizer = useMutation({
    mutationFn: async (values: OrganizerFormValues) => {
      const { error } = await api.PUT('/api/visitors/organizers/{organizerId}', {
        params: { path: { organizerId } },
        body: values,
      });

      if (error) {
        throw new Error('Could not save organizer.');
      }
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: organizersQueryKey }),
        queryClient.invalidateQueries({ queryKey: [...organizersQueryKey, organizerId] }),
      ]);
      toast.success('Organizer saved.');
    },
  });

  function handleSubmit(values: OrganizerFormValues) {
    updateOrganizer.mutate(values);
  }

  const initialValues: OrganizerFormValues = organizerQuery.data
    ? {
        firstName: organizerQuery.data.firstName ?? '',
        lastName: organizerQuery.data.lastName ?? '',
        email: organizerQuery.data.email ?? '',
      }
    : emptyOrganizer;

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
          <h2 className="text-[20px] font-semibold tracking-tight">Edit organizer</h2>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Update organizer contact details.</p>
        </div>
      </header>

      <section className="rounded-structural border border-border bg-content p-6">
        {organizerQuery.isError || updateOrganizer.isError ? (
          <p className="mb-6 rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">
            {organizerQuery.isError ? 'Could not load organizer.' : 'Could not save organizer.'}
          </p>
        ) : null}

        {organizerQuery.isLoading ? <p className="text-[14px] text-muted-foreground">Loading organizer...</p> : null}

        {!organizerQuery.isLoading && !organizerQuery.isError ? (
          <OrganizerForm initialValues={initialValues} isSubmitting={updateOrganizer.isPending} submitLabel="Save" onSubmit={handleSubmit} />
        ) : null}
      </section>
    </div>
  );
}
