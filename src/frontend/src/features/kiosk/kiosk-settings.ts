export type KioskSettings = {
  readonly kioskId: string;
  readonly kioskApiKey: string;
  readonly languageCode?: string;
};

const kioskSettingsKey = 'fabric.kiosk.settings';

export function getKioskSettings(): KioskSettings | null {
  if (typeof window === 'undefined') return null;

  const rawSettings = window.localStorage.getItem(kioskSettingsKey);
  if (!rawSettings) return null;

  try {
    const settings = JSON.parse(rawSettings) as Partial<KioskSettings>;
    const kioskId = settings.kioskId?.trim();
    const kioskApiKey = settings.kioskApiKey?.trim();
    const languageCode = settings.languageCode?.trim();

    if (!kioskId || !kioskApiKey) return null;

    return languageCode ? { kioskId, kioskApiKey, languageCode } : { kioskId, kioskApiKey };
  } catch {
    return null;
  }
}

export function hasKioskSettings() {
  return getKioskSettings() !== null;
}

export function getStoredKioskId() {
  if (typeof window === 'undefined') return '';

  const rawSettings = window.localStorage.getItem(kioskSettingsKey);
  if (!rawSettings) return '';

  try {
    const settings = JSON.parse(rawSettings) as Partial<KioskSettings>;
    return settings.kioskId?.trim() ?? '';
  } catch {
    return '';
  }
}

export function getStoredKioskLanguage() {
  return getKioskSettings()?.languageCode ?? '';
}

export function saveKioskSettings(settings: KioskSettings) {
  window.localStorage.setItem(
    kioskSettingsKey,
    JSON.stringify({
      kioskId: settings.kioskId.trim(),
      kioskApiKey: settings.kioskApiKey.trim(),
      languageCode: settings.languageCode?.trim() || undefined,
    }),
  );
}

export function saveKioskLanguage(languageCode: string) {
  const settings = getKioskSettings();
  if (!settings) return;

  saveKioskSettings({ ...settings, languageCode });
}

export function clearKioskSettings() {
  window.localStorage.removeItem(kioskSettingsKey);
}
