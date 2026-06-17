export default function HomePage() {
  return (
    <section className="grid gap-6">
      <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <p className="text-sm font-medium text-primary">PIAM platform</p>
        <h1 className="mt-3 text-3xl font-semibold tracking-tight">Fabric access overview</h1>
        <p className="mt-3 max-w-2xl text-muted-foreground">
          Manage identities, credentials, physical access, organizations, and audit activity from one shell.
        </p>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <MetricCard label="Active identities" value="0" tone="primary" />
        <MetricCard label="Open access requests" value="0" tone="danger" />
        <MetricCard label="Successful syncs" value="0" tone="success" />
      </div>
    </section>
  );
}

function MetricCard({ label, value, tone }: { label: string; value: string; tone: 'primary' | 'danger' | 'success' }) {
  const toneClass = tone === 'primary' ? 'text-primary' : tone === 'danger' ? 'text-danger' : 'text-success';

  return (
    <article className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className={`mt-2 text-3xl font-semibold ${toneClass}`}>{value}</p>
    </article>
  );
}
