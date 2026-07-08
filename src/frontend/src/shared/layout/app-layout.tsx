import { Link, useLocation } from '@tanstack/react-router';
import { type ReactNode } from 'react';
import { useAuth } from 'react-oidc-context';

import { FabricLogo } from '@/shared/branding/fabric-logo';
import { useBranding } from '@/shared/branding/branding-context';
import { SidebarProvider, SidebarTrigger } from '@/shared/components/ui/sidebar';
import { isElsaStudioFullscreenRoute } from '@/features/automation/elsa-studio-fullscreen';
import { getModuleByPathname } from '@/shared/modules/app-modules';

import { ModuleSidebar } from './module-sidebar';

export function AppLayout({ children }: { children: ReactNode }) {
  const location = useLocation();
  const auth = useAuth();
  const branding = useBranding();
  const isFullscreenElsaRoute = isElsaStudioFullscreenRoute(location.pathname);
  const activeModule = auth.isAuthenticated ? getModuleByPathname(location.pathname) : undefined;

  return (
    <SidebarProvider>
      <div className="min-h-screen bg-background text-foreground">
        {isFullscreenElsaRoute ? (
          <main className="min-h-screen">{children}</main>
        ) : (
          <>
            <header className="sticky top-0 z-10 border-b border-border bg-content">
              <div className="flex items-center gap-3 px-3 py-3 sm:px-4 sm:py-4">
                {activeModule ? <SidebarTrigger /> : null}
                <Link to="/" className="flex items-center gap-3" aria-label={`${branding.appName} home`}>
                  <FabricLogo logoUrl={branding.logoUrl} />
                  <span className="hidden text-[20px] font-semibold tracking-tight min-[380px]:inline">{branding.appName}</span>
                </Link>
                <div className="ml-auto">
                  {auth.isAuthenticated ? (
                    <button
                      type="button"
                      className="rounded-interactive border border-border bg-content px-3 py-2 text-[14px] font-semibold transition hover:bg-hover-gray sm:px-4"
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
              <main className="min-w-0 flex-1 px-3 py-5 sm:px-4 sm:py-6 md:px-8 md:py-8">
                <div className="mx-auto max-w-7xl">{children}</div>
              </main>
            </div>
          </>
        )}
      </div>
    </SidebarProvider>
  );
}
