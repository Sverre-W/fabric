import { ArrowRight, ShieldCheck } from 'lucide-react';
import { useAuth } from 'react-oidc-context';

import { FabricLogo } from '@/shared/branding/fabric-logo';
import { useBranding } from '@/shared/branding/branding-context';
import { Button } from '@/shared/components/ui/button';
import { appModules } from '@/shared/modules/app-modules';

import { ModuleCard } from './module-card';

export default function HomePage() {
  const auth = useAuth();
  const branding = useBranding();

  if (!auth.isAuthenticated) {
    return <PublicHomePage />;
  }

  return (
    <section className="grid gap-6">
      <div className="rounded-structural border border-border bg-content p-8">
        <p className="text-[14px] font-semibold uppercase text-primary">PIAM platform</p>
        <h1 className="mt-3 text-[32px] font-semibold tracking-tight">{branding.appName} modules</h1>
        <p className="mt-3 max-w-2xl text-[14px] text-muted-foreground">
          Select a module to manage a focused part of your physical identity and access workflows.
        </p>
      </div>
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {appModules.map((module) => (
          <ModuleCard key={module.id} module={module} />
        ))}
      </div>
    </section>
  );
}

function PublicHomePage() {
  const auth = useAuth();
  const branding = useBranding();

  return (
    <section className="grid gap-8 lg:grid-cols-[1.1fr_0.9fr] lg:items-stretch">
      <div className="relative overflow-hidden rounded-structural border border-border bg-content p-8 md:p-12">
        <div className="absolute right-[-80px] top-[-120px] size-72 rounded-full bg-primary/10" />
        <div className="relative max-w-2xl">
          <div className="flex items-center gap-3">
            <FabricLogo logoUrl={branding.logoUrl} />
            <span className="text-[18px] font-semibold tracking-tight">{branding.appName}</span>
          </div>
          <p className="mt-10 text-[14px] font-semibold uppercase tracking-wide text-primary">Physical identity and access management</p>
          <h1 className="mt-4 text-[42px] font-semibold leading-tight tracking-tight md:text-[56px]">
            Welcome to your visitor and access workspace.
          </h1>
          <p className="mt-5 max-w-xl text-[16px] leading-7 text-muted-foreground">
            Sign in to manage visits, identities, credentials, access policies, organizations, and audit workflows from one tenant-aware portal.
          </p>
          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Button type="button" onClick={() => void auth.signinRedirect({ state: { returnTo: '/' } })} className="h-12 px-6 text-[15px]">
              Sign in
              <ArrowRight className="ml-2 size-4" />
            </Button>
          </div>
        </div>
      </div>

      <div className="rounded-structural border border-border bg-content p-6 md:p-8">
        <div className="flex size-12 items-center justify-center rounded-interactive bg-active-blue text-primary">
          <ShieldCheck className="size-6" />
        </div>
        <h2 className="mt-6 text-[24px] font-semibold tracking-tight">Secure by default</h2>
        <p className="mt-3 text-[14px] leading-6 text-muted-foreground">
          Modules stay behind your tenant identity provider. Fabric starts authorization code flow with PKCE using settings loaded from tenant configuration.
        </p>
        <div className="mt-6 grid gap-3 text-[14px]">
          {['Tenant-specific sign in', 'OIDC code flow with PKCE', 'Protected module routes', 'Bearer tokens for API requests'].map((item) => (
            <div key={item} className="rounded-interactive bg-hover-blue px-4 py-3 font-medium text-foreground">
              {item}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
