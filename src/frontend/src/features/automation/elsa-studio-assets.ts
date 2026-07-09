import { useEffect, useState } from 'react';

const fontPreconnectOrigins = ['https://fonts.googleapis.com', 'https://fonts.gstatic.com'] as const;

const headLinks = [
  { href: 'https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Ubuntu:wght@300;400;500;700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&display=swap', rel: 'stylesheet' },
  { href: 'https://fonts.googleapis.com/css2?family=Grandstander:wght@100&display=swap', rel: 'stylesheet' },
  { href: '/_content/MudBlazor/MudBlazor.min.css', rel: 'stylesheet' },
  { href: '/_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.css', rel: 'stylesheet' },
  { href: '/_content/Radzen.Blazor/css/material-base.css', rel: 'stylesheet' },
  { href: '/_content/Elsa.Studio.Shell/css/shell.css', rel: 'stylesheet' },
  { href: '/_content/Elsa.Studio.Workflows.Designer/designer.css', rel: 'stylesheet', optional: true },
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

const dynamicLinkAttribute = 'data-fabric-elsa-dynamic';
const preconnectAttribute = 'data-fabric-elsa-preconnect';

let activeStyleUsers = 0;
let scriptsReadyPromise: Promise<void> | null = null;

const _dynamicLinks = new Set<HTMLLinkElement>();
let _observer: MutationObserver | null = null;

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
  console.log('[ElsaLoader] Starting asset boot sequence…');
  await ensureElsaScriptsReady();
  console.log('[ElsaLoader] All scripts loaded — waiting for custom elements…');
  await Promise.all(requiredElements.map(waitForCustomElement));
  console.log('[ElsaLoader] All custom elements registered — ready.');
}

function attachElsaStyles() {
  activeStyleUsers++;
  if (activeStyleUsers > 1)
    return;

  console.log('[ElsaLoader] Injecting CSS and preconnects…');
  injectPreconnects();

  for (const entry of headLinks) {
    const { href, rel } = entry;
    if (document.head.querySelector(`link[${injectedLinkAttribute}="${href}"]`))
      continue;

    const link = document.createElement('link');
    link.rel = rel;
    link.href = href;
    link.setAttribute(injectedLinkAttribute, href);

    if ('optional' in entry && entry.optional) {
      link.addEventListener('error', () => link.remove(), { once: true });
    }

    document.head.appendChild(link);
  }

  ensureMutationObserver();
}

function detachElsaStyles() {
  activeStyleUsers = Math.max(0, activeStyleUsers - 1);
  if (activeStyleUsers > 0)
    return;

  for (const link of document.head.querySelectorAll(`link[${injectedLinkAttribute}]`))
    link.remove();

  for (const link of _dynamicLinks)
    link.remove();
  _dynamicLinks.clear();

  _observer?.disconnect();
  _observer = null;
}

function injectPreconnects() {
  for (const origin of fontPreconnectOrigins) {
    if (document.head.querySelector(`link[rel="preconnect"][${preconnectAttribute}="${origin}"]`))
      continue;

    const link = document.createElement('link');
    link.rel = 'preconnect';
    link.href = origin;
    link.setAttribute(preconnectAttribute, origin);
    if (origin.includes('gstatic'))
      link.crossOrigin = 'anonymous';
    document.head.appendChild(link);
  }
}

function ensureMutationObserver() {
  if (_observer)
    return;

  _observer = new MutationObserver((mutations) => {
    for (const mutation of mutations) {
      for (const node of Array.from(mutation.addedNodes)) {
        if (node instanceof HTMLLinkElement && node.rel === 'stylesheet' && !node.hasAttribute(injectedLinkAttribute)) {
          const href = node.getAttribute('href') ?? '';
          try {
            const url = new URL(href, location.href);
            if (url.pathname.startsWith('/assets/elsa/') || url.pathname.startsWith('/_content/') || url.pathname.startsWith('/_framework/')) {
              node.setAttribute(dynamicLinkAttribute, '');
              _dynamicLinks.add(node);
            }
          } catch {
            // ignore malformed hrefs
          }
        }
      }
    }
  });

  _observer.observe(document.head, { childList: true });
}

function ensureElsaScriptsReady() {
  scriptsReadyPromise ??= loadScriptsSequentially();
  return scriptsReadyPromise;
}

async function loadScriptsSequentially() {
  let index = 0;
  for (const src of scripts) {
    index++;
    console.log(`[ElsaLoader] Injecting script [${index}/${scripts.length}]: ${src}`);
    await loadScript(src);
    console.log(`[ElsaLoader] Loaded script [${index}/${scripts.length}]: ${src}`);
  }
}

function loadScript(src: string) {
  const existingScript = document.body.querySelector(`script[${injectedScriptAttribute}="${src}"]`) as HTMLScriptElement | null;

  if (existingScript?.dataset.loaded === 'true')
    return Promise.resolve();

  if (existingScript)
    return waitForScript(existingScript);

  return new Promise<void>((resolve, reject) => {
    const isBlazor = src.includes('blazor.webassembly.js');
    const script = document.createElement('script');
    script.src = src;
    script.async = false;
    script.setAttribute(injectedScriptAttribute, src);

    if (isBlazor)
      script.setAttribute('autostart', 'false');

    script.addEventListener('load', () => {
      script.dataset.loaded = 'true';
      if (isBlazor) {
        startBlazor().then(resolve).catch(reject);
      } else {
        resolve();
      }
    }, { once: true });
    script.addEventListener('error', () => reject(new Error(`Could not load Elsa Studio script '${src}'.`)), { once: true });
    document.body.appendChild(script);
  });
}

async function startBlazor() {
  const win = window as any;
  if (!win.Blazor) {
    console.error('[ElsaLoader] window.Blazor not found after script load');
    return;
  }
  console.log('[ElsaLoader] Calling Blazor.start()…');
  await win.Blazor.start();
  console.log('[ElsaLoader] Blazor.start() completed');
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
