'use client';

import * as React from 'react';
import { DayPicker } from 'react-day-picker';
import { ChevronLeftIcon, ChevronRightIcon } from 'lucide-react';

import { cn } from '@/shared/utils/cn';
import { buttonVariants } from '@/shared/components/ui/button';

function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  ...props
}: React.ComponentProps<typeof DayPicker>) {
  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      className={cn('w-fit p-3', className)}
      classNames={{
        months: 'flex flex-col sm:flex-row gap-2',
        month: 'flex flex-col gap-4',
        month_caption: 'flex justify-center pt-1 relative items-center',
        caption_label: 'text-sm font-medium',
        nav: 'flex items-center gap-1',
        button_previous: cn(
          buttonVariants({ variant: 'ghost', size: 'icon-sm' }),
          'absolute left-1 top-0',
        ),
        button_next: cn(
          buttonVariants({ variant: 'ghost', size: 'icon-sm' }),
          'absolute right-1 top-0',
        ),
        month_grid: 'w-full border-collapse',
        weekdays: 'flex',
        weekday: 'w-8 text-[13px] font-normal text-muted-foreground',
        week: 'flex w-full mt-2',
        day: 'flex size-8 items-center justify-center text-sm',
        day_button: cn(
          buttonVariants({ variant: 'ghost', size: 'icon-sm' }),
          'size-8 font-normal aria-selected:opacity-100',
        ),
        range_start: 'day-range-start',
        range_end: 'day-range-end',
        selected:
          'bg-primary text-white hover:!bg-primary hover:!text-white focus-visible:bg-primary focus-visible:text-white rounded-interactive',
        today: 'bg-hover-blue text-foreground rounded-interactive',
        outside:
          'day-outside text-muted-foreground aria-selected:bg-hover-blue aria-selected:text-muted-foreground',
        disabled: 'text-muted-foreground/50 opacity-50',
        range_middle:
          'aria-selected:bg-hover-blue aria-selected:text-foreground',
        hidden: 'invisible',
        ...classNames,
      }}
      components={{
        Chevron: (props) => {
          if (props.orientation === 'left') {
            return <ChevronLeftIcon {...props} className={cn('size-4', props.className)} />;
          }
          return <ChevronRightIcon {...props} className={cn('size-4', props.className)} />;
        },
      }}
      {...props}
    />
  );
}

export { Calendar };
