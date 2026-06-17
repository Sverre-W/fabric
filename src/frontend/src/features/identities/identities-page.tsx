export default function IdentitiesPage() {
  return <PlaceholderPage title="Identities" description="Search and manage people, workers, visitors, and linked identity records." />;
}

function PlaceholderPage({ title, description }: { title: string; description: string }) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
      <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
      <p className="mt-3 max-w-2xl text-muted-foreground">{description}</p>
    </section>
  );
}
