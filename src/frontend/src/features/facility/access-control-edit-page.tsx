import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useParams } from '@tanstack/react-router';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { useEffect, useState, type FormEvent, type ReactNode } from 'react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Button } from '@/shared/components/ui/button';
import { Card } from '@/shared/components/ui/card';
import { Pagination, PaginationContent, PaginationEllipsis, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/shared/components/ui/pagination';

type AccessControlSystem = components['schemas']['AccessControlSystemResponse'];
type SystemMetadata = components['schemas']['SystemMetadata'];
type AccessPolicy = components['schemas']['AccessPolicyResponse'];
type IdentityMapping = components['schemas']['IdentityMappingResponse'];

type ConfigValues = {
  readonly endpoint: string;
  readonly sslValidation: boolean;
  readonly username: string;
  readonly secret: string;
};

const pageSize = 10;
const systemsQueryKey = ['facility', 'access-control-systems'] as const;
const emptyConfig: ConfigValues = { endpoint: '', sslValidation: true, username: '', secret: '' };

export default function AccessControlEditPage() {
  const { systemId } = useParams({ from: '/facility/access-control/$systemId/edit' });
  const queryClient = useQueryClient();
  const [configValues, setConfigValues] = useState<ConfigValues>(emptyConfig);
  const [badgeName, setBadgeName] = useState('');
  const [rangeStart, setRangeStart] = useState('');
  const [rangeStop, setRangeStop] = useState('');
  const [metadataBadgeTypeId, setMetadataBadgeTypeId] = useState('');
  const [accessLevelName, setAccessLevelName] = useState('');
  const [siteId, setSiteId] = useState('');
  const [accessRuleId, setAccessRuleId] = useState('');
  const [metadataAccessLevelId, setMetadataAccessLevelId] = useState('');
  const [lenelBadgeTypeIds, setLenelBadgeTypeIds] = useState<string[]>([]);
  const [policiesPage, setPoliciesPage] = useState(0);
  const [mappingsPage, setMappingsPage] = useState(0);

  const systemQuery = useQuery({
    queryKey: [...systemsQueryKey, systemId],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/access-policies/access-control-systems/{systemId}', {
        params: { path: { systemId } },
      });

      if (error || !data) {
        throw new Error('Could not load access control system.');
      }

      return data;
    },
  });

  const metadataQuery = useQuery({
    queryKey: [...systemsQueryKey, systemId, 'metadata'],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/access-policies/access-control-systems/{systemId}/metadata', {
        params: { path: { systemId } },
      });

      if (error || !data) {
        throw new Error('Could not load provider metadata.');
      }

      return data;
    },
  });

  const policiesQuery = useQuery({
    queryKey: [...systemsQueryKey, systemId, 'policies', policiesPage, pageSize],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/access-policies/policies', {
        params: { query: { SystemId: systemId, ActiveOnly: true, Page: policiesPage, PageSize: pageSize, ids: [] } },
      });

      if (error) {
        throw new Error('Could not load active policies.');
      }

      return data;
    },
  });

  const mappingsQuery = useQuery({
    queryKey: [...systemsQueryKey, systemId, 'identity-mappings', mappingsPage, pageSize],
    queryFn: async () => {
      const { data, error } = await api.GET('/api/access-policies/access-control-systems/{systemId}/identity-mappings', {
        params: { path: { systemId }, query: { Page: mappingsPage, PageSize: pageSize, subjectIds: [] } },
      });

      if (error) {
        throw new Error('Could not load identity mappings.');
      }

      return data;
    },
  });

  const system = systemQuery.data;
  const metadata = metadataQuery.data;

  useEffect(() => {
    if (!system) {
      return;
    }

    setConfigValues({ endpoint: system.endpoint, sslValidation: system.sslValidation, username: system.type === 'unipass' ? system.username : '', secret: '' });
  }, [system]);

  useEffect(() => {
    if (!metadata) {
      return;
    }

    if ('sites' in metadata) {
      setSiteId((current) => current || metadata.sites[0]?.id || '');
      setAccessRuleId((current) => current || metadata.accessRules[0]?.id || '');
      return;
    }

    setMetadataBadgeTypeId((current) => current || metadata.badgeTypes[0]?.id || '');
    setMetadataAccessLevelId((current) => current || metadata.accessLevels[0]?.id || '');
  }, [metadata]);

  const updateConfig = useMutation({
    mutationFn: async () => {
      if (!system) {
        throw new Error('Could not save configuration.');
      }

      if (system.type === 'lenel') {
        const { error } = await api.PUT('/api/access-policies/access-control-systems/{systemId}/lenel/config', {
          params: { path: { systemId } },
          body: { endpoint: configValues.endpoint, sslValidation: configValues.sslValidation, apiKey: configValues.secret || null },
        });

        if (error) {
          throw new Error('Could not save configuration.');
        }

        return;
      }

      const { error } = await api.PUT('/api/access-policies/access-control-systems/{systemId}/unipass/config', {
        params: { path: { systemId } },
        body: { endpoint: configValues.endpoint, sslValidation: configValues.sslValidation, username: configValues.username, password: configValues.secret || null },
      });

      if (error) {
        throw new Error('Could not save configuration.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId] });
      setConfigValues((current) => ({ ...current, secret: '' }));
      toast.success('Configuration saved.');
    },
    onError: () => toast.error('Could not save configuration.'),
  });

  const addBadgeType = useMutation({
    mutationFn: async () => {
      if (system?.type === 'lenel') {
        const { error } = await api.POST('/api/access-policies/access-control-systems/{systemId}/lenel/badge-types', {
          params: { path: { systemId } },
          body: { name: badgeName, badgeTypeId: metadataBadgeTypeId, metadata: metadata as components['schemas']['LenelMetadata'] },
        });

        if (error) {
          throw new Error('Could not add badge type.');
        }

        return;
      }

      const { error } = await api.POST('/api/access-policies/access-control-systems/{systemId}/unipass/badge-types', {
        params: { path: { systemId } },
        body: { name: badgeName, rangeStart, rangeStop },
      });

      if (error) {
        throw new Error('Could not add badge type.');
      }
    },
    onSuccess: async () => {
      setBadgeName('');
      setRangeStart('');
      setRangeStop('');
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId] });
      toast.success('Badge type added.');
    },
    onError: () => toast.error('Could not add badge type.'),
  });

  const addAccessLevel = useMutation({
    mutationFn: async () => {
      if (system?.type === 'lenel') {
        const { error } = await api.POST('/api/access-policies/access-control-systems/{systemId}/lenel/access-level-types', {
          params: { path: { systemId } },
          body: { name: accessLevelName, accessLevelId: metadataAccessLevelId, badgeTypeIds: lenelBadgeTypeIds, metadata: metadata as components['schemas']['LenelMetadata'] },
        });

        if (error) {
          throw new Error('Could not add access rule.');
        }

        return;
      }

      const { error } = await api.POST('/api/access-policies/access-control-systems/{systemId}/unipass/access-level-types', {
        params: { path: { systemId } },
        body: { name: accessLevelName, siteId, accessRuleId, metadata: metadata as components['schemas']['UnipassMetadata'] },
      });

      if (error) {
        throw new Error('Could not add access rule.');
      }
    },
    onSuccess: async () => {
      setAccessLevelName('');
      setLenelBadgeTypeIds([]);
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId] });
      toast.success('Access rule added.');
    },
    onError: () => toast.error('Could not add access rule.'),
  });

  const deleteBadgeType = useMutation({
    mutationFn: async (badgeTypeId: string) => {
      const { error } = await api.DELETE('/api/access-policies/access-control-systems/{systemId}/badge-types/{badgeTypeId}', {
        params: { path: { systemId, badgeTypeId } },
      });

      if (error) {
        throw new Error('Could not delete badge type.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId] });
      toast.success('Badge type deleted.');
    },
    onError: () => toast.error('Could not delete badge type.'),
  });

  const deleteAccessLevel = useMutation({
    mutationFn: async (accessLevelTypeId: string) => {
      const { error } = await api.DELETE('/api/access-policies/access-control-systems/{systemId}/access-level-types/{accessLevelTypeId}', {
        params: { path: { systemId, accessLevelTypeId } },
      });

      if (error) {
        throw new Error('Could not delete access rule.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId] });
      toast.success('Access rule deleted.');
    },
    onError: () => toast.error('Could not delete access rule.'),
  });

  const retractPolicy = useMutation({
    mutationFn: async (policyId: string) => {
      const { error } = await api.POST('/api/access-policies/policies/{policyId}/retract', {
        params: { path: { policyId } },
      });

      if (error) {
        throw new Error('Could not retract policy.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [...systemsQueryKey, systemId, 'policies'] });
      toast.success('Policy retracted.');
    },
    onError: () => toast.error('Could not retract policy.'),
  });

  function handleSaveConfig(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    updateConfig.mutate();
  }

  function handleAddBadgeType(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    addBadgeType.mutate();
  }

  function handleAddAccessLevel(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    addAccessLevel.mutate();
  }

  return (
    <div className="grid gap-6">
      <header className="flex items-start gap-4">
        <Button variant="outline" size="icon" aria-label="Go back" onClick={() => window.history.back()}>
          <ArrowLeft className="size-4" aria-hidden="true" />
        </Button>
        <div>
          <h2 className="text-[20px] font-semibold tracking-tight">{system?.name ?? 'Access control system'}</h2>
          <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">Edit configuration, provider rules, active policies, and identity mappings.</p>
        </div>
      </header>

      {systemQuery.isError ? <PanelError>Could not load access control system.</PanelError> : null}
      {systemQuery.isLoading ? <Card className="p-6 text-[14px] text-muted-foreground">Loading access control system...</Card> : null}

      {system ? (
        <>
          <ConfigurationPanel system={system} values={configValues} isSaving={updateConfig.isPending} setValues={setConfigValues} onSubmit={handleSaveConfig} />
          <RulesPanel
            system={system}
            metadata={metadata}
            metadataError={metadataQuery.isError}
            metadataLoading={metadataQuery.isLoading}
            badgeName={badgeName}
            setBadgeName={setBadgeName}
            rangeStart={rangeStart}
            setRangeStart={setRangeStart}
            rangeStop={rangeStop}
            setRangeStop={setRangeStop}
            metadataBadgeTypeId={metadataBadgeTypeId}
            setMetadataBadgeTypeId={setMetadataBadgeTypeId}
            accessLevelName={accessLevelName}
            setAccessLevelName={setAccessLevelName}
            siteId={siteId}
            setSiteId={setSiteId}
            accessRuleId={accessRuleId}
            setAccessRuleId={setAccessRuleId}
            metadataAccessLevelId={metadataAccessLevelId}
            setMetadataAccessLevelId={setMetadataAccessLevelId}
            lenelBadgeTypeIds={lenelBadgeTypeIds}
            setLenelBadgeTypeIds={setLenelBadgeTypeIds}
            isSavingBadge={addBadgeType.isPending}
            isSavingAccessLevel={addAccessLevel.isPending}
            isDeletingBadge={deleteBadgeType.isPending}
            isDeletingAccessLevel={deleteAccessLevel.isPending}
            onAddBadgeType={handleAddBadgeType}
            onAddAccessLevel={handleAddAccessLevel}
            onDeleteBadgeType={(id) => deleteBadgeType.mutate(id)}
            onDeleteAccessLevel={(id) => deleteAccessLevel.mutate(id)}
          />
          <PoliciesPanel query={policiesQuery} page={policiesPage} setPage={setPoliciesPage} isRetracting={retractPolicy.isPending} onRetract={(id) => retractPolicy.mutate(id)} />
          <IdentityMappingsPanel query={mappingsQuery} page={mappingsPage} setPage={setMappingsPage} />
        </>
      ) : null}
    </div>
  );
}

function ConfigurationPanel({ system, values, isSaving, setValues, onSubmit }: { readonly system: AccessControlSystem; readonly values: ConfigValues; readonly isSaving: boolean; readonly setValues: (values: ConfigValues | ((current: ConfigValues) => ConfigValues)) => void; readonly onSubmit: (event: FormEvent<HTMLFormElement>) => void }) {
  return (
    <Card className="p-6">
      <PanelHeader title="Configuration" description="Update provider connection settings. Leave secret blank to keep current value." />
      <form className="grid gap-4 md:grid-cols-2" onSubmit={onSubmit}>
        <label className="grid gap-2 text-[14px] font-medium md:col-span-2">
          Endpoint
          <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={values.endpoint} onChange={(event) => setValues((current) => ({ ...current, endpoint: event.target.value }))} required />
        </label>
        {system.type === 'unipass' ? (
          <label className="grid gap-2 text-[14px] font-medium">
            Username
            <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={values.username} onChange={(event) => setValues((current) => ({ ...current, username: event.target.value }))} required />
          </label>
        ) : null}
        <label className="grid gap-2 text-[14px] font-medium">
          {system.type === 'lenel' ? 'API key' : 'Password'}
          <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" type="password" value={values.secret} onChange={(event) => setValues((current) => ({ ...current, secret: event.target.value }))} placeholder={system.hasSecret ? 'Configured' : ''} />
        </label>
        <label className="flex items-center gap-3 text-[14px] font-medium md:col-span-2">
          <input type="checkbox" checked={values.sslValidation} onChange={(event) => setValues((current) => ({ ...current, sslValidation: event.target.checked }))} />
          Validate SSL certificate
        </label>
        <div className="flex justify-end md:col-span-2">
          <Button type="submit" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save configuration'}</Button>
        </div>
      </form>
    </Card>
  );
}

function RulesPanel(props: {
  readonly system: AccessControlSystem;
  readonly metadata: SystemMetadata | undefined;
  readonly metadataError: boolean;
  readonly metadataLoading: boolean;
  readonly badgeName: string;
  readonly setBadgeName: (value: string) => void;
  readonly rangeStart: string;
  readonly setRangeStart: (value: string) => void;
  readonly rangeStop: string;
  readonly setRangeStop: (value: string) => void;
  readonly metadataBadgeTypeId: string;
  readonly setMetadataBadgeTypeId: (value: string) => void;
  readonly accessLevelName: string;
  readonly setAccessLevelName: (value: string) => void;
  readonly siteId: string;
  readonly setSiteId: (value: string) => void;
  readonly accessRuleId: string;
  readonly setAccessRuleId: (value: string) => void;
  readonly metadataAccessLevelId: string;
  readonly setMetadataAccessLevelId: (value: string) => void;
  readonly lenelBadgeTypeIds: string[];
  readonly setLenelBadgeTypeIds: (value: string[]) => void;
  readonly isSavingBadge: boolean;
  readonly isSavingAccessLevel: boolean;
  readonly isDeletingBadge: boolean;
  readonly isDeletingAccessLevel: boolean;
  readonly onAddBadgeType: (event: FormEvent<HTMLFormElement>) => void;
  readonly onAddAccessLevel: (event: FormEvent<HTMLFormElement>) => void;
  readonly onDeleteBadgeType: (id: string) => void;
  readonly onDeleteAccessLevel: (id: string) => void;
}) {
  const { system, metadata } = props;

  return (
    <Card className="grid gap-6 p-6">
      <PanelHeader title="Badge Types and Access Rules" description="Use provider metadata to configure available badge types and access rules." />
      {props.metadataLoading ? <p className="text-[14px] text-muted-foreground">Loading provider metadata...</p> : null}
      {props.metadataError ? <PanelError>Could not load provider metadata.</PanelError> : null}

      <div className="grid gap-6 xl:grid-cols-2">
        <section className="grid gap-4">
          <h4 className="text-[15px] font-semibold">Badge types</h4>
          <form className="grid gap-3" onSubmit={props.onAddBadgeType}>
            <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.badgeName} onChange={(event) => props.setBadgeName(event.target.value)} placeholder="Badge type name" required />
            {system.type === 'lenel' && metadata && 'badgeTypes' in metadata ? (
              <select className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.metadataBadgeTypeId} onChange={(event) => props.setMetadataBadgeTypeId(event.target.value)} required>
                {metadata.badgeTypes.map((type) => <option key={type.id} value={type.id}>{type.name}</option>)}
              </select>
            ) : (
              <div className="grid gap-3 sm:grid-cols-2">
                <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.rangeStart} onChange={(event) => props.setRangeStart(event.target.value)} placeholder="Range start" type="number" required />
                <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.rangeStop} onChange={(event) => props.setRangeStop(event.target.value)} placeholder="Range stop" type="number" required />
              </div>
            )}
            <Button type="submit" disabled={props.isSavingBadge || props.metadataLoading}><Plus className="size-4" aria-hidden="true" />Add badge type</Button>
          </form>
          <SimpleList items={system.badgeTypes} emptyLabel="No badge types configured." isDeleting={props.isDeletingBadge} onDelete={props.onDeleteBadgeType} />
        </section>

        <section className="grid gap-4">
          <h4 className="text-[15px] font-semibold">Access rules</h4>
          <form className="grid gap-3" onSubmit={props.onAddAccessLevel}>
            <input className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.accessLevelName} onChange={(event) => props.setAccessLevelName(event.target.value)} placeholder="Access rule name" required />
            {system.type === 'lenel' && metadata && 'accessLevels' in metadata ? (
              <>
                <select className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.metadataAccessLevelId} onChange={(event) => props.setMetadataAccessLevelId(event.target.value)} required>
                  {metadata.accessLevels.map((level) => <option key={level.id} value={level.id}>{level.name}</option>)}
                </select>
                <select className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" multiple value={props.lenelBadgeTypeIds} onChange={(event) => props.setLenelBadgeTypeIds([...event.target.selectedOptions].map((option) => option.value))}>
                  {system.badgeTypes.map((type) => <option key={type.id} value={type.id}>{type.name}</option>)}
                </select>
              </>
            ) : metadata && 'sites' in metadata ? (
              <div className="grid gap-3 sm:grid-cols-2">
                <select aria-label="Site" className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.siteId} onChange={(event) => props.setSiteId(event.target.value)} required>
                  {metadata.sites.map((site) => <option key={site.id} value={site.id}>{site.name}</option>)}
                </select>
                <select aria-label="Access rule" className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary" value={props.accessRuleId} onChange={(event) => props.setAccessRuleId(event.target.value)} required>
                  {metadata.accessRules.map((rule) => <option key={rule.id} value={rule.id}>{rule.name}</option>)}
                </select>
              </div>
            ) : null}
            <Button type="submit" disabled={props.isSavingAccessLevel || props.metadataLoading}><Plus className="size-4" aria-hidden="true" />Add access rule</Button>
          </form>
          <SimpleList items={system.accessLevels} emptyLabel="No access rules configured." isDeleting={props.isDeletingAccessLevel} onDelete={props.onDeleteAccessLevel} />
        </section>
      </div>
    </Card>
  );
}

function PoliciesPanel({ query, page, setPage, isRetracting, onRetract }: { readonly query: ReturnType<typeof useQuery<components['schemas']['PageOfAccessPolicyResponse'] | undefined>>; readonly page: number; readonly setPage: (page: number) => void; readonly isRetracting: boolean; readonly onRetract: (id: string) => void }) {
  const policies = query.data?.items ?? [];
  const pagination = getPaginationState(query.data, policies.length, page);

  return (
    <Card className="grid gap-4 p-6">
      <PanelHeader title="Active Policies" description="Paged active policies for this access control system." />
      {query.isLoading ? <p className="text-[14px] text-muted-foreground">Loading active policies...</p> : null}
      {query.isError ? <PanelError>Could not load active policies.</PanelError> : null}
      {!query.isLoading && !query.isError && policies.length === 0 ? <p className="rounded-structural border border-dashed border-border p-6 text-[14px] text-muted-foreground">No active policies.</p> : null}
      {policies.length > 0 ? <PoliciesTable policies={policies} isRetracting={isRetracting} onRetract={onRetract} /> : null}
      <PagedFooter label="policies" pagination={pagination} isVisible={!query.isLoading && !query.isError && pagination.totalItems > 0} setPage={setPage} />
    </Card>
  );
}

function IdentityMappingsPanel({ query, page, setPage }: { readonly query: ReturnType<typeof useQuery<components['schemas']['PageOfIdentityMappingResponse'] | undefined>>; readonly page: number; readonly setPage: (page: number) => void }) {
  const mappings = query.data?.items ?? [];
  const pagination = getPaginationState(query.data, mappings.length, page);

  return (
    <Card className="grid gap-4 p-6">
      <PanelHeader title="Identity Mappings" description="Paged identities linked to this provider system." />
      {query.isLoading ? <p className="text-[14px] text-muted-foreground">Loading identity mappings...</p> : null}
      {query.isError ? <PanelError>Could not load identity mappings.</PanelError> : null}
      {!query.isLoading && !query.isError && mappings.length === 0 ? <p className="rounded-structural border border-dashed border-border p-6 text-[14px] text-muted-foreground">No identity mappings.</p> : null}
      {mappings.length > 0 ? <MappingsTable mappings={mappings} /> : null}
      <PagedFooter label="identity mappings" pagination={pagination} isVisible={!query.isLoading && !query.isError && pagination.totalItems > 0} setPage={setPage} />
    </Card>
  );
}

function SimpleList({ items, emptyLabel, isDeleting, onDelete }: { readonly items: readonly { readonly id: string; readonly name: string }[]; readonly emptyLabel: string; readonly isDeleting: boolean; readonly onDelete: (id: string) => void }) {
  if (items.length === 0) {
    return <p className="rounded-structural border border-dashed border-border p-4 text-[14px] text-muted-foreground">{emptyLabel}</p>;
  }

  return (
    <div className="divide-y divide-border rounded-structural border border-border">
      {items.map((item) => (
        <div key={item.id} className="flex items-center justify-between gap-3 p-3 text-[14px]">
          <span className="font-medium text-foreground">{item.name}</span>
          <button type="button" className="inline-flex size-9 items-center justify-center rounded-interactive border border-error text-error transition hover:bg-error-background disabled:cursor-not-allowed disabled:opacity-60" aria-label={`Delete ${item.name}`} disabled={isDeleting} onClick={() => onDelete(item.id)}>
            <Trash2 className="size-4" aria-hidden="true" />
          </button>
        </div>
      ))}
    </div>
  );
}

function PoliciesTable({ policies, isRetracting, onRetract }: { readonly policies: AccessPolicy[]; readonly isRetracting: boolean; readonly onRetract: (id: string) => void }) {
  return (
    <div className="overflow-x-auto rounded-structural border border-border">
      <table className="w-full min-w-[56rem] border-collapse text-left text-[14px]">
        <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground"><tr><th className="px-4 py-3 font-semibold">Subject</th><th className="px-4 py-3 font-semibold">Requirement</th><th className="px-4 py-3 font-semibold">Effective</th><th className="px-4 py-3 font-semibold">Status</th><th className="px-4 py-3 text-right font-semibold">Actions</th></tr></thead>
        <tbody className="divide-y divide-border">
          {policies.map((policy) => (
            <tr key={policy.id}>
              <td className="px-4 py-4 font-medium text-foreground">{policy.subject.firstName} {policy.subject.lastName}</td>
              <td className="px-4 py-4 text-muted-foreground">{getRequirementLabel(policy.requirement)}</td>
              <td className="px-4 py-4 text-muted-foreground">{formatDate(policy.effectiveFrom)} - {formatDate(policy.effectiveUntil)}</td>
              <td className="px-4 py-4 text-muted-foreground">{policy.reconciliationFailureReason || policy.reconciliationStatus}</td>
              <td className="px-4 py-4 text-right"><Button type="button" variant="outline" disabled={isRetracting} onClick={() => onRetract(policy.id)}>Retract</Button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function MappingsTable({ mappings }: { readonly mappings: IdentityMapping[] }) {
  return (
    <div className="overflow-x-auto rounded-structural border border-border">
      <table className="w-full min-w-[46rem] border-collapse text-left text-[14px]">
        <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground"><tr><th className="px-4 py-3 font-semibold">Name</th><th className="px-4 py-3 font-semibold">Subject type</th><th className="px-4 py-3 font-semibold">Subject ID</th><th className="px-4 py-3 font-semibold">External ID</th></tr></thead>
        <tbody className="divide-y divide-border">
          {mappings.map((mapping) => (
            <tr key={`${mapping.systemId}-${mapping.subjectId}`}>
              <td className="px-4 py-4 font-medium text-foreground">{mapping.firstName} {mapping.lastName}</td>
              <td className="px-4 py-4 text-muted-foreground">{mapping.subjectType}</td>
              <td className="px-4 py-4 text-muted-foreground">{mapping.subjectId}</td>
              <td className="px-4 py-4 text-muted-foreground">{mapping.externalId}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

type PaginationState = ReturnType<typeof getPaginationState>;

function PagedFooter({ isVisible, label, pagination, setPage }: { readonly isVisible: boolean; readonly label: string; readonly pagination: PaginationState; readonly setPage: (page: number) => void }) {
  if (!isVisible) {
    return null;
  }

  return (
    <div className="flex flex-col gap-3 text-[14px] text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
      <p>Showing {pagination.firstItem}-{pagination.lastItem} of {pagination.totalItems} {label}</p>
      <Pagination className="sm:mx-0 sm:w-auto"><PaginationContent><PaginationItem><PaginationPrevious disabled={pagination.currentPage === 0} onClick={() => setPage(Math.max(0, pagination.currentPage - 1))} /></PaginationItem>{pagination.visiblePages.map((visiblePage, index) => visiblePage === 'ellipsis' ? <PaginationItem key={`${visiblePage}-${index}`}><PaginationEllipsis /></PaginationItem> : <PaginationItem key={visiblePage}><PaginationLink isActive={visiblePage === pagination.currentPage} onClick={() => setPage(visiblePage)}>{visiblePage + 1}</PaginationLink></PaginationItem>)}<PaginationItem><PaginationNext disabled={pagination.currentPage >= pagination.totalPages - 1} onClick={() => setPage(Math.min(pagination.totalPages - 1, pagination.currentPage + 1))} /></PaginationItem></PaginationContent></Pagination>
    </div>
  );
}

function PanelHeader({ title, description }: { readonly title: string; readonly description: string }) {
  return <div><h3 className="text-[18px] font-semibold tracking-tight">{title}</h3><p className="mt-2 text-[14px] text-muted-foreground">{description}</p></div>;
}

function PanelError({ children }: { readonly children: ReactNode }) {
  return <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">{children}</p>;
}

function getRequirementLabel(requirement: AccessPolicy['requirement']) {
  return 'badgeType' in requirement ? `Credential: ${requirement.badgeType.name}` : `Access: ${requirement.accessLevel.name}`;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}

function getPaginationState(page: { readonly currentPage?: number | string; readonly totalPages?: null | number | string; readonly totalItems?: null | number | string } | undefined, itemCount: number, requestedPage: number) {
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
  const sortedPages = [...pages].filter((page) => page >= 0 && page < totalPages).sort((first, second) => first - second);

  return sortedPages.flatMap((pageNumber, index) => {
    const previousPage = sortedPages[index - 1];
    return previousPage !== undefined && pageNumber - previousPage > 1 ? ['ellipsis' as const, pageNumber] : [pageNumber];
  });
}
