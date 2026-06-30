import { api } from '@/shared/api/client';
import type { components } from '@/shared/api/generated/schema';

import { getReceptionKioskSettings } from './reception-kiosk-settings';

export type ReceptionKioskExpectedArrival = components['schemas']['ReceptionKioskExpectedArrivalResponse'];

const receptionKioskArrivalKey = 'fabric.reception-kiosk.arrival';
const receptionKioskMissedCodeKey = 'fabric.reception-kiosk.missed-code';

export class ReceptionKioskArrivalNotFoundError extends Error {
  constructor(readonly code: string) {
    super('No expected arrival found for this QR code.');
    this.name = 'ReceptionKioskArrivalNotFoundError';
  }
}

export async function lookupReceptionKioskArrival(code: string): Promise<ReceptionKioskExpectedArrival> {
  const settings = getReceptionKioskSettings();
  if (!settings) {
    throw new Error('Reception kiosk setup is required.');
  }

  const { data, error, response } = await api.GET('/api/reception/kiosk/arrivals/lookup', {
    params: { query: { code } },
    headers: {
      'reception-kiosk-id': settings.kioskId,
      'reception-kiosk-key': settings.kioskApiKey,
    },
  });

  if (response.status === 404) {
    throw new ReceptionKioskArrivalNotFoundError(code);
  }

  if (error || !data) {
    throw new Error('Could not look up expected arrival.');
  }

  return data;
}

export function saveReceptionKioskArrival(arrival: ReceptionKioskExpectedArrival) {
  window.sessionStorage.setItem(receptionKioskArrivalKey, JSON.stringify(arrival));
}

export function getReceptionKioskArrival(): ReceptionKioskExpectedArrival | null {
  const rawArrival = window.sessionStorage.getItem(receptionKioskArrivalKey);
  if (!rawArrival) {
    return null;
  }

  try {
    return JSON.parse(rawArrival) as ReceptionKioskExpectedArrival;
  } catch {
    return null;
  }
}

export function saveReceptionKioskMissedCode(code: string) {
  window.sessionStorage.setItem(receptionKioskMissedCodeKey, code);
}

export function getReceptionKioskMissedCode(): string {
  return window.sessionStorage.getItem(receptionKioskMissedCodeKey) ?? '';
}
