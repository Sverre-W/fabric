import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';
import { Pencil, Plus, ShieldCheck } from 'lucide-react';
import { useState, type FormEvent } from 'react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/shared/components/ui/empty';

type AccessControlSystem = components['schemas']['AccessControlSystemResponse'];
type CreateAccessControlSystemRequest = components['schemas']['CreateAccessControlSystemRequest'];
type Provider = 'unipass' | 'lenel';

type FormValues = {
  readonly provider: Provider;
  readonly name: string;
  readonly endpoint: string;
  readonly sslValidation: boolean;
  readonly username: string;
  readonly password: string;
  readonly apiKey: string;
};

const systemsQueryKey = ['facility', 'access-control-systems'] as const;
const emptyFormValues: FormValues = {
  provider: 'unipass',
  name: '',
  endpoint: '',
  sslValidation: true,
  username: '',
  password: '',
  apiKey: '',
};

export default function AccessControlPage() {
  const queryClient = useQueryClient();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [values, setValues] = useState<FormValues>(emptyFormValues);

  const systemsQuery = useQuery({
    queryKey: systemsQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/access-policies/access-control-systems', {
        params: { query: { ids: [] } },
      });

      if (error) {
        throw new Error('Could not load access control systems.');
      }

      return data;
    },
  });

  const createSystem = useMutation({
    mutationFn: async (request: CreateAccessControlSystemRequest) => {
      const { data, error } = await api.POST('/api/access-policies/access-control-systems', { body: request });

      if (error || !data) {
        throw new Error('Could not register access control system.');
      }

      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: systemsQueryKey });
      setValues(emptyFormValues);
      setIsFormOpen(false);
      toast.success('Access control system registered.');
    },
    onError: () => {
      toast.error('Could not register access control system.');
    },
  });

  const systems = systemsQuery.data?.items ?? [];
  const totalItems = Number(systemsQuery.data?.totalItems ?? systems.length);

  function updateValue<TKey extends keyof FormValues>(key: TKey, value: FormValues[TKey]) {
    setValues((current) => ({ ...current, [key]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createSystem.mutate(toRequest(values));
  }

  return (
    <section className="rounded-structural border border-border bg-content">
      <div className="border-b border-border p-4 sm:p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h1 className="text-[20px] font-semibold tracking-tight">Access Control</h1>
            <p className="mt-2 max-w-2xl text-[14px] text-muted-foreground">View and register facility access control system integrations.</p>
          </div>
          <button
            type="button"
            className="inline-flex w-full items-center justify-center gap-2 rounded-interactive bg-primary px-4 py-2 text-[14px] font-semibold text-white transition hover:opacity-90 sm:w-fit"
            onClick={() => setIsFormOpen((current) => !current)}
          >
            <Plus className="size-4" aria-hidden="true" />
            Register system
          </button>
        </div>
      </div>

      <div className="grid gap-6 p-4 sm:p-6">
        {isFormOpen ? (
          <form className="grid gap-5 rounded-structural border border-border p-4" onSubmit={handleSubmit}>
            {createSystem.isError ? (
              <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error" role="alert">
                Could not register access control system.
              </p>
            ) : null}

            <div className="grid gap-4 md:grid-cols-2">
              <label className="grid gap-2 text-[14px] font-medium">
                Provider
                <select
                  className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                  value={values.provider}
                  onChange={(event) => updateValue('provider', event.target.value as Provider)}
                >
                  <option value="unipass">Unipass</option>
                  <option value="lenel">Lenel</option>
                </select>
              </label>

              <label className="grid gap-2 text-[14px] font-medium">
                Name
                <input
                  className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                  value={values.name}
                  onChange={(event) => updateValue('name', event.target.value)}
                  required
                />
              </label>

              <label className="grid gap-2 text-[14px] font-medium md:col-span-2">
                Endpoint
                <input
                  className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                  value={values.endpoint}
                  onChange={(event) => updateValue('endpoint', event.target.value)}
                  required
                />
              </label>

              {values.provider === 'unipass' ? (
                <>
                  <label className="grid gap-2 text-[14px] font-medium">
                    Username
                    <input
                      className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                      value={values.username}
                      onChange={(event) => updateValue('username', event.target.value)}
                      required
                    />
                  </label>

                  <label className="grid gap-2 text-[14px] font-medium">
                    Password
                    <input
                      className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                      type="password"
                      value={values.password}
                      onChange={(event) => updateValue('password', event.target.value)}
                      required
                    />
                  </label>
                </>
              ) : (
                <label className="grid gap-2 text-[14px] font-medium md:col-span-2">
                  API key
                  <input
                    className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] outline-none transition focus:border-primary"
                    type="password"
                    value={values.apiKey}
                    onChange={(event) => updateValue('apiKey', event.target.value)}
                    required
                  />
                </label>
              )}
            </div>

            <label className="flex items-center gap-3 text-[14px] font-medium">
              <input type="checkbox" checked={values.sslValidation} onChange={(event) => updateValue('sslValidation', event.target.checked)} />
              Validate SSL certificate
            </label>

            <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
              <button
                type="button"
                className="rounded-interactive border border-border px-4 py-2 text-[14px] font-semibold transition hover:bg-hover-gray"
                onClick={() => setIsFormOpen(false)}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="rounded-interactive bg-primary px-4 py-2 text-[14px] font-semibold text-white transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={createSystem.isPending}
              >
                {createSystem.isPending ? 'Registering...' : 'Register access control system'}
              </button>
            </div>
          </form>
        ) : null}

        {!systemsQuery.isLoading && !systemsQuery.isError && totalItems === 0 ? (
          <Empty>
            <EmptyHeader>
              <EmptyTitle>No access control systems yet</EmptyTitle>
              <EmptyDescription>Register a system before configuring access rules, badge types, or reception assignments.</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <div className="grid gap-3">
            <div className="grid gap-3 md:hidden">
              {systemsQuery.isLoading ? <p className="rounded-structural border border-border p-4 text-[14px] text-muted-foreground">Loading access control systems...</p> : null}
              {systemsQuery.isError ? <p className="rounded-structural border border-border p-4 text-[14px] text-error">Could not load access control systems.</p> : null}
              {systems.map((system) => (
                <SystemCard key={system.id} system={system} />
              ))}
            </div>

            <div className="hidden overflow-x-auto rounded-structural border border-border md:block">
              <table className="w-full min-w-[42rem] border-collapse text-left text-[14px]">
                <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">System</th>
                    <th className="px-4 py-3 font-semibold">Provider</th>
                    <th className="px-4 py-3 font-semibold">Badge types</th>
                    <th className="px-4 py-3 font-semibold">Access levels</th>
                    <th className="px-4 py-3 text-right font-semibold">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {systemsQuery.isLoading ? (
                    <tr>
                      <td className="px-4 py-5 text-muted-foreground" colSpan={5}>
                        Loading access control systems...
                      </td>
                    </tr>
                  ) : null}

                  {systemsQuery.isError ? (
                    <tr>
                      <td className="px-4 py-5 text-error" colSpan={5}>
                        Could not load access control systems.
                      </td>
                    </tr>
                  ) : null}

                  {systems.map((system) => (
                    <SystemRow key={system.id} system={system} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </section>
  );
}

function SystemCard({ system }: { readonly system: AccessControlSystem }) {
  return (
    <article className="rounded-structural border border-border p-4">
      <div className="flex items-start gap-3">
        <div className="flex size-10 shrink-0 items-center justify-center rounded-interactive bg-active-blue text-primary">
          <ShieldCheck className="size-5" aria-hidden="true" />
        </div>
        <div className="min-w-0">
          <h2 className="truncate text-[15px] font-semibold text-foreground">{system.name}</h2>
          <p className="mt-1 text-[14px] text-muted-foreground">{getProviderLabel(system)}</p>
          <p className="mt-3 text-[13px] text-muted-foreground">
            {system.badgeTypes.length} badge types | {system.accessLevels.length} access levels
          </p>
        </div>
        <Link
          to="/facility/access-control/$systemId/edit"
          params={{ systemId: system.id }}
          className="ml-auto inline-flex size-10 shrink-0 items-center justify-center rounded-interactive border border-border text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
          aria-label={`Edit ${system.name}`}
        >
          <Pencil className="size-4" aria-hidden="true" />
        </Link>
      </div>
    </article>
  );
}

function SystemRow({ system }: { readonly system: AccessControlSystem }) {
  return (
    <tr>
      <td className="px-4 py-4 font-medium text-foreground">{system.name}</td>
      <td className="px-4 py-4 text-muted-foreground">{getProviderLabel(system)}</td>
      <td className="px-4 py-4 text-muted-foreground">{system.badgeTypes.length}</td>
      <td className="px-4 py-4 text-muted-foreground">{system.accessLevels.length}</td>
      <td className="px-4 py-4">
        <div className="flex justify-end">
          <Link
            to="/facility/access-control/$systemId/edit"
            params={{ systemId: system.id }}
            className="inline-flex size-9 items-center justify-center rounded-interactive border border-border text-muted-foreground transition hover:bg-hover-blue hover:text-foreground"
            aria-label={`Edit ${system.name}`}
          >
            <Pencil className="size-4" aria-hidden="true" />
          </Link>
        </div>
      </td>
    </tr>
  );
}

function getProviderLabel(system: AccessControlSystem) {
  return system.type === 'lenel' ? 'Lenel' : 'Unipass';
}

function toRequest(values: FormValues): CreateAccessControlSystemRequest {
  if (values.provider === 'lenel') {
    return {
      type: 'lenel',
      name: values.name,
      endpoint: values.endpoint,
      sslValidation: values.sslValidation,
      apiKey: values.apiKey,
    };
  }

  return {
    type: 'unipass',
    name: values.name,
    endpoint: values.endpoint,
    sslValidation: values.sslValidation,
    username: values.username,
    password: values.password,
  };
}
