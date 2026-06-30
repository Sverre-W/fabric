import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from '@tanstack/react-router';
import { ArrowLeft, Cpu, HeartPulse } from 'lucide-react';
import { useState } from 'react';

import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';

type HardwareAgent = components['schemas']['HardwareAgentResponse'];
type HardwareDevice = components['schemas']['HardwareDeviceResponse'];
type HardwareDeviceHealth = components['schemas']['HardwareDeviceHealthResponse'];

const agentsQueryKey = ['facility', 'hardware-agents'] as const;

export default function HardwareAgentDetailPage() {
  const { agentId } = useParams({ from: '/main/facility/hardware/$agentId' });
  const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);

  const agentQuery = useQuery({
    queryKey: [...agentsQueryKey, agentId],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/hardware/agents/{agentId}', {
        params: { path: { agentId } },
      });

      if (error || !data) {
        throw new Error('Could not load hardware agent.');
      }

      return data;
    },
  });

  const devicesQuery = useQuery({
    queryKey: [...agentsQueryKey, agentId, 'devices'],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/hardware/agents/{agentId}/devices', {
        params: { path: { agentId } },
      });

      if (error || !data) {
        throw new Error('Could not load hardware devices.');
      }

      return data;
    },
  });

  const healthQuery = useQuery({
    queryKey: [...agentsQueryKey, agentId, 'devices', selectedDeviceId, 'health'],
    enabled: selectedDeviceId !== null,
    queryFn: async () => {
      if (!selectedDeviceId) {
        throw new Error('No device selected.');
      }

      const { data, error } = await api.GET('/api/hardware/agents/{agentId}/devices/{deviceId}/health', {
        params: { path: { agentId, deviceId: selectedDeviceId } },
      });

      if (error || !data) {
        throw new Error('Could not load hardware device health.');
      }

      return data;
    },
  });

  const agent = agentQuery.data;
  const devices = devicesQuery.data ?? [];

  return (
    <div className="grid gap-6">
      <Link to="/facility/hardware" className="inline-flex w-fit items-center gap-2 text-[14px] font-medium text-muted-foreground transition hover:text-foreground">
        <ArrowLeft className="size-4" aria-hidden="true" />
        Back to hardware
      </Link>

      <section className="rounded-structural border border-border bg-content">
        <div className="border-b border-border p-4 sm:p-6">
          {agentQuery.isLoading ? <p className="text-[14px] text-muted-foreground">Loading hardware agent...</p> : null}
          {agentQuery.isError ? <p className="text-[14px] text-error">Could not load hardware agent.</p> : null}
          {agent ? <AgentHeader agent={agent} deviceCount={devices.length} /> : null}
        </div>

        <div className="grid gap-6 p-4 sm:p-6">
          <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_24rem]">
            <DeviceInventory
              devices={devices}
              loading={devicesQuery.isLoading}
              error={devicesQuery.isError}
              selectedDeviceId={selectedDeviceId}
              onSelectDevice={setSelectedDeviceId}
            />
            <DeviceHealthPanel health={healthQuery.data} loading={healthQuery.isLoading} error={healthQuery.isError} selectedDeviceId={selectedDeviceId} />
          </div>
        </div>
      </section>
    </div>
  );
}

function AgentHeader({ agent, deviceCount }: { readonly agent: HardwareAgent; readonly deviceCount: number }) {
  return (
    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
      <div>
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="text-[20px] font-semibold tracking-tight">{agent.name}</h1>
          <StatusBadge enabled={agent.enabled} />
        </div>
        <p className="mt-2 text-[14px] text-muted-foreground">{agent.id}</p>
      </div>

      <dl className="grid gap-3 rounded-structural border border-border bg-background p-4 text-[13px] sm:grid-cols-2 lg:min-w-[32rem]">
        <InfoItem label="Location" value={agent.locationId || 'Unassigned'} />
        <InfoItem label="Devices" value={String(deviceCount)} />
        <InfoItem label="Last seen" value={formatDate(agent.lastSeenAt)} />
        <InfoItem label="Inventory" value={formatDate(agent.lastInventoryAt)} />
      </dl>
    </div>
  );
}

function DeviceInventory({
  devices,
  loading,
  error,
  selectedDeviceId,
  onSelectDevice,
}: {
  readonly devices: HardwareDevice[];
  readonly loading: boolean;
  readonly error: boolean;
  readonly selectedDeviceId: string | null;
  readonly onSelectDevice: (deviceId: string) => void;
}) {
  if (!loading && !error && devices.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>No devices reported</EmptyTitle>
          <EmptyDescription>Once agent posts inventory, logical scanners, printers, and readers show here.</EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <div className="grid gap-3">
      <div className="flex items-center gap-2">
        <Cpu className="size-4 text-muted-foreground" aria-hidden="true" />
        <h2 className="text-[16px] font-semibold">Devices</h2>
      </div>

      <div className="grid gap-3 md:hidden">
        {loading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading hardware devices...</p> : null}
        {error ? <p className="rounded-structural border border-border p-4 text-[14px] text-error">Could not load hardware devices.</p> : null}
        {devices.map((device) => (
          <DeviceCard key={device.deviceId} device={device} selected={selectedDeviceId === device.deviceId} onSelect={() => onSelectDevice(device.deviceId)} />
        ))}
      </div>

      <div className="hidden overflow-x-auto rounded-structural border border-border md:block">
        <table className="w-full min-w-[48rem] border-collapse text-left text-[14px]">
          <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
            <tr>
              <th className="px-4 py-3 font-semibold">Device</th>
              <th className="px-4 py-3 font-semibold">Kind</th>
              <th className="px-4 py-3 font-semibold">Capabilities</th>
              <th className="px-4 py-3 font-semibold">State</th>
              <th className="px-4 py-3 font-semibold">Last seen</th>
              <th className="px-4 py-3 text-right font-semibold">Health</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {loading ? (
              <tr>
                <td className="px-4 py-5 text-muted-foreground" colSpan={6}>Loading hardware devices...</td>
              </tr>
            ) : null}
            {error ? (
              <tr>
                <td className="px-4 py-5 text-error" colSpan={6}>Could not load hardware devices.</td>
              </tr>
            ) : null}
            {devices.map((device) => (
              <DeviceRow key={device.deviceId} device={device} selected={selectedDeviceId === device.deviceId} onSelect={() => onSelectDevice(device.deviceId)} />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function DeviceCard({ device, selected, onSelect }: { readonly device: HardwareDevice; readonly selected: boolean; readonly onSelect: () => void }) {
  return (
    <article className="rounded-structural border border-border p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="truncate text-[15px] font-semibold text-foreground">{device.deviceId}</h3>
          <p className="mt-1 text-[13px] text-muted-foreground">{device.kind} via {device.driver}</p>
        </div>
        <StateBadge state={device.state} enabled={device.enabled} />
      </div>
      <div className="mt-3 flex flex-wrap gap-2">
        {device.capabilities.map((capability) => <Badge key={capability} variant="outline">{capability}</Badge>)}
      </div>
      <p className="mt-3 text-[13px] text-muted-foreground">Last seen {formatDate(device.lastSeenAt)}</p>
      <Button type="button" variant={selected ? 'secondary' : 'outline'} size="sm" className="mt-4 w-full" onClick={onSelect}>
        View health
      </Button>
    </article>
  );
}

function DeviceRow({ device, selected, onSelect }: { readonly device: HardwareDevice; readonly selected: boolean; readonly onSelect: () => void }) {
  return (
    <tr className={selected ? 'bg-hover-blue' : undefined}>
      <td className="px-4 py-4">
        <div className="font-medium text-foreground">{device.deviceId}</div>
        <div className="mt-1 text-[13px] text-muted-foreground">{device.driver}</div>
      </td>
      <td className="px-4 py-4 text-muted-foreground">{device.kind}</td>
      <td className="px-4 py-4">
        <div className="flex flex-wrap gap-2">
          {device.capabilities.map((capability) => <Badge key={capability} variant="outline">{capability}</Badge>)}
        </div>
      </td>
      <td className="px-4 py-4"><StateBadge state={device.state} enabled={device.enabled} /></td>
      <td className="px-4 py-4 text-muted-foreground">{formatDate(device.lastSeenAt)}</td>
      <td className="px-4 py-4">
        <div className="flex justify-end">
          <Button type="button" variant={selected ? 'secondary' : 'outline'} size="sm" onClick={onSelect}>View health</Button>
        </div>
      </td>
    </tr>
  );
}

function DeviceHealthPanel({
  health,
  loading,
  error,
  selectedDeviceId,
}: {
  readonly health: HardwareDeviceHealth | undefined;
  readonly loading: boolean;
  readonly error: boolean;
  readonly selectedDeviceId: string | null;
}) {
  return (
    <aside className="rounded-structural border border-border p-4">
      <div className="flex items-center gap-2">
        <HeartPulse className="size-4 text-muted-foreground" aria-hidden="true" />
        <h2 className="text-[16px] font-semibold">Health</h2>
      </div>

      {!selectedDeviceId ? <p className="mt-4 text-[14px] text-muted-foreground">Select a device to inspect health and diagnostics.</p> : null}
      {loading ? <p className="mt-4 text-[14px] text-muted-foreground">Loading health...</p> : null}
      {error ? <p className="mt-4 text-[14px] text-error">Could not load device health.</p> : null}

      {health ? (
        <div className="mt-4 grid gap-4">
          <dl className="grid gap-3 text-[13px]">
            <InfoItem label="Device" value={health.deviceId} />
            <InfoItem label="State" value={health.state} />
            <InfoItem label="Enabled" value={health.enabled ? 'Yes' : 'No'} />
            <InfoItem label="Last seen" value={formatDate(health.lastSeenAt)} />
          </dl>
          <div>
            <h3 className="text-[13px] font-semibold uppercase text-muted-foreground">Diagnostics</h3>
            <pre className="mt-2 max-h-80 overflow-auto rounded-interactive border border-border bg-background p-3 text-[12px] text-foreground">{formatDiagnostics(health.diagnosticsJson)}</pre>
          </div>
        </div>
      ) : null}
    </aside>
  );
}

function InfoItem({ label, value }: { readonly label: string; readonly value: string }) {
  return (
    <div>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="mt-1 break-words font-medium text-foreground">{value}</dd>
    </div>
  );
}

function StatusBadge({ enabled }: { readonly enabled: boolean }) {
  return <Badge variant={enabled ? 'success' : 'secondary'}>{enabled ? 'Enabled' : 'Disabled'}</Badge>;
}

function StateBadge({ state, enabled }: { readonly state: string; readonly enabled: boolean }) {
  if (!enabled) {
    return <Badge variant="secondary">Disabled</Badge>;
  }

  return <Badge variant={state.toLowerCase() === 'ready' ? 'success' : 'warning'}>{state}</Badge>;
}

function formatDate(value: string | null) {
  if (!value) {
    return 'Never';
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function formatDiagnostics(value: string) {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}
