import { Link } from '@tanstack/react-router';
import { type ReactNode } from 'react';

import { FabricLogo } from '@/shared/branding/fabric-logo';
import { appBranding } from '@/shared/branding/fabric-branding';
import { cn } from '@/shared/utils/cn';

const navItems = [
  { to: '/', label: 'Overview' },
  { to: '/identities', label: 'Identities' },
  { to: '/access', label: 'Access' },
  { to: '/credentials', label: 'Credentials' },
  { to: '/organizations', label: 'Organizations' },
  { to: '/audit', label: 'Audit' },
  { to: '/settings', label: 'Settings' },
] as const;

export function AppLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="border-b border-slate-200 bg-white/90 backdrop-blur">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-4 md:flex-row md:items-center md:justify-between">
          <Link to="/" className="flex items-center gap-3" aria-label={`${appBranding.appName} home`}>
            <FabricLogo />
            <span className="text-xl font-semibold tracking-tight">{appBranding.appName}</span>
          </Link>
          <nav aria-label="Primary navigation" className="flex gap-1 overflow-x-auto pb-1 md:pb-0">
            {navItems.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                activeOptions={{ exact: item.to === '/' }}
                className="rounded-md px-3 py-2 text-sm font-medium text-muted-foreground transition hover:bg-slate-100 hover:text-foreground"
                activeProps={{ className: cn('bg-primary text-white hover:bg-primary hover:text-white') }}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-7xl px-4 py-8">{children}</main>
    </div>
  );
}
