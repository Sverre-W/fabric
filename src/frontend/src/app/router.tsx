import { Outlet, createRootRoute, createRoute, createRouter } from '@tanstack/react-router';
import { lazy, Suspense } from 'react';

import { AppLayout } from '@/shared/layout/app-layout';
import { ProtectedRoute } from '@/shared/auth/protected-route';
import { ReceptionKioskLayout } from '@/features/reception-kiosk/layout/reception-kiosk-layout';

const AccessPage = lazy(() => import('@/features/access/access-page'));
const AuditPage = lazy(() => import('@/features/audit/audit-page'));
const AuthCallbackPage = lazy(() => import('@/features/auth/auth-callback-page'));
const CredentialsPage = lazy(() => import('@/features/credentials/credentials-page'));
const FacilityAccessControlEditPage = lazy(() => import('@/features/facility/access-control-edit-page'));
const FacilityAccessControlPage = lazy(() => import('@/features/facility/access-control-page'));
const FacilityHardwareAgentDetailPage = lazy(() => import('@/features/facility/hardware-agent-detail-page'));
const FacilityBuildingEditPage = lazy(() => import('@/features/facility/building-edit-page'));
const FacilityHardwarePage = lazy(() => import('@/features/facility/hardware-page'));
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
const ReceptionKioskArrivalPage = lazy(() => import('@/features/reception-kiosk/reception-kiosk-arrival-page'));
const ReceptionKioskNoRegistrationPage = lazy(() => import('@/features/reception-kiosk/reception-kiosk-no-registration-page'));
const ReceptionKioskPage = lazy(() => import('@/features/reception-kiosk/reception-kiosk-page'));
const ReceptionKioskScanQrPage = lazy(() => import('@/features/reception-kiosk/reception-kiosk-scan-qr-page'));
const ReceptionKioskSetupPage = lazy(() => import('@/features/reception-kiosk/reception-kiosk-setup-page'));
const TenantSettingsPage = lazy(() => import('@/features/settings/tenant-settings-page'));
const VisitorsSettingsPage = lazy(() => import('@/features/settings/visitors-settings-page'));
const VisitorsManagementLayout = lazy(() => import('@/features/visitors-management/visitors-management-layout'));
const VisitorConfirmationPage = lazy(() => import('@/features/visitor-confirmation/visitor-confirmation-page'));
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
    <Outlet />
  ),
});

const mainLayoutRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: 'main',
  component: () => (
    <AppLayout>
      <Outlet />
    </AppLayout>
  ),
});

const receptionKioskLayoutRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/reception-kiosk',
  component: () => (
    <ReceptionKioskLayout>
      <Outlet />
    </ReceptionKioskLayout>
  ),
});

const indexRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/',
  component: () => <LazyRoute component={<HomePage />} />,
});

const authCallbackRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/auth/callback',
  component: () => <LazyRoute component={<AuthCallbackPage />} />,
});

const identitiesRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/identities',
  component: () => <ProtectedLazyRoute component={<IdentitiesPage />} />,
});

const accessRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/access',
  component: () => <ProtectedLazyRoute component={<AccessPage />} />,
});

const credentialsRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/credentials',
  component: () => <ProtectedLazyRoute component={<CredentialsPage />} />,
});

const organizationsRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/organizations',
  component: () => <ProtectedLazyRoute component={<OrganizationsPage />} />,
});

const auditRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/audit',
  component: () => <ProtectedLazyRoute component={<AuditPage />} />,
});

const receptionDeskRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/reception-desk',
  component: () => <ProtectedLazyRoute component={<ReceptionDeskPage />} />,
});

const visitorConfirmationRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/visitor-confirmation/$visitId/$invitationId',
  component: () => <LazyRoute component={<VisitorConfirmationPage />} />,
});

const facilityRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
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

const facilityAccessControlRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/access-control',
  component: () => <LazyRoute component={<FacilityAccessControlPage />} />,
});

const facilityHardwareRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/hardware',
  component: () => <LazyRoute component={<FacilityHardwarePage />} />,
});

const facilityHardwareAgentDetailRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/hardware/$agentId',
  component: () => <LazyRoute component={<FacilityHardwareAgentDetailPage />} />,
});

const facilityAccessControlEditRoute = createRoute({
  getParentRoute: () => facilityRoute,
  path: '/access-control/$systemId/edit',
  component: () => <LazyRoute component={<FacilityAccessControlEditPage />} />,
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
  getParentRoute: () => mainLayoutRoute,
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
  getParentRoute: () => mainLayoutRoute,
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

const receptionKioskIndexRoute = createRoute({
  getParentRoute: () => receptionKioskLayoutRoute,
  path: '/',
  component: () => <LazyRoute component={<ReceptionKioskPage />} />,
});

const receptionKioskSetupRoute = createRoute({
  getParentRoute: () => receptionKioskLayoutRoute,
  path: '/setup',
  component: () => <LazyRoute component={<ReceptionKioskSetupPage />} />,
});

const receptionKioskScanQrRoute = createRoute({
  getParentRoute: () => receptionKioskLayoutRoute,
  path: '/scan-qr',
  component: () => <LazyRoute component={<ReceptionKioskScanQrPage />} />,
});

const receptionKioskArrivalRoute = createRoute({
  getParentRoute: () => receptionKioskLayoutRoute,
  path: '/arrival',
  component: () => <LazyRoute component={<ReceptionKioskArrivalPage />} />,
});

const receptionKioskNoRegistrationRoute = createRoute({
  getParentRoute: () => receptionKioskLayoutRoute,
  path: '/no-registration',
  component: () => <LazyRoute component={<ReceptionKioskNoRegistrationPage />} />,
});

const routeTree = rootRoute.addChildren([
  mainLayoutRoute.addChildren([
    indexRoute,
    authCallbackRoute,
    identitiesRoute,
    accessRoute,
    credentialsRoute,
    organizationsRoute,
    auditRoute,
    receptionDeskRoute,
    visitorConfirmationRoute,
    facilityRoute.addChildren([
      facilityIndexRoute,
      facilityLocationsRoute,
      facilityAccessControlRoute,
      facilityHardwareRoute,
      facilityHardwareAgentDetailRoute,
      facilityAccessControlEditRoute,
      facilitySiteCreateRoute,
      facilitySiteEditRoute,
      facilityBuildingEditRoute,
      facilityRoomEditRoute,
    ]),
    settingsRoute.addChildren([settingsIndexRoute, visitorsSettingsRoute, receptionDeskSettingsRoute, tenantSettingsRoute]),
    visitorsManagementRoute.addChildren([visitsIndexRoute, visitsRoute, visitCreateRoute, visitEditRoute, visitorsRoute, organizersRoute, organizerCreateRoute, organizerEditRoute, visitorReportingRoute]),
  ]),
  receptionKioskLayoutRoute.addChildren([receptionKioskIndexRoute, receptionKioskSetupRoute, receptionKioskScanQrRoute, receptionKioskArrivalRoute, receptionKioskNoRegistrationRoute]),
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
