import { Navigate, useNavigate } from '@tanstack/react-router';
import { useMutation } from '@tanstack/react-query';
import { useEffect, useMemo, useRef, useState, type MutableRefObject } from 'react';
import { Button } from '@/shared/components/ui/button';

import { useReceptionKioskCamera } from './reception-kiosk-camera';
import {
  ReceptionKioskCameraSettings,
  ReceptionKioskCancelLink,
  ReceptionKioskCaptureReview,
  ReceptionKioskCaptureShell,
  ReceptionKioskFooterActions,
  ReceptionKioskPreviewStage,
  getStepProgressLabel,
} from './reception-kiosk-capture-ui';
import { clearReceptionKioskArrival, getReceptionKioskArrival, onboardReceptionKioskArrival } from './reception-kiosk-api';
import {
  buildOnboardingRequest,
  clearOnboardingState,
  getOnboardingStepIndex,
  getRequiredOnboardingSteps,
  getStoredOnboardingState,
  saveDocumentCapture,
} from './reception-kiosk-onboarding';
import { saveReceptionKioskResult } from './reception-kiosk-result';
import { hasReceptionKioskSettings } from './reception-kiosk-settings';

const cardAspectRatio = 1.586;
const sampleWidth = 240;
const sampleHeight = 180;

export default function ReceptionKioskDocumentScanPage() {
  const navigate = useNavigate();
  const arrival = getReceptionKioskArrival();
  const steps = useMemo(() => (arrival ? getRequiredOnboardingSteps(arrival) : []), [arrival]);
  const stepIndex = arrival ? getOnboardingStepIndex(arrival, 'document') : -1;
  const storedCapture = getStoredOnboardingState().documentCapture;
  const [capture, setCapture] = useState(storedCapture);
  const [status, setStatus] = useState('Place the identity document inside the frame.');
  const [isDocumentReady, setIsDocumentReady] = useState(false);
  const [countdown, setCountdown] = useState<number | null>(null);
  const sampleCanvasRef = useRef<HTMLCanvasElement | null>(null);
  const previousSampleRef = useRef<Uint8ClampedArray | null>(null);
  const {
    availableDevices,
    captureStillFrame,
    error,
    isStarting,
    isStreamReady,
    orientation,
    selectedDeviceId,
    setOrientation,
    setSelectedDeviceId,
    videoRef,
  } = useReceptionKioskCamera('document');

  const submitOnboarding = useMutation({
    mutationFn: async () => {
      if (!arrival) {
        throw new Error('Expected arrival is required.');
      }

      await onboardReceptionKioskArrival(arrival.id, buildOnboardingRequest(arrival));
    },
    onSuccess: async () => {
      clearOnboardingState();
      clearReceptionKioskArrival();
      saveReceptionKioskResult('onboarding-success');
      await navigate({ to: '/reception-kiosk/success' });
    },
    onError: () => {
      saveReceptionKioskResult('action-failed');
      void navigate({ to: '/reception-kiosk/failed' });
    },
  });

  useEffect(() => {
    if (!isStreamReady || !videoRef.current || capture) {
      return;
    }

    let frameId = 0;
    let lastProcessedAt = 0;

    const analyzeFrame = (now: number) => {
      if (!videoRef.current || capture) {
        return;
      }

      if (now - lastProcessedAt >= 180) {
        lastProcessedAt = now;
        const result = evaluateDocumentFrame(videoRef.current, sampleCanvasRef, previousSampleRef);
        setIsDocumentReady(result.ready);
        setStatus(result.message);
      }

      frameId = window.requestAnimationFrame(analyzeFrame);
    };

    frameId = window.requestAnimationFrame(analyzeFrame);
    return () => window.cancelAnimationFrame(frameId);
  }, [capture, isStreamReady, videoRef]);

  useEffect(() => {
    if (!isDocumentReady || capture) {
      setCountdown(null);
      return;
    }

    setCountdown((current) => current ?? 3);
  }, [capture, isDocumentReady]);

  useEffect(() => {
    if (countdown === null || capture || !isDocumentReady) {
      return;
    }

    if (countdown === 0) {
      const videoElement = videoRef.current;
      if (!videoElement) {
        return;
      }

      const stillFrame = captureStillFrame({ sourceRect: getDocumentFrameRect(videoElement.videoWidth, videoElement.videoHeight) });
      if (stillFrame) {
        setCapture(stillFrame);
      }
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setCountdown((current) => (current === null ? null : current - 1));
    }, 1000);

    return () => window.clearTimeout(timeoutId);
  }, [capture, captureStillFrame, countdown, isDocumentReady]);

  if (!hasReceptionKioskSettings()) {
    return <Navigate to="/reception-kiosk/setup" replace />;
  }

  if (!arrival) {
    return <Navigate to="/reception-kiosk" replace />;
  }

  if (arrival.onboardingRequirements?.identityVerificationMethod !== 'Picture') {
    return <Navigate to="/reception-kiosk/arrival" replace />;
  }

  async function handleConfirm() {
    if (!capture) {
      return;
    }

    saveDocumentCapture(capture);
    submitOnboarding.mutate();
  }

  function handleRetake() {
    setCapture(null);
    setCountdown(null);
    setStatus('Place the identity document inside the frame.');
  }

  function handleManualCapture() {
    const videoElement = videoRef.current;
    if (!videoElement) {
      return;
    }

    setCapture(captureStillFrame({ sourceRect: getDocumentFrameRect(videoElement.videoWidth, videoElement.videoHeight) }));
  }

  const progressLabel = getStepProgressLabel('document', steps.length, stepIndex);
  const statusLabel = capture
    ? 'Document picture ready. Review before continuing.'
    : countdown !== null
      ? `Taking photo in ${countdown}...`
      : status;

  return (
    <ReceptionKioskCaptureShell
      backTo={arrival.onboardingRequirements?.requireFacePicture ? '/reception-kiosk/scan-face' : '/reception-kiosk/arrival'}
      progressLabel={progressLabel}
      title="Scan identity document"
      description="Hold the identity document inside the frame. We will capture a picture automatically once it is stable."
    >
      <ReceptionKioskCameraSettings
        availableDevices={availableDevices}
        orientation={orientation}
        selectedDeviceId={selectedDeviceId}
        setOrientation={setOrientation}
        setSelectedDeviceId={setSelectedDeviceId}
      />

      {capture ? (
        <ReceptionKioskCaptureReview
          capture={capture}
          title="Identity document preview"
          onConfirm={() => void handleConfirm()}
          onRetake={handleRetake}
          confirmLabel={submitOnboarding.isPending ? 'Completing self-onboarding...' : 'Complete self-onboarding'}
        />
      ) : (
        <>
          <ReceptionKioskPreviewStage error={error} isStarting={isStarting} status={statusLabel}>
            <div className="relative h-full w-full">
              <video ref={videoRef} className="h-full w-full object-cover" muted playsInline aria-label="Document camera preview" />
              <div className="pointer-events-none absolute inset-0 bg-black/20" aria-hidden="true" />
              <div className="pointer-events-none absolute inset-0 flex items-center justify-center px-[8%]" aria-hidden="true">
                <div className="w-full max-w-[82%] rounded-[1.5rem] border-4 border-white/85 shadow-[0_0_0_9999px_rgba(0,0,0,0.3)] aspect-[1.586/1]" />
              </div>
              {countdown !== null ? <div className="absolute inset-0 flex items-center justify-center text-[96px] font-semibold text-white drop-shadow-lg">{countdown}</div> : null}
            </div>
          </ReceptionKioskPreviewStage>

          <ReceptionKioskFooterActions>
            <Button type="button" variant="outline" className="h-14 rounded-[1rem] text-[18px]" onClick={handleManualCapture}>
              Capture now
            </Button>
            <ReceptionKioskCancelLink to="/reception-kiosk/arrival" label="Cancel self-onboarding" />
          </ReceptionKioskFooterActions>
        </>
      )}
    </ReceptionKioskCaptureShell>
  );
}

function evaluateDocumentFrame(
  videoElement: HTMLVideoElement,
  sampleCanvasRef: MutableRefObject<HTMLCanvasElement | null>,
  previousSampleRef: MutableRefObject<Uint8ClampedArray | null>,
) {
  const sampleCanvas = sampleCanvasRef.current ?? document.createElement('canvas');
  sampleCanvasRef.current = sampleCanvas;
  sampleCanvas.width = sampleWidth;
  sampleCanvas.height = sampleHeight;

  const context = sampleCanvas.getContext('2d', { willReadFrequently: true });
  if (!context) {
    return { ready: false, message: 'Place the identity document inside the frame.' };
  }

  context.drawImage(videoElement, 0, 0, sampleCanvas.width, sampleCanvas.height);

  const frameRect = getDocumentFrameRect(sampleCanvas.width, sampleCanvas.height);
  const roiX = frameRect.x;
  const roiY = frameRect.y;
  const roiWidth = frameRect.width;
  const roiHeight = frameRect.height;
  const imageData = context.getImageData(roiX, roiY, roiWidth, roiHeight);
  const grayscale = toGrayscale(imageData.data);

  const variance = calculateVariance(grayscale);
  const edgeDensity = calculateEdgeDensity(grayscale, roiWidth, roiHeight);
  const motion = calculateMotion(previousSampleRef.current, grayscale);
  const borderStrength = calculateBorderStrength(grayscale, roiWidth, roiHeight);
  previousSampleRef.current = grayscale;

  if (variance < 160 || edgeDensity < 7) {
    return { ready: false, message: 'Move the document closer and fill the frame.' };
  }

  if (borderStrength < 16) {
    return { ready: false, message: 'Keep all card edges inside the frame.' };
  }

  if (motion > 9) {
    return { ready: false, message: 'Hold the document still and avoid glare.' };
  }

  return { ready: true, message: 'Hold still. We are preparing the capture.' };
}

function getDocumentFrameRect(width: number, height: number) {
  if (width === 0 || height === 0) {
    return { x: 0, y: 0, width: 0, height: 0 };
  }

  const maxWidth = width * 0.82;
  const maxHeight = height * 0.56;
  const widthFromHeight = maxHeight * cardAspectRatio;
  const frameWidth = Math.round(Math.min(maxWidth, widthFromHeight));
  const frameHeight = Math.round(frameWidth / cardAspectRatio);

  return {
    x: Math.round((width - frameWidth) / 2),
    y: Math.round((height - frameHeight) / 2),
    width: frameWidth,
    height: frameHeight,
  };
}

function toGrayscale(data: Uint8ClampedArray): Uint8ClampedArray {
  const grayscale = new Uint8ClampedArray(data.length / 4);

  for (let index = 0; index < data.length; index += 4) {
    grayscale[index / 4] = Math.round((data[index] + data[index + 1] + data[index + 2]) / 3);
  }

  return grayscale;
}

function calculateVariance(grayscale: Uint8ClampedArray): number {
  let total = 0;

  grayscale.forEach((value) => {
    total += value;
  });

  const mean = total / grayscale.length;
  let squaredDiff = 0;

  grayscale.forEach((value) => {
    squaredDiff += (value - mean) * (value - mean);
  });

  return squaredDiff / grayscale.length;
}

function calculateEdgeDensity(grayscale: Uint8ClampedArray, width: number, height: number): number {
  let edges = 0;

  for (let y = 1; y < height; y += 1) {
    for (let x = 1; x < width; x += 1) {
      const index = y * width + x;
      const left = grayscale[index - 1] ?? 0;
      const top = grayscale[index - width] ?? 0;
      const value = grayscale[index] ?? 0;

      if (Math.abs(value - left) + Math.abs(value - top) > 42) {
        edges += 1;
      }
    }
  }

  return (edges / (width * height)) * 100;
}

function calculateMotion(previous: Uint8ClampedArray | null, current: Uint8ClampedArray): number {
  if (!previous || previous.length !== current.length) {
    return Number.POSITIVE_INFINITY;
  }

  let totalDifference = 0;

  for (let index = 0; index < current.length; index += 1) {
    totalDifference += Math.abs(current[index] - previous[index]);
  }

  return totalDifference / current.length;
}

function calculateBorderStrength(grayscale: Uint8ClampedArray, width: number, height: number): number {
  const insetX = Math.max(1, Math.round(width * 0.06));
  const insetY = Math.max(1, Math.round(height * 0.08));
  let totalContrast = 0;
  let samples = 0;

  for (let x = 0; x < width; x += 1) {
    totalContrast += Math.abs((grayscale[x] ?? 0) - (grayscale[Math.min(height - 1, insetY) * width + x] ?? 0));
    totalContrast += Math.abs((grayscale[(height - 1) * width + x] ?? 0) - (grayscale[Math.max(0, height - 1 - insetY) * width + x] ?? 0));
    samples += 2;
  }

  for (let y = 0; y < height; y += 1) {
    totalContrast += Math.abs((grayscale[y * width] ?? 0) - (grayscale[y * width + Math.min(width - 1, insetX)] ?? 0));
    totalContrast += Math.abs((grayscale[y * width + (width - 1)] ?? 0) - (grayscale[y * width + Math.max(0, width - 1 - insetX)] ?? 0));
    samples += 2;
  }

  return samples > 0 ? totalContrast / samples : 0;
}
