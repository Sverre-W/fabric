import { RouterProvider } from '@tanstack/react-router';

import { AppProviders } from './providers';
import { type AppRouter, router } from './router';

export function App({ appRouter = router }: { appRouter?: AppRouter }) {
  return (
    <AppProviders>
      <RouterProvider router={appRouter} />
    </AppProviders>
  );
}
