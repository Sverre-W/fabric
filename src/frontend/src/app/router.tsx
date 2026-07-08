import { Navigate, Outlet, createRootRoute, createRoute, createRouter, redirect } from '@tanstack/react-router';
import { lazy, Suspense } from 'react';

import { AppLayout } from '@/shared/layout/app-layout';
import { ProtectedRoute } from '@/shared/auth/protected-route';
import { ReceptionKioskLayout } from '@/features/reception-kiosk/layout/reception-kiosk-layout';

const AccessPage = lazy(() => import('@/features/access/access-page'));
const AuditPage = lazy(() => import('@/features/audit/audit-page'));
const AuthCallbackPage = lazy(() => import('@/features/auth/auth-callback-page'));
const AutomationWorkflowDefinitionEditorPage = lazy(() => import('@/features/automation/workflow-definition-editor-page'));
const AutomationWorkflowInstanceViewerPage = lazy(() => import('@/features/automation/workflow-instance-viewer-page'));
const AutomationWorkflowPage = lazy(() => import('@/features/automation/workflow-page'));
const AutomationKioskPage = lazy(() => import('@/features/automation/kiosk-admin-page'));
const AutomationKioskEditPage = lazy(() => import('@/features/automation/kiosk-edit-page'));
const AutomationKioskProfileEditPage = lazy(() => import('@/features/automation/kiosk-profile-edit-page'));
const CardManagementChipDesignCreatePage = lazy(() => import('@/features/card-management/chip-design-create-page'));
const CardManagementChipDesignEditPage = lazy(() => import('@/features/card-management/chip-design-form-page'));
const CardManagementChipDesignerPage = lazy(() => import('@/features/card-management/chip-designer-page'));
const CardManagementKeyGroupCreatePage = lazy(() => import('@/features/card-management/key-group-create-page'));
const CardManagementKeyGroupEditPage = lazy(() => import('@/features/card-management/key-group-form-page'));
const CardManagementKeyManagementPage = lazy(() => import('@/features/card-management/key-management-page'));
const CardManagementPrintBatchCreatePage = lazy(() => import('@/features/card-management/print-batch-create-page'));
const CardManagementPrintBatchDetailPage = lazy(() => import('@/features/card-management/print-batch-detail-page'));
const CardManagementEncoderFormPage = lazy(() => import('@/features/card-management/encoder-form-page'));
const CardManagementPrintRunDetailPage = lazy(() => import('@/features/card-management/print-run-detail-page'));
const CardManagementPrintingPage = lazy(() => import('@/features/card-management/printing-page'));
const CardManagementStrategyCreatePage = lazy(() => import('@/features/card-management/diversification-strategy-create-page'));
const CardManagementStrategyEditPage = lazy(() => import('@/features/card-management/diversification-strategy-form-page'));
const CardManagementSystemProviderCreatePage = lazy(() => import('@/features/card-management/system-provider-create-page'));
const CardManagementTransformationCreatePage = lazy(() => import('@/features/card-management/transformation-create-page'));
const CardManagementTransformationEditPage = lazy(() => import('@/features/card-management/transformation-form-page'));
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
const KioskPage = lazy(() => import('@/features/kiosk/kiosk-page'));
const KioskSetupPage = lazy(() => import('@/features/kiosk/kiosk-setup-page'));
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

const kioskLayoutRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/kiosk',
  component: () => <Outlet />,
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

const automationRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/automation',
  component: () => (
    <ProtectedRoute>
      <Outlet />
    </ProtectedRoute>
  ),
});

const workflowsAliasRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/workflows',
  component: () => (
    <ProtectedRoute>
      <Outlet />
    </ProtectedRoute>
  ),
});

const automationIndexRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/',
  component: () => <Navigate to="/automation/workflow" search={{ tab: 'definitions' } as never} />,
});

const automationWorkflowRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/workflow',
  component: () => <LazyRoute component={<AutomationWorkflowPage />} />,
});

const automationWorkflowDefinitionsRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/workflow-definitions',
  component: () => <Navigate to="/automation/workflow" search={{ tab: 'definitions' } as never} />,
});

const automationWorkflowDefinitionEditorRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/workflow-definitions/$definitionId/edit',
  component: () => <LazyRoute component={<AutomationWorkflowDefinitionEditorPage />} />,
});

const automationWorkflowInstancesRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/workflow-instances',
  component: () => <Navigate to="/automation/workflow" search={{ tab: 'history' } as never} />,
});

const automationWorkflowInstanceViewerRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/workflow-instances/$instanceId',
  component: () => <LazyRoute component={<AutomationWorkflowInstanceViewerPage />} />,
});

const workflowDefinitionEditorAliasRoute = createRoute({
  getParentRoute: () => workflowsAliasRoute,
  path: '/definitions/$definitionId/edit',
  beforeLoad: ({ params }) => {
    throw redirect({
      to: '/automation/workflow-definitions/$definitionId/edit',
      params: { definitionId: params.definitionId },
      replace: true,
    });
  },
});

const workflowInstanceViewerAliasRoute = createRoute({
  getParentRoute: () => workflowsAliasRoute,
  path: '/instances/$instanceId/view',
  beforeLoad: ({ params }) => {
    throw redirect({
      to: '/automation/workflow-instances/$instanceId',
      params: { instanceId: params.instanceId },
      replace: true,
    });
  },
});

const automationKioskRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/kiosk',
  component: () => <LazyRoute component={<AutomationKioskPage />} />,
});

const automationKioskEditRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/kiosk/$kioskId/edit',
  component: () => <LazyRoute component={<AutomationKioskEditPage />} />,
});

const automationKioskProfileEditRoute = createRoute({
  getParentRoute: () => automationRoute,
  path: '/kiosk/profiles/$profileId/edit',
  component: () => <LazyRoute component={<AutomationKioskProfileEditPage />} />,
});

const cardManagementRoute = createRoute({
  getParentRoute: () => mainLayoutRoute,
  path: '/card-management',
  component: () => (
    <ProtectedRoute>
      <Outlet />
    </ProtectedRoute>
  ),
});

const cardManagementIndexRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/',
  component: () => <Navigate to="/card-management/key-management" />,
});

const cardManagementKeyManagementRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/key-management',
  component: () => <LazyRoute component={<CardManagementKeyManagementPage />} />,
});

const cardManagementChipDesignerRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/chip-designer',
  component: () => <LazyRoute component={<CardManagementChipDesignerPage />} />,
});

const cardManagementChipDesignCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/chip-designs/new',
  component: () => <LazyRoute component={<CardManagementChipDesignCreatePage />} />,
});

const cardManagementChipDesignEditRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/chip-designs/$chipDesignId/edit',
  component: () => <LazyRoute component={<CardManagementChipDesignEditPage />} />,
});

const cardManagementKeyGroupCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/key-groups/new',
  component: () => <LazyRoute component={<CardManagementKeyGroupCreatePage />} />,
});

const cardManagementKeyGroupEditRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/key-groups/$keyGroupId/edit',
  component: () => <LazyRoute component={<CardManagementKeyGroupEditPage />} />,
});

const cardManagementStrategyCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/diversification-strategies/new',
  component: () => <LazyRoute component={<CardManagementStrategyCreatePage />} />,
});

const cardManagementStrategyEditRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/diversification-strategies/$strategyId/edit',
  component: () => <LazyRoute component={<CardManagementStrategyEditPage />} />,
});

const cardManagementTransformationCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/transformations/new',
  component: () => <LazyRoute component={<CardManagementTransformationCreatePage />} />,
});

const cardManagementTransformationEditRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/transformations/$transformationId/edit',
  component: () => <LazyRoute component={<CardManagementTransformationEditPage />} />,
});

const cardManagementSystemProviderCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/system-providers/new',
  component: () => <LazyRoute component={<CardManagementSystemProviderCreatePage />} />,
});

const cardManagementPrintingRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing',
  component: () => <LazyRoute component={<CardManagementPrintingPage />} />,
});

const cardManagementPrintBatchCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing/new',
  component: () => <LazyRoute component={<CardManagementPrintBatchCreatePage />} />,
});

const cardManagementPrintBatchDetailRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing/$batchId',
  component: () => <LazyRoute component={<CardManagementPrintBatchDetailPage />} />,
});

const cardManagementEncoderCreateRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing/encoders/new',
  component: () => <LazyRoute component={<CardManagementEncoderFormPage />} />,
});

const cardManagementEncoderEditRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing/encoders/$encoderId/edit',
  component: () => <LazyRoute component={<CardManagementEncoderFormPage />} />,
});

const cardManagementPrintRunDetailRoute = createRoute({
  getParentRoute: () => cardManagementRoute,
  path: '/printing/runs/$runId',
  component: () => <LazyRoute component={<CardManagementPrintRunDetailPage />} />,
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

const kioskIndexRoute = createRoute({
  getParentRoute: () => kioskLayoutRoute,
  path: '/',
  component: () => <LazyRoute component={<KioskPage />} />,
});

const kioskSetupRoute = createRoute({
  getParentRoute: () => kioskLayoutRoute,
  path: '/setup',
  component: () => <LazyRoute component={<KioskSetupPage />} />,
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
    automationRoute.addChildren([
      automationIndexRoute,
      automationWorkflowRoute,
      automationWorkflowDefinitionsRoute,
      automationWorkflowDefinitionEditorRoute,
      automationWorkflowInstancesRoute,
      automationWorkflowInstanceViewerRoute,
      automationKioskRoute,
      automationKioskEditRoute,
      automationKioskProfileEditRoute,
    ]),
    workflowsAliasRoute.addChildren([
      workflowDefinitionEditorAliasRoute,
      workflowInstanceViewerAliasRoute,
    ]),
    cardManagementRoute.addChildren([
      cardManagementIndexRoute,
      cardManagementKeyManagementRoute,
      cardManagementChipDesignerRoute,
      cardManagementChipDesignCreateRoute,
      cardManagementChipDesignEditRoute,
      cardManagementKeyGroupCreateRoute,
      cardManagementKeyGroupEditRoute,
      cardManagementStrategyCreateRoute,
      cardManagementStrategyEditRoute,
      cardManagementTransformationCreateRoute,
      cardManagementTransformationEditRoute,
      cardManagementSystemProviderCreateRoute,
      cardManagementPrintingRoute,
      cardManagementPrintBatchCreateRoute,
      cardManagementPrintBatchDetailRoute,
      cardManagementEncoderCreateRoute,
      cardManagementEncoderEditRoute,
      cardManagementPrintRunDetailRoute,
    ]),
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
  kioskLayoutRoute.addChildren([kioskIndexRoute, kioskSetupRoute]),
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
