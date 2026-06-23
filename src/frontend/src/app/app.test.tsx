import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { App } from './app';
import { createAppRouter } from './router';

const apiGetMock = vi.hoisted(() => vi.fn());
const apiPostMock = vi.hoisted(() => vi.fn());
const apiPutMock = vi.hoisted(() => vi.fn());
const apiDeleteMock = vi.hoisted(() => vi.fn());
const signinRedirectMock = vi.hoisted(() => vi.fn());
const signoutRedirectMock = vi.hoisted(() => vi.fn());
const removeUserMock = vi.hoisted(() => vi.fn());
const authState = vi.hoisted(() => ({
  isAuthenticated: true,
  isLoading: false,
  activeNavigator: undefined as string | undefined,
  error: undefined as Error | undefined,
  user: { access_token: 'access-token' } as { access_token: string } | undefined,
}));

vi.mock('@/shared/api/client', () => ({
  apiBaseUrl: 'http://localhost:5245',
  setAccessToken: vi.fn(),
  api: {
    GET: apiGetMock,
    POST: apiPostMock,
    PUT: apiPutMock,
    DELETE: apiDeleteMock,
  },
}));

vi.mock('react-oidc-context', () => ({
  AuthProvider: ({ children }: { children: ReactNode }) => <>{children}</>,
  useAuth: () => ({
    ...authState,
    signinRedirect: signinRedirectMock,
    signoutRedirect: signoutRedirectMock,
    removeUser: removeUserMock,
  }),
}));

const emptyVisitPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 250,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const emptySitePage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 250,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const emptyAccessRuleAssignmentPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 100,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const emptyAccessControlSystemPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 100,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const emptyAccessPolicyPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 10,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const emptyIdentityMappingPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 10,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

const tenantSettingsResponse = {
  oidc: {
    metadataUrl: 'http://localhost:7080/realms/dev/.well-known/openid-configuration',
    clientId: 'portal',
    requireHttpsMetadata: false,
  },
  theme: {
    backgroundColor: '#f8f8f8',
    contentColor: '#ffffff',
    primaryColor: '#238cff',
    textColor: '#212529',
    textMutedColor: '#6c757d',
    borderColor: '#dddddd',
    hoverBlueColor: '#eef6ff',
    activeBlueColor: '#deeeff',
    hoverGrayColor: '#f3f3f3',
    errorColor: '#ff6467',
    errorBackgroundColor: '#feeaea',
    dangerColor: '#ff6467',
    successColor: '#00c950',
    successBackgroundColor: '#e6faeb',
  },
  logo: null,
};

describe('App', () => {
  beforeEach(() => {
    apiGetMock.mockReset();
    apiPostMock.mockReset();
    apiPutMock.mockReset();
    apiDeleteMock.mockReset();
    signinRedirectMock.mockReset();
    signoutRedirectMock.mockReset();
    removeUserMock.mockReset();
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/sites') {
        return Promise.resolve({ data: emptySitePage });
      }

      if (path === '/api/locations/sites/{siteId}/buildings') {
        return Promise.resolve({ data: [] });
      }

      if (path === '/api/locations/sites/{siteId}/buildings/{buildingId}/rooms') {
        return Promise.resolve({ data: [] });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({ data: undefined, error: undefined });
      }

      if (path === '/api/reception/access-rule-assignments') {
        return Promise.resolve({ data: emptyAccessRuleAssignmentPage });
      }

      if (path === '/api/access-policies/access-control-systems') {
        return Promise.resolve({ data: emptyAccessControlSystemPage });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });
    apiPostMock.mockResolvedValue({ data: undefined });
    apiPutMock.mockResolvedValue({ data: undefined });
    apiDeleteMock.mockResolvedValue({ data: undefined });
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    authState.isAuthenticated = true;
    authState.isLoading = false;
    authState.activeNavigator = undefined;
    authState.error = undefined;
    authState.user = { access_token: 'access-token' };
    window.sessionStorage.clear();
    window.history.pushState({}, '', '/');
  });

  it('renders public home page when unauthenticated', async () => {
    authState.isAuthenticated = false;
    authState.user = undefined;

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('link', { name: /fabric home/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /select module/i })).not.toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: /welcome to your visitor and access workspace/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('renders module launcher on home page when authenticated', async () => {
    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /fabric modules/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /facility/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /visitors management/i })).toBeInTheDocument();
  });

  it('renders Locations for Facility module root', async () => {
    window.history.pushState({}, '', '/facility');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('button', { name: /facility/i })).toBeInTheDocument();
    expect(await screen.findByRole('navigation', { name: /facility menu/i })).toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: /locations/i })).toBeInTheDocument();
  });

  it('renders Locations for explicit locations sub-route', async () => {
    window.history.pushState({}, '', '/facility/locations');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /locations/i })).toBeInTheDocument();
  });

  it('renders sites on the Locations page', async () => {
    window.history.pushState({}, '', '/facility/locations');

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/sites') {
        return Promise.resolve({
          data: {
            ...emptySitePage,
            totalItems: 1,
            items: [{ id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' }],
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings') {
        return Promise.resolve({ data: [] });
      }

      if (path === '/api/locations/sites/{siteId}/buildings/{buildingId}/rooms') {
        return Promise.resolve({ data: [] });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect((await screen.findAllByText('Oslo HQ')).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Karl Johans gate 1').length).toBeGreaterThan(0);
    expect(screen.getByRole('link', { name: /add site/i })).toBeInTheDocument();
    expect(screen.getAllByRole('link', { name: /edit oslo hq/i }).length).toBeGreaterThan(0);
  });

  it('renders Access Control systems for Facility module', async () => {
    window.history.pushState({}, '', '/facility/access-control');

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/access-policies/access-control-systems') {
        return Promise.resolve({
          data: {
            ...emptyAccessControlSystemPage,
            totalItems: 1,
            items: [
              {
                type: 'unipass',
                id: 'system-1',
                name: 'Unipass HQ',
                endpoint: 'https://unipass.example.test',
                sslValidation: true,
                hasSecret: true,
                username: 'api-user',
                badgeTypes: [{ type: 'unipass', id: 'badge-1', systemId: 'system-1', name: 'Visitor badge', rangeStart: 1, rangeStop: 100 }],
                accessLevels: [{ type: 'unipass', id: 'level-1', systemId: 'system-1', name: 'Lobby access', siteId: 1, accessRuleId: 100 }],
              },
            ],
          },
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /access control/i })).toBeInTheDocument();
    expect(await screen.findAllByText('Unipass HQ')).toHaveLength(2);
    expect(screen.getAllByText('Unipass').length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: /register system/i })).toBeInTheDocument();
  });

  it('registers an Access Control system', async () => {
    window.history.pushState({}, '', '/facility/access-control');
    apiPostMock.mockResolvedValue({
      data: {
        type: 'unipass',
        id: 'system-1',
        name: 'Unipass HQ',
        endpoint: 'https://unipass.example.test',
        sslValidation: true,
        hasSecret: true,
        username: 'api-user',
        badgeTypes: [],
        accessLevels: [],
      },
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /access control/i })).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /register system/i }));
    fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'Unipass HQ' } });
    fireEvent.change(screen.getByLabelText('Endpoint'), { target: { value: 'https://unipass.example.test' } });
    fireEvent.change(screen.getByLabelText('Username'), { target: { value: 'api-user' } });
    fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'secret' } });
    fireEvent.click(screen.getByRole('button', { name: /register access control system/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/access-policies/access-control-systems', {
        body: {
          type: 'unipass',
          name: 'Unipass HQ',
          endpoint: 'https://unipass.example.test',
          sslValidation: true,
          username: 'api-user',
          password: 'secret',
        },
      });
    });
  });

  it('renders Access Control edit panels and creates metadata-backed access rule', async () => {
    window.history.pushState({}, '', '/facility/access-control/system-1/edit');

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/access-policies/access-control-systems/{systemId}') {
        return Promise.resolve({
          data: {
            type: 'unipass',
            id: 'system-1',
            name: 'Unipass HQ',
            endpoint: 'https://unipass.example.test',
            sslValidation: true,
            hasSecret: true,
            username: 'api-user',
            badgeTypes: [{ type: 'unipass', id: 'badge-1', systemId: 'system-1', name: 'Visitor badge', rangeStart: 1, rangeStop: 100 }],
            accessLevels: [],
          },
        });
      }

      if (path === '/api/access-policies/access-control-systems/{systemId}/metadata') {
        return Promise.resolve({
          data: {
            type: 'unipass',
            sites: [{ id: '10', name: 'Main campus' }],
            accessRules: [{ id: '200', name: 'Lobby access' }],
          },
        });
      }

      if (path === '/api/access-policies/policies') {
        return Promise.resolve({
          data: {
            ...emptyAccessPolicyPage,
            totalItems: 1,
            items: [
              {
                id: 'policy-1',
                systemId: 'system-1',
                subject: { id: 'subject-1', firstName: 'Ada', lastName: 'Lovelace', subjectType: 'Employee' },
                effectiveFrom: '2026-06-01T00:00:00Z',
                effectiveUntil: '2026-07-01T00:00:00Z',
                requirement: { type: 'credential', badgeType: { type: 'unipass', id: 'badge-1', systemId: 'system-1', name: 'Visitor badge', rangeStart: 1, rangeStop: 100 }, badgeNumber: null },
                reconciliationStatus: 'Reconciled',
                reconciliationFailureReason: null,
              },
            ],
          },
        });
      }

      if (path === '/api/access-policies/access-control-systems/{systemId}/identity-mappings') {
        return Promise.resolve({
          data: {
            ...emptyIdentityMappingPage,
            totalItems: 1,
            items: [{ subjectId: 'subject-1', systemId: 'system-1', firstName: 'Ada', lastName: 'Lovelace', subjectType: 'Employee', externalId: 'external-1' }],
          },
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /unipass hq/i })).toBeInTheDocument();
    expect(screen.getByText('Configuration')).toBeInTheDocument();
    expect(screen.getByText('Badge Types and Access Rules')).toBeInTheDocument();
    expect(screen.getByText('Active Policies')).toBeInTheDocument();
    expect(screen.getByText('Identity Mappings')).toBeInTheDocument();
    expect(await screen.findByRole('option', { name: 'Main campus' })).toBeInTheDocument();
    expect(await screen.findByRole('option', { name: 'Lobby access' })).toBeInTheDocument();
    expect(await screen.findAllByText('Ada Lovelace')).toHaveLength(2);

    fireEvent.change(screen.getByPlaceholderText('Access rule name'), { target: { value: 'Main lobby' } });
    fireEvent.click(screen.getByRole('button', { name: /add access rule/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/access-policies/access-control-systems/{systemId}/unipass/access-level-types', {
        params: { path: { systemId: 'system-1' } },
        body: {
          name: 'Main lobby',
          siteId: '10',
          accessRuleId: '200',
          metadata: { type: 'unipass', sites: [{ id: '10', name: 'Main campus' }], accessRules: [{ id: '200', name: 'Lobby access' }] },
        },
      });
    });

    expect(apiGetMock).toHaveBeenCalledWith('/api/access-policies/policies', {
      params: { query: { SystemId: 'system-1', ActiveOnly: true, Page: 0, PageSize: 10, ids: [] } },
    });
    expect(apiGetMock).toHaveBeenCalledWith('/api/access-policies/access-control-systems/{systemId}/identity-mappings', {
      params: { path: { systemId: 'system-1' }, query: { Page: 0, PageSize: 10, subjectIds: [] } },
    });

    fireEvent.click(screen.getByRole('button', { name: /delete identity mapping for ada lovelace/i }));

    await waitFor(() => {
      expect(window.confirm).toHaveBeenCalledWith("Delete identity mapping for Ada Lovelace? This will remove this subject's policies and Fabric-managed resources for this system.");
      expect(apiDeleteMock).toHaveBeenCalledWith('/api/access-policies/access-control-systems/{systemId}/identity-mappings/{subjectId}', {
        params: { path: { systemId: 'system-1', subjectId: 'subject-1' } },
      });
    });
  });

  it('creates a site and replaces create route with edit route', async () => {
    window.history.pushState({}, '', '/facility/locations/new');
    apiPostMock.mockResolvedValue({
      data: {
        id: '011c0366-57c6-48ff-842a-0c193bfa0102',
        type: 'Site',
        site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
        building: null,
        room: null,
      },
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /add site/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go back/i })).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/name/i), { target: { value: 'Oslo HQ' } });
    fireEvent.change(screen.getByLabelText(/address/i), { target: { value: 'Karl Johans gate 1' } });
    fireEvent.click(screen.getByRole('button', { name: /create site/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/locations/sites', {
        body: { id: null, name: 'Oslo HQ', address: 'Karl Johans gate 1' },
      });
    });

    await waitFor(() => {
      expect(window.location.pathname).toBe('/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/edit');
    });
  });

  it('loads and saves a site from the edit page', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: '011c0366-57c6-48ff-842a-0c193bfa0102',
            type: 'Site',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: null,
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings') {
        return Promise.resolve({ data: [] });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /edit site/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go back/i })).toBeInTheDocument();
    expect(await screen.findByDisplayValue('Karl Johans gate 1')).toBeInTheDocument();

    fireEvent.change(screen.getByDisplayValue('Oslo HQ'), { target: { value: 'Oslo Office' } });
    fireEvent.change(screen.getByDisplayValue('Karl Johans gate 1'), { target: { value: 'New address not wired yet' } });
    fireEvent.click(screen.getByRole('button', { name: /^save$/i }));

    await waitFor(() => {
      expect(apiPutMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102' } },
        body: { name: 'Oslo Office' },
      });
    });
  });

  it('renders building actions and toggles the add building form', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: '011c0366-57c6-48ff-842a-0c193bfa0102',
            type: 'Site',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: null,
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings') {
        return Promise.resolve({
          data: [{ id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' }],
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /buildings/i })).toBeInTheDocument();
    expect((await screen.findAllByText('Main building')).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Karl Johans gate 1A').length).toBeGreaterThan(0);
    expect(screen.getAllByRole('link', { name: /edit main building/i }).length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: /delete main building/i }).length).toBeGreaterThan(0);
    expect(screen.queryByLabelText(/building name/i)).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /add building/i }));
    fireEvent.change(screen.getByLabelText(/building name/i), { target: { value: 'Annex' } });
    fireEvent.change(screen.getByLabelText(/building address/i), { target: { value: 'Karl Johans gate 1B' } });
    fireEvent.click(screen.getByRole('button', { name: /create/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102' } },
        body: { name: 'Annex', address: 'Karl Johans gate 1B' },
      });
    });
  });

  it('deletes a building from the site edit page', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: '011c0366-57c6-48ff-842a-0c193bfa0102',
            type: 'Site',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: null,
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings') {
        return Promise.resolve({
          data: [{ id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' }],
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    fireEvent.click((await screen.findAllByRole('button', { name: /delete main building/i }))[0]);

    await waitFor(() => {
      expect(apiDeleteMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings/{buildingId}', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102', buildingId: 'building-1' } },
      });
    });
  });

  it('loads and saves a building from the edit page', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/buildings/building-1/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: 'building-1',
            type: 'Building',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: { id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' },
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings/{buildingId}/rooms') {
        return Promise.resolve({ data: [] });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /edit building/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go back/i })).toBeInTheDocument();
    expect(await screen.findByDisplayValue('Karl Johans gate 1A')).toBeInTheDocument();

    fireEvent.change(screen.getByDisplayValue('Main building'), { target: { value: 'Main office' } });
    fireEvent.change(screen.getByDisplayValue('Karl Johans gate 1A'), { target: { value: 'Address not wired yet' } });
    fireEvent.click(screen.getByRole('button', { name: /^save$/i }));

    await waitFor(() => {
      expect(apiPutMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings/{buildingId}', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102', buildingId: 'building-1' } },
        body: { name: 'Main office' },
      });
    });
  });

  it('renders room actions and toggles the add room form', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/buildings/building-1/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: 'building-1',
            type: 'Building',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: { id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' },
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings/{buildingId}/rooms') {
        return Promise.resolve({
          data: [{ id: 'room-1', name: 'Board room', capacity: 12, wheelchairAccessible: true }],
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /rooms/i })).toBeInTheDocument();
    expect((await screen.findAllByText('Board room')).length).toBeGreaterThan(0);
    expect(screen.getAllByText('12').length).toBeGreaterThan(0);
    expect(screen.getAllByRole('link', { name: /edit board room/i }).length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: /delete board room/i }).length).toBeGreaterThan(0);
    expect(screen.queryByLabelText(/room name/i)).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /add room/i }));
    fireEvent.change(screen.getByLabelText(/room name/i), { target: { value: 'Focus room' } });
    fireEvent.change(screen.getByLabelText(/capacity/i), { target: { value: '4' } });
    fireEvent.click(screen.getByLabelText(/wheelchair accessible/i));
    fireEvent.click(screen.getByRole('button', { name: /create/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings/{buildingId}/rooms', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102', buildingId: 'building-1' } },
        body: { name: 'Focus room', capacity: '4', wheelchairAccessible: true },
      });
    });
  });

  it('deletes a room from the building edit page', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/buildings/building-1/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: 'building-1',
            type: 'Building',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: { id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' },
            room: null,
          },
        });
      }

      if (path === '/api/locations/sites/{siteId}/buildings/{buildingId}/rooms') {
        return Promise.resolve({
          data: [{ id: 'room-1', name: 'Board room', capacity: 12, wheelchairAccessible: true }],
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    fireEvent.click((await screen.findAllByRole('button', { name: /delete board room/i }))[0]);

    await waitFor(() => {
      expect(apiDeleteMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings/{buildingId}/rooms/{roomId}', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102', buildingId: 'building-1', roomId: 'room-1' } },
      });
    });
  });

  it('loads and saves a room from the edit page', async () => {
    window.history.pushState({}, '', '/facility/locations/011c0366-57c6-48ff-842a-0c193bfa0102/buildings/building-1/rooms/room-1/edit');
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/locations/locations/{id}') {
        return Promise.resolve({
          data: {
            id: 'room-1',
            type: 'Room',
            site: { id: '011c0366-57c6-48ff-842a-0c193bfa0102', name: 'Oslo HQ', address: 'Karl Johans gate 1' },
            building: { id: 'building-1', name: 'Main building', address: 'Karl Johans gate 1A' },
            room: { id: 'room-1', name: 'Board room', capacity: 12, wheelchairAccessible: true },
          },
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /edit room/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /go back/i })).toBeInTheDocument();
    expect(await screen.findByDisplayValue('Board room')).toBeInTheDocument();
    expect(screen.getByDisplayValue('12')).toBeInTheDocument();
    expect(screen.getByLabelText(/wheelchair accessible/i)).toBeChecked();

    fireEvent.change(screen.getByDisplayValue('Board room'), { target: { value: 'Large board room' } });
    fireEvent.change(screen.getByDisplayValue('12'), { target: { value: '16' } });
    fireEvent.click(screen.getByLabelText(/wheelchair accessible/i));
    fireEvent.click(screen.getByRole('button', { name: /^save$/i }));

    await waitFor(() => {
      expect(apiPutMock).toHaveBeenCalledWith('/api/locations/sites/{siteId}/buildings/{buildingId}/rooms/{roomId}', {
        params: { path: { siteId: '011c0366-57c6-48ff-842a-0c193bfa0102', buildingId: 'building-1', roomId: 'room-1' } },
        body: { name: 'Large board room', capacity: '16', wheelchairAccessible: false },
      });
    });
  });

  it('renders Visits for Visitors Management module root', async () => {
    window.history.pushState({}, '', '/visitors-management');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('button', { name: /visitors management/i })).toBeInTheDocument();
    expect(await screen.findByRole('navigation', { name: /visitors management menu/i })).toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: /visits/i })).toBeInTheDocument();
  });

  it('renders Reception Desk settings and creates access level assignment', async () => {
    window.history.pushState({}, '', '/settings/reception-desk');

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/reception/access-rule-assignments') {
        return Promise.resolve({ data: emptyAccessRuleAssignmentPage });
      }

      if (path === '/api/access-policies/access-control-systems') {
        return Promise.resolve({
          data: {
            ...emptyAccessControlSystemPage,
            totalItems: 1,
            items: [
              {
                type: 'unipass',
                id: 'system-1',
                name: 'Unipass',
                badgeTypes: [],
                accessLevels: [{ type: 'unipass', id: 'level-1', systemId: 'system-1', name: 'Lobby access', siteId: 1, accessRuleId: 100 }],
              },
            ],
          },
        });
      }

      if (path === '/api/locations/sites') {
        return Promise.resolve({
          data: {
            ...emptySitePage,
            totalItems: 1,
            items: [{ id: 'site-1', name: 'Oslo HQ', address: 'Karl Johans gate 1' }],
          },
        });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /reception desk/i })).toBeInTheDocument();
    expect(await screen.findByText('Access Level Assignments')).toBeInTheDocument();
    expect(screen.queryByDisplayValue('Oslo HQ')).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /create assignment/i }));

    expect(await screen.findByRole('option', { name: 'Oslo HQ' })).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText('Site'), { target: { value: 'site-1' } });

    expect(screen.getByLabelText('Site')).toHaveValue('site-1');
    expect(screen.getByLabelText('Access control system')).toHaveValue('system-1');
    expect(screen.getByLabelText('Access level')).toHaveValue('level-1');

    fireEvent.click(screen.getByRole('button', { name: /save assignment/i }));

    await waitFor(() => {
      expect(apiPostMock).toHaveBeenCalledWith('/api/reception/access-rule-assignments', {
        body: {
          locationId: 'site-1',
          systemId: 'system-1',
          accessLevelTypeId: 'level-1',
          trigger: 'ExpectedVisitorAdded',
          gracePeriodMinutes: 0,
        },
      });
    });
  });

  it('keeps saved visitor badge type selected after systems load', async () => {
    window.history.pushState({}, '', '/settings/visitors');
    let resolveSystems: (value: unknown) => void = () => {};
    const systemsPromise = new Promise((resolve) => {
      resolveSystems = resolve;
    });

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      if (path === '/api/sagas/visitor-pre-onboarding/configuration') {
        return Promise.resolve({
          data: {
            useCustomInviteNotification: false,
            customInviteNotification: null,
            qrGenerationMode: 'AccessControlQr',
            systemId: 'system-1',
            badgeTypeId: 'badge-2',
            sendConfirmNotificationToOrganizer: false,
            useCustomConfirmNotification: false,
            customConfirmNotification: null,
            sendCancellationNotification: false,
            useCustomCancellationNotification: false,
            customCancellationNotification: null,
            sendRescheduleNotification: false,
            useCustomRescheduleNotification: false,
            customRescheduleNotification: null,
            sendRelocationNotification: false,
            useCustomRelocationNotification: false,
            customRelocationNotification: null,
          },
        });
      }

      if (path === '/api/access-policies/access-control-systems') {
        return systemsPromise;
      }

      return Promise.resolve({ data: emptyVisitPage });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /visitors/i })).toBeInTheDocument();
    expect(await screen.findByText(/loading access control systems/i)).toBeInTheDocument();

    resolveSystems({
      data: {
        ...emptyAccessControlSystemPage,
        totalItems: 1,
        items: [
          {
            type: 'unipass',
            id: 'system-1',
            name: 'Unipass',
            endpoint: 'https://unipass.example.test',
            sslValidation: true,
            hasSecret: true,
            username: 'api-user',
            badgeTypes: [
              { type: 'unipass', id: 'badge-1', systemId: 'system-1', name: 'Contractor badge', rangeStart: 1, rangeStop: 100 },
              { type: 'unipass', id: 'badge-2', systemId: 'system-1', name: 'Visitor badge', rangeStart: 101, rangeStop: 200 },
            ],
            accessLevels: [
              { type: 'unipass', id: 'level-1', systemId: 'system-1', name: 'Lobby access', siteId: 1, accessRuleId: 100 },
              { type: 'unipass', id: 'level-2', systemId: 'system-1', name: 'Office access', siteId: 1, accessRuleId: 200 },
            ],
          },
        ],
      },
    });

    await waitFor(() => {
      expect(screen.getByLabelText('Access control system')).toHaveValue('system-1');
      expect(screen.getByLabelText('Badge type')).toHaveValue('badge-2');
    });
  });

  it('renders Visits for explicit visits sub-route', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /visits/i })).toBeInTheDocument();
  });

  it('renders visits in the calendar', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');
    const today = new Date();
    const start = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 9, 30);
    const stop = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 10, 45);
    window.sessionStorage.setItem('fabric.visits.calendar', JSON.stringify({ view: 'today', statuses: [], anchorDate: start.toISOString() }));

    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      return Promise.resolve({ data: {
        ...emptyVisitPage,
        totalItems: 1,
        items: [
          {
            id: '9ac1d35c-d7ce-4db3-9fc4-0e978e6fd22c',
            summary: 'Security briefing',
            status: 'Scheduled',
            start: start.toISOString(),
            stop: stop.toISOString(),
            invitations: [{ id: 'participant-1' }, { id: 'participant-2' }],
          },
        ],
      } });
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByText('Security briefing')).toBeInTheDocument();
    expect(screen.getAllByText('Scheduled').length).toBeGreaterThan(0);
    expect(screen.getByText('2 participants')).toBeInTheDocument();
  });

  it('renders an empty calendar when no visits match', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('heading', { name: /visits/i })).toBeInTheDocument();
    expect(await screen.findByText('0 visits')).toBeInTheDocument();
  });

  it('restores persisted visits view and status filters', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');
    window.sessionStorage.setItem(
      'fabric.visits.calendar',
      JSON.stringify({ view: 'month', statuses: ['Completed', 'Cancelled'], anchorDate: new Date().toISOString() }),
    );

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('button', { name: /^Month$/i })).toHaveAttribute('aria-pressed', 'true');
    expect(screen.getByLabelText('Completed')).toBeChecked();
    expect(screen.getByLabelText('Cancelled')).toBeChecked();
    expect(screen.getByLabelText('Scheduled')).not.toBeChecked();

    await waitFor(() => {
      expect(apiGetMock).toHaveBeenCalledWith('/api/visitors/visits', expect.objectContaining({
        params: expect.objectContaining({
          query: expect.objectContaining({ withStatus: ['Completed', 'Cancelled'] }),
        }),
      }));
    });
  });

  it('persists visits view and status filter changes', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');

    render(<App appRouter={createAppRouter()} />);

    fireEvent.click(await screen.findByRole('button', { name: /^Month$/i }));
    fireEvent.click(screen.getByLabelText('Cancelled'));
    fireEvent.click(screen.getByLabelText('Completed'));

    await waitFor(() => {
      expect(window.sessionStorage.getItem('fabric.visits.calendar')).toContain('month');
      expect(window.sessionStorage.getItem('fabric.visits.calendar')).toContain('Cancelled');
      expect(window.sessionStorage.getItem('fabric.visits.calendar')).toContain('Completed');
    });
  });
});
