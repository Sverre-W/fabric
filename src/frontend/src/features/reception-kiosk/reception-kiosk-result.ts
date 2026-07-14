export type ReceptionKioskResultKind = 'onboarding-success' | 'check-in-success' | 'check-out-success' | 'visit-completed' | 'action-failed';

type ReceptionKioskResult = {
  readonly kind: ReceptionKioskResultKind;
};

const receptionKioskResultKey = 'fabric.reception-kiosk.result';

export function saveReceptionKioskResult(kind: ReceptionKioskResultKind) {
  window.sessionStorage.setItem(receptionKioskResultKey, JSON.stringify({ kind } satisfies ReceptionKioskResult));
}

export function getReceptionKioskResult(): ReceptionKioskResult | null {
  const rawResult = window.sessionStorage.getItem(receptionKioskResultKey);
  if (!rawResult) {
    return null;
  }

  try {
    const parsed = JSON.parse(rawResult) as Partial<ReceptionKioskResult>;
    return isReceptionKioskResultKind(parsed.kind) ? { kind: parsed.kind } : null;
  } catch {
    return null;
  }
}

export function clearReceptionKioskResult() {
  window.sessionStorage.removeItem(receptionKioskResultKey);
}

function isReceptionKioskResultKind(value: unknown): value is ReceptionKioskResultKind {
  return value === 'onboarding-success'
    || value === 'check-in-success'
    || value === 'check-out-success'
    || value === 'visit-completed'
    || value === 'action-failed';
}
