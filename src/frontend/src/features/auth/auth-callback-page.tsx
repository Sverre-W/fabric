import { useAuth } from 'react-oidc-context';

export default function AuthCallbackPage() {
  const auth = useAuth();

  return (
    <section className="rounded-structural border border-border bg-content p-8">
      <p className="text-[14px] font-semibold uppercase text-primary">Authentication</p>
      <h1 className="mt-3 text-[28px] font-semibold tracking-tight">Completing sign in...</h1>
      {auth.error ? <p className="mt-3 text-[14px] text-error">{auth.error.message}</p> : <p className="mt-3 text-[14px] text-muted-foreground">You will be redirected shortly.</p>}
    </section>
  );
}
