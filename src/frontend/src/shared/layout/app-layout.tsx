import { Link, useLocation } from '@tanstack/react-router';
import { type ReactNode } from 'react';
import { useAuth } from 'react-oidc-context';

import { FabricLogo } from '@/shared/branding/fabric-logo';
import { useBranding } from '@/shared/branding/branding-context';
import { SidebarProvider, SidebarTrigger } from '@/shared/components/ui/sidebar';
import { getModuleByPathname } from '@/shared/modules/app-modules';

import { ModuleSidebar } from './module-sidebar';

export function AppLayout({ children }: { children: ReactNode }) {
  const location = useLocation();
  const auth = useAuth();
  const branding = useBranding();
  const activeModule = auth.isAuthenticated ? getModuleByPathname(location.pathname) : undefined;

  return (
    <SidebarProvider>
      <div className="min-h-screen bg-background text-foreground">
        <header className="sticky top-0 z-10 border-b border-border bg-content">
          <div className="flex items-center gap-3 px-4 py-4">
            {activeModule ? <SidebarTrigger /> : null}
            <Link to="/" className="flex items-center gap-3" aria-label={`${branding.appName} home`}>
              <FabricLogo logoUrl={branding.logoUrl} />
              <span className="text-[20px] font-semibold tracking-tight">{branding.appName}</span>
            </Link>
            <div className="ml-auto">
              {auth.isAuthenticated ? (
                <button
                  type="button"
                  className="rounded-interactive border border-border bg-content px-4 py-2 text-[14px] font-semibold transition hover:bg-hover-gray"
                  onClick={() => void auth.signoutRedirect().catch(() => auth.removeUser())}
                >
                  Sign out
                </button>
              ) : null}
            </div>
          </div>
        </header>
        <div className="flex min-h-[calc(100vh-73px)]">
          {activeModule ? <ModuleSidebar module={activeModule} /> : null}
          <main className="min-w-0 flex-1 px-4 py-6 md:px-8 md:py-8">
            <div className="mx-auto max-w-7xl">{children}</div>
          </main>
        </div>
      </div>
    </SidebarProvider>
  );
}
