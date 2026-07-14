import type { components } from '@/shared/api/generated/schema';

export type ReceptionKioskOnboardingStep = 'face' | 'document';
export type ReceptionKioskCameraOrientation = 'user' | 'environment';
type IdentityVerificationMethod = components['schemas']['IdentityVerificationMethod'];
type ReceptionKioskExpectedArrival = components['schemas']['ReceptionKioskExpectedArrivalResponse'];

export type ReceptionKioskCapturedImage = {
  readonly base64: string;
  readonly mimeType: string;
  readonly width: number;
  readonly height: number;
};

type ReceptionKioskOnboardingState = {
  readonly faceCapture: ReceptionKioskCapturedImage | null;
  readonly documentCapture: ReceptionKioskCapturedImage | null;
};

type ReceptionKioskCameraPreference = {
  readonly deviceId: string | null;
  readonly orientation: ReceptionKioskCameraOrientation;
};

type ReceptionKioskCameraPreferences = {
  readonly face: ReceptionKioskCameraPreference;
  readonly document: ReceptionKioskCameraPreference;
};

const onboardingStateKey = 'fabric.reception-kiosk.onboarding';
const cameraPreferencesKey = 'fabric.reception-kiosk.camera-preferences';

const defaultCameraPreferences: ReceptionKioskCameraPreferences = {
  face: { deviceId: null, orientation: 'user' },
  document: { deviceId: null, orientation: 'user' },
};

export function getRequiredOnboardingSteps(arrival: ReceptionKioskExpectedArrival): ReceptionKioskOnboardingStep[] {
  const steps: ReceptionKioskOnboardingStep[] = [];

  if (arrival.onboardingRequirements?.requireFacePicture) {
    steps.push('face');
  }

  if (arrival.onboardingRequirements?.identityVerificationMethod === 'Picture') {
    steps.push('document');
  }

  return steps;
}

export function getFirstOnboardingStep(arrival: ReceptionKioskExpectedArrival): ReceptionKioskOnboardingStep | null {
  return getRequiredOnboardingSteps(arrival)[0] ?? null;
}

export function getNextIncompleteOnboardingStep(arrival: ReceptionKioskExpectedArrival): ReceptionKioskOnboardingStep | null {
  const state = getStoredOnboardingState();

  return getRequiredOnboardingSteps(arrival).find((step) =>
    step === 'face' ? state.faceCapture === null : state.documentCapture === null,
  ) ?? null;
}

export function getNextOnboardingStep(
  arrival: ReceptionKioskExpectedArrival,
  currentStep: ReceptionKioskOnboardingStep,
): ReceptionKioskOnboardingStep | null {
  const steps = getRequiredOnboardingSteps(arrival);
  const currentIndex = steps.indexOf(currentStep);
  return currentIndex >= 0 ? steps[currentIndex + 1] ?? null : null;
}

export function getOnboardingStepIndex(arrival: ReceptionKioskExpectedArrival, step: ReceptionKioskOnboardingStep): number {
  return getRequiredOnboardingSteps(arrival).indexOf(step);
}

export function getOnboardingStepPath(step: ReceptionKioskOnboardingStep): '/reception-kiosk/scan-face' | '/reception-kiosk/scan-document' {
  return step === 'face' ? '/reception-kiosk/scan-face' : '/reception-kiosk/scan-document';
}

export function getStoredOnboardingState(): ReceptionKioskOnboardingState {
  if (typeof window === 'undefined') {
    return { faceCapture: null, documentCapture: null };
  }

  const rawState = window.sessionStorage.getItem(onboardingStateKey);
  if (!rawState) {
    return { faceCapture: null, documentCapture: null };
  }

  try {
    const state = JSON.parse(rawState) as Partial<ReceptionKioskOnboardingState>;
    return {
      faceCapture: isCapturedImage(state.faceCapture) ? state.faceCapture : null,
      documentCapture: isCapturedImage(state.documentCapture) ? state.documentCapture : null,
    };
  } catch {
    return { faceCapture: null, documentCapture: null };
  }
}

export function saveFaceCapture(capture: ReceptionKioskCapturedImage) {
  const state = getStoredOnboardingState();
  saveOnboardingState({ ...state, faceCapture: capture });
}

export function saveDocumentCapture(capture: ReceptionKioskCapturedImage) {
  const state = getStoredOnboardingState();
  saveOnboardingState({ ...state, documentCapture: capture });
}

export function clearOnboardingState() {
  if (typeof window !== 'undefined') {
    window.sessionStorage.removeItem(onboardingStateKey);
  }
}

export function getCapturePreviewUrl(capture: ReceptionKioskCapturedImage | null): string | null {
  return capture ? `data:${capture.mimeType};base64,${capture.base64}` : null;
}

export function buildOnboardingRequest(arrival: ReceptionKioskExpectedArrival): {
  readonly facePicture?: string;
  readonly identityVerification?: {
    readonly method: IdentityVerificationMethod;
    readonly content: string;
  };
} {
  const state = getStoredOnboardingState();
  const request: {
    facePicture?: string;
    identityVerification?: {
      method: IdentityVerificationMethod;
      content: string;
    };
  } = {};

  if (arrival.onboardingRequirements?.requireFacePicture && state.faceCapture) {
    request.facePicture = state.faceCapture.base64;
  }

  if (arrival.onboardingRequirements?.identityVerificationMethod === 'Picture' && state.documentCapture) {
    request.identityVerification = {
      method: 'Picture',
      content: state.documentCapture.base64,
    };
  }

  return request;
}

export function getSavedCameraPreference(step: ReceptionKioskOnboardingStep): ReceptionKioskCameraPreference {
  if (typeof window === 'undefined') {
    return defaultCameraPreferences[step];
  }

  const rawPreferences = window.localStorage.getItem(cameraPreferencesKey);
  if (!rawPreferences) {
    return defaultCameraPreferences[step];
  }

  try {
    const parsed = JSON.parse(rawPreferences) as Partial<ReceptionKioskCameraPreferences>;
    const preference = parsed[step];
    if (!preference || (preference.orientation !== 'user' && preference.orientation !== 'environment')) {
      return defaultCameraPreferences[step];
    }

    return {
      deviceId: typeof preference.deviceId === 'string' && preference.deviceId ? preference.deviceId : null,
      orientation: preference.orientation,
    };
  } catch {
    return defaultCameraPreferences[step];
  }
}

export function saveCameraPreference(step: ReceptionKioskOnboardingStep, preference: ReceptionKioskCameraPreference) {
  if (typeof window === 'undefined') {
    return;
  }

  const currentPreferences = getAllCameraPreferences();
  window.localStorage.setItem(cameraPreferencesKey, JSON.stringify({
    ...currentPreferences,
    [step]: preference,
  }));
}

function getAllCameraPreferences(): ReceptionKioskCameraPreferences {
  if (typeof window === 'undefined') {
    return defaultCameraPreferences;
  }

  const rawPreferences = window.localStorage.getItem(cameraPreferencesKey);
  if (!rawPreferences) {
    return defaultCameraPreferences;
  }

  try {
    const parsed = JSON.parse(rawPreferences) as Partial<ReceptionKioskCameraPreferences>;
    return {
      face: isCameraPreference(parsed.face) ? parsed.face : defaultCameraPreferences.face,
      document: isCameraPreference(parsed.document) ? parsed.document : defaultCameraPreferences.document,
    };
  } catch {
    return defaultCameraPreferences;
  }
}

function saveOnboardingState(state: ReceptionKioskOnboardingState) {
  window.sessionStorage.setItem(onboardingStateKey, JSON.stringify(state));
}

function isCapturedImage(value: unknown): value is ReceptionKioskCapturedImage {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const image = value as Partial<ReceptionKioskCapturedImage>;
  return typeof image.base64 === 'string'
    && typeof image.mimeType === 'string'
    && typeof image.width === 'number'
    && typeof image.height === 'number';
}

function isCameraPreference(value: unknown): value is ReceptionKioskCameraPreference {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const preference = value as Partial<ReceptionKioskCameraPreference>;
  return (preference.deviceId === null || typeof preference.deviceId === 'string')
    && (preference.orientation === 'user' || preference.orientation === 'environment');
}
