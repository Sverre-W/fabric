import { ChevronLeft, ChevronRight, MoreHorizontal } from 'lucide-react';
import type { ComponentProps } from 'react';

import { cn } from '@/shared/utils/cn';

export function Pagination({ className, ...props }: ComponentProps<'nav'>) {
  return <nav className={cn('mx-auto flex w-full justify-center', className)} role="navigation" aria-label="pagination" {...props} />;
}

export function PaginationContent({ className, ...props }: ComponentProps<'ul'>) {
  return <ul className={cn('flex flex-row items-center gap-1', className)} {...props} />;
}

export function PaginationItem({ className, ...props }: ComponentProps<'li'>) {
  return <li className={cn('', className)} {...props} />;
}

type PaginationLinkProps = ComponentProps<'button'> & {
  readonly isActive?: boolean;
};

export function PaginationLink({ className, isActive, disabled, ...props }: PaginationLinkProps) {
  return (
    <button
      type="button"
      aria-current={isActive ? 'page' : undefined}
      disabled={disabled}
      className={cn(
        'inline-flex size-9 items-center justify-center rounded-interactive border border-transparent text-[14px] font-medium transition hover:bg-hover-blue disabled:pointer-events-none disabled:opacity-50',
        isActive && 'border-border bg-content text-foreground shadow-sm',
        className,
      )}
      {...props}
    />
  );
}

export function PaginationPrevious({ className, ...props }: ComponentProps<typeof PaginationLink>) {
  return (
    <PaginationLink aria-label="Go to previous page" className={cn('gap-1 px-3 sm:w-auto', className)} {...props}>
      <ChevronLeft className="size-4" aria-hidden="true" />
      <span className="hidden sm:block">Previous</span>
    </PaginationLink>
  );
}

export function PaginationNext({ className, ...props }: ComponentProps<typeof PaginationLink>) {
  return (
    <PaginationLink aria-label="Go to next page" className={cn('gap-1 px-3 sm:w-auto', className)} {...props}>
      <span className="hidden sm:block">Next</span>
      <ChevronRight className="size-4" aria-hidden="true" />
    </PaginationLink>
  );
}

export function PaginationEllipsis({ className, ...props }: ComponentProps<'span'>) {
  return (
    <span className={cn('flex size-9 items-center justify-center', className)} aria-hidden="true" {...props}>
      <MoreHorizontal className="size-4" />
      <span className="sr-only">More pages</span>
    </span>
  );
}
