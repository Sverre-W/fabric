import { type ReactNode } from 'react';
import { useAuth } from 'react-oidc-context';

import { elsaApiBaseUrl } from './automation-settings';
import { useElsaStudioAssets } from './elsa-studio-assets';

export type ElsaRuntimeProps = {
  readonly remoteEndpoint: string;
  readonly accessToken?: string;
};

export function AutomationPageShell({ title, description, children }: { title: string; description: string; children: (props: ElsaRuntimeProps) => ReactNode }) {
  const auth = useAuth();
  useElsaStudioAssets();

  return (
    <section className="grid gap-4">
      <div className="rounded-structural border border-border bg-content p-4 sm:p-6">
        <p className="text-[14px] font-semibold uppercase text-primary">Automation</p>
        <h1 className="mt-2 text-[24px] font-semibold tracking-tight">{title}</h1>
        <p className="mt-2 max-w-3xl text-[14px] text-muted-foreground">{description}</p>
      </div>

      <div className="min-h-[42rem] overflow-hidden rounded-structural border border-border bg-content p-2 sm:p-3">
        {children({ remoteEndpoint: elsaApiBaseUrl, accessToken: auth.user?.access_token })}
      </div>
    </section>
  );
}
