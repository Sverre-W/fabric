import { Link, Navigate } from '@tanstack/react-router';
import { Keyboard, QrCode } from 'lucide-react';

import { Button, buttonVariants } from '@/shared/components/ui/button';
import { hasReceptionKioskSettings } from './reception-kiosk-settings';

export default function ReceptionKioskPage() {
  if (!hasReceptionKioskSettings()) {
    return <Navigate to="/reception-kiosk/setup" replace />;
  }

  return (
    <section className="w-full rounded-[2rem] border border-border bg-content p-8 text-center shadow-sm sm:p-12">
      <p className="text-[14px] font-semibold uppercase tracking-[0.28em] text-muted-foreground">Reception kiosk</p>
      <h2 className="mt-3 text-[36px] font-semibold tracking-tight sm:text-[56px]">Welcome</h2>
      <p className="mx-auto mt-5 max-w-2xl text-[18px] leading-8 text-muted-foreground sm:text-[22px] sm:leading-9">
        Choose how you want to find your expected arrival.
      </p>

      <div className="mt-10 grid gap-4 sm:grid-cols-2">
        <Link to="/reception-kiosk/scan-qr" className={buttonVariants({ size: 'lg', className: 'h-auto min-h-48 flex-col rounded-[1.5rem] p-8 text-[22px] sm:min-h-64 sm:text-[28px]' })}>
          <QrCode className="size-14" aria-hidden="true" />
          <span>I have a QR</span>
        </Link>
        <Button size="lg" variant="outline" disabled className="h-auto min-h-48 flex-col rounded-[1.5rem] p-8 text-[22px] sm:min-h-64 sm:text-[28px]">
          <Keyboard className="size-14" aria-hidden="true" />
          <span>I don't have a QR</span>
        </Button>
      </div>
    </section>
  );
}
