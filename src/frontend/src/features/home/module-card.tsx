import { Link } from '@tanstack/react-router';

import type { AppModule } from '@/shared/modules/app-modules';

export function ModuleCard({ module }: { module: AppModule }) {
  const Logo = module.logo;

  return (
    <Link
      to={module.to}
      className="group rounded-interactive border border-border bg-content p-4 transition hover:border-primary hover:bg-hover-blue sm:p-6"
    >
      <div className="flex items-start gap-4">
        <div className="flex size-12 shrink-0 items-center justify-center rounded-interactive bg-active-blue text-primary transition group-hover:bg-primary group-hover:text-white">
          <Logo className="size-6" />
        </div>
        <div>
          <h2 className="text-[20px] font-semibold tracking-tight">{module.name}</h2>
          <p className="mt-2 text-[14px] leading-6 text-muted-foreground">{module.description}</p>
        </div>
      </div>
    </Link>
  );
}
