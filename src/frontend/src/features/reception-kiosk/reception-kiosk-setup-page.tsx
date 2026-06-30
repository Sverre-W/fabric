import { useNavigate } from '@tanstack/react-router';
import { KeyRound, TabletSmartphone } from 'lucide-react';
import { useState, type FormEvent } from 'react';

import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';

import { getReceptionKioskSettings, getStoredReceptionKioskId, saveReceptionKioskSettings } from './reception-kiosk-settings';

export default function ReceptionKioskSetupPage() {
  const navigate = useNavigate();
  const [kioskId, setKioskId] = useState(getStoredReceptionKioskId);
  const [kioskApiKey, setKioskApiKey] = useState('');
  const [error, setError] = useState<string | null>(null);
  const hasExistingApiKey = getReceptionKioskSettings() !== null;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const currentSettings = getReceptionKioskSettings();
    const nextKioskId = kioskId.trim();
    const nextApiKey = kioskApiKey.trim() || currentSettings?.kioskApiKey || '';

    if (!nextKioskId) {
      setError('Kiosk ID is required.');
      return;
    }

    if (!nextApiKey) {
      setError('Kiosk API key is required.');
      return;
    }

    saveReceptionKioskSettings({ kioskId: nextKioskId, kioskApiKey: nextApiKey });
    await navigate({ to: '/reception-kiosk' });
  }

  return (
    <section className="grid w-full gap-6 lg:grid-cols-[0.9fr_1.1fr] lg:items-stretch">
      <div className="rounded-[2rem] border border-border bg-content p-7 shadow-sm sm:p-10">
        <div className="flex size-16 items-center justify-center rounded-full bg-hover-blue text-primary sm:size-20">
          <TabletSmartphone className="size-8 sm:size-10" aria-hidden="true" />
        </div>
        <p className="mt-8 text-[13px] font-semibold uppercase tracking-[0.28em] text-muted-foreground">Device setup</p>
        <h2 className="mt-3 text-[34px] font-semibold tracking-tight sm:text-[48px]">Configure reception kiosk</h2>
        <p className="mt-5 text-[18px] leading-8 text-muted-foreground">
          Enter credentials issued from Reception Desk settings. API key is write-only on this device: configured keys are never shown again.
        </p>
      </div>

      <form className="grid gap-6 rounded-[2rem] border border-border bg-content p-7 shadow-sm sm:p-10" onSubmit={handleSubmit}>
        <div className="flex items-center gap-4">
          <div className="flex size-14 items-center justify-center rounded-full bg-hover-gray text-muted-foreground">
            <KeyRound className="size-7" aria-hidden="true" />
          </div>
          <div>
            <h3 className="text-[24px] font-semibold tracking-tight">Kiosk credentials</h3>
            <p className="mt-1 text-[15px] text-muted-foreground">Saved locally in this browser profile.</p>
          </div>
        </div>

        {error ? <p className="rounded-interactive border border-error bg-error-background px-4 py-3 text-[15px] font-medium text-error">{error}</p> : null}

        <div className="grid gap-3">
          <Label htmlFor="reception-kiosk-id" className="text-[16px]">Kiosk ID</Label>
          <Input
            id="reception-kiosk-id"
            className="h-14 rounded-xl px-4 text-[18px] md:text-[18px]"
            value={kioskId}
            autoComplete="off"
            placeholder="00000000-0000-0000-0000-000000000000"
            onChange={(event) => setKioskId(event.target.value)}
          />
        </div>

        <div className="grid gap-3">
          <Label htmlFor="reception-kiosk-api-key" className="text-[16px]">Kiosk API key</Label>
          <Input
            id="reception-kiosk-api-key"
            className="h-14 rounded-xl px-4 text-[18px] md:text-[18px]"
            type="password"
            value={kioskApiKey}
            autoComplete="new-password"
            placeholder={hasExistingApiKey ? 'API key is configured. Enter a new key to replace it.' : 'Paste API key'}
            onChange={(event) => setKioskApiKey(event.target.value)}
          />
          <p className="text-[14px] leading-6 text-muted-foreground">Leave empty to keep existing key when rotating only the kiosk ID.</p>
        </div>

        <div className="pt-2">
          <Button type="submit" className="h-14 w-full rounded-xl text-[18px] font-semibold sm:w-auto sm:px-10">
            Save kiosk setup
          </Button>
        </div>
      </form>
    </section>
  );
}
