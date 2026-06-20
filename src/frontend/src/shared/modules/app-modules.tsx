import { Settings, UsersRound } from 'lucide-react';
import type { ComponentType } from 'react';

export type ModuleNavigationItem = {
  label: string;
  to: string;
};

export type AppModule = {
  id: string;
  name: string;
  description: string;
  logo: ComponentType<{ className?: string }>;
  to: string;
  navigation: readonly ModuleNavigationItem[];
};

export const appModules = [
  {
    id: 'visitors-management',
    name: 'Visitors Management',
    description: 'Plan visits, manage visitor records, and review visitor activity reporting.',
    logo: UsersRound,
    to: '/visitors-management',
    navigation: [
      { label: 'Visits', to: '/visitors-management/visits' },
      { label: 'Visitors', to: '/visitors-management/visitors' },
      { label: 'Organizers', to: '/visitors-management/organizers' },
      { label: 'Reporting', to: '/visitors-management/reporting' },
    ],
  },
  {
    id: 'settings',
    name: 'Settings',
    description: 'Configure platform modules, defaults, integrations, and operational behavior.',
    logo: Settings,
    to: '/settings',
    navigation: [
      { label: 'Tenant', to: '/settings/tenant' },
      { label: 'Visitors', to: '/settings/visitors' },
    ],
  },
] as const satisfies readonly AppModule[];

export function getModuleById(moduleId: string) {
  return appModules.find((module) => module.id === moduleId);
}

export function getModuleByPathname(pathname: string) {
  return appModules.find((module) => pathname === module.to || pathname.startsWith(`${module.to}/`));
}
