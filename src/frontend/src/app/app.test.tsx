import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import { App } from './app';

describe('App', () => {
  it('renders Fabric shell', async () => {
    render(<App />);

    expect(await screen.findByRole('link', { name: /fabric home/i })).toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: /fabric access overview/i })).toBeInTheDocument();
  });
});
