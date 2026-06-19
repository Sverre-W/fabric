import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { I18nextProvider } from 'react-i18next';
import { type ReactNode, useEffect, useState } from 'react';

import { i18n } from '@/shared/i18n/i18n';
import { Toaster } from '@/shared/components/ui/sonner';
import { applyFabricTheme, defaultFabricTheme } from '@/shared/theme/fabric-theme';

export function AppProviders({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  useEffect(() => {
    applyFabricTheme(defaultFabricTheme);
  }, []);

  return (
    <I18nextProvider i18n={i18n}>
      <QueryClientProvider client={queryClient}>
        {children}
        <Toaster />
      </QueryClientProvider>
    </I18nextProvider>
  );
}
