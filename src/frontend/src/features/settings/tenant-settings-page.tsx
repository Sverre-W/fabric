import { useEffect, useId, useState, type Dispatch, type FormEvent, type ReactNode, type SetStateAction } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { KeyRound, Mail, Palette } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { applyFabricTheme, defaultFabricTheme, type FabricTheme } from '@/shared/theme/fabric-theme';
import {
  fetchAdminTenantSettings,
  tenantSettingsQueryKey,
  updateAdminTenantSettings,
  type AdminTenantSettings,
  type UpdateTenantSettingsRequest,
} from '@/shared/tenant/tenant-settings';

type ThemeField = keyof FabricTheme;

const themeFields: { readonly key: ThemeField; readonly label: string }[] = [
  { key: 'backgroundColor', label: 'Background' },
  { key: 'contentColor', label: 'Content' },
  { key: 'primaryColor', label: 'Primary' },
  { key: 'textColor', label: 'Text' },
  { key: 'textMutedColor', label: 'Muted text' },
  { key: 'borderColor', label: 'Border' },
  { key: 'hoverBlueColor', label: 'Hover blue' },
  { key: 'activeBlueColor', label: 'Active blue' },
  { key: 'hoverGrayColor', label: 'Hover gray' },
  { key: 'errorColor', label: 'Error' },
  { key: 'errorBackgroundColor', label: 'Error background' },
  { key: 'dangerColor', label: 'Danger' },
  { key: 'successColor', label: 'Success' },
  { key: 'successBackgroundColor', label: 'Success background' },
];

export default function TenantSettingsPage() {
  const queryClient = useQueryClient();
  const settingsQuery = useQuery({
    queryKey: tenantSettingsQueryKey,
    queryFn: fetchAdminTenantSettings,
  });
  const [values, setValues] = useState<UpdateTenantSettingsRequest>(getDefaultSettingsRequest);
  const [emailHasSecret, setEmailHasSecret] = useState(false);

  useEffect(() => {
    if (!settingsQuery.data) {
      return;
    }

    setValues(toRequest(settingsQuery.data));
    setEmailHasSecret(settingsQuery.data.email?.hasSecret ?? false);
  }, [settingsQuery.data]);

  const updateSettings = useMutation({
    mutationFn: updateAdminTenantSettings,
    onSuccess: async (settings) => {
      setValues(toRequest(settings));
      setEmailHasSecret(settings.email?.hasSecret ?? false);
      applyFabricTheme(settings.theme);
      await queryClient.invalidateQueries({ queryKey: tenantSettingsQueryKey });
      toast.success('Tenant settings saved.');
    },
    onError: () => {
      toast.error('Could not save tenant settings.');
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    updateSettings.mutate(normalizeRequest(values));
  }

  return (
    <section className="grid gap-6">
      <div className="rounded-structural border border-border bg-content p-6">
        <p className="text-[13px] font-semibold uppercase tracking-[0.18em] text-muted-foreground">Settings</p>
        <h1 className="mt-3 text-[32px] font-semibold tracking-tight">Tenant</h1>
        <p className="mt-3 max-w-2xl text-[14px] text-muted-foreground">Manage tenant authentication, appearance, and Graph email delivery settings.</p>
      </div>

      {settingsQuery.isLoading ? <p className="rounded-structural border border-border bg-content p-6 text-[14px] text-muted-foreground">Loading tenant settings...</p> : null}
      {settingsQuery.isError ? <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error">Could not load tenant settings.</p> : null}

      {!settingsQuery.isLoading && !settingsQuery.isError ? (
        <form className="grid gap-6" onSubmit={handleSubmit}>
          <SettingsCard
            icon={<KeyRound className="size-5" aria-hidden="true" />}
            title="Oidc Settings"
            description="Configure OpenID Connect metadata used for browser sign-in and API token validation."
          >
            <div className="grid gap-4 lg:grid-cols-2">
              <Field label="Metadata URL">
                <Input value={values.oidc.metadataUrl} onChange={(event) => setValues((current) => ({ ...current, oidc: { ...current.oidc, metadataUrl: event.target.value } }))} />
              </Field>
              <Field label="Client ID">
                <Input value={values.oidc.clientId} onChange={(event) => setValues((current) => ({ ...current, oidc: { ...current.oidc, clientId: event.target.value } }))} />
              </Field>
            </div>
            <CheckboxRow
              label="Require HTTPS metadata"
              checked={values.oidc.requireHttpsMetadata}
              onChange={(checked) => setValues((current) => ({ ...current, oidc: { ...current.oidc, requireHttpsMetadata: checked } }))}
            />
          </SettingsCard>

          <SettingsCard
            icon={<Palette className="size-5" aria-hidden="true" />}
            title="Theme Settings"
            description="Tune tenant colors. Changes apply to this session after save."
          >
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {themeFields.map((field) => (
                <Field key={field.key} label={field.label}>
                  <div className="flex gap-2">
                    <Input
                      type="color"
                      className="w-14 shrink-0 px-1 py-1"
                      value={toColorInputValue(values.theme[field.key])}
                      aria-label={`${field.label} color picker`}
                      onChange={(event) => setThemeValue(setValues, field.key, event.target.value)}
                    />
                    <Input value={values.theme[field.key]} onChange={(event) => setThemeValue(setValues, field.key, event.target.value)} />
                  </div>
                </Field>
              ))}
            </div>
            <div className="rounded-structural border border-border bg-background p-4">
              <div className="rounded-structural border border-border bg-content p-4" style={{ borderColor: values.theme.borderColor, backgroundColor: values.theme.contentColor }}>
                <p className="text-[14px] font-semibold" style={{ color: values.theme.textColor }}>Theme preview</p>
                <p className="mt-1 text-[13px]" style={{ color: values.theme.textMutedColor }}>Primary, success, and error states use tenant colors.</p>
                <div className="mt-4 flex flex-wrap gap-2">
                  <span className="rounded-full px-3 py-1 text-[12px] font-semibold text-white" style={{ backgroundColor: values.theme.primaryColor }}>Primary</span>
                  <span className="rounded-full px-3 py-1 text-[12px] font-semibold text-white" style={{ backgroundColor: values.theme.successColor }}>Success</span>
                  <span className="rounded-full px-3 py-1 text-[12px] font-semibold text-white" style={{ backgroundColor: values.theme.errorColor }}>Error</span>
                </div>
              </div>
            </div>
          </SettingsCard>

          <SettingsCard
            icon={<Mail className="size-5" aria-hidden="true" />}
            title="Email Settings"
            description="Configure Microsoft Graph sender settings for tenant notifications. Secret values are never returned by the API."
          >
            <CheckboxRow label="Enable Graph email" checked={values.email !== null} onChange={(checked) => setValues((current) => ({ ...current, email: checked ? current.email ?? getDefaultEmailRequest() : null }))} />

            {values.email ? (
              <div className="grid gap-4 lg:grid-cols-2">
                <Field label="From email">
                  <Input type="email" value={values.email.fromEmail} onChange={(event) => setEmailValue(setValues, 'fromEmail', event.target.value)} />
                </Field>
                <Field label="From name">
                  <Input value={values.email.fromName} onChange={(event) => setEmailValue(setValues, 'fromName', event.target.value)} />
                </Field>
                <Field label="Azure tenant ID">
                  <Input value={values.email.azureTenantId} onChange={(event) => setEmailValue(setValues, 'azureTenantId', event.target.value)} />
                </Field>
                <Field label="Application ID">
                  <Input value={values.email.applicationId} onChange={(event) => setEmailValue(setValues, 'applicationId', event.target.value)} />
                </Field>
                <Field label={emailHasSecret ? 'Secret replacement' : 'Secret'}>
                  <Input
                    type="password"
                    value={values.email.secret ?? ''}
                    placeholder={emailHasSecret ? 'Leave empty to keep current secret' : 'Required before saving'}
                    onChange={(event) => setEmailValue(setValues, 'secret', event.target.value)}
                  />
                </Field>
                <div className="flex items-end">
                  <CheckboxRow label="Save sent items" checked={values.email.saveSentItems} onChange={(checked) => setEmailValue(setValues, 'saveSentItems', checked)} />
                </div>
              </div>
            ) : (
              <p className="rounded-interactive border border-border bg-background px-4 py-3 text-[14px] text-muted-foreground">Graph email is disabled for this tenant. Global email settings may still be used by the server.</p>
            )}
          </SettingsCard>

          <div className="flex justify-end border-t border-border pt-6">
            <Button type="submit" disabled={updateSettings.isPending}>{updateSettings.isPending ? 'Saving...' : 'Save tenant settings'}</Button>
          </div>
        </form>
      ) : null}
    </section>
  );
}

function SettingsCard({ icon, title, description, children }: { readonly icon: ReactNode; readonly title: string; readonly description: string; readonly children: ReactNode }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-[20px]">
          <span className="rounded-interactive bg-hover-blue p-2 text-primary">{icon}</span>
          {title}
        </CardTitle>
        <CardDescription className="max-w-3xl">{description}</CardDescription>
      </CardHeader>
      <CardContent className="grid gap-5">{children}</CardContent>
    </Card>
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

function CheckboxRow({ label, checked, onChange }: { readonly label: string; readonly checked: boolean; readonly onChange: (checked: boolean) => void }) {
  return (
    <label className="flex w-fit items-center gap-3 rounded-interactive border border-border bg-content px-3 py-2 text-[14px] font-medium">
      <input type="checkbox" className="size-4 accent-primary" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      {label}
    </label>
  );
}

function getDefaultSettingsRequest(): UpdateTenantSettingsRequest {
  return {
    oidc: {
      metadataUrl: '',
      clientId: '',
      requireHttpsMetadata: true,
    },
    theme: defaultFabricTheme,
    email: null,
  };
}

function getDefaultEmailRequest(): NonNullable<UpdateTenantSettingsRequest['email']> {
  return {
    fromEmail: '',
    fromName: '',
    azureTenantId: '',
    applicationId: '',
    secret: '',
    saveSentItems: false,
  };
}

function toRequest(settings: AdminTenantSettings): UpdateTenantSettingsRequest {
  return {
    oidc: settings.oidc,
    theme: settings.theme,
    email: settings.email
      ? {
        fromEmail: settings.email.fromEmail,
        fromName: settings.email.fromName,
        azureTenantId: settings.email.azureTenantId,
        applicationId: settings.email.applicationId,
        secret: '',
        saveSentItems: settings.email.saveSentItems,
      }
      : null,
  };
}

function normalizeRequest(request: UpdateTenantSettingsRequest): UpdateTenantSettingsRequest {
  return {
    oidc: {
      metadataUrl: request.oidc.metadataUrl.trim(),
      clientId: request.oidc.clientId.trim(),
      requireHttpsMetadata: request.oidc.requireHttpsMetadata,
    },
    theme: Object.fromEntries(Object.entries(request.theme).map(([key, value]) => [key, value.trim()])) as FabricTheme,
    email: request.email
      ? {
        fromEmail: request.email.fromEmail.trim(),
        fromName: request.email.fromName.trim(),
        azureTenantId: request.email.azureTenantId.trim(),
        applicationId: request.email.applicationId.trim(),
        secret: request.email.secret?.trim() || null,
        saveSentItems: request.email.saveSentItems,
      }
      : null,
  };
}

function toColorInputValue(value: string): string {
  return /^#[0-9a-fA-F]{6}$/.test(value) ? value : '#000000';
}

function setThemeValue(setValues: Dispatch<SetStateAction<UpdateTenantSettingsRequest>>, key: ThemeField, value: string) {
  setValues((current) => ({ ...current, theme: { ...current.theme, [key]: value as FabricTheme[ThemeField] } }));
}

function setEmailValue<TKey extends keyof NonNullable<UpdateTenantSettingsRequest['email']>>(
  setValues: Dispatch<SetStateAction<UpdateTenantSettingsRequest>>,
  key: TKey,
  value: NonNullable<UpdateTenantSettingsRequest['email']>[TKey],
) {
  setValues((current) => current.email ? { ...current, email: { ...current.email, [key]: value } } : current);
}
