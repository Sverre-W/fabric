import { render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import { ElsaStudioEditorScreen } from './elsa-studio-fullscreen';

const invalidateQueriesMock = vi.hoisted(() => vi.fn());
const navigateMock = vi.hoisted(() => vi.fn());

vi.mock('@tanstack/react-query', () => ({
  useQueryClient: () => ({ invalidateQueries: invalidateQueriesMock }),
}));

vi.mock('@tanstack/react-router', () => ({
  useNavigate: () => navigateMock,
}));

vi.mock('./elsa-studio-assets', () => ({
  useElsaStudioAssets: () => ({ status: 'ready' as const }),
}));

describe('ElsaStudioEditorScreen', () => {
  it('remounts editor when access token rotates', async () => {
    const view = render(
      <ElsaStudioEditorScreen
        definitionId="definition-1"
        runtime={{ remoteEndpoint: 'http://localhost:5245/elsa/api', accessToken: 'access-token' }}
      />,
    );

    await screen.findByRole('heading', { name: /workflow definition editor/i });

    const initialEditor = document.querySelector('elsa-workflow-definition-editor');
    expect(initialEditor).toHaveAttribute('definition-id', 'definition-1');
    expect(initialEditor).toHaveAttribute('access-token', 'access-token');

    view.rerender(
      <ElsaStudioEditorScreen
        definitionId="definition-1"
        runtime={{ remoteEndpoint: 'http://localhost:5245/elsa/api', accessToken: 'rotated-access-token' }}
      />,
    );

    await waitFor(() => {
      const nextEditor = document.querySelector('elsa-workflow-definition-editor');
      expect(nextEditor).not.toBe(initialEditor);
      expect(nextEditor).toHaveAttribute('definition-id', 'definition-1');
      expect(nextEditor).toHaveAttribute('access-token', 'rotated-access-token');
    });
  });
});
