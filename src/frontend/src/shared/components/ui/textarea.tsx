import * as React from 'react';

import { cn } from '@/shared/utils/cn';

function Textarea({ className, ...props }: React.ComponentProps<'textarea'>) {
  return (
    <textarea
      data-slot="textarea"
      className={cn(
        'min-h-32 w-full resize-y rounded-interactive border border-border bg-transparent px-3 py-2 text-base shadow-xs transition-[color,box-shadow] outline-none placeholder:text-muted-foreground disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm',
        'focus-visible:border-primary focus-visible:ring-[3px] focus-visible:ring-primary/20',
        'aria-invalid:border-error aria-invalid:ring-error/20',
        className,
      )}
      {...props}
    />
  );
}

export { Textarea };
