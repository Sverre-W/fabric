import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';

import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';
import { Pagination, PaginationContent, PaginationEllipsis, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/shared/components/ui/pagination';

type Organizer = components['schemas']['OrganizerResponse'];

const organizersQueryKey = ['visitors-management', 'organizers'] as const;
const pageSize = 10;

export default function OrganizersPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);

  const organizersQuery = useQuery({
    queryKey: [...organizersQueryKey, page, pageSize],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/visitors/organizers', {
        params: { query: {} },
      });

      if (error) {
        throw new Error('Could not load organizers.');
      }

      return data;
    },
  });

  const pagedOrganizers = organizersQuery.data;
  const organizers = pagedOrganizers?.items ?? [];
  const totalItems = Number(pagedOrganizers?.totalItems ?? organizers.length);
  const totalPages = Math.max(Number(pagedOrganizers?.totalPages ?? 1), 1);
  const currentPage = Math.min(Number(pagedOrganizers?.currentPage ?? page), totalPages - 1);
  const firstItem = totalItems === 0 ? 0 : currentPage * pageSize + 1;
  const lastItem = Math.min((currentPage + 1) * pageSize, totalItems);
  const visiblePages = getVisiblePages(totalPages, currentPage);

  const deleteOrganizer = useMutation({
    mutationFn: async (organizerId: string) => {
      const { error } = await api.DELETE('/api/visitors/organizers/{organizerId}', {
        params: { path: { organizerId } },
      });

      if (error) {
        throw new Error('Could not delete organizer.');
      }
    },
    onSuccess: async () => {
      if (organizers.length === 1 && page > 0) {
        setPage(page - 1);
      }

      await queryClient.invalidateQueries({ queryKey: organizersQueryKey });
    },
  });

  function handleDelete(organizer: Organizer) {
    if (!organizer.id) {
      return;
    }

    const name = getOrganizerName(organizer);
    const confirmed = window.confirm(`Delete organizer ${name}?`);

    if (confirmed) {
      deleteOrganizer.mutate(organizer.id);
    }
  }

  return (
    <section className="rounded-structural border border-border bg-content">
      <div className="flex flex-col gap-4 border-b border-border p-4 sm:flex-row sm:items-start sm:justify-between sm:p-6">
        <div>
          <h2 className="text-[20px] font-semibold tracking-tight">Organizers</h2>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Manage hosts and organizers responsible for coordinating visits.</p>
        </div>
        <Link
          to="/visitors-management/organizers/new"
          className="inline-flex w-full items-center justify-center gap-2 rounded-interactive bg-primary px-4 py-2 text-[14px] font-semibold text-white transition hover:opacity-90 sm:w-fit"
        >
          <Plus className="size-4" aria-hidden="true" />
          Add organizer
        </Link>
      </div>

      <div className="p-4 sm:p-6">
        {deleteOrganizer.isError ? (
          <p className="mb-6 rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">
            Could not delete organizer.
          </p>
        ) : null}

        {!organizersQuery.isLoading && !organizersQuery.isError && totalItems === 0 ? (
          <Empty>
            <EmptyHeader>
              <EmptyTitle>No organizers yet</EmptyTitle>
              <EmptyDescription>Create an organizer before scheduling visits with hosts.</EmptyDescription>
            </EmptyHeader>
            <EmptyContent>
              <Link
                to="/visitors-management/organizers/new"
                className="inline-flex items-center gap-2 rounded-interactive bg-primary px-4 py-2 text-[14px] font-semibold text-white transition hover:opacity-90"
              >
                <Plus className="size-4" aria-hidden="true" />
                Add organizer
              </Link>
            </EmptyContent>
          </Empty>
        ) : (
          <div className="grid gap-4">
            <div className="grid gap-3 md:hidden">
              {organizersQuery.isLoading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading organizers...</p> : null}
              {organizersQuery.isError ? <p className="rounded-structural border border-border p-4 text-[14px] text-error">Could not load organizers.</p> : null}
              {organizers.map((organizer) => (
                <OrganizerCard key={organizer.id} organizer={organizer} isDeleting={deleteOrganizer.isPending} onDelete={handleDelete} />
              ))}
            </div>
            <div className="hidden overflow-x-auto rounded-structural border border-border md:block">
              <table className="w-full min-w-[40rem] border-collapse text-left text-[14px]">
                <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Name</th>
                    <th className="px-4 py-3 font-semibold">Email</th>
                    <th className="px-4 py-3 text-right font-semibold">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {organizersQuery.isLoading ? (
                    <tr>
                      <td className="px-4 py-5 text-muted-foreground" colSpan={3}>
                        Loading organizers...
                      </td>
                    </tr>
                  ) : null}

                  {organizersQuery.isError ? (
                    <tr>
                      <td className="px-4 py-5 text-error" colSpan={3}>
                        Could not load organizers.
                      </td>
                    </tr>
                  ) : null}

                  {organizers.map((organizer) => (
                    <tr key={organizer.id} className="align-middle">
                      <td className="px-4 py-4 font-medium text-foreground">{getOrganizerName(organizer)}</td>
                      <td className="px-4 py-4 text-muted-foreground">{organizer.email}</td>
                      <td className="px-4 py-4">
                        <div className="flex justify-end gap-2">
                          {organizer.id ? (
                            <Link
                              to="/visitors-management/organizers/$organizerId/edit"
                              params={{ organizerId: organizer.id }}
                              className="inline-flex size-9 items-center justify-center rounded-interactive border border-border text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
                              aria-label={`Edit ${getOrganizerName(organizer)}`}
                            >
                              <Pencil className="size-4" aria-hidden="true" />
                            </Link>
                          ) : null}
                          <button
                            type="button"
                            className="inline-flex size-9 items-center justify-center rounded-interactive border border-error text-error transition hover:bg-error-background disabled:cursor-not-allowed disabled:opacity-60"
                            aria-label={`Delete ${getOrganizerName(organizer)}`}
                            disabled={deleteOrganizer.isPending}
                            onClick={() => handleDelete(organizer)}
                          >
                            <Trash2 className="size-4" aria-hidden="true" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {!organizersQuery.isLoading && !organizersQuery.isError && totalItems > 0 ? (
              <div className="flex flex-col gap-3 text-[14px] text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
                <p>
                  Showing {firstItem}-{lastItem} of {totalItems} organizers
                </p>
                <Pagination className="sm:mx-0 sm:w-auto">
                  <PaginationContent>
                    <PaginationItem>
                      <PaginationPrevious disabled={currentPage === 0} onClick={() => setPage(Math.max(0, currentPage - 1))} />
                    </PaginationItem>

                    {visiblePages.map((visiblePage, index) =>
                      visiblePage === 'ellipsis' ? (
                        <PaginationItem key={`${visiblePage}-${index}`}>
                          <PaginationEllipsis />
                        </PaginationItem>
                      ) : (
                        <PaginationItem key={visiblePage}>
                          <PaginationLink isActive={visiblePage === currentPage} onClick={() => setPage(visiblePage)}>
                            {visiblePage + 1}
                          </PaginationLink>
                        </PaginationItem>
                      ),
                    )}

                    <PaginationItem>
                      <PaginationNext disabled={currentPage >= totalPages - 1} onClick={() => setPage(Math.min(totalPages - 1, currentPage + 1))} />
                    </PaginationItem>
                  </PaginationContent>
                </Pagination>
              </div>
            ) : null}
          </div>
        )}
      </div>
    </section>
  );
}

function OrganizerCard({ organizer, isDeleting, onDelete }: { readonly organizer: Organizer; readonly isDeleting: boolean; readonly onDelete: (organizer: Organizer) => void }) {
  const name = getOrganizerName(organizer);

  return (
    <article className="rounded-structural border border-border p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="truncate text-[15px] font-semibold text-foreground">{name}</h3>
          <p className="mt-1 truncate text-[14px] text-muted-foreground">{organizer.email}</p>
        </div>
        <div className="flex shrink-0 gap-2">
          {organizer.id ? (
            <Link
              to="/visitors-management/organizers/$organizerId/edit"
              params={{ organizerId: organizer.id }}
              className="inline-flex size-10 items-center justify-center rounded-interactive border border-border text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
              aria-label={`Edit ${name}`}
            >
              <Pencil className="size-4" aria-hidden="true" />
            </Link>
          ) : null}
          <button
            type="button"
            className="inline-flex size-10 items-center justify-center rounded-interactive border border-error text-error transition hover:bg-error-background disabled:cursor-not-allowed disabled:opacity-60"
            aria-label={`Delete ${name}`}
            disabled={isDeleting}
            onClick={() => onDelete(organizer)}
          >
            <Trash2 className="size-4" aria-hidden="true" />
          </button>
        </div>
      </div>
    </article>
  );
}

function getOrganizerName(organizer: Organizer) {
  return [organizer.firstName, organizer.lastName].filter(Boolean).join(' ') || 'Unnamed organizer';
}

function getVisiblePages(totalPages: number, currentPage: number) {
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, index) => index);
  }

  const pages = new Set([0, totalPages - 1, currentPage - 1, currentPage, currentPage + 1]);
  const sortedPages = [...pages]
    .filter((pageNumber) => pageNumber >= 0 && pageNumber < totalPages)
    .sort((first, second) => first - second);

  return sortedPages.flatMap((pageNumber, index) => {
    const previousPage = sortedPages[index - 1];

    if (previousPage !== undefined && pageNumber - previousPage > 1) {
      return ['ellipsis' as const, pageNumber];
    }

    return [pageNumber];
  });
}
