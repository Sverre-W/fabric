import { type ReactNode } from 'react';

import { FabricLogo } from '@/shared/branding/fabric-logo';
import { useBranding } from '@/shared/branding/branding-context';

import type { KioskConfig } from './kiosk-types';

export function KioskLayout({ config, languageCode, onLanguageChange, children }: { readonly config?: KioskConfig | null; readonly languageCode?: string; readonly onLanguageChange?: (languageCode: string) => void; readonly children: ReactNode }) {
  const branding = useBranding();
  const theme = config?.theme ?? {};
  const primaryColor = theme.primaryColor ?? theme.primary ?? undefined;
  const backgroundColor = theme.backgroundColor ?? theme.background ?? undefined;
  const surfaceColor = theme.surfaceColor ?? theme.surface ?? undefined;

  return (
    <div className="min-h-screen text-foreground" style={{ backgroundColor }}>
      <header className="border-b border-border bg-content/90 px-5 py-4 shadow-sm backdrop-blur sm:px-8" style={{ backgroundColor: surfaceColor }}>
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <FabricLogo logoUrl={config?.resolvedWelcome?.logoUrl || branding.logoUrl} />
            <div>
              <p className="text-[13px] font-semibold uppercase tracking-[0.24em] text-muted-foreground">Kiosk</p>
              <h1 className="text-[22px] font-semibold tracking-tight sm:text-[28px]">{config?.kiosk.name ?? branding.appName}</h1>
            </div>
          </div>

          {config?.languages.length ? (
            <label className="grid gap-1 text-[13px] font-medium text-muted-foreground">
              Language
              <select className="min-w-32 rounded-interactive border border-border bg-content px-3 py-2 text-[15px] text-foreground outline-none transition focus:border-primary" value={languageCode ?? config.profile.defaultLanguageCode} onChange={(event) => onLanguageChange?.(event.target.value)}>
                {config.languages.map((language) => <option key={language.languageCode} value={language.languageCode}>{language.displayName}</option>)}
              </select>
            </label>
          ) : null}
        </div>
      </header>

      <main className="min-h-[calc(100vh-89px)] px-4 py-6 sm:px-8 sm:py-10" style={{ color: primaryColor ? undefined : undefined }}>
        <div className="mx-auto flex min-h-[calc(100vh-161px)] w-full max-w-7xl items-center justify-center">{children}</div>
      </main>
    </div>
  );
}
