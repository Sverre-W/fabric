export default function IdentitiesPage() {
  return <PlaceholderPage title="Identities" description="Search and manage people, workers, visitors, and linked identity records." />;
}

function PlaceholderPage({ title, description }: { title: string; description: string }) {
  return (
    <section className="rounded-structural border border-border bg-content p-4 sm:p-6 md:p-8">
      <h1 className="text-[32px] font-semibold tracking-tight">{title}</h1>
      <p className="mt-3 max-w-2xl text-[14px] text-muted-foreground">{description}</p>
    </section>
  );
}
