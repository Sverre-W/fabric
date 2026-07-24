export function NoPerspectiveWarning() {
  return (
    <div className="flex min-h-[60vh] items-center justify-center px-4 text-foreground">
      <div className="max-w-xl rounded-structural border border-error bg-content p-6 sm:p-8">
        <p className="text-[14px] font-semibold uppercase text-error">Configuration error</p>
        <h1 className="mt-3 text-[24px] font-semibold tracking-tight">Your account is not configured</h1>
        <p className="mt-3 text-[14px] leading-6 text-muted-foreground">
          Your account has not been configured properly for any available perspective. Contact your administrator.
        </p>
      </div>
    </div>
  );
}
