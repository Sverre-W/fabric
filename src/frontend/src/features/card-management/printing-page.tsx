import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Eye, Pencil, Plus, Printer, Trash2 } from 'lucide-react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';
import { Badge } from '@/shared/components/ui/badge';
import { Button, buttonVariants } from '@/shared/components/ui/button';
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';

import { encodersQueryKey, formatDateTime, printingBatchesQueryKey, transformationsQueryKey, type Encoder, type EncodingBatch, type Transformation } from './card-management-types';

export default function PrintingPage() {
  const queryClient = useQueryClient();
  const batchesQuery = useQuery({
    queryKey: printingBatchesQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/desfire/encoding-batches', { params: { query: { Page: 0, PageSize: 100 } } });
      if (error || !data) {
        throw new Error('Could not load print batches.');
      }
      return data;
    },
  });

  const transformationsQuery = useQuery({
    queryKey: transformationsQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/desfire/transformations', { params: { query: { Page: 0, PageSize: 100 } } });
      if (error || !data) {
        throw new Error('Could not load transformations.');
      }
      return data.items ?? [];
    },
  });

  const encodersQuery = useQuery({
    queryKey: encodersQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/desfire/encoders', { params: { query: { Page: 0, PageSize: 100 } } });
      if (error || !data) {
        throw new Error('Could not load encoders.');
      }
      return data;
    },
  });

  const deleteEncoder = useMutation({
    mutationFn: async (id: string) => {
      const { error } = await api.DELETE('/api/desfire/encoders/{id}', { params: { path: { id } } });
      if (error) {
        throw new Error('Could not delete encoder.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: encodersQueryKey });
      toast.success('Encoder deleted.');
    },
    onError: () => toast.error('Could not delete encoder. Print history may reference it.'),
  });

  const batches = batchesQuery.data?.items ?? [];
  const encoders = encodersQuery.data?.items ?? [];
  const transformationById = new Map((transformationsQuery.data ?? []).map((transformation) => [transformation.id, transformation]));

  return (
    <section className="rounded-structural border border-border bg-content">
      <div className="flex flex-col gap-3 border-b border-border p-4 sm:flex-row sm:items-start sm:justify-between sm:p-6">
        <div>
          <h1 className="text-[20px] font-semibold tracking-tight">Printing</h1>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Schedule DESFire card print batches and inspect every card encoding run.</p>
        </div>
        <Link to="/card-management/printing/new" className={buttonVariants({ className: 'w-full sm:w-fit' })}>
          <Plus className="size-4" aria-hidden="true" />
          Schedule print batch
        </Link>
      </div>

      <div className="p-4 sm:p-6">
        <Tabs defaultValue="batches">
          <TabsList>
            <TabsTrigger value="batches">Print batches</TabsTrigger>
            <TabsTrigger value="encoders">Encoders</TabsTrigger>
          </TabsList>
          <TabsContent value="batches">
            {batchesQuery.isError ? <PanelError>Could not load print batches.</PanelError> : null}
            {batchesQuery.isLoading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading print batches...</p> : null}
            {!batchesQuery.isLoading && !batchesQuery.isError && batches.length === 0 ? <Empty><EmptyHeader><EmptyTitle>No print batches yet</EmptyTitle><EmptyDescription>Schedule a print batch from a transformation and CSV rows.</EmptyDescription></EmptyHeader></Empty> : null}
            {batches.length > 0 ? <PrintBatchesTable batches={batches} transformationById={transformationById} /> : null}
          </TabsContent>
          <TabsContent value="encoders">
            <EncodersPanel encoders={encoders} isLoading={encodersQuery.isLoading} isError={encodersQuery.isError} isDeleting={deleteEncoder.isPending} onDelete={(encoder) => {
              if (window.confirm(`Delete encoder "${encoder.name}"?`)) {
                deleteEncoder.mutate(encoder.id);
              }
            }} />
          </TabsContent>
        </Tabs>
      </div>
    </section>
  );
}

function EncodersPanel({ encoders, isLoading, isError, isDeleting, onDelete }: { readonly encoders: Encoder[]; readonly isLoading: boolean; readonly isError: boolean; readonly isDeleting: boolean; readonly onDelete: (encoder: Encoder) => void }) {
  return (
    <div className="grid gap-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-[16px] font-semibold tracking-tight">Encoders</h2>
          <p className="mt-1 text-[14px] text-muted-foreground">Named DESFire encoder bindings used when scheduling print batches.</p>
        </div>
        <Link to="/card-management/printing/encoders/new" className={buttonVariants({ className: 'w-full sm:w-fit' })}><Plus className="size-4" aria-hidden="true" />Add encoder</Link>
      </div>
      {isError ? <PanelError>Could not load encoders.</PanelError> : null}
      {isLoading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading encoders...</p> : null}
      {!isLoading && !isError && encoders.length === 0 ? <Empty><EmptyHeader><EmptyTitle>No encoders yet</EmptyTitle><EmptyDescription>Add an encoder before scheduling print batches.</EmptyDescription></EmptyHeader></Empty> : null}
      {encoders.length > 0 ? <EncodersTable encoders={encoders} isDeleting={isDeleting} onDelete={onDelete} /> : null}
    </div>
  );
}

function EncodersTable({ encoders, isDeleting, onDelete }: { readonly encoders: Encoder[]; readonly isDeleting: boolean; readonly onDelete: (encoder: Encoder) => void }) {
  return (
    <div className="overflow-x-auto rounded-structural border border-border">
      <table className="w-full min-w-[68rem] border-collapse text-left text-[14px]">
        <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground"><tr><th className="px-4 py-3 font-semibold">Name</th><th className="px-4 py-3 font-semibold">Hardware</th><th className="px-4 py-3 font-semibold">State</th><th className="px-4 py-3 font-semibold">Capabilities</th><th className="px-4 py-3 font-semibold">Updated</th><th className="px-4 py-3 text-right font-semibold">Actions</th></tr></thead>
        <tbody className="divide-y divide-border">
          {encoders.map((encoder) => <tr key={encoder.id}><td className="px-4 py-4 font-medium text-foreground">{encoder.name}</td><td className="px-4 py-4 text-muted-foreground">{encoder.agentId} / {encoder.deviceId}</td><td className="px-4 py-4"><Badge variant={encoder.enabled ? 'success' : 'secondary'}>{encoder.enabled ? 'Enabled' : 'Disabled'}</Badge></td><td className="px-4 py-4"><div className="flex flex-wrap gap-2"><Badge variant={encoder.supportsEncoding ? 'success' : 'secondary'}>Encoding</Badge><Badge variant={encoder.supportsPrinting ? 'success' : 'secondary'}>{encoder.supportsPrinting ? 'Printing' : 'No printing'}</Badge></div></td><td className="px-4 py-4 text-muted-foreground">{formatDateTime(encoder.updatedAt)}</td><td className="px-4 py-4"><div className="flex justify-end gap-2"><Link to="/card-management/printing/encoders/$encoderId/edit" params={{ encoderId: encoder.id }} className={buttonVariants({ variant: 'outline', size: 'sm' })}><Pencil className="size-4" aria-hidden="true" />Edit</Link><Button type="button" variant="outline" size="sm" disabled={isDeleting} onClick={() => onDelete(encoder)}><Trash2 className="size-4" aria-hidden="true" />Delete</Button></div></td></tr>)}
        </tbody>
      </table>
    </div>
  );
}

function PrintBatchesTable({ batches, transformationById }: { readonly batches: EncodingBatch[]; readonly transformationById: Map<string, Transformation> }) {
  return (
    <div className="overflow-x-auto rounded-structural border border-border">
      <table className="w-full min-w-[70rem] border-collapse text-left text-[14px]">
        <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
          <tr><th className="px-4 py-3 font-semibold">Name</th><th className="px-4 py-3 font-semibold">Transformation</th><th className="px-4 py-3 font-semibold">Status</th><th className="px-4 py-3 font-semibold">Progress</th><th className="px-4 py-3 font-semibold">Failures</th><th className="px-4 py-3 font-semibold">Created</th><th className="px-4 py-3 text-right font-semibold">Actions</th></tr>
        </thead>
        <tbody className="divide-y divide-border">
          {batches.map((batch) => (
            <tr key={batch.id}>
              <td className="px-4 py-4 font-medium text-foreground"><span className="inline-flex items-center gap-2"><Printer className="size-4 text-primary" aria-hidden="true" />{batch.name}</span></td>
              <td className="px-4 py-4 text-muted-foreground">{transformationById.get(batch.transformationId)?.name ?? batch.transformationId}</td>
              <td className="px-4 py-4"><StatusBadge status={batch.status} /></td>
              <td className="px-4 py-4 text-muted-foreground">{Number(batch.succeededRuns)} / {Number(batch.totalRuns)} printed</td>
              <td className="px-4 py-4 text-muted-foreground">{Number(batch.failedRuns) + Number(batch.cancelledRuns)}</td>
              <td className="px-4 py-4 text-muted-foreground">{formatDateTime(batch.createdAt)}</td>
              <td className="px-4 py-4">
                <div className="flex justify-end gap-2">
                  <Link to="/card-management/printing/$batchId" params={{ batchId: batch.id }} className={buttonVariants({ variant: 'outline', size: 'sm' })}><Eye className="size-4" aria-hidden="true" />View</Link>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function StatusBadge({ status }: { readonly status: string }) {
  const variant = status === 'Completed' || status === 'Succeeded' ? 'success' : status === 'Failed' || status === 'Timeout' || status === 'DeviceUnavailable' ? 'error' : status === 'Running' || status === 'Claimed' ? 'warning' : 'secondary';
  return <Badge variant={variant}>{status}</Badge>;
}

export function JsonDetails({ title, value }: { readonly title: string; readonly value: unknown }) {
  return (
    <details className="rounded-structural border border-border bg-content p-4">
      <summary className="cursor-pointer text-[14px] font-semibold text-foreground">{title}</summary>
      <pre className="mt-3 max-h-[28rem] overflow-auto rounded-interactive bg-hover-gray p-3 text-[12px] text-muted-foreground">{JSON.stringify(value, null, 2)}</pre>
    </details>
  );
}

function PanelError({ children }: { readonly children: React.ReactNode }) {
  return <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">{children}</p>;
}
