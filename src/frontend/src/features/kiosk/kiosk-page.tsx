import { Navigate } from '@tanstack/react-router';
import { Loader2, Settings } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';

import { Button } from '@/shared/components/ui/button';

import { NoActiveKioskSessionError, cancelCurrentSession, changeKioskLanguage, getCurrentInstruction, getKioskConfig, postKioskHeartbeat, startKioskSession, submitInstructionResponse } from './kiosk-api';
import { KioskLayout } from './kiosk-layout';
import { KioskRenderer } from './kiosk-renderer';
import { getKioskSettings, hasKioskSettings, saveKioskLanguage } from './kiosk-settings';
import type { KioskConfig, KioskInstructionEnvelope, KioskInstructionResponse, KioskSession } from './kiosk-types';

type RuntimeState = 'loading-config' | 'welcome' | 'starting' | 'polling' | 'offline' | 'error';

export default function KioskPage() {
  const settings = getKioskSettings();
  const [config, setConfig] = useState<KioskConfig | null>(null);
  const [languageCode, setLanguageCode] = useState(settings?.languageCode ?? '');
  const [session, setSession] = useState<KioskSession | null>(null);
  const [instruction, setInstruction] = useState<KioskInstructionResponse | null>(null);
  const [state, setState] = useState<RuntimeState>('loading-config');
  const [error, setError] = useState<string | null>(null);
  const [isEndingSession, setIsEndingSession] = useState(false);

  const parsedInstruction = useMemo(() => parseInstruction(instruction), [instruction]);
  const selectedLanguage = languageCode || config?.profile.defaultLanguageCode || '';

  useEffect(() => {
    if (!settings) return;

    let disposed = false;
    async function loadConfig() {
      setState('loading-config');
      setError(null);
      try {
        const nextConfig = await getKioskConfig(selectedLanguage || undefined);
        if (disposed) return;

        setConfig(nextConfig);
        const nextLanguage = selectedLanguage || nextConfig.profile.defaultLanguageCode;
        setLanguageCode(nextLanguage);
        saveKioskLanguage(nextLanguage);
        setState('welcome');
      } catch (loadError) {
        if (disposed) return;
        setState('offline');
        setError(loadError instanceof Error ? loadError.message : 'Could not load kiosk.');
      }
    }

    void loadConfig();
    return () => { disposed = true; };
  }, []);

  useEffect(() => {
    if (!settings) return;

    const interval = window.setInterval(() => {
      void postKioskHeartbeat().catch(() => undefined);
    }, 30_000);

    return () => window.clearInterval(interval);
  }, [settings]);

  useEffect(() => {
    if (!session || (session.status !== 'Starting' && session.status !== 'Running')) return;

    let disposed = false;
    async function poll() {
      let sinceVersion = Number(instruction?.version ?? 0);
      while (!disposed) {
        try {
          setState('polling');
          const nextInstruction = await getCurrentInstruction(sinceVersion);
          if (disposed) return;
          setInstruction(nextInstruction);
          sinceVersion = Number(nextInstruction.version ?? sinceVersion);
          if (nextInstruction.status !== 'Starting' && nextInstruction.status !== 'Running') {
            resetToWelcome();
            return;
          }

          setSession((current) => current ? { ...current, status: nextInstruction.status } : current);
        } catch (pollError) {
          if (disposed) return;
          if (pollError instanceof NoActiveKioskSessionError) {
            resetToWelcome();
            return;
          }

          setState('offline');
          setError(pollError instanceof Error ? pollError.message : 'Connection lost. Retrying...');
          await delay(3000);
        }
      }
    }

    void poll();
    return () => { disposed = true; };
  }, [instruction?.version, session?.id, session?.status]);

  if (!hasKioskSettings()) return <Navigate to="/kiosk/setup" replace />;

  async function refreshConfig(nextLanguage = selectedLanguage) {
    const nextConfig = await getKioskConfig(nextLanguage || undefined);
    setConfig(nextConfig);
  }

  async function handleLanguageChange(nextLanguage: string) {
    setLanguageCode(nextLanguage);
    saveKioskLanguage(nextLanguage);
    try {
      if (session?.status === 'Running') await changeKioskLanguage(nextLanguage);
      await refreshConfig(nextLanguage);
    } catch {
      setError('Could not change language.');
    }
  }

  async function start() {
    setState('starting');
    setError(null);
    try {
      const nextSession = await startKioskSession(selectedLanguage || undefined);
      setSession(nextSession);
    } catch (startError) {
      setState('error');
      setError(startError instanceof Error ? startError.message : 'Could not start session.');
    }
  }

  async function submit(values: Record<string, string>) {
    if (!instruction?.instructionId) return;
    try {
      const nextSession = await submitInstructionResponse(instruction.instructionId, values);
      if (nextSession.status !== 'Starting' && nextSession.status !== 'Running') {
        resetToWelcome();
        return;
      }

      setSession(nextSession);
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Could not submit response.');
    }
  }

  async function handleHomePress() {
    if (isEndingSession) return;

    const hasActiveSession = session?.status === 'Starting' || session?.status === 'Running';
    if (!hasActiveSession) {
      resetToWelcome();
      return;
    }

    setIsEndingSession(true);
    setError(null);
    try {
      await cancelCurrentSession();
      resetToWelcome();
    } catch (cancelError) {
      setError(cancelError instanceof Error ? cancelError.message : 'Could not end session.');
    } finally {
      setIsEndingSession(false);
    }
  }

  function resetToWelcome() {
    setSession(null);
    setInstruction(null);
    setState('welcome');
    setError(null);
  }

  return (
    <KioskLayout config={config} languageCode={selectedLanguage} onLanguageChange={handleLanguageChange} onHomePress={handleHomePress} homeDisabled={isEndingSession}>
      {state === 'loading-config' ? <StateCard title="Loading kiosk" message="Preparing kiosk configuration..." loading /> : null}
      {state === 'offline' ? <StateCard title="Kiosk unavailable" message={error ?? 'Could not connect to server.'} actionLabel="Retry" onAction={() => void refreshConfig()} /> : null}
      {config?.kiosk.mode === 'Disabled' ? <StateCard title="Kiosk disabled" message="This kiosk is currently disabled." /> : null}
      {config?.kiosk.mode === 'Maintenance' ? <StateCard title="Maintenance" message="This kiosk is temporarily unavailable." /> : null}
      {config?.kiosk.mode === 'Active' && !session && state !== 'loading-config' && state !== 'offline' ? <Welcome config={config} onStart={start} busy={state === 'starting'} /> : null}
      {config?.kiosk.mode === 'Active' && (session?.status === 'Starting' || session?.status === 'Running') && parsedInstruction ? <InstructionBackdrop instruction={parsedInstruction} config={config}><KioskRenderer instruction={parsedInstruction} onSubmit={submit} /></InstructionBackdrop> : null}
      {config?.kiosk.mode === 'Active' && (session?.status === 'Starting' || session?.status === 'Running') && !parsedInstruction ? <StateCard title={session.status === 'Starting' ? 'Starting session' : 'Please wait'} message={session.status === 'Starting' ? 'Launching workflow and waiting for first instruction...' : 'Waiting for next instruction...'} loading /> : null}
      {error && state !== 'offline' ? <p className="fixed bottom-6 left-1/2 z-10 -translate-x-1/2 rounded-interactive border border-error bg-error-background px-4 py-3 text-[15px] font-medium text-error shadow-sm">{error}</p> : null}
      <a href="/kiosk/setup" className="fixed bottom-4 right-4 rounded-full border border-border bg-content p-3 text-muted-foreground shadow-sm transition hover:text-foreground" aria-label="Kiosk setup"><Settings className="size-5" /></a>
    </KioskLayout>
  );
}

function Welcome({ config, onStart, busy }: { readonly config: KioskConfig; readonly onStart: () => void; readonly busy: boolean }) {
  const welcome = config.resolvedWelcome;
  const backgroundUrl = welcome?.backgroundUrl;
  return <InstructionBackdrop config={config} backgroundUrl={backgroundUrl}><section className="w-full max-w-5xl rounded-[2rem] border border-border bg-content/95 p-8 text-center shadow-sm backdrop-blur sm:p-14"><p className="text-[14px] font-semibold uppercase tracking-[0.28em] text-muted-foreground">{config.kiosk.name}</p><h2 className="mt-4 text-[40px] font-semibold tracking-tight sm:text-[64px]">{welcome?.title ?? 'Welcome'}</h2>{welcome?.subtitle ? <p className="mx-auto mt-5 max-w-3xl text-[19px] leading-8 text-muted-foreground sm:text-[24px] sm:leading-9">{welcome.subtitle}</p> : null}<Button type="button" className="mt-10 h-16 rounded-[1.25rem] px-12 text-[22px] font-semibold" disabled={busy} onClick={onStart}>{busy ? 'Starting...' : welcome?.startButton ?? 'Start'}</Button></section></InstructionBackdrop>;
}

function InstructionBackdrop({ instruction, config, backgroundUrl, children }: { readonly instruction?: KioskInstructionEnvelope; readonly config: KioskConfig; readonly backgroundUrl?: string | null; readonly children: React.ReactNode }) {
  const instructionBackgroundUrl = resolveInstructionAssetUrl(instruction?.layout?.backgroundAssetName, instruction?.languageCode);
  const resolvedBackground = backgroundUrl ?? instructionBackgroundUrl ?? config.theme.defaultBackgroundUrl ?? null;
  return <div className="flex min-h-[calc(100vh-161px)] w-full items-center justify-center rounded-[2rem] bg-cover bg-center p-0" style={resolvedBackground ? { backgroundImage: `linear-gradient(rgba(0,0,0,.08), rgba(0,0,0,.08)), url(${resolvedBackground})` } : undefined}>{children}</div>;
}

function StateCard({ title, message, loading, actionLabel, onAction }: { readonly title: string; readonly message: string; readonly loading?: boolean; readonly actionLabel?: string; readonly onAction?: () => void }) {
  return <section className="w-full max-w-3xl rounded-[2rem] border border-border bg-content p-8 text-center shadow-sm sm:p-12">{loading ? <Loader2 className="mx-auto size-12 animate-spin text-primary" aria-hidden="true" /> : null}<h2 className="mt-4 text-[34px] font-semibold tracking-tight sm:text-[48px]">{title}</h2><p className="mx-auto mt-4 max-w-2xl text-[18px] leading-8 text-muted-foreground">{message}</p>{actionLabel ? <Button type="button" className="mt-8 h-14 rounded-xl px-8 text-[18px]" onClick={onAction}>{actionLabel}</Button> : null}</section>;
}

function parseInstruction(instruction: KioskInstructionResponse | null): KioskInstructionEnvelope | null {
  if (!instruction?.instructionJson) return null;
  try {
    const parsed = JSON.parse(instruction.instructionJson) as KioskInstructionEnvelope;
    return Object.keys(parsed).length === 0 ? null : parsed;
  } catch {
    return { type: 'error', content: { title: 'Invalid instruction', message: 'Kiosk received an instruction it could not render.' } };
  }
}

function delay(milliseconds: number) {
  return new Promise((resolve) => window.setTimeout(resolve, milliseconds));
}

function resolveInstructionAssetUrl(assetName: string | null | undefined, languageCode: string | undefined) {
  if (!assetName) return null;
  const encodedAssetName = encodeURIComponent(assetName);
  const encodedLanguageCode = languageCode ? `?languageCode=${encodeURIComponent(languageCode)}` : '';
  return `/api/kiosk/assets/${encodedAssetName}${encodedLanguageCode}`;
}
