import { useEffect, useRef, type ReactNode } from 'react';
import { useAuth } from 'react-oidc-context';

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const signInStarted = useRef(false);

  useEffect(() => {
    if (auth.isLoading || auth.isAuthenticated || auth.activeNavigator || signInStarted.current) {
      return;
    }

    signInStarted.current = true;
    void auth.signinRedirect({ state: { returnTo: `${window.location.pathname}${window.location.search}${window.location.hash}` } });
  }, [auth]);

  if (auth.isLoading || auth.activeNavigator || !auth.isAuthenticated) {
    return <div className="rounded-structural border border-border bg-content p-6 text-[14px] text-muted-foreground">Redirecting to sign in...</div>;
  }

  return children;
}
