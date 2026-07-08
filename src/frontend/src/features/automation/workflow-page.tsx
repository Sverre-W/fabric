import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useLocation, useNavigate } from '@tanstack/react-router';
import { Eye, MoreHorizontal } from 'lucide-react';
import { useAuth } from 'react-oidc-context';
import { toast } from 'sonner';

import { apiBaseUrl } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Badge, type BadgeVariant } from '@/shared/components/ui/badge';
import { Button, buttonVariants } from '@/shared/components/ui/button';
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/ui/popover';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { cn } from '@/shared/utils/cn';

type WorkflowTab = 'definitions' | 'history';
type WorkflowDefinition = components['schemas']['LinkedWorkflowDefinitionSummary'];
type WorkflowInstance = components['schemas']['WorkflowInstanceSummary'];
type WorkflowDefinitionsResponse = components['schemas']['PagedListResponseOfLinkedWorkflowDefinitionSummary'];
type WorkflowInstancesResponse = components['schemas']['Response'];

const workflowDefinitionsQueryKey = ['automation', 'workflow', 'definitions'] as const;
const workflowHistoryQueryKey = ['automation', 'workflow', 'history'] as const;

export default function WorkflowPage() {
  const auth = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const activeTab = getActiveTab(location.pathname, location.searchStr);

  const definitionsQuery = useQuery({
    queryKey: workflowDefinitionsQueryKey,
    queryFn: () => fetchElsa<WorkflowDefinitionsResponse>('/elsa/api/workflow-definitions', auth.user?.access_token),
  });

  const historyQuery = useQuery({
    queryKey: workflowHistoryQueryKey,
    queryFn: () => fetchElsa<WorkflowInstancesResponse>('/elsa/api/workflow-instances', auth.user?.access_token, { method: 'POST', body: '{}' }),
  });

  function updateTab(tab: string) {
    const nextTab = tab === 'history' ? 'history' : 'definitions';
    void navigate({ to: '/automation/workflow', search: { tab: nextTab } as never });
  }

  return (
    <section className="grid gap-4">
      <div className="rounded-structural border border-border bg-content p-4 sm:p-6">
        <p className="text-[14px] font-semibold uppercase text-primary">Automation</p>
        <h1 className="mt-2 text-[24px] font-semibold tracking-tight">Workflow</h1>
        <p className="mt-2 max-w-3xl text-[14px] text-muted-foreground">Manage workflow definitions and inspect workflow run history from one API-backed view.</p>
      </div>

      <div className="rounded-structural border border-border bg-content p-4 sm:p-6">
        <Tabs value={activeTab} onValueChange={updateTab}>
          <TabsList aria-label="Workflow sections">
            <TabsTrigger value="definitions">Definitions</TabsTrigger>
            <TabsTrigger value="history">History</TabsTrigger>
          </TabsList>

          <TabsContent value="definitions">
            <WorkflowDefinitionsPanel query={definitionsQuery} />
          </TabsContent>

          <TabsContent value="history">
            <WorkflowHistoryPanel query={historyQuery} />
          </TabsContent>
        </Tabs>
      </div>
    </section>
  );
}

function WorkflowDefinitionsPanel({ query }: { readonly query: ReturnType<typeof useQuery<WorkflowDefinitionsResponse>> }) {
  const auth = useAuth();
  const queryClient = useQueryClient();
  const definitions = query.data?.items ?? [];
  const totalCount = Number(query.data?.totalCount ?? definitions.length);

  const publishDefinition = useMutation({
    mutationFn: (definition: WorkflowDefinition) => updateWorkflowDefinition(definition, 'publish', auth.user?.access_token),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: workflowDefinitionsQueryKey });
      toast.success('Workflow definition published.');
    },
    onError: () => toast.error('Could not publish workflow definition.'),
  });

  const retractDefinition = useMutation({
    mutationFn: (definition: WorkflowDefinition) => updateWorkflowDefinition(definition, 'retract', auth.user?.access_token),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: workflowDefinitionsQueryKey });
      toast.success('Workflow definition unpublished.');
    },
    onError: () => toast.error('Could not unpublish workflow definition.'),
  });

  const deleteDefinition = useMutation({
    mutationFn: (definition: WorkflowDefinition) => deleteWorkflowDefinition(definition, auth.user?.access_token),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: workflowDefinitionsQueryKey });
      toast.success('Workflow definition deleted.');
    },
    onError: () => toast.error('Could not delete workflow definition.'),
  });

  function confirmDelete(definition: WorkflowDefinition) {
    const name = definition.name || definition.definitionId || 'this workflow definition';

    if (window.confirm(`Delete "${name}"? This cannot be undone.`)) {
      deleteDefinition.mutate(definition);
    }
  }

  return (
    <div className="grid gap-4">
      <PanelHeader title="Definitions" description="Browse workflow definitions and open the Elsa Studio designer for edits." count={totalCount} />

      {query.isLoading ? <StatusMessage>Loading workflow definitions...</StatusMessage> : null}
      {query.isError ? <StatusMessage tone="error">Could not load workflow definitions.</StatusMessage> : null}
      {!query.isLoading && !query.isError && definitions.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>No workflow definitions</EmptyTitle>
            <EmptyDescription>Create definitions in Elsa before they appear here.</EmptyDescription>
          </EmptyHeader>
        </Empty>
      ) : null}
      {definitions.length > 0 ? (
        <WorkflowDefinitionsTable
          definitions={definitions}
          onPublish={(definition) => publishDefinition.mutate(definition)}
          onRetract={(definition) => retractDefinition.mutate(definition)}
          onDelete={confirmDelete}
          busy={publishDefinition.isPending || retractDefinition.isPending || deleteDefinition.isPending}
        />
      ) : null}
    </div>
  );
}

function WorkflowHistoryPanel({ query }: { readonly query: ReturnType<typeof useQuery<WorkflowInstancesResponse>> }) {
  const instances = query.data?.items ?? [];
  const totalCount = Number(query.data?.totalCount ?? instances.length);

  return (
    <div className="grid gap-4">
      <PanelHeader title="History" description="Inspect workflow runs, statuses, execution history, and active automation work." count={totalCount} />

      {query.isLoading ? <StatusMessage>Loading workflow history...</StatusMessage> : null}
      {query.isError ? <StatusMessage tone="error">Could not load workflow history.</StatusMessage> : null}
      {!query.isLoading && !query.isError && instances.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>No workflow history</EmptyTitle>
            <EmptyDescription>Workflow runs appear here after execution starts.</EmptyDescription>
          </EmptyHeader>
        </Empty>
      ) : null}
      {instances.length > 0 ? <WorkflowHistoryTable instances={instances} /> : null}
    </div>
  );
}

function PanelHeader({ title, description, count }: { readonly title: string; readonly description: string; readonly count: number }) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <h2 className="text-[20px] font-semibold tracking-tight">{title}</h2>
        <p className="mt-1 max-w-2xl text-[14px] text-muted-foreground">{description}</p>
      </div>
      <Badge variant="outline">{count} total</Badge>
    </div>
  );
}

function WorkflowDefinitionsTable({ definitions, onPublish, onRetract, onDelete, busy }: { readonly definitions: readonly WorkflowDefinition[]; readonly onPublish: (definition: WorkflowDefinition) => void; readonly onRetract: (definition: WorkflowDefinition) => void; readonly onDelete: (definition: WorkflowDefinition) => void; readonly busy: boolean }) {
  const navigate = useNavigate();

  function openDefinition(definition: WorkflowDefinition) {
    void navigate({ to: '/automation/workflow-definitions/$definitionId/edit', params: { definitionId: getDefinitionRouteId(definition) } });
  }

  return (
    <div className="overflow-hidden rounded-structural border border-border">
      <div className="hidden grid-cols-[minmax(0,1.6fr)_7rem_7rem_9rem_auto] gap-4 border-b border-border bg-hover-gray px-4 py-3 text-[12px] font-semibold uppercase text-muted-foreground lg:grid">
        <span>Name</span>
        <span>Version</span>
        <span>Status</span>
        <span>Created</span>
        <span className="text-right">Actions</span>
      </div>

      <div className="divide-y divide-border">
        {definitions.map((definition) => (
          <div
            key={definition.definitionId ?? definition.id ?? definition.name}
            role="link"
            tabIndex={0}
            className="grid cursor-pointer gap-4 px-4 py-4 transition hover:bg-hover-gray focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-primary/20 lg:grid-cols-[minmax(0,1.6fr)_7rem_7rem_9rem_auto] lg:items-center"
            onClick={() => openDefinition(definition)}
            onKeyDown={(event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                openDefinition(definition);
              }
            }}
          >
            <div className="min-w-0">
              <p className="truncate text-[14px] font-semibold">{definition.name || 'Untitled workflow'}</p>
              <p className="mt-1 line-clamp-2 text-[13px] text-muted-foreground">{definition.description || definition.definitionId || 'No description'}</p>
            </div>
            <p className="text-[14px] text-muted-foreground">v{definition.version ?? '-'}</p>
            <div><PublicationBadge definition={definition} /></div>
            <p className="text-[14px] text-muted-foreground">{formatDate(definition.createdAt)}</p>
            <WorkflowDefinitionActions definition={definition} busy={busy} onPublish={onPublish} onRetract={onRetract} onDelete={onDelete} />
          </div>
        ))}
      </div>
    </div>
  );
}

function WorkflowDefinitionActions({ definition, busy, onPublish, onRetract, onDelete }: { readonly definition: WorkflowDefinition; readonly busy: boolean; readonly onPublish: (definition: WorkflowDefinition) => void; readonly onRetract: (definition: WorkflowDefinition) => void; readonly onDelete: (definition: WorkflowDefinition) => void }) {
  return (
    <div className="justify-self-start lg:justify-self-end" onClick={(event) => event.stopPropagation()} onKeyDown={(event) => event.stopPropagation()}>
      <Popover>
        <PopoverTrigger render={<Button type="button" variant="ghost" size="icon-sm" aria-label={`Actions for ${definition.name || 'workflow definition'}`} disabled={busy} />}>
          <MoreHorizontal className="size-4" aria-hidden="true" />
        </PopoverTrigger>
        <PopoverContent align="end" className="grid min-w-44 gap-1 p-1">
          {definition.isPublished ? (
            <WorkflowActionButton onClick={() => onRetract(definition)} disabled={busy}>Unpublish</WorkflowActionButton>
          ) : (
            <WorkflowActionButton onClick={() => onPublish(definition)} disabled={busy}>Publish</WorkflowActionButton>
          )}
          <WorkflowActionButton onClick={() => onDelete(definition)} disabled={busy} destructive>Delete</WorkflowActionButton>
        </PopoverContent>
      </Popover>
    </div>
  );
}

function WorkflowActionButton({ destructive, className, ...props }: React.ComponentProps<'button'> & { readonly destructive?: boolean }) {
  return (
    <button
      type="button"
      className={cn(
        'w-full rounded-interactive px-3 py-2 text-left text-[14px] font-medium transition hover:bg-hover-blue disabled:pointer-events-none disabled:opacity-50',
        destructive && 'text-error hover:bg-error-background',
        className,
      )}
      {...props}
    />
  );
}

function WorkflowHistoryTable({ instances }: { readonly instances: readonly WorkflowInstance[] }) {
  return (
    <div className="overflow-hidden rounded-structural border border-border">
      <div className="hidden grid-cols-[minmax(0,1.4fr)_7rem_7rem_9rem_9rem_auto] gap-4 border-b border-border bg-hover-gray px-4 py-3 text-[12px] font-semibold uppercase text-muted-foreground lg:grid">
        <span>Name</span>
        <span>Status</span>
        <span>Incidents</span>
        <span>Started</span>
        <span>Finished</span>
        <span className="text-right">Action</span>
      </div>

      <div className="divide-y divide-border">
        {instances.map((instance) => (
          <div key={instance.id ?? instance.correlationId ?? instance.createdAt} className="grid gap-4 px-4 py-4 lg:grid-cols-[minmax(0,1.4fr)_7rem_7rem_9rem_9rem_auto] lg:items-center">
            <div className="min-w-0">
              <p className="truncate text-[14px] font-semibold">{instance.name || 'Workflow instance'}</p>
              <p className="mt-1 truncate text-[13px] text-muted-foreground">{instance.id || instance.correlationId || instance.definitionId}</p>
            </div>
            <div><InstanceStatusBadge instance={instance} /></div>
            <p className="text-[14px] text-muted-foreground">{instance.incidentCount ?? 0}</p>
            <p className="text-[14px] text-muted-foreground">{formatDate(instance.createdAt)}</p>
            <p className="text-[14px] text-muted-foreground">{formatDate(instance.finishedAt)}</p>
            <Link to="/automation/workflow-instances/$instanceId" params={{ instanceId: instance.id ?? '' }} className={cn(buttonVariants({ variant: 'outline', size: 'sm' }), 'justify-self-start lg:justify-self-end')}>
              <Eye className="size-4" aria-hidden="true" />
              View
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}

function PublicationBadge({ definition }: { readonly definition: WorkflowDefinition }) {
  if (definition.isPublished) {
    return <Badge variant="success">Published</Badge>;
  }

  if (definition.isLatest) {
    return <Badge variant="secondary">Draft</Badge>;
  }

  return <Badge variant="outline">Archived</Badge>;
}

function InstanceStatusBadge({ instance }: { readonly instance: WorkflowInstance }) {
  const status = formatStatus(instance.status, 'Status');
  const variant: BadgeVariant = Number(instance.incidentCount ?? 0) > 0 ? 'error' : status === 'Finished' ? 'success' : 'secondary';

  return <Badge variant={variant}>{status}</Badge>;
}

function StatusMessage({ children, tone = 'muted' }: { readonly children: string; readonly tone?: 'muted' | 'error' }) {
  return <p className={cn('rounded-structural border border-border p-4 text-[14px]', tone === 'error' ? 'text-error' : 'text-muted-foreground')}>{children}</p>;
}

async function fetchElsa<TResponse>(path: string, accessToken: string | undefined, init?: RequestInit): Promise<TResponse> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw new Error(`Elsa API request failed with status ${response.status}.`);
  }

  if (response.status === 204 || response.headers.get('content-length') === '0') {
    return undefined as TResponse;
  }

  return response.json() as Promise<TResponse>;
}

function updateWorkflowDefinition(definition: WorkflowDefinition, action: 'publish' | 'retract', accessToken: string | undefined) {
  return fetchElsa<void>(`/elsa/api/workflow-definitions/${encodeURIComponent(getDefinitionRouteId(definition))}/${action}`, accessToken, { method: 'POST' });
}

function deleteWorkflowDefinition(definition: WorkflowDefinition, accessToken: string | undefined) {
  return fetchElsa<void>(`/elsa/api/workflow-definitions/${encodeURIComponent(getDefinitionRouteId(definition))}`, accessToken, { method: 'DELETE' });
}

function getDefinitionRouteId(definition: WorkflowDefinition) {
  return definition.definitionId ?? definition.id ?? '';
}

function getActiveTab(pathname: string, searchStr: string): WorkflowTab {
  const tab = new URLSearchParams(searchStr).get('tab');

  if (tab === 'history' || pathname.endsWith('/workflow-instances')) {
    return 'history';
  }

  return 'definitions';
}

function formatDate(value: string | null | undefined) {
  if (!value) {
    return '-';
  }

  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
}

function formatStatus(value: number | string | undefined, fallback: string) {
  if (value === undefined || value === null) {
    return fallback;
  }

  const knownStatuses: Record<number, string> = {
    0: 'Idle',
    1: 'Running',
    2: 'Finished',
    3: 'Canceled',
    4: 'Faulted',
  };

  return knownStatuses[Number(value)] ?? String(value);
}
