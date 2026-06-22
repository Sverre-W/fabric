import { Outlet, createRootRoute, createRoute, createRouter } from '@tanstack/react-router';
import { lazy, Suspense } from 'react';

import { AppLayout } from '@/shared/layout/app-layout';
import { ProtectedRoute } from '@/shared/auth/protected-route';

const AccessPage = lazy(() => import('@/features/access/access-page'));
const AuditPage = lazy(() => import('@/features/audit/audit-page'));
const AuthCallbackPage = lazy(() => import('@/features/auth/auth-callback-page'));
const CredentialsPage = lazy(() => import('@/features/credentials/credentials-page'));
const FacilityBuildingEditPage = lazy(() => import('@/features/facility/building-edit-page'));
const FacilityRoomEditPage = lazy(() => import('@/features/facility/room-edit-page'));
const FacilitySiteCreatePage = lazy(() => import('@/features/facility/site-create-page'));
const FacilitySiteEditPage = lazy(() => import('@/features/facility/site-edit-page'));
const FacilityLocationsPage = lazy(() => import('@/features/facility/locations-page'));
const HomePage = lazy(() => import('@/features/home/home-page'));
const IdentitiesPage = lazy(() => import('@/features/identities/identities-page'));
const OrganizationsPage = lazy(() => import('@/features/organizations/organizations-page'));
const ReceptionDeskPage = lazy(() => import('@/features/reception-desk/reception-desk-page'));
const SettingsLayout = lazy(() => import('@/features/settings/settings-layout'));
const ReceptionDeskSettingsPage = lazy(() => import('@/features/settings/reception-desk-settings-page'));
const TenantSettingsPage = lazy(() => import('@/features/settings/tenant-settings-page'));
const VisitorsSettingsPage = lazy(() => import('@/features/settings/visitors-settings-page'));
const VisitorsManagementLayout = lazy(() => import('@/features/visitors-management/visitors-management-layout'));
const OrganizerCreatePage = lazy(() => import('@/features/visitors-management/organizer-create-page'));
const OrganizerEditPage = lazy(() => import('@/features/visitors-management/organizer-edit-page'));
const OrganizersPage = lazy(() => import('@/features/visitors-management/organizers-page'));
const VisitorReportingPage = lazy(() => import('@/features/visitors-management/reporting-page'));
const VisitCreatePage = lazy(() => import('@/features/visitors-management/visit-create-page'));
const VisitEditPage = lazy(() => import('@/features/visitors-management/visit-edit-page'));
const VisitorsPage = lazy(() => import('@/features/visitors-management/visitors-page'));
const VisitsPage = lazy(() => import('@/features/visitors-management/visits-page'));

const rootRoute = createRootRoute({
  component: () => (
    <AppLayout>
      <Outlet />
    </AppLayout>
  ),
});

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: () => <LazyRoute component={<HomePage />} />,
});

const authCallbackRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/auth/callback',
  component: () => <LazyRoute component={<AuthCallbackPage />} />,
});

const identitiesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/identities',
  component: () => <ProtectedLazyRoute component={<IdentitiesPage />} />,
});

const accessRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/access',
  component: () => <ProtectedLazyRoute component={<AccessPage />} />,
});

const credentialsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/credentials',
  component: () => <ProtectedLazyRoute component={<CredentialsPage />} />,
});

const organizationsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/organizations',
  component: () => <ProtectedLazyRoute component={<OrganizationsPage />} />,
});

const auditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/audit',
  component: () => <ProtectedLazyRoute component={<AuditPage />} />,
});

const receptionDeskRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/reception-desk',
  component: () => <ProtectedLazyRoute component={<ReceptionDeskPage />} />,
});

const facilityRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/facility',
  component: () => (
    <ProtectedRoute>
      <Outlet />
    </ProtectedRoute>
  ),
});

const facilityIndexRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/',
  component: () => <LazyRoute component={<FacilityLocationsPage />} />,
});

const facilityLocationsRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/locations',
  component: () => <LazyRoute component={<FacilityLocationsPage />} />,
});

const facilitySiteCreateRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/locations/new',
  component: () => <LazyRoute component={<FacilitySiteCreatePage />} />,
});

const facilitySiteEditRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/locations/$siteId/edit',
  component: () => <LazyRoute component={<FacilitySiteEditPage />} />,
});

const facilityBuildingEditRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/locations/$siteId/buildings/$buildingId/edit',
  component: () => <LazyRoute component={<FacilityBuildingEditPage />} />,
});

const facilityRoomEditRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/locations/$siteId/buildings/$buildingId/rooms/$roomId/edit',
  component: () => <LazyRoute component={<FacilityRoomEditPage />} />,
});

const settingsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/settings',
  component: () => <ProtectedLazyRoute component={<SettingsLayout />} />,
});

const settingsIndexRoute = createRoute({
  getParentRoute: () => settingsRoute,
  path: '/',
  component: () => <LazyRoute component={<TenantSettingsPage />} />,
});

const visitorsSettingsRoute = createRoute({
  getParentRoute: () => settingsRoute,
  path: '/visitors',
  component: () => <LazyRoute component={<VisitorsSettingsPage />} />,
});

const receptionDeskSettingsRoute = createRoute({
  getParentRoute: () => settingsRoute,
  path: '/reception-desk',
  component: () => <LazyRoute component={<ReceptionDeskSettingsPage />} />,
});

const tenantSettingsRoute = createRoute({
  getParentRoute: () => settingsRoute,
  path: '/tenant',
  component: () => <LazyRoute component={<TenantSettingsPage />} />,
});

const visitorsManagementRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/visitors-management',
  component: () => <ProtectedLazyRoute component={<VisitorsManagementLayout />} />,
});

const visitsIndexRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/',
  component: () => <LazyRoute component={<VisitsPage />} />,
});

const visitsRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/visits',
  component: () => <LazyRoute component={<VisitsPage />} />,
});

const visitCreateRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/visits/new',
  component: () => <LazyRoute component={<VisitCreatePage />} />,
});

const visitorsRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/visitors',
  component: () => <LazyRoute component={<VisitorsPage />} />,
});

const organizersRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/organizers',
  component: () => <LazyRoute component={<OrganizersPage />} />,
});

const organizerCreateRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/organizers/new',
  component: () => <LazyRoute component={<OrganizerCreatePage />} />,
});

const organizerEditRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/organizers/$organizerId/edit',
  component: () => <LazyRoute component={<OrganizerEditPage />} />,
});

const visitEditRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/visits/$visitId/edit',
  component: () => <LazyRoute component={<VisitEditPage />} />,
});

const visitorReportingRoute = createRoute({
  getParentRoute: () => visitorsManagementRoute,
  path: '/reporting',
  component: () => <LazyRoute component={<VisitorReportingPage />} />,
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  authCallbackRoute,
  identitiesRoute,
  accessRoute,
  credentialsRoute,
  organizationsRoute,
  auditRoute,
  receptionDeskRoute,
  facilityRoute.addChildren([facilityIndexRoute, facilityLocationsRoute, facilitySiteCreateRoute, facilitySiteEditRoute, facilityBuildingEditRoute, facilityRoomEditRoute]),
  settingsRoute.addChildren([settingsIndexRoute, visitorsSettingsRoute, receptionDeskSettingsRoute, tenantSettingsRoute]),
  visitorsManagementRoute.addChildren([visitsIndexRoute, visitsRoute, visitCreateRoute, visitEditRoute, visitorsRoute, organizersRoute, organizerCreateRoute, organizerEditRoute, visitorReportingRoute]),
]);

export function createAppRouter() {
  return createRouter({ routeTree });
}

export const router = createAppRouter();

export type AppRouter = typeof router;

declare module '@tanstack/react-router' {
  interface Register {
    router: AppRouter;
  }
}

function LazyRoute({ component }: { component: React.ReactNode }) {
  return <Suspense fallback={<RouteFallback />}>{component}</Suspense>;
}

function ProtectedLazyRoute({ component }: { component: React.ReactNode }) {
  return (
    <ProtectedRoute>
      <LazyRoute component={component} />
    </ProtectedRoute>
  );
}

function RouteFallback() {
  return <div className="rounded-structural border border-border bg-content p-6 text-[14px] text-muted-foreground">Loading...</div>;
}
