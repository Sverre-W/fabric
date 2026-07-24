import { Link, useLocation } from '@tanstack/react-router';
import { type ReactNode, useEffect } from 'react';
import { useAuth } from 'react-oidc-context';

import { useCurrentActor } from '@/shared/actors/current-actor';
import { FabricLogo } from '@/shared/branding/fabric-logo';
import { useBranding } from '@/shared/branding/branding-context';
import { isElsaStudioFullscreenRoute } from '@/features/automation/elsa-studio-fullscreen';
import { PerspectiveSidebar } from '@/shared/layout/perspective-sidebar';
import { NoPerspectiveWarning } from '@/shared/perspectives/no-perspective-warning';
import { getAvailablePerspectives, getPerspectiveByPathname } from '@/shared/perspectives/app-perspectives';
import { useTenantSettings } from '@/shared/tenant/tenant-settings-context';

export function AppLayout({ children }: { children: ReactNode }) {
  const location = useLocation();
  const auth = useAuth();
  const branding = useBranding();
  const actorQuery = useCurrentActor();
  const tenantSettings = useTenantSettings();
  const isFullscreenElsaRoute = isElsaStudioFullscreenRoute(location.pathname);
  const availablePerspectives = getAvailablePerspectives(actorQuery.data);
  const activePerspective = getPerspectiveByPathname(location.pathname);
  const showPerspectiveShell = auth.isAuthenticated && !isFullscreenElsaRoute && activePerspective && availablePerspectives.length > 0;
  const showNoPerspectiveWarning = auth.isAuthenticated && !isFullscreenElsaRoute && !actorQuery.isLoading && !actorQuery.isError && availablePerspectives.length === 0;

  useEffect(() => {
    document.body.classList.toggle('fabric-app-body', !isFullscreenElsaRoute);

    return () => {
      document.body.classList.add('fabric-app-body');
    };
  }, [isFullscreenElsaRoute]);

  return (
    <div className="min-h-screen bg-background text-foreground">
      {isFullscreenElsaRoute ? (
        <main className="min-h-screen">{children}</main>
      ) : (
        <>
          <header className="sticky top-0 z-10 border-b border-border bg-content">
            <div className="flex items-center gap-3 px-3 py-3 sm:px-4 sm:py-4">
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
          <main className="min-w-0">
            {showNoPerspectiveWarning ? <NoPerspectiveWarning /> : null}
            {!showNoPerspectiveWarning && showPerspectiveShell ? (
              <div className="flex min-h-[calc(100vh-73px)] items-stretch">
                <PerspectiveSidebar perspectives={availablePerspectives} version={tenantSettings.version} />
                <div className="min-w-0 flex-1 px-4 py-5 sm:px-6 sm:py-6 md:px-10 md:py-8">
                  <div className="mx-auto w-full max-w-6xl">{children}</div>
                </div>
              </div>
            ) : null}
            {!showNoPerspectiveWarning && !showPerspectiveShell ? (
              <div className="px-3 py-5 sm:px-4 sm:py-6 md:px-8 md:py-8">
                <div className="mx-auto max-w-7xl">{children}</div>
              </div>
            ) : null}
          </main>
        </>
      )}
    </div>
  );
}
