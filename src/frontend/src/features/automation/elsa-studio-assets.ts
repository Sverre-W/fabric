import { useEffect } from 'react';

const styles = [
  '/_content/MudBlazor/MudBlazor.min.css',
  '/_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.css',
  '/_content/Radzen.Blazor/css/material-base.css',
  '/_content/Elsa.Studio.Shell/css/shell.css',
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

export function useElsaStudioAssets() {
  useEffect(() => {
    for (const href of styles) {
      if (document.querySelector(`link[href="${href}"]`)) {
        continue;
      }

      const link = document.createElement('link');
      link.rel = 'stylesheet';
      link.href = href;
      document.head.appendChild(link);
    }

    for (const src of scripts) {
      if (document.querySelector(`script[src="${src}"]`)) {
        continue;
      }

      const script = document.createElement('script');
      script.src = src;
      script.async = false;
      document.body.appendChild(script);
    }
  }, []);
}
