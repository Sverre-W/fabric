import { Building2, ConciergeBell, Settings, UsersRound, Workflow } from 'lucide-react';
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
    id: 'facility',
    name: 'Facility',
    description: 'Manage physical locations, sites, buildings, and room inventory.',
    logo: Building2,
    to: '/facility',
    navigation: [
      { label: 'Locations', to: '/facility/locations' },
      { label: 'Access Control', to: '/facility/access-control' },
      { label: 'Hardware', to: '/facility/hardware' },
    ],
  },
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
    id: 'automation',
    name: 'Automation',
    description: 'Design workflow definitions, inspect workflow instances, and manage process automation.',
    logo: Workflow,
    to: '/automation',
    navigation: [
      { label: 'Workflow Definitions', to: '/automation/workflow-definitions' },
      { label: 'Workflow Instances', to: '/automation/workflow-instances' },
    ],
  },
  {
    id: 'reception-desk',
    name: 'Reception Desk',
    description: 'Handle front desk arrival workflows, expected visitors, and reception history.',
    logo: ConciergeBell,
    to: '/reception-desk',
    navigation: [{ label: 'Arrivals', to: '/reception-desk' }],
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
      { label: 'Reception Desk', to: '/settings/reception-desk' },
    ],
  },
] as const satisfies readonly AppModule[];

export function getModuleById(moduleId: string) {
  return appModules.find((module) => module.id === moduleId);
}

export function getModuleByPathname(pathname: string) {
  return appModules.find((module) => pathname === module.to || pathname.startsWith(`${module.to}/`));
}
