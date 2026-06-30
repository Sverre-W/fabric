import { BrowserQRCodeReader } from '@zxing/browser';
import type { IScannerControls } from '@zxing/browser';
import { Link, Navigate, useNavigate } from '@tanstack/react-router';
import { ArrowLeft, Camera } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';

import { ReceptionKioskArrivalNotFoundError, lookupReceptionKioskArrival, saveReceptionKioskArrival, saveReceptionKioskMissedCode } from './reception-kiosk-api';
import { hasReceptionKioskSettings } from './reception-kiosk-settings';

export default function ReceptionKioskScanQrPage() {
  const navigate = useNavigate();
  const videoRef = useRef<HTMLVideoElement>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLookingUp, setIsLookingUp] = useState(false);

  useEffect(() => {
    let controls: IScannerControls | null = null;
    let disposed = false;
    let handled = false;

    async function startScanner() {
      if (!videoRef.current) {
        return;
      }

      try {
        const reader = new BrowserQRCodeReader(undefined, { delayBetweenScanAttempts: 300 });
        controls = await reader.decodeFromVideoDevice(undefined, videoRef.current, (result, scanError, activeControls) => {
          if (scanError && scanError.name !== 'NotFoundException') {
            setError('Camera is active. Keep the QR code inside the frame.');
          }

          if (!result || handled) {
            return;
          }

          handled = true;
          activeControls.stop();
          setIsLookingUp(true);

          lookupReceptionKioskArrival(result.getText().trim())
            .then((arrival) => {
              if (disposed) {
                return;
              }

              saveReceptionKioskArrival(arrival);
              void navigate({ to: '/reception-kiosk/arrival' });
            })
            .catch((lookupError: unknown) => {
              if (disposed) {
                return;
              }

              if (lookupError instanceof ReceptionKioskArrivalNotFoundError) {
                saveReceptionKioskMissedCode(lookupError.code);
                void navigate({ to: '/reception-kiosk/no-registration' });
                return;
              }

              handled = false;
              setIsLookingUp(false);
              setError(lookupError instanceof Error ? lookupError.message : 'Could not look up expected arrival.');
            });
        });
      } catch {
        if (!disposed) {
          setError('Camera access is required to scan a QR code.');
        }
      }
    }

    void startScanner();

    return () => {
      disposed = true;
      controls?.stop();
    };
  }, [navigate]);

  if (!hasReceptionKioskSettings()) {
    return <Navigate to="/reception-kiosk/setup" replace />;
  }

  return (
    <section className="w-full rounded-[2rem] border border-border bg-content p-6 shadow-sm sm:p-10">
      <Link to="/reception-kiosk" className="inline-flex items-center gap-2 text-[16px] font-medium text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-5" aria-hidden="true" />
        Back
      </Link>

      <div className="mt-8 text-center">
        <div className="mx-auto flex size-20 items-center justify-center rounded-full bg-hover-blue text-primary">
          <Camera className="size-10" aria-hidden="true" />
        </div>
        <h2 className="mt-5 text-[32px] font-semibold tracking-tight sm:text-[48px]">Scan QR code</h2>
        <p className="mx-auto mt-3 max-w-2xl text-[18px] leading-8 text-muted-foreground">Hold your QR code in front of the camera.</p>
      </div>

      <div className="relative mx-auto mt-8 max-w-3xl overflow-hidden rounded-[2rem] border border-border bg-black">
        <video ref={videoRef} className="aspect-[4/3] w-full object-cover" muted playsInline aria-label="QR scanner camera preview" />
        <div className="pointer-events-none absolute inset-8 rounded-[1.5rem] border-4 border-white/70" aria-hidden="true" />
      </div>

      {isLookingUp ? <p className="mt-6 text-center text-[18px] font-medium text-primary">Looking up arrival...</p> : null}
      {error ? <p className="mt-6 rounded-interactive bg-error-background p-4 text-center text-[16px] font-medium text-error" role="alert">{error}</p> : null}
    </section>
  );
}
