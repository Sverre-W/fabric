import { useEffect, useId, useState, type FormEvent, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Bell, MailCheck, Route, Send } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Textarea } from '@/shared/components/ui/textarea';

import {
  fetchVisitorPreOnboardingConfig,
  getDefaultVisitorPreOnboardingConfig,
  updateVisitorPreOnboardingConfig,
  visitorPreOnboardingConfigQueryKey,
  type CredentialGenerationMode,
  type VisitorPreOnboardingSagaConfigRequest,
} from './visitor-pre-onboarding-config';

export default function VisitorsSettingsPage() {
  const queryClient = useQueryClient();
  const configQuery = useQuery({
    queryKey: visitorPreOnboardingConfigQueryKey,
    queryFn: fetchVisitorPreOnboardingConfig,
  });
  const [values, setValues] = useState<VisitorPreOnboardingSagaConfigRequest>(getDefaultVisitorPreOnboardingConfig);

  useEffect(() => {
    if (configQuery.data) {
      setValues(toRequest(configQuery.data));
    }
  }, [configQuery.data]);

  const updateConfig = useMutation({
    mutationFn: updateVisitorPreOnboardingConfig,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: visitorPreOnboardingConfigQueryKey });
      toast.success('Visitor journey settings saved.');
    },
    onError: () => {
      toast.error('Could not save visitor journey settings.');
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    updateConfig.mutate(normalize(values));
  }

  return (
    <section className="grid gap-6">
      <div className="rounded-structural border border-border bg-content p-4 sm:p-6">
        <p className="text-[13px] font-semibold uppercase tracking-[0.18em] text-muted-foreground">Settings</p>
        <h1 className="mt-3 text-[32px] font-semibold tracking-tight">Visitors</h1>
        <p className="mt-3 max-w-2xl text-[14px] text-muted-foreground">Configure visitor journey defaults and notification behavior for pre-onboarding.</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
              <CardTitle className="flex items-center gap-2 text-[20px]">
                <Route className="size-5 text-primary" aria-hidden="true" />
                Visitor journey
              </CardTitle>
              <CardDescription className="mt-2 max-w-3xl">
                Decide how visitor pre-onboarding issues credentials and which emails are sent during invitation, confirmation, cancellation, and rescheduling.
              </CardDescription>
            </div>
            <StatusPill mode={values.qrGenerationMode} />
          </div>
        </CardHeader>
        <CardContent>
          {configQuery.isLoading ? <p className="text-[14px] text-muted-foreground">Loading visitor journey settings...</p> : null}
          {configQuery.isError ? <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[14px] text-error">Could not load visitor journey settings.</p> : null}

          {!configQuery.isLoading && !configQuery.isError ? (
            <form className="grid gap-6" onSubmit={handleSubmit}>
              <div className="grid gap-2">
                <label className="text-[14px] font-medium" htmlFor="qr-generation-mode">Credential generation</label>
                <select
                  id="qr-generation-mode"
                  className="h-9 w-full rounded-interactive border border-border bg-content px-3 text-[14px] outline-none transition focus:border-primary focus:ring-[3px] focus:ring-primary/20 md:max-w-sm"
                  value={values.qrGenerationMode}
                  onChange={(event) => setValues((current) => ({ ...current, qrGenerationMode: event.target.value as CredentialGenerationMode }))}
                >
                  <option value="PlatformQr">Platform QR</option>
                  <option value="AccessControlQr">Access control QR</option>
                </select>
                <p className="text-[13px] text-muted-foreground">Controls which system should generate visitor credentials during pre-onboarding.</p>
              </div>

              <div className="grid gap-4 lg:grid-cols-2">
                <NotificationTemplateSection
                  icon={<Send className="size-4" aria-hidden="true" />}
                  title="Invitation"
                  description="Sent to visitors after arrival registration and QR setup. The default template is used unless custom HTML is enabled."
                  customEnabled={values.useCustomInviteNotification}
                  customValue={values.customInviteNotification ?? ''}
                  onCustomEnabledChange={(checked) => setValues((current) => ({ ...current, useCustomInviteNotification: checked }))}
                  onCustomValueChange={(customInviteNotification) => setValues((current) => ({ ...current, customInviteNotification }))}
                />

                <NotificationTemplateSection
                  icon={<MailCheck className="size-4" aria-hidden="true" />}
                  title="Organizer confirmation"
                  description="Optionally notify organizers when a visitor confirms participation."
                  sendEnabled={values.sendConfirmNotificationToOrganizer}
                  sendLabel="Send confirmation to organizer"
                  customEnabled={values.useCustomConfirmNotification}
                  customValue={values.customConfirmNotification ?? ''}
                  onSendEnabledChange={(checked) => setValues((current) => ({ ...current, sendConfirmNotificationToOrganizer: checked }))}
                  onCustomEnabledChange={(checked) => setValues((current) => ({ ...current, useCustomConfirmNotification: checked }))}
                  onCustomValueChange={(customConfirmNotification) => setValues((current) => ({ ...current, customConfirmNotification }))}
                />

                <NotificationTemplateSection
                  icon={<Bell className="size-4" aria-hidden="true" />}
                  title="Cancellation"
                  description="Notify visitors when visit cancellation moves their onboarding saga into cancellation."
                  sendEnabled={values.sendCancellationNotification}
                  sendLabel="Send cancellation notification"
                  customEnabled={values.useCustomCancellationNotification}
                  customValue={values.customCancellationNotification ?? ''}
                  onSendEnabledChange={(checked) => setValues((current) => ({ ...current, sendCancellationNotification: checked }))}
                  onCustomEnabledChange={(checked) => setValues((current) => ({ ...current, useCustomCancellationNotification: checked }))}
                  onCustomValueChange={(customCancellationNotification) => setValues((current) => ({ ...current, customCancellationNotification }))}
                />

                <NotificationTemplateSection
                  icon={<Bell className="size-4" aria-hidden="true" />}
                  title="Reschedule"
                  description="Notify visitors when a planned visit changes start time."
                  sendEnabled={values.sendRescheduleNotification}
                  sendLabel="Send reschedule notification"
                  customEnabled={values.useCustomRescheduleNotification}
                  customValue={values.customRescheduleNotification ?? ''}
                  onSendEnabledChange={(checked) => setValues((current) => ({ ...current, sendRescheduleNotification: checked }))}
                  onCustomEnabledChange={(checked) => setValues((current) => ({ ...current, useCustomRescheduleNotification: checked }))}
                  onCustomValueChange={(customRescheduleNotification) => setValues((current) => ({ ...current, customRescheduleNotification }))}
                />
              </div>

              <div className="flex justify-end border-t border-border pt-6">
                <Button type="submit" className="w-full sm:w-auto" disabled={updateConfig.isPending}>
                  {updateConfig.isPending ? 'Saving...' : 'Save visitor journey'}
                </Button>
              </div>
            </form>
          ) : null}
        </CardContent>
      </Card>
    </section>
  );
}

function NotificationTemplateSection({
  icon,
  title,
  description,
  sendEnabled,
  sendLabel,
  customEnabled,
  customValue,
  onSendEnabledChange,
  onCustomEnabledChange,
  onCustomValueChange,
}: {
  readonly icon: ReactNode;
  readonly title: string;
  readonly description: string;
  readonly sendEnabled?: boolean;
  readonly sendLabel?: string;
  readonly customEnabled: boolean;
  readonly customValue: string;
  readonly onSendEnabledChange?: (checked: boolean) => void;
  readonly onCustomEnabledChange: (checked: boolean) => void;
  readonly onCustomValueChange: (value: string) => void;
}) {
  const customTemplateId = useId();
  const disabledBySendToggle = sendEnabled === false;

  return (
    <section className="grid gap-4 rounded-structural border border-border bg-background p-4">
      <div className="flex items-start gap-3">
        <div className="mt-0.5 rounded-interactive bg-hover-blue p-2 text-primary">{icon}</div>
        <div>
          <h2 className="text-[15px] font-semibold">{title}</h2>
          <p className="mt-1 text-[13px] leading-5 text-muted-foreground">{description}</p>
        </div>
      </div>

      {sendLabel && onSendEnabledChange ? (
        <CheckboxRow label={sendLabel} checked={sendEnabled ?? false} onChange={onSendEnabledChange} />
      ) : null}

      <CheckboxRow label="Use custom HTML template" checked={customEnabled} disabled={disabledBySendToggle} onChange={onCustomEnabledChange} />

      <div className="grid gap-2">
        <label className="text-[13px] font-medium" htmlFor={customTemplateId}>Custom HTML</label>
        <Textarea
          id={customTemplateId}
          value={customValue}
          onChange={(event) => onCustomValueChange(event.target.value)}
          disabled={!customEnabled || disabledBySendToggle}
          placeholder="Paste full email HTML here."
          spellCheck={false}
          className="font-mono text-[13px]"
        />
      </div>
    </section>
  );
}

function CheckboxRow({ label, checked, disabled, onChange }: { readonly label: string; readonly checked: boolean; readonly disabled?: boolean; readonly onChange: (checked: boolean) => void }) {
  return (
    <label className="flex items-center gap-3 rounded-interactive border border-border bg-content px-3 py-2 text-[14px] font-medium">
      <input
        type="checkbox"
        className="size-4 accent-primary disabled:cursor-not-allowed disabled:opacity-50"
        checked={checked}
        disabled={disabled}
        onChange={(event) => onChange(event.target.checked)}
      />
      {label}
    </label>
  );
}

function StatusPill({ mode }: { readonly mode: CredentialGenerationMode }) {
  return (
    <div className="w-fit rounded-full border border-border bg-hover-blue px-3 py-1 text-[12px] font-semibold text-primary">
      {mode === 'PlatformQr' ? 'Platform QR' : 'Access control QR'}
    </div>
  );
}

function toRequest(config: VisitorPreOnboardingSagaConfigRequest): VisitorPreOnboardingSagaConfigRequest {
  return {
    useCustomInviteNotification: config.useCustomInviteNotification,
    customInviteNotification: config.customInviteNotification,
    qrGenerationMode: config.qrGenerationMode,
    sendConfirmNotificationToOrganizer: config.sendConfirmNotificationToOrganizer,
    useCustomConfirmNotification: config.useCustomConfirmNotification,
    customConfirmNotification: config.customConfirmNotification,
    sendCancellationNotification: config.sendCancellationNotification,
    useCustomCancellationNotification: config.useCustomCancellationNotification,
    customCancellationNotification: config.customCancellationNotification,
    sendRescheduleNotification: config.sendRescheduleNotification,
    useCustomRescheduleNotification: config.useCustomRescheduleNotification,
    customRescheduleNotification: config.customRescheduleNotification,
  };
}

function normalize(config: VisitorPreOnboardingSagaConfigRequest): VisitorPreOnboardingSagaConfigRequest {
  return {
    ...config,
    customInviteNotification: normalizeTemplate(config.customInviteNotification),
    customConfirmNotification: normalizeTemplate(config.customConfirmNotification),
    customCancellationNotification: normalizeTemplate(config.customCancellationNotification),
    customRescheduleNotification: normalizeTemplate(config.customRescheduleNotification),
  };
}

function normalizeTemplate(value: string | null) {
  return value?.trim() ? value : null;
}
