import { Menu, X } from 'lucide-react';
import { createContext, type ReactNode, useContext, useMemo, useState } from 'react';

import { cn } from '@/shared/utils/cn';

type SidebarContextValue = {
  isMobileOpen: boolean;
  setIsMobileOpen: (isOpen: boolean) => void;
};

const SidebarContext = createContext<SidebarContextValue | null>(null);

export function SidebarProvider({ children }: { children: ReactNode }) {
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const value = useMemo(() => ({ isMobileOpen, setIsMobileOpen }), [isMobileOpen]);

  return <SidebarContext.Provider value={value}>{children}</SidebarContext.Provider>;
}

export function Sidebar({ children, className }: { children: ReactNode; className?: string }) {
  const { isMobileOpen, setIsMobileOpen } = useSidebar();

  return (
    <>
      <aside className={cn('hidden w-72 shrink-0 md:block', className)}>{children}</aside>
      {isMobileOpen ? (
        <div className="fixed inset-0 z-30 md:hidden">
          <button
            type="button"
            className="absolute inset-0 bg-foreground/40"
            aria-label="Close sidebar"
            onClick={() => setIsMobileOpen(false)}
          />
          <aside className={cn('relative flex h-full w-80 max-w-[85vw] flex-col border-r border-border bg-content', className)}>{children}</aside>
        </div>
      ) : null}
    </>
  );
}

export function SidebarTrigger({ className }: { className?: string }) {
  const { setIsMobileOpen } = useSidebar();

  return (
    <button
      type="button"
      className={cn('inline-flex size-10 items-center justify-center rounded-interactive border border-border bg-content text-foreground md:hidden', className)}
      aria-label="Open sidebar"
      onClick={() => setIsMobileOpen(true)}
    >
      <Menu className="size-5" />
    </button>
  );
}

export function SidebarClose({ className }: { className?: string }) {
  const { setIsMobileOpen } = useSidebar();

  return (
    <button
      type="button"
      className={cn('inline-flex size-9 items-center justify-center rounded-interactive text-muted-foreground hover:bg-hover-gray hover:text-foreground md:hidden', className)}
      aria-label="Close sidebar"
      onClick={() => setIsMobileOpen(false)}
    >
      <X className="size-5" />
    </button>
  );
}

export function SidebarHeader({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('border-b border-border p-4', className)}>{children}</div>;
}

export function SidebarContent({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('flex-1 p-4', className)}>{children}</div>;
}

export function SidebarGroup({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('grid gap-3', className)}>{children}</div>;
}

export function SidebarGroupLabel({ children, className }: { children: ReactNode; className?: string }) {
  return <p className={cn('px-2 text-[14px] font-semibold uppercase tracking-wide text-muted-foreground', className)}>{children}</p>;
}

export function SidebarMenu({ children, className, 'aria-label': ariaLabel }: { children: ReactNode; className?: string; 'aria-label'?: string }) {
  return (
    <nav aria-label={ariaLabel} className={cn('grid gap-1', className)}>
      {children}
    </nav>
  );
}

export function SidebarMenuButton({ children, isActive, className }: { children: ReactNode; isActive?: boolean; className?: string }) {
  return (
    <span
      className={cn(
        'flex rounded-interactive px-3 py-2 text-[14px] font-medium text-muted-foreground transition hover:bg-hover-blue hover:text-foreground',
        isActive && 'bg-active-blue text-foreground hover:bg-active-blue hover:text-foreground',
        className,
      )}
    >
      {children}
    </span>
  );
}

export function useSidebar() {
  const context = useContext(SidebarContext);

  if (!context) {
    throw new Error('Sidebar components must be used within SidebarProvider.');
  }

  return context;
}
