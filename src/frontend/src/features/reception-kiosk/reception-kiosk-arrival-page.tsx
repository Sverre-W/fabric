import { Link, Navigate } from '@tanstack/react-router';
import { ArrowLeft, CalendarClock, CheckCircle2, UserRound } from 'lucide-react';

import { Button, buttonVariants } from '@/shared/components/ui/button';

import { getReceptionKioskArrival } from './reception-kiosk-api';
import { hasReceptionKioskSettings } from './reception-kiosk-settings';

export default function ReceptionKioskArrivalPage() {
  if (!hasReceptionKioskSettings()) {
    return <Navigate to="/reception-kiosk/setup" replace />;
  }

  const arrival = getReceptionKioskArrival();
  if (!arrival) {
    return <Navigate to="/reception-kiosk" replace />;
  }

  const fullName = `${arrival.firstName} ${arrival.lastName}`.trim();

  return (
    <section className="w-full rounded-[2rem] border border-border bg-content p-6 shadow-sm sm:p-10">
      <Link to="/reception-kiosk" className="inline-flex items-center gap-2 text-[16px] font-medium text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-5" aria-hidden="true" />
        Back
      </Link>

      <div className="mt-8 grid gap-8 lg:grid-cols-[1fr_0.9fr]">
        <div className="rounded-[2rem] bg-hover-blue p-8">
          <div className="flex size-20 items-center justify-center rounded-full bg-content text-primary">
            <UserRound className="size-10" aria-hidden="true" />
          </div>
          <p className="mt-8 text-[14px] font-semibold uppercase tracking-[0.28em] text-muted-foreground">Expected arrival</p>
          <h2 className="mt-3 text-[40px] font-semibold tracking-tight sm:text-[56px]">{fullName}</h2>
          {arrival.company ? <p className="mt-3 text-[24px] text-muted-foreground">{arrival.company}</p> : null}
          <div className="mt-8 grid gap-3 text-[18px] sm:grid-cols-2">
            <Detail label="Arrival type" value={arrival.type} />
            <Detail label="Status" value={arrival.status} />
            <Detail label="Expected arrival" value={formatDateTime(arrival.expectedArrivalTime)} />
            <Detail label="Expected offboard" value={formatDateTime(arrival.expectedOffboardTime)} />
          </div>
        </div>

        <div className="space-y-4">
          {arrival.visitor?.visit ? (
            <article className="rounded-[2rem] border border-border p-6">
              <div className="flex items-center gap-3 text-primary">
                <CalendarClock className="size-7" aria-hidden="true" />
                <h3 className="text-[24px] font-semibold">Visit details</h3>
              </div>
              <dl className="mt-6 space-y-4 text-[17px]">
                <Detail label="Summary" value={arrival.visitor.visit.summary} />
                <Detail label="Organizer" value={arrival.visitor.visit.organizerName} />
                <Detail label="Organizer email" value={arrival.visitor.visit.organizerEmail} />
                <Detail label="Visit start" value={formatDateTime(arrival.visitor.visit.start)} />
                <Detail label="Visit stop" value={formatDateTime(arrival.visitor.visit.stop)} />
                <Detail label="Confirmation" value={arrival.visitor.confirmationStatus} />
                {arrival.visitor.transport ? <Detail label="Transport" value={arrival.visitor.transport} /> : null}
                {arrival.visitor.licensePlate ? <Detail label="License plate" value={arrival.visitor.licensePlate} /> : null}
              </dl>
            </article>
          ) : null}

          {arrival.type === 'Contractor' ? (
            <article className="rounded-[2rem] border border-border p-6">
              <h3 className="text-[24px] font-semibold">Contractor details</h3>
              <p className="mt-3 text-[17px] leading-7 text-muted-foreground">Contractor details will be added later.</p>
            </article>
          ) : null}

          <div className="rounded-[2rem] border border-border p-6">
            <div className="flex items-center gap-3 text-success">
              <CheckCircle2 className="size-7" aria-hidden="true" />
              <h3 className="text-[24px] font-semibold">Arrival found</h3>
            </div>
            <p className="mt-3 text-[17px] leading-7 text-muted-foreground">Continue when check-in steps are ready.</p>
            <Button className="mt-6 h-14 w-full rounded-[1rem] text-[18px]" disabled>Continue</Button>
          </div>
        </div>
      </div>

      <Link to="/reception-kiosk" className={buttonVariants({ variant: 'outline', size: 'lg', className: 'mt-8 h-14 w-full rounded-[1rem] text-[18px]' })}>Start over</Link>
    </section>
  );
}

function Detail({ label, value }: { readonly label: string; readonly value: string }) {
  return (
    <div>
      <dt className="text-[13px] font-semibold uppercase tracking-[0.18em] text-muted-foreground">{label}</dt>
      <dd className="mt-1 font-medium text-foreground">{value}</dd>
    </div>
  );
}

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}
