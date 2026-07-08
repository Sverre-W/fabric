import { useEffect, useState } from 'react';

const headLinks = [
  { href: 'https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Ubuntu:wght@300;400;500;700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Grandstander:wght@100&display=swap', rel: 'stylesheet' },
  { href: '/_content/MudBlazor/MudBlazor.min.css', rel: 'stylesheet' },
  { href: '/_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.css', rel: 'stylesheet' },
  { href: '/_content/Radzen.Blazor/css/material-base.css', rel: 'stylesheet' },
  { href: '/_content/Elsa.Studio.Shell/css/shell.css', rel: 'stylesheet' },
  { href: '/Elsa.Studio.Host.CustomElements.styles.css', rel: 'stylesheet', optional: true },
] as const;

const scripts = [
  '/_content/BlazorMonaco/jsInterop.js',
  '/_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js',
  '/_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js',
  '/_content/MudBlazor/MudBlazor.min.js',
  '/_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.js',
  '/_content/Radzen.Blazor/Radzen.Blazor.js',
  '/_framework/blazor.webassembly.js',
] as const;

const injectedLinkAttribute = 'data-fabric-elsa-link';
const injectedScriptAttribute = 'data-fabric-elsa-script';

let activeStyleUsers = 0;
let scriptsReadyPromise: Promise<void> | null = null;

export function useElsaStudioAssets(requiredElements: readonly string[]) {
  const [status, setStatus] = useState<'loading' | 'ready' | 'error'>(isJsDomEnvironment() ? 'ready' : 'loading');
  const [error, setError] = useState<string | null>(null);
  const requiredElementsKey = requiredElements.join('|');

  useEffect(() => {
    if (isJsDomEnvironment())
      return;

    let cancelled = false;
    attachElsaStyles();

    void ensureElsaAssetsReady(requiredElements)
      .then(() => {
        if (!cancelled)
          setStatus('ready');
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setStatus('error');
          setError(reason instanceof Error ? reason.message : 'Could not load Elsa Studio assets.');
        }
      });

    return () => {
      cancelled = true;
      detachElsaStyles();
    };
  }, [requiredElementsKey]);

  return { status, error };
}

async function ensureElsaAssetsReady(requiredElements: readonly string[]) {
  await ensureElsaScriptsReady();
  await Promise.all(requiredElements.map(waitForCustomElement));
}

function attachElsaStyles() {
  activeStyleUsers++;
  if (activeStyleUsers > 1)
    return;

  for (const { href, rel } of headLinks) {
    if (document.head.querySelector(`link[${injectedLinkAttribute}="${href}"]`))
      continue;

    const link = document.createElement('link');
    link.rel = rel;
    link.href = href;
    link.setAttribute(injectedLinkAttribute, href);
    document.head.appendChild(link);
  }
}

function detachElsaStyles() {
  activeStyleUsers = Math.max(0, activeStyleUsers - 1);
  if (activeStyleUsers > 0)
    return;

  for (const link of document.head.querySelectorAll(`link[${injectedLinkAttribute}]`))
    link.remove();
}

function ensureElsaScriptsReady() {
  scriptsReadyPromise ??= loadScriptsSequentially();
  return scriptsReadyPromise;
}

async function loadScriptsSequentially() {
  for (const src of scripts)
    await loadScript(src);
}

function loadScript(src: string) {
  const existingScript = document.body.querySelector(`script[${injectedScriptAttribute}="${src}"]`) as HTMLScriptElement | null;

  if (existingScript?.dataset.loaded === 'true')
    return Promise.resolve();

  if (existingScript)
    return waitForScript(existingScript);

  return new Promise<void>((resolve, reject) => {
    const script = document.createElement('script');
    script.src = src;
    script.async = false;
    script.setAttribute(injectedScriptAttribute, src);
    script.addEventListener('load', () => {
      script.dataset.loaded = 'true';
      resolve();
    }, { once: true });
    script.addEventListener('error', () => reject(new Error(`Could not load Elsa Studio script '${src}'.`)), { once: true });
    document.body.appendChild(script);
  });
}

function waitForScript(script: HTMLScriptElement) {
  return new Promise<void>((resolve, reject) => {
    script.addEventListener('load', () => {
      script.dataset.loaded = 'true';
      resolve();
    }, { once: true });
    script.addEventListener('error', () => reject(new Error(`Could not load Elsa Studio script '${script.src}'.`)), { once: true });
  });
}

function waitForCustomElement(tagName: string) {
  if (customElements.get(tagName))
    return Promise.resolve();

  return customElements.whenDefined(tagName);
}

function isJsDomEnvironment() {
  return typeof navigator !== 'undefined' && navigator.userAgent.includes('jsdom');
}
