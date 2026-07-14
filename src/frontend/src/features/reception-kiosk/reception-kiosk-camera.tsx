import { useEffect, useRef, useState } from 'react';

import {
  type ReceptionKioskCameraOrientation,
  type ReceptionKioskCapturedImage,
  type ReceptionKioskOnboardingStep,
  getSavedCameraPreference,
  saveCameraPreference,
} from './reception-kiosk-onboarding';

export type VideoInputDevice = {
  readonly deviceId: string;
  readonly label: string;
};

type CaptureSourceRect = {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
};

export function useReceptionKioskCamera(step: ReceptionKioskOnboardingStep) {
  const savedPreference = getSavedCameraPreference(step);
  const videoRef = useRef<HTMLVideoElement>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const [availableDevices, setAvailableDevices] = useState<VideoInputDevice[]>([]);
  const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(savedPreference.deviceId);
  const [orientation, setOrientation] = useState<ReceptionKioskCameraOrientation>(savedPreference.orientation);
  const [error, setError] = useState<string | null>(null);
  const [isStarting, setIsStarting] = useState(true);
  const [isStreamReady, setIsStreamReady] = useState(false);

  useEffect(() => {
    let disposed = false;

    async function startCamera() {
      if (!videoRef.current || !navigator.mediaDevices?.getUserMedia) {
        setError('Camera access is not available on this device.');
        setIsStarting(false);
        return;
      }

      setError(null);
      setIsStarting(true);
      setIsStreamReady(false);

      stopStream(streamRef.current);
      streamRef.current = null;

      try {
        const constraints: MediaStreamConstraints = {
          video: selectedDeviceId
            ? { deviceId: { exact: selectedDeviceId } }
            : { facingMode: { ideal: orientation } },
          audio: false,
        };

        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        if (disposed) {
          stopStream(stream);
          return;
        }

        streamRef.current = stream;
        videoRef.current.srcObject = stream;
        try {
          await videoRef.current.play();
        } catch (playError) {
          if (!(playError instanceof DOMException && playError.name === 'AbortError')) {
            throw playError;
          }
        }
        setIsStreamReady(true);
        setIsStarting(false);
        saveCameraPreference(step, { deviceId: selectedDeviceId, orientation });

        if (navigator.mediaDevices.enumerateDevices) {
          const devices = await navigator.mediaDevices.enumerateDevices();
          if (!disposed) {
            const videoInputs = devices
              .filter((device) => device.kind === 'videoinput')
              .map((device, index) => ({
                deviceId: device.deviceId,
                label: device.label || `Camera ${index + 1}`,
              }));

            setAvailableDevices(videoInputs);

            if (selectedDeviceId && !videoInputs.some((device) => device.deviceId === selectedDeviceId)) {
              setSelectedDeviceId(null);
            }
          }
        }
      } catch {
        if (!disposed) {
          setError('Camera access is required to continue.');
          setIsStarting(false);
          setIsStreamReady(false);
        }
      }
    }

    void startCamera();

    return () => {
      disposed = true;
      stopStream(streamRef.current);
      if (videoRef.current) {
        videoRef.current.pause();
        videoRef.current.srcObject = null;
      }
      streamRef.current = null;
    };
  }, [orientation, selectedDeviceId, step]);

  function updateSelectedDeviceId(deviceId: string) {
    setSelectedDeviceId(deviceId || null);
  }

  function updateOrientation(nextOrientation: ReceptionKioskCameraOrientation) {
    setOrientation(nextOrientation);
  }

  function captureStillFrame(options?: { readonly mirrored?: boolean; readonly quality?: number; readonly sourceRect?: CaptureSourceRect }): ReceptionKioskCapturedImage | null {
    const videoElement = videoRef.current;
    if (!videoElement || !videoElement.videoWidth || !videoElement.videoHeight) {
      return null;
    }

    const sourceRect = options?.sourceRect ?? {
      x: 0,
      y: 0,
      width: videoElement.videoWidth,
      height: videoElement.videoHeight,
    };

    const canvas = document.createElement('canvas');
    canvas.width = sourceRect.width;
    canvas.height = sourceRect.height;
    const context = canvas.getContext('2d');
    if (!context) {
      return null;
    }

    if (options?.mirrored) {
      context.translate(canvas.width, 0);
      context.scale(-1, 1);
    }

    context.drawImage(videoElement, sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height, 0, 0, canvas.width, canvas.height);
    const dataUrl = canvas.toDataURL('image/jpeg', options?.quality ?? 0.9);
    const [prefix, base64] = dataUrl.split(',');
    const mimeType = prefix.match(/data:(.*);base64/)?.[1] ?? 'image/jpeg';

    return {
      base64,
      mimeType,
      width: canvas.width,
      height: canvas.height,
    };
  }

  return {
    videoRef,
    availableDevices,
    selectedDeviceId,
    orientation,
    error,
    isStarting,
    isStreamReady,
    setSelectedDeviceId: updateSelectedDeviceId,
    setOrientation: updateOrientation,
    captureStillFrame,
  };
}

function stopStream(stream: MediaStream | null) {
  stream?.getTracks().forEach((track) => track.stop());
}
