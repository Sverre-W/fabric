import { useEffect } from 'react';
import { useAuth } from 'react-oidc-context';

import { setAccessToken } from '@/shared/api/client';

export function AuthTokenBridge() {
  const auth = useAuth();

  useEffect(() => {
    setAccessToken(auth.user?.access_token);

    return () => setAccessToken(undefined);
  }, [auth.user?.access_token]);

  return null;
}
