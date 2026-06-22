import { useEffect, useId, useState, type FormEvent, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Pencil, Plus, ShieldCheck, Trash2 } from 'lucide-react';
import { toast } from 'sonner';

import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';

type AccessControlSystem = components['schemas']['AccessControlSystemResponse'];
type AccessLevelType = components['schemas']['AccessLevelTypeResponse'];
type AccessRuleAssignment = components['schemas']['AccessRuleAssignmentResponse'];
type AccessRuleAssignmentRequest = components['schemas']['CreateAccessRuleAssignmentRequest'];
type ReceptionAccessPolicyTrigger = components['schemas']['ReceptionAccessPolicyTrigger'];
type Site = components['schemas']['SiteResponse'];

type FormValues = {
  readonly locationId: string;
  readonly systemId: string;
  readonly accessLevelTypeId: string;
  readonly trigger: ReceptionAccessPolicyTrigger;
  readonly gracePeriodMinutes: string;
};

const assignmentsQueryKey = ['settings', 'reception-desk', 'access-rule-assignments'] as const;
const systemsQueryKey = ['settings', 'reception-desk', 'access-control-systems'] as const;
const sitesQueryKey = ['settings', 'reception-desk', 'sites'] as const;
const pageSize = 100;

const triggerOptions: { readonly label: string; readonly value: ReceptionAccessPolicyTrigger }[] = [
  { label: 'Expected visitor added', value: 'ExpectedVisitorAdded' },
  { label: 'Visitor confirmed', value: 'VisitorConfirmed' },
  { label: 'Visitor onboarded', value: 'VisitorOnboarded' },
  { label: 'Contractor expected added', value: 'ContractorExpectedAdded' },
  { label: 'Contractor onboarded', value: 'ContractorOnboarded' },
];

export default function ReceptionDeskSettingsPage() {
  const queryClient = useQueryClient();
  const [editingAssignmentId, setEditingAssignmentId] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [values, setValues] = useState<FormValues>(getDefaultFormValues);

  const assignmentsQuery = useQuery({
    queryKey: assignmentsQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/reception/access-rule-assignments', {
        params: { query: { Page: 0, PageSize: pageSize } },
      });

      if (error) {
        throw new Error('Could not load access level assignments.');
      }

      return data;
    },
  });

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

  const sitesQuery = useQuery({
    queryKey: sitesQueryKey,
    queryFn: async () => {
      const { data, error } = await api.GET('/api/locations/sites');

      if (error) {
        throw new Error('Could not load locations.');
      }

      return data;
    },
  });

  const assignments = assignmentsQuery.data?.items ?? [];
  const systems = systemsQuery.data?.items ?? [];
  const sites = sitesQuery.data?.items ?? [];
  const selectedSystem = systems.find((system) => system.id === values.systemId) ?? null;
  const accessLevels = selectedSystem?.accessLevels ?? [];
  const isLoading = assignmentsQuery.isLoading || systemsQuery.isLoading || sitesQuery.isLoading;
  const isError = assignmentsQuery.isError || systemsQuery.isError || sitesQuery.isError;

  useEffect(() => {
    setValues((current) => {
      const locationId = current.locationId || sites[0]?.id || '';
      const systemId = current.systemId || systems[0]?.id || '';
      const system = systems.find((item) => item.id === systemId);
      const accessLevelTypeId = current.accessLevelTypeId || system?.accessLevels[0]?.id || '';

      if (locationId === current.locationId && systemId === current.systemId && accessLevelTypeId === current.accessLevelTypeId) {
        return current;
      }

      return { ...current, locationId, systemId, accessLevelTypeId };
    });
  }, [sites, systems]);

  const createAssignment = useMutation({
    mutationFn: async (request: AccessRuleAssignmentRequest) => {
      const { error } = await api.POST('/api/reception/access-rule-assignments', { body: request });

      if (error) {
        throw new Error('Could not create access level assignment.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: assignmentsQueryKey });
      resetForm();
      toast.success('Access level assignment created.');
    },
    onError: () => {
      toast.error('Could not create access level assignment.');
    },
  });

  const updateAssignment = useMutation({
    mutationFn: async ({ id, request }: { readonly id: string; readonly request: AccessRuleAssignmentRequest }) => {
      const { error } = await api.PUT('/api/reception/access-rule-assignments/{id}', {
        params: { path: { id } },
        body: request,
      });

      if (error) {
        throw new Error('Could not update access level assignment.');
      }
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: assignmentsQueryKey });
      resetForm();
      toast.success('Access level assignment saved.');
    },
    onError: () => {
      toast.error('Could not save access level assignment.');
    },
  });

  const deleteAssignment = useMutation({
    mutationFn: async (assignment: AccessRuleAssignment) => {
      const { error } = await api.DELETE('/api/reception/access-rule-assignments/{id}', {
        params: { path: { id: assignment.id } },
      });

      if (error) {
        throw new Error('Could not delete access level assignment.');
      }
    },
    onSuccess: async (_data, assignment) => {
      await queryClient.invalidateQueries({ queryKey: assignmentsQueryKey });
      if (editingAssignmentId === assignment.id) {
        resetForm();
      }
      toast.success('Access level assignment deleted.');
    },
    onError: () => {
      toast.error('Could not delete access level assignment.');
    },
  });

  function resetForm() {
    const system = systems[0];
    setEditingAssignmentId(null);
    setIsFormOpen(false);
    setValues({
      locationId: sites[0]?.id ?? '',
      systemId: system?.id ?? '',
      accessLevelTypeId: system?.accessLevels[0]?.id ?? '',
      trigger: 'ExpectedVisitorAdded',
      gracePeriodMinutes: '0',
    });
  }

  function openCreateForm() {
    const system = systems[0];
    setEditingAssignmentId(null);
    setIsFormOpen(true);
    setValues({
      locationId: sites[0]?.id ?? '',
      systemId: system?.id ?? '',
      accessLevelTypeId: system?.accessLevels[0]?.id ?? '',
      trigger: 'ExpectedVisitorAdded',
      gracePeriodMinutes: '0',
    });
  }

  function handleSystemChange(systemId: string) {
    const system = systems.find((item) => item.id === systemId);
    setValues((current) => ({ ...current, systemId, accessLevelTypeId: system?.accessLevels[0]?.id ?? '' }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const request = toRequest(values);

    if (!request) {
      toast.error('Complete access level assignment before saving.');
      return;
    }

    if (editingAssignmentId) {
      updateAssignment.mutate({ id: editingAssignmentId, request });
      return;
    }

    createAssignment.mutate(request);
  }

  function editAssignment(assignment: AccessRuleAssignment) {
    setEditingAssignmentId(assignment.id);
    setIsFormOpen(true);
    setValues({
      locationId: assignment.locationId,
      systemId: assignment.systemId,
      accessLevelTypeId: assignment.accessLevelTypeId,
      trigger: assignment.trigger,
      gracePeriodMinutes: String(assignment.gracePeriodMinutes),
    });
  }

  function confirmDelete(assignment: AccessRuleAssignment) {
    const location = getSiteName(sites, assignment.locationId);
    const confirmed = window.confirm(`Delete assignment for ${location}?`);

    if (confirmed) {
      deleteAssignment.mutate(assignment);
    }
  }

  return (
    <section className="grid gap-6">
      <div className="rounded-structural border border-border bg-content p-4 sm:p-6">
        <p className="text-[13px] font-semibold uppercase tracking-[0.18em] text-muted-foreground">Settings</p>
        <h1 className="mt-3 text-[32px] font-semibold tracking-tight">Reception Desk</h1>
        <p className="mt-3 max-w-2xl text-[14px] text-muted-foreground">Configure reception desk access automation and arrival policy defaults.</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <CardTitle className="flex items-center gap-2 text-[20px]">
                <span className="rounded-interactive bg-hover-blue p-2 text-primary">
                  <ShieldCheck className="size-5" aria-hidden="true" />
                </span>
                Access Level Assignments
              </CardTitle>
              <CardDescription className="mt-2 max-w-3xl">
                Assign access levels when reception desk events occur. Locations currently use configured sites.
              </CardDescription>
            </div>
            <Button type="button" className="w-full sm:w-auto" disabled={isLoading || isError} onClick={openCreateForm}>
              <Plus className="size-4" aria-hidden="true" />
              Create Assignment
            </Button>
          </div>
        </CardHeader>
        <CardContent className="grid gap-6">
          {isLoading ? <p className="text-[14px] text-muted-foreground">Loading reception desk settings...</p> : null}
          {isError ? <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error">Could not load reception desk settings.</p> : null}

          {!isLoading && !isError ? (
            <>
              {isFormOpen ? (
                <form className="grid gap-5 rounded-structural border border-border bg-background p-4" onSubmit={handleSubmit}>
                <div>
                  <h2 className="text-[16px] font-semibold">{editingAssignmentId ? 'Edit assignment' : 'New assignment'}</h2>
                  <p className="mt-1 text-[13px] text-muted-foreground">Select a site, access system, access level, and trigger.</p>
                </div>

                <div className="grid gap-4 lg:grid-cols-2">
                  <SelectField label="Location" value={values.locationId} onChange={(value) => setValues((current) => ({ ...current, locationId: value }))}>
                    <option value="" disabled>Select location</option>
                    {sites.map((site) => (
                      <option key={site.id} value={site.id}>{site.name}</option>
                    ))}
                  </SelectField>

                  <SelectField label="Access control system" value={values.systemId} onChange={handleSystemChange}>
                    <option value="" disabled>Select system</option>
                    {systems.map((system) => (
                      <option key={system.id} value={system.id}>{system.name}</option>
                    ))}
                  </SelectField>

                  <SelectField label="Access level" value={values.accessLevelTypeId} onChange={(value) => setValues((current) => ({ ...current, accessLevelTypeId: value }))}>
                    <option value="" disabled>Select access level</option>
                    {accessLevels.map((accessLevel) => (
                      <option key={accessLevel.id} value={accessLevel.id}>{accessLevel.name}</option>
                    ))}
                  </SelectField>

                  <SelectField label="Trigger" value={values.trigger} onChange={(value) => setValues((current) => ({ ...current, trigger: value as ReceptionAccessPolicyTrigger }))}>
                    {triggerOptions.map((trigger) => (
                      <option key={trigger.value} value={trigger.value}>{trigger.label}</option>
                    ))}
                  </SelectField>

                  <Field label="Grace period minutes">
                    <Input
                      type="number"
                      min="0"
                      step="1"
                      value={values.gracePeriodMinutes}
                      onChange={(event) => setValues((current) => ({ ...current, gracePeriodMinutes: event.target.value }))}
                      required
                    />
                  </Field>
                </div>

                <div className="flex flex-col gap-2 border-t border-border pt-5 sm:flex-row sm:justify-end">
                  {editingAssignmentId ? (
                    <Button type="button" variant="outline" onClick={resetForm}>Cancel edit</Button>
                  ) : null}
                  <Button type="submit" disabled={createAssignment.isPending || updateAssignment.isPending || sites.length === 0 || systems.length === 0 || accessLevels.length === 0}>
                    {createAssignment.isPending || updateAssignment.isPending ? 'Saving...' : 'Save assignment'}
                  </Button>
                </div>
                </form>
              ) : null}

              <AccessAssignmentsTable
                assignments={assignments}
                deleteAssignmentId={deleteAssignment.isPending ? deleteAssignment.variables?.id : null}
                editingAssignmentId={editingAssignmentId}
                sites={sites}
                systems={systems}
                onDelete={confirmDelete}
                onEdit={editAssignment}
              />
            </>
          ) : null}
        </CardContent>
      </Card>
    </section>
  );
}

function AccessAssignmentsTable({
  assignments,
  deleteAssignmentId,
  editingAssignmentId,
  sites,
  systems,
  onDelete,
  onEdit,
}: {
  readonly assignments: readonly AccessRuleAssignment[];
  readonly deleteAssignmentId: string | null;
  readonly editingAssignmentId: string | null;
  readonly sites: readonly Site[];
  readonly systems: readonly AccessControlSystem[];
  readonly onDelete: (assignment: AccessRuleAssignment) => void;
  readonly onEdit: (assignment: AccessRuleAssignment) => void;
}) {
  if (assignments.length === 0) {
    return <p className="rounded-structural border border-border bg-background p-4 text-[14px] text-muted-foreground">No access level assignments configured.</p>;
  }

  return (
    <div className="overflow-x-auto rounded-structural border border-border">
      <table className="w-full min-w-[56rem] border-collapse text-left text-[14px]">
        <thead className="bg-hover-gray text-[12px] uppercase text-muted-foreground">
          <tr>
            <th className="px-4 py-3 font-semibold">Location</th>
            <th className="px-4 py-3 font-semibold">System</th>
            <th className="px-4 py-3 font-semibold">Access level</th>
            <th className="px-4 py-3 font-semibold">Trigger</th>
            <th className="px-4 py-3 font-semibold">Grace</th>
            <th className="px-4 py-3 text-right font-semibold">Actions</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {assignments.map((assignment) => (
            <tr key={assignment.id} className={editingAssignmentId === assignment.id ? 'bg-hover-blue/60' : undefined}>
              <td className="px-4 py-4 font-medium text-foreground">{getSiteName(sites, assignment.locationId)}</td>
              <td className="px-4 py-4 text-muted-foreground">{getSystemName(systems, assignment.systemId)}</td>
              <td className="px-4 py-4 text-muted-foreground">{getAccessLevelName(systems, assignment.systemId, assignment.accessLevelTypeId)}</td>
              <td className="px-4 py-4 text-muted-foreground">{getTriggerLabel(assignment.trigger)}</td>
              <td className="px-4 py-4 text-muted-foreground">{assignment.gracePeriodMinutes} min</td>
              <td className="px-4 py-4">
                <div className="flex justify-end gap-2">
                  <Button type="button" variant="outline" size="icon-sm" aria-label={`Edit ${getTriggerLabel(assignment.trigger)} assignment`} onClick={() => onEdit(assignment)}>
                    <Pencil className="size-4" aria-hidden="true" />
                  </Button>
                  <Button type="button" variant="outline" size="icon-sm" aria-label={`Delete ${getTriggerLabel(assignment.trigger)} assignment`} disabled={deleteAssignmentId === assignment.id} onClick={() => onDelete(assignment)}>
                    <Trash2 className="size-4" aria-hidden="true" />
                  </Button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Field({ label, children }: { readonly label: string; readonly children: ReactNode }) {
  const id = useId();

  return (
    <div className="grid gap-2">
      <label className="text-[14px] font-medium" htmlFor={id}>{label}</label>
      <div id={id}>{children}</div>
    </div>
  );
}

function SelectField({ label, value, children, onChange }: { readonly label: string; readonly value: string; readonly children: ReactNode; readonly onChange: (value: string) => void }) {
  const id = useId();

  return (
    <div className="grid gap-2">
      <label className="text-[14px] font-medium" htmlFor={id}>{label}</label>
      <select
        id={id}
        className="h-9 w-full rounded-interactive border border-border bg-content px-3 text-[14px] outline-none transition focus:border-primary focus:ring-[3px] focus:ring-primary/20"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        required
      >
        {children}
      </select>
    </div>
  );
}

function getDefaultFormValues(): FormValues {
  return {
    locationId: '',
    systemId: '',
    accessLevelTypeId: '',
    trigger: 'ExpectedVisitorAdded',
    gracePeriodMinutes: '0',
  };
}

function toRequest(values: FormValues): AccessRuleAssignmentRequest | null {
  const gracePeriodMinutes = Number.parseInt(values.gracePeriodMinutes, 10);

  if (!values.locationId || !values.systemId || !values.accessLevelTypeId || Number.isNaN(gracePeriodMinutes) || gracePeriodMinutes < 0) {
    return null;
  }

  return {
    locationId: values.locationId,
    systemId: values.systemId,
    accessLevelTypeId: values.accessLevelTypeId,
    trigger: values.trigger,
    gracePeriodMinutes,
  };
}

function getSiteName(sites: readonly Site[], locationId: string) {
  return sites.find((site) => site.id === locationId)?.name ?? locationId;
}

function getSystemName(systems: readonly AccessControlSystem[], systemId: string) {
  return systems.find((system) => system.id === systemId)?.name ?? systemId;
}

function getAccessLevelName(systems: readonly AccessControlSystem[], systemId: string, accessLevelTypeId: string) {
  const system = systems.find((item) => item.id === systemId);
  return system?.accessLevels.find((accessLevel) => accessLevel.id === accessLevelTypeId)?.name ?? accessLevelTypeId;
}

function getTriggerLabel(trigger: ReceptionAccessPolicyTrigger) {
  return triggerOptions.find((option) => option.value === trigger)?.label ?? trigger;
}
