import * as React from 'react';

import { cn } from '@/shared/utils/cn';

type BadgeVariant = 'default' | 'secondary' | 'outline' | 'success' | 'warning' | 'error';

const variantStyles: Record<BadgeVariant, string> = {
  default: 'bg-primary text-primary-foreground shadow-sm',
  secondary: 'bg-secondary text-secondary-foreground',
  outline: 'border border-border text-foreground',
  success: 'bg-success text-success-foreground shadow-sm',
  warning: 'bg-warning text-warning-foreground shadow-sm',
  error: 'bg-error text-error-foreground shadow-sm',
};

interface BadgeProps extends React.ComponentProps<'span'> {
  variant?: BadgeVariant;
}

function Badge({ className, variant = 'default', ...props }: BadgeProps) {
  return (
    <span
      data-slot="badge"
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium whitespace-nowrap transition-colors',
        variantStyles[variant],
        className,
      )}
      {...props}
    />
  );
}

export { Badge, type BadgeProps, type BadgeVariant };
