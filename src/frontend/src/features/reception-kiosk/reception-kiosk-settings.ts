export type ReceptionKioskSettings = {
  readonly kioskId: string;
  readonly kioskApiKey: string;
};

const receptionKioskSettingsKey = 'fabric.reception-kiosk.settings';

export function getReceptionKioskSettings(): ReceptionKioskSettings | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const rawSettings = window.localStorage.getItem(receptionKioskSettingsKey);
  if (!rawSettings) {
    return null;
  }

  try {
    const settings = JSON.parse(rawSettings) as Partial<ReceptionKioskSettings>;
    const kioskId = settings.kioskId?.trim();
    const kioskApiKey = settings.kioskApiKey?.trim();

    if (!kioskId || !kioskApiKey) {
      return null;
    }

    return { kioskId, kioskApiKey };
  } catch {
    return null;
  }
}

export function hasReceptionKioskSettings(): boolean {
  return getReceptionKioskSettings() !== null;
}

export function saveReceptionKioskSettings(settings: ReceptionKioskSettings) {
  window.localStorage.setItem(
    receptionKioskSettingsKey,
    JSON.stringify({
      kioskId: settings.kioskId.trim(),
      kioskApiKey: settings.kioskApiKey.trim(),
    }),
  );
}

export function getStoredReceptionKioskId(): string {
  if (typeof window === 'undefined') {
    return '';
  }

  const rawSettings = window.localStorage.getItem(receptionKioskSettingsKey);
  if (!rawSettings) {
    return '';
  }

  try {
    const settings = JSON.parse(rawSettings) as Partial<ReceptionKioskSettings>;
    return settings.kioskId?.trim() ?? '';
  } catch {
    return '';
  }
}
