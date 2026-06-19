import type { ComponentProps } from 'react';

import type { components } from '@/shared/api/generated/schema';
import { cn } from '@/shared/utils/cn';

type VisitStatus = components['schemas']['VisitStatus'];

type VisitStatusBadgeProps = ComponentProps<'span'> & {
  readonly status?: VisitStatus | null;
};

const statusStyles: Record<VisitStatus, string> = {
  Scheduled: 'border-primary/25 bg-hover-blue text-primary',
  Cancelled: 'border-error/25 bg-error-background text-error',
  Completed: 'border-success/25 bg-success-background text-success',
};

export function VisitStatusBadge({ status, className, ...props }: VisitStatusBadgeProps) {
  const label = status ?? 'Unknown';

  return (
    <span
      className={cn(
        'inline-flex w-fit items-center rounded-status border px-2 py-0.5 text-[12px] font-semibold leading-5',
        status ? statusStyles[status] : 'border-border bg-hover-gray text-muted-foreground',
        className,
      )}
      {...props}
    >
      {label}
    </span>
  );
}
