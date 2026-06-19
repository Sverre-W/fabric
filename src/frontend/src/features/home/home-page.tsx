import { appModules } from '@/shared/modules/app-modules';

import { ModuleCard } from './module-card';

export default function HomePage() {
  return (
    <section className="grid gap-6">
      <div className="rounded-structural border border-border bg-content p-8">
        <p className="text-[14px] font-semibold uppercase text-primary">PIAM platform</p>
        <h1 className="mt-3 text-[32px] font-semibold tracking-tight">Fabric modules</h1>
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
