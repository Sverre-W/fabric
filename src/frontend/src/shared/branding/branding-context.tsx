import { createContext, useContext, type ReactNode } from 'react';

import { appBranding, type FabricBranding } from './fabric-branding';

const BrandingContext = createContext<FabricBranding>(appBranding);

export function BrandingProvider({ branding, children }: { branding: FabricBranding; children: ReactNode }) {
  return <BrandingContext.Provider value={branding}>{children}</BrandingContext.Provider>;
}

export function useBranding() {
  return useContext(BrandingContext);
}
