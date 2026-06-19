import type { ComponentProps } from 'react';

import { cn } from '@/shared/utils/cn';

export function Empty({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('flex min-h-52 flex-col items-center justify-center gap-6 rounded-structural border border-dashed border-border p-8 text-center', className)} {...props} />;
}

export function EmptyHeader({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('grid gap-2', className)} {...props} />;
}

export function EmptyTitle({ className, ...props }: ComponentProps<'h3'>) {
  return <h3 className={cn('text-[16px] font-semibold tracking-tight', className)} {...props} />;
}

export function EmptyDescription({ className, ...props }: ComponentProps<'p'>) {
  return <p className={cn('max-w-sm text-[14px] text-muted-foreground', className)} {...props} />;
}

export function EmptyContent({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('flex items-center justify-center gap-2', className)} {...props} />;
}
