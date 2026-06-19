import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { App } from './app';
import { createAppRouter } from './router';

const apiGetMock = vi.hoisted(() => vi.fn());

vi.mock('@/shared/api/client', () => ({
  api: {
    GET: apiGetMock,
  },
}));

const emptyVisitPage = {
  currentPage: 0,
  totalPages: 0,
  pageSize: 250,
  totalItems: 0,
  items: [],
  isLastPage: true,
};

describe('App', () => {
  beforeEach(() => {
    apiGetMock.mockReset();
    apiGetMock.mockResolvedValue({ data: emptyVisitPage });
    window.sessionStorage.clear();
    window.history.pushState({}, '', '/');
  });

  it('renders module launcher on home page', async () => {
    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByRole('link', { name: /fabric home/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /select module/i })).not.toBeInTheDocument();
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

    apiGetMock.mockResolvedValue({
      data: {
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
      },
    });

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByText('Security briefing')).toBeInTheDocument();
    expect(screen.getAllByText('Scheduled').length).toBeGreaterThan(0);
    expect(screen.getByText('2 participants')).toBeInTheDocument();
  });

  it('renders an empty calendar when no visits match', async () => {
    window.history.pushState({}, '', '/visitors-management/visits');

    render(<App appRouter={createAppRouter()} />);

    expect(await screen.findByText('No visits match this interval and filter.')).toBeInTheDocument();
    expect(screen.getAllByText('No visits').length).toBeGreaterThan(0);
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
