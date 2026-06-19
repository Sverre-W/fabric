export function FabricLogo({ logoUrl }: { logoUrl?: string }) {
  if (logoUrl) {
    return <img src={logoUrl} alt="" className="size-10 rounded-interactive object-contain" aria-hidden="true" />;
  }

  return (
    <div className="flex size-10 flex-col items-center justify-center gap-1 rounded-interactive bg-primary" aria-hidden="true">
      <span className="h-1 w-5 rounded-sm bg-foreground" />
      <span className="h-1 w-5 rounded-sm bg-foreground" />
      <span className="h-1 w-5 rounded-sm bg-foreground" />
    </div>
  );
}
