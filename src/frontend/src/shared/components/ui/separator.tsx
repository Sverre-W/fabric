import * as React from 'react';

import { cn } from '@/shared/utils/cn';

function Separator({ className, ...props }: React.ComponentProps<'hr'>) {
  return (
    <hr
      data-slot="separator"
      className={cn('shrink-0 bg-border border-none h-px', className)}
      {...props}
    />
  );
}

export { Separator };
