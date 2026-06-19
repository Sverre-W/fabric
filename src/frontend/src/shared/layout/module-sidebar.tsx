import { Link, useLocation } from '@tanstack/react-router';

import {
  Sidebar,
  SidebarClose,
  SidebarContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
} from '@/shared/components/ui/sidebar';
import type { AppModule } from '@/shared/modules/app-modules';

import { ModuleSelector } from './module-selector';

export function ModuleSidebar({ module }: { module: AppModule }) {
  return (
    <Sidebar className="rounded-none md:min-h-[calc(100vh-73px)] md:border-r md:border-border md:bg-content">
      <SidebarHeader>
        <div className="mb-3 flex items-center justify-between md:hidden">
          <p className="text-sm font-semibold">Navigation</p>
          <SidebarClose />
        </div>
        <ModuleSelector />
      </SidebarHeader>
      <SidebarContent>
        <ModuleMenu module={module} />
      </SidebarContent>
    </Sidebar>
  );
}

function ModuleMenu({ module }: { module: AppModule }) {
  const location = useLocation();

  return (
    <SidebarMenu aria-label={`${module.name} menu`}>
      {module.navigation.map((item) => {
        const isActive = location.pathname === item.to || (location.pathname === module.to && item.to === module.navigation[0]?.to);

        return (
          <Link key={item.to} to={item.to}>
            <SidebarMenuButton isActive={isActive}>{item.label}</SidebarMenuButton>
          </Link>
        );
      })}
    </SidebarMenu>
  );
}
