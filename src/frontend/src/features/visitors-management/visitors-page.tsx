import { useQuery } from '@tanstack/react-query';
import { Search, X } from 'lucide-react';
import { type FormEvent, useState } from 'react';

import { api } from '@/shared/api/client';
import type { components, paths } from '@/shared/api/generated/schema';
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';
import { Pagination, PaginationContent, PaginationEllipsis, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/shared/components/ui/pagination';

type Visitor = components['schemas']['VisitorResponse'];
type VisitorsQuery = NonNullable<paths['/api/visitors/visitors']['get']['parameters']['query']> & {
  readonly page: number;
  readonly pageSize: number;
};

const pageSize = 10;

export default function VisitorsPage() {
  const [page, setPage] = useState(0);
  const [searchInput, setSearchInput] = useState('');
  const [query, setQuery] = useState('');

  const visitorsQuery = useQuery({
    queryKey: ['visitors-management', 'visitors', page, pageSize, query],
    queryFn: async () => {
      const paramsQuery: VisitorsQuery = {
        Query: query || undefined,
        ids: [],
        page,
        pageSize,
      };

      const { data, error } = await api.GET('/api/visitors/visitors', {
        params: { query: paramsQuery },
      });

      if (error) {
        throw new Error('Could not load visitors.');
      }

      return data;
    },
  });

  const pagedVisitors = visitorsQuery.data;
  const visitors = pagedVisitors?.items ?? [];
  const pagination = getPaginationState(pagedVisitors, visitors.length, page);
  const hasSearch = query.trim().length > 0;

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(0);
    setQuery(searchInput.trim());
  }

  function clearSearch() {
    setPage(0);
    setSearchInput('');
    setQuery('');
  }

  return (
    <section className="rounded-structural border border-border bg-content">
      <div className="border-b border-border p-4 sm:p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <h2 className="text-[20px] font-semibold tracking-tight">Visitors</h2>
            <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Maintain visitor profiles, contact details, visit history, and credential associations.</p>
          </div>

          <form className="flex w-full flex-col gap-2 sm:flex-row lg:max-w-xl" onSubmit={handleSearch}>
            <label className="sr-only" htmlFor="visitor-search">
              Search visitors
            </label>
            <div className="relative flex-1">
              <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
              <input
                id="visitor-search"
                type="search"
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                placeholder="Search by name, email, or company"
                className="h-10 w-full rounded-interactive border border-border bg-content pl-9 pr-3 text-[14px] text-foreground outline-none transition placeholder:text-muted-foreground focus:border-primary"
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="inline-flex h-10 flex-1 items-center justify-center rounded-interactive bg-primary px-4 text-[14px] font-semibold text-white transition hover:opacity-90 sm:flex-none">
                Search
              </button>
              {hasSearch ? (
                <button
                  type="button"
                  className="inline-flex h-10 items-center justify-center gap-2 rounded-interactive border border-border px-3 text-[14px] font-semibold text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
                  onClick={clearSearch}
                >
                  <X className="size-4" aria-hidden="true" />
                  Clear
                </button>
              ) : null}
            </div>
          </form>
        </div>
      </div>

      <div className="p-4 sm:p-6">
        {visitorsQuery.isError ? (
          <p className="mb-6 rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">
            Could not load visitors.
          </p>
        ) : null}

        {!visitorsQuery.isLoading && !visitorsQuery.isError && pagination.totalItems === 0 ? (
          <Empty>
            <EmptyHeader>
              <EmptyTitle>{hasSearch ? 'No matching visitors' : 'No visitors yet'}</EmptyTitle>
              <EmptyDescription>{hasSearch ? `No visitor matches "${query}". Try a different name, email, or company.` : 'Visitors appear here after they are invited or confirmed for visits.'}</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <div className="grid gap-4">
            <div className="grid gap-3 md:hidden">
              {visitorsQuery.isLoading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading visitors...</p> : null}
              {visitorsQuery.isError ? <p className="rounded-structural border border-border p-4 text-[14px] text-error">Could not load visitors.</p> : null}
              {visitors.map((visitor) => (
                <VisitorCard key={visitor.id} visitor={visitor} />
              ))}
            </div>

            <div className="hidden overflow-x-auto rounded-structural border border-border md:block">
              <table className="w-full min-w-[46rem] border-collapse text-left text-[14px]">
                <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Name</th>
                    <th className="px-4 py-3 font-semibold">Email</th>
                    <th className="px-4 py-3 font-semibold">Company</th>
                    <th className="px-4 py-3 font-semibold">License plate</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {visitorsQuery.isLoading ? (
                    <tr>
                      <td className="px-4 py-5 text-muted-foreground" colSpan={4}>
                        Loading visitors...
                      </td>
                    </tr>
                  ) : null}

                  {visitorsQuery.isError ? (
                    <tr>
                      <td className="px-4 py-5 text-error" colSpan={4}>
                        Could not load visitors.
                      </td>
                    </tr>
                  ) : null}

                  {visitors.map((visitor) => (
                    <tr key={visitor.id} className="align-middle">
                      <td className="px-4 py-4 font-medium text-foreground">{getVisitorName(visitor)}</td>
                      <td className="px-4 py-4 text-muted-foreground">{visitor.email}</td>
                      <td className="px-4 py-4 text-muted-foreground">{formatOptional(visitor.company)}</td>
                      <td className="px-4 py-4 text-muted-foreground">{formatOptional(visitor.licensePlate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {!visitorsQuery.isLoading && !visitorsQuery.isError && pagination.totalItems > 0 ? (
              <VisitorPagination pagination={pagination} setPage={setPage} />
            ) : null}
          </div>
        )}
      </div>
    </section>
  );
}

function VisitorCard({ visitor }: { readonly visitor: Visitor }) {
  return (
    <article className="rounded-structural border border-border p-4">
      <h3 className="truncate text-[15px] font-semibold text-foreground">{getVisitorName(visitor)}</h3>
      <p className="mt-1 truncate text-[14px] text-muted-foreground">{visitor.email}</p>
      <dl className="mt-4 grid gap-3 text-[14px] sm:grid-cols-2">
        <div>
          <dt className="text-[12px] font-medium uppercase text-muted-foreground">Company</dt>
          <dd className="mt-1 text-foreground">{formatOptional(visitor.company)}</dd>
        </div>
        <div>
          <dt className="text-[12px] font-medium uppercase text-muted-foreground">License plate</dt>
          <dd className="mt-1 text-foreground">{formatOptional(visitor.licensePlate)}</dd>
        </div>
      </dl>
    </article>
  );
}

function VisitorPagination({ pagination, setPage }: { readonly pagination: PaginationState; readonly setPage: (page: number) => void }) {
  return (
    <div className="flex flex-col gap-3 text-[14px] text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
      <p>
        Showing {pagination.firstItem}-{pagination.lastItem} of {pagination.totalItems} visitors
      </p>
      <Pagination className="sm:mx-0 sm:w-auto">
        <PaginationContent>
          <PaginationItem>
            <PaginationPrevious disabled={pagination.currentPage === 0} onClick={() => setPage(Math.max(0, pagination.currentPage - 1))} />
          </PaginationItem>

          {pagination.visiblePages.map((visiblePage, index) =>
            visiblePage === 'ellipsis' ? (
              <PaginationItem key={`${visiblePage}-${index}`}>
                <PaginationEllipsis />
              </PaginationItem>
            ) : (
              <PaginationItem key={visiblePage}>
                <PaginationLink isActive={visiblePage === pagination.currentPage} onClick={() => setPage(visiblePage)}>
                  {visiblePage + 1}
                </PaginationLink>
              </PaginationItem>
            ),
          )}

          <PaginationItem>
            <PaginationNext disabled={pagination.currentPage >= pagination.totalPages - 1} onClick={() => setPage(Math.min(pagination.totalPages - 1, pagination.currentPage + 1))} />
          </PaginationItem>
        </PaginationContent>
      </Pagination>
    </div>
  );
}

type PaginationState = ReturnType<typeof getPaginationState>;

function getPaginationState(page: components['schemas']['PageOfVisitorResponse'] | undefined, itemCount: number, requestedPage: number) {
  const totalItems = Number(page?.totalItems ?? itemCount);
  const totalPages = Math.max(Number(page?.totalPages ?? 1), 1);
  const currentPage = Math.min(Number(page?.currentPage ?? requestedPage), totalPages - 1);
  const firstItem = totalItems === 0 ? 0 : currentPage * pageSize + 1;
  const lastItem = Math.min((currentPage + 1) * pageSize, totalItems);
  const visiblePages = getVisiblePages(totalPages, currentPage);

  return { currentPage, firstItem, lastItem, totalItems, totalPages, visiblePages };
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

function getVisitorName(visitor: Visitor) {
  return [visitor.firstName, visitor.lastName].filter(Boolean).join(' ') || 'Unnamed visitor';
}

function formatOptional(value: string | null | undefined) {
  return value?.trim() || '-';
}
