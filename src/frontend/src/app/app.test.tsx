import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { App } from './app';
import { createAppRouter } from './router';

const apiGetMock = vi.hoisted(() => vi.fn());
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
    signinRedirectMock.mockReset();
    signoutRedirectMock.mockReset();
    removeUserMock.mockReset();
    apiGetMock.mockImplementation((path: string) => {
      if (path === '/api/tenants/settings') {
        return Promise.resolve({ data: tenantSettingsResponse, response: { status: 200 } });
      }

      return Promise.resolve({ data: emptyVisitPage });
    });
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
    expect(screen.getByRole('link', { name: /visitors management/i })).toBeInTheDocument();
  });

  it('renders Visits for Visitors Management module root', async () => {
    window.history.pushState({}, '', '/visitors-management');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('button', { name: /visitors management/i })).toBeInTheDocument();
    expect(await screen.findByRole('navigation', { name: /visitors management menu/i })).toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: /visits/i })).toBeInTheDocument();
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
