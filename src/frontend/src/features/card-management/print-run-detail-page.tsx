import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from '@tanstack/react-router';
import { ArrowLeft } from 'lucide-react';

import { api } from '@/shared/api/client';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';

import { encodersQueryKey, formatDateTime, printingRunsQueryKey, transformationsQueryKey } from './card-management-types';
import { JsonDetails, StatusBadge } from './printing-page';

export default function PrintRunDetailPage() {
  const { runId } = useParams({ from: '/main/card-management/printing/runs/$runId' });

  const runQuery = useQuery({
    queryKey: [...printingRunsQueryKey, runId],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/desfire/encoding-runs/{id}', { params: { path: { id: runId } } });
      if (error || !data) {
        throw new Error('Could not load print run.');
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
      return data.items ?? [];
    },
  });

  const run = runQuery.data;
  const transformation = (transformationsQuery.data ?? []).find((item) => item.id === run?.transformationId);
  const encoder = (encodersQuery.data ?? []).find((item) => item.id === run?.encoderId);

  if (runQuery.isLoading) {
    return <p className="rounded-structural border border-border bg-content p-6 text-[14px] text-muted-foreground">Loading print run...</p>;
  }

  if (!run) {
    return <PanelError>Could not load print run.</PanelError>;
  }

  return (
    <section className="grid gap-6">
      <Link to={run.batchId ? '/card-management/printing/$batchId' : '/card-management/printing'} params={run.batchId ? { batchId: run.batchId } : undefined} className="inline-flex w-fit items-center gap-2 text-[14px] font-medium text-muted-foreground transition hover:text-foreground"><ArrowLeft className="size-4" />Back to batch</Link>
      <Card>
        <CardHeader>
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <CardTitle>Print Run</CardTitle>
              <CardDescription>{transformation?.name ?? run.transformationId}</CardDescription>
            </div>
            <StatusBadge status={run.status} />
          </div>
        </CardHeader>
        <CardContent className="grid gap-4">
          {run.errorMessage ? <PanelError>{run.errorMessage}</PanelError> : null}
          <div className="grid gap-3 md:grid-cols-3">
            <Info label="Card UID" value={run.cardUid ?? 'Not read'} />
            <Info label="Encoder" value={encoder?.name ?? run.encoderId ?? 'Unknown'} />
            <Info label="Device" value={run.hardwareAgentId && run.deviceId ? `${run.hardwareAgentId} / ${run.deviceId}` : 'Unassigned'} />
            <Info label="Requested" value={formatDateTime(run.requestedAt)} />
            <Info label="Started" value={run.startedAt ? formatDateTime(run.startedAt) : 'Not started'} />
            <Info label="Completed" value={run.completedAt ? formatDateTime(run.completedAt) : 'Not completed'} />
            <Info label="Kind" value={run.kind} />
          </div>
        </CardContent>
      </Card>

      <JsonDetails title="Input" value={run.input} />
      <JsonDetails title="Resolved variables" value={run.resolvedVariables} />
      <JsonDetails title="Plan summary" value={run.planSummary} />
      <JsonDetails title="Command audit" value={run.commandAudit} />
    </section>
  );
}

function Info({ label, value }: { readonly label: string; readonly value: string }) {
  return <div className="rounded-interactive border border-border p-3"><div className="text-[12px] uppercase text-muted-foreground">{label}</div><div className="mt-1 break-all text-[14px] font-medium text-foreground">{value}</div></div>;
}

function PanelError({ children }: { readonly children: React.ReactNode }) {
  return <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">{children}</p>;
}
