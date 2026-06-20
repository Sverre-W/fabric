import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';

export type CredentialGenerationMode = components['schemas']['CredentialGenerationMode'];
export type VisitorPreOnboardingSagaConfig = components['schemas']['VisitorPreOnboardingSagaConfig'];
export type VisitorPreOnboardingSagaConfigRequest = components['schemas']['VisitorPreOnboardingSagaConfigRequest'];

export const visitorPreOnboardingConfigQueryKey = ['settings', 'visitors', 'pre-onboarding-config'] as const;

export async function fetchVisitorPreOnboardingConfig(): Promise<VisitorPreOnboardingSagaConfigRequest> {
  const { data, error } = await api.GET('/api/sagas/visitor-pre-onboarding/configuration');

  if (error || !data) {
    throw new Error('Could not load visitor journey settings.');
  }

  return toRequest(data);
}

export async function updateVisitorPreOnboardingConfig(values: VisitorPreOnboardingSagaConfigRequest): Promise<VisitorPreOnboardingSagaConfigRequest> {
  const { data, error } = await api.PUT('/api/sagas/visitor-pre-onboarding/configuration', {
    body: values,
  });

  if (error || !data) {
    throw new Error('Could not save visitor journey settings.');
  }

  return toRequest(data);
}

export function getDefaultVisitorPreOnboardingConfig(): VisitorPreOnboardingSagaConfigRequest {
  return {
    useCustomInviteNotification: false,
    customInviteNotification: null,
    qrGenerationMode: 'PlatformQr',
    sendConfirmNotificationToOrganizer: false,
    useCustomConfirmNotification: false,
    customConfirmNotification: null,
    sendCancellationNotification: false,
    useCustomCancellationNotification: false,
    customCancellationNotification: null,
    sendRescheduleNotification: false,
    useCustomRescheduleNotification: false,
    customRescheduleNotification: null,
  };
}

function toRequest(config: VisitorPreOnboardingSagaConfig): VisitorPreOnboardingSagaConfigRequest {
  const defaults = getDefaultVisitorPreOnboardingConfig();

  return {
    useCustomInviteNotification: config.useCustomInviteNotification ?? defaults.useCustomInviteNotification,
    customInviteNotification: config.customInviteNotification ?? defaults.customInviteNotification,
    qrGenerationMode: config.qrGenerationMode ?? defaults.qrGenerationMode,
    sendConfirmNotificationToOrganizer: config.sendConfirmNotificationToOrganizer ?? defaults.sendConfirmNotificationToOrganizer,
    useCustomConfirmNotification: config.useCustomConfirmNotification ?? defaults.useCustomConfirmNotification,
    customConfirmNotification: config.customConfirmNotification ?? defaults.customConfirmNotification,
    sendCancellationNotification: config.sendCancellationNotification ?? defaults.sendCancellationNotification,
    useCustomCancellationNotification: config.useCustomCancellationNotification ?? defaults.useCustomCancellationNotification,
    customCancellationNotification: config.customCancellationNotification ?? defaults.customCancellationNotification,
    sendRescheduleNotification: config.sendRescheduleNotification ?? defaults.sendRescheduleNotification,
    useCustomRescheduleNotification: config.useCustomRescheduleNotification ?? defaults.useCustomRescheduleNotification,
    customRescheduleNotification: config.customRescheduleNotification ?? defaults.customRescheduleNotification,
  };
}
