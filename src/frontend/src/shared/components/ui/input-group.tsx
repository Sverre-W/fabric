import * as React from 'react';

import { cn } from '@/shared/utils/cn';

function InputGroup({ className, ...props }: React.ComponentProps<'div'>) {
  return (
    <div
      data-slot="input-group"
      className={cn(
        'flex w-full items-center rounded-interactive border border-border bg-content shadow-xs transition-[color,box-shadow] focus-within:border-primary focus-within:ring-[3px] focus-within:ring-primary/20 has-aria-invalid:border-error has-aria-invalid:ring-error/20',
        className,
      )}
      {...props}
    />
  );
}

function InputGroupInput({ className, ...props }: React.ComponentProps<'input'>) {
  return (
    <input
      data-slot="input-group-input"
      className={cn(
        'h-9 w-full min-w-0 bg-transparent px-3 py-1 text-base outline-none placeholder:text-muted-foreground disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm',
        className,
      )}
      {...props}
    />
  );
}

function InputGroupAddon({
  className,
  align = 'inline-start',
  ...props
}: React.ComponentProps<'div'> & { align?: 'inline-start' | 'inline-end' }) {
  return (
    <div
      data-slot="input-group-addon"
      data-align={align}
      className={cn(
        'flex items-center',
        'data-[align=inline-start]:order-first data-[align=inline-end]:order-last',
        className,
      )}
      {...props}
    />
  );
}

function InputGroupButton({
  className,
  variant,
  size,
  ...props
}: React.ComponentProps<'button'> & {
  variant?: string;
  size?: string;
}) {
  return (
    <button
      data-slot="input-group-button"
      className={cn(
        'inline-flex size-8 items-center justify-center rounded-sm text-muted-foreground hover:bg-hover-blue hover:text-foreground transition',
        className,
      )}
      {...props}
    />
  );
}

export { InputGroup, InputGroupInput, InputGroupAddon, InputGroupButton };
