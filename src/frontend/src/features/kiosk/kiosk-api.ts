import { api } from '@/shared/api/client';

import { getKioskSettings } from './kiosk-settings';
import type { KioskConfig, KioskInstructionResponse, KioskSession } from './kiosk-types';

function getHeaders() {
  const settings = getKioskSettings();
  if (!settings) throw new Error('Kiosk setup is required.');

  return {
    'fabric-kiosk-id': settings.kioskId,
    'fabric-kiosk-key': settings.kioskApiKey,
  };
}

export async function getKioskConfig(languageCode?: string): Promise<KioskConfig> {
  const { data, error } = await api.GET('/api/kiosk/config', {
    headers: getHeaders(),
    params: { query: { languageCode } },
  });

  if (error || !data) throw new Error('Could not load kiosk configuration.');
  return data;
}

export async function postKioskHeartbeat() {
  const { error } = await api.POST('/api/kiosk/heartbeat', {
    headers: getHeaders(),
    body: { reportedAt: new Date().toISOString() },
  });

  if (error) throw new Error('Could not post kiosk heartbeat.');
}

export async function startKioskSession(languageCode?: string): Promise<KioskSession> {
  const { data, error } = await api.POST('/api/kiosk/sessions', {
    headers: getHeaders(),
    body: { languageCode: languageCode || null },
  });

  if (error || !data) throw new Error('Could not start kiosk session.');
  return data;
}

export async function changeKioskLanguage(languageCode: string): Promise<KioskSession> {
  const { data, error } = await api.POST('/api/kiosk/language', {
    headers: getHeaders(),
    body: { languageCode },
  });

  if (error || !data) throw new Error('Could not change kiosk language.');
  return data;
}

export async function getCurrentInstruction(sinceVersion?: number): Promise<KioskInstructionResponse> {
  const { data, error } = await api.GET('/api/kiosk/sessions/current/instruction', {
    headers: getHeaders(),
    params: { query: { sinceVersion } },
  });

  if (error || !data) throw new Error('Could not load kiosk instruction.');
  return data;
}

export async function submitInstructionResponse(instructionId: string, values: Record<string, string>): Promise<KioskSession> {
  const { data, error } = await api.POST('/api/kiosk/sessions/current/instructions/{instructionId}/response', {
    headers: getHeaders(),
    params: { path: { instructionId } },
    body: { values },
  });

  if (error || !data) throw new Error('Could not submit kiosk response.');
  return data;
}

export async function cancelCurrentSession(): Promise<KioskSession> {
  const { data, error } = await api.POST('/api/kiosk/sessions/current/cancel', {
    headers: getHeaders(),
  });

  if (error || !data) throw new Error('Could not cancel kiosk session.');
  return data;
}
