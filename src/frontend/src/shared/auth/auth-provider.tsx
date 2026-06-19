import { AuthProvider } from 'react-oidc-context';
import { WebStorageStateStore } from 'oidc-client-ts';
import type { ReactNode } from 'react';

import type { TenantSettings } from '@/shared/tenant/tenant-settings';

export function FabricAuthProvider({ tenantSettings, children }: { tenantSettings: TenantSettings; children: ReactNode }) {
  const origin = window.location.origin;

  return (
    <AuthProvider
      authority={getAuthority(tenantSettings.oidc.metadataUrl)}
      metadataUrl={tenantSettings.oidc.metadataUrl}
      client_id={tenantSettings.oidc.clientId}
      redirect_uri={`${origin}/auth/callback`}
      post_logout_redirect_uri={`${origin}/`}
      response_type="code"
      scope="openid profile email"
      automaticSilentRenew
      userStore={new WebStorageStateStore({ store: window.localStorage })}
      onSigninCallback={(user) => {
        window.history.replaceState({}, document.title, getReturnTo(user?.state));
      }}
    >
      {children}
    </AuthProvider>
  );
}

function getAuthority(metadataUrl: string): string {
  const url = new URL(metadataUrl);
  const wellKnownPath = '/.well-known/openid-configuration';

  if (url.pathname.endsWith(wellKnownPath)) {
    url.pathname = url.pathname.slice(0, -wellKnownPath.length) || '/';
    url.search = '';
    url.hash = '';
  }

  return url.toString().replace(/\/$/, '');
}

function getReturnTo(state: unknown): string {
  if (isRecord(state) && typeof state.returnTo === 'string' && state.returnTo.startsWith('/')) {
    return state.returnTo;
  }

  return '/';
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}
