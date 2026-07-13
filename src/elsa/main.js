const DEFAULT_BACKEND_URL = "https://localhost:5001/elsa/api";
const DEFAULT_ACCESS_TOKEN = "";
const DEFAULT_DEFINITION_ID = "";

const CSS_ASSETS = [
  "https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap",
  "https://fonts.googleapis.com/css2?family=Ubuntu:wght@300;400;500;700&display=swap",
  "https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&display=swap",
  "https://fonts.googleapis.com/css2?family=Grandstander:wght@100&display=swap",
  "./_content/MudBlazor/MudBlazor.min.css",
  "./_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.css",
  "./_content/Radzen.Blazor/css/material-base.css",
  "./_content/Elsa.Studio.Shell/css/shell.css",
  "./_content/Elsa.Studio.Workflows.Designer/designer.css",
  "./Elsa.Studio.Host.CustomElements.styles.css",
];

const SCRIPT_ASSETS = [
  "./_content/BlazorMonaco/jsInterop.js",
  "./_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js",
  "./_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js",
  "./_content/MudBlazor/MudBlazor.min.js",
  "./_content/CodeBeam.MudBlazor.Extensions/MudExtensions.min.js",
  "./_content/Radzen.Blazor/Radzen.Blazor.js",
  "./_framework/blazor.webassembly.js",
];

const form = document.querySelector("#config-form");
const backendInput = document.querySelector("#backend-url");
const tokenInput = document.querySelector("#access-token");
const definitionInput = document.querySelector("#definition-id");
const editorHost = document.querySelector("#editor-host");
const statusText = document.querySelector("#status-text");
const logOutput = document.querySelector("#log-output");
const clearLogButton = document.querySelector("#clear-log");

const search = new URLSearchParams(window.location.search);
const storageKey = "fabric-elsa-harness-config";
const customElementTag = "elsa-workflow-definition-editor";

let assetsReadyPromise = null;

init();

function init() {
  const config = loadConfig();
  backendInput.value = config.backendUrl;
  tokenInput.value = config.accessToken;
  definitionInput.value = config.definitionId;

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    void mountEditor(readFormConfig());
  });

  clearLogButton.addEventListener("click", () => {
    logOutput.textContent = "";
  });

  void mountEditor(config);
}

async function mountEditor(config) {
  saveConfig(config);
  setStatus("Loading Elsa assets…");
  log(`Mount requested for definition '${config.definitionId || "<empty>"}'.`);

  if (!config.backendUrl) {
    setStatus("Missing backend URL");
    log("Backend URL empty.");
    return;
  }

  if (!config.definitionId) {
    setStatus("Missing definition ID");
    log("Definition ID empty.");
    return;
  }

  try {
    await ensureAssetsReady();
    await customElements.whenDefined(customElementTag);
    log(`Custom element '${customElementTag}' defined.`);

    editorHost.replaceChildren();

    const editor = document.createElement(customElementTag);
    editor.className = "fabric-elsa-editor";
    editor.remoteEndpoint = config.backendUrl;
    editor.setAttribute("remote-endpoint", config.backendUrl);
    editor.setAttribute("definition-id", config.definitionId);

    if (config.accessToken) {
      editor.accessToken = config.accessToken;
      editor.setAttribute("access-token", config.accessToken);
    }

    editorHost.appendChild(editor);

    setStatus("Editor mounted");
    log("Editor mounted.");
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    setStatus("Boot failed");
    log(`Boot failed: ${message}`);
    console.error(error);
  }
}

function loadConfig() {
  const stored = readStoredConfig();

  return {
    backendUrl: search.get("backend") ?? stored.backendUrl ?? DEFAULT_BACKEND_URL,
    accessToken: search.get("token") ?? stored.accessToken ?? DEFAULT_ACCESS_TOKEN,
    definitionId: search.get("definitionId") ?? stored.definitionId ?? DEFAULT_DEFINITION_ID,
  };
}

function readStoredConfig() {
  try {
    const raw = window.localStorage.getItem(storageKey);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function saveConfig(config) {
  try {
    window.localStorage.setItem(storageKey, JSON.stringify(config));
  } catch {
    log("Could not persist local config.");
  }
}

function readFormConfig() {
  return {
    backendUrl: backendInput.value.trim(),
    accessToken: tokenInput.value.trim(),
    definitionId: definitionInput.value.trim(),
  };
}

async function ensureAssetsReady() {
  assetsReadyPromise ??= loadAssets();
  return assetsReadyPromise;
}

async function loadAssets() {
  log("Injecting Elsa CSS assets.");
  injectPreconnect("https://fonts.googleapis.com");
  injectPreconnect("https://fonts.gstatic.com", true);

  for (const href of CSS_ASSETS)
    injectStylesheet(href);

  let index = 0;
  for (const src of SCRIPT_ASSETS) {
    index += 1;
    log(`Loading script ${index}/${SCRIPT_ASSETS.length}: ${src}`);
    await loadScript(src);
    log(`Loaded script ${index}/${SCRIPT_ASSETS.length}: ${src}`);
  }
}

function injectPreconnect(href, crossOrigin = false) {
  if (document.head.querySelector(`link[rel="preconnect"][href="${href}"]`))
    return;

  const link = document.createElement("link");
  link.rel = "preconnect";
  link.href = href;
  if (crossOrigin)
    link.crossOrigin = "anonymous";
  document.head.appendChild(link);
}

function injectStylesheet(href) {
  if (document.head.querySelector(`link[rel="stylesheet"][href="${href}"]`))
    return;

  const link = document.createElement("link");
  link.rel = "stylesheet";
  link.href = href;
  link.addEventListener("error", () => {
    log(`Optional stylesheet failed: ${href}`);
  }, { once: true });
  document.head.appendChild(link);
}

function loadScript(src) {
  const existing = document.body.querySelector(`script[src="${CSS.escape(src)}"]`);
  if (existing?.dataset.loaded === "true")
    return Promise.resolve();

  if (existing)
    return waitForScript(existing);

  return new Promise((resolve, reject) => {
    const script = document.createElement("script");
    const isBlazor = src.endsWith("blazor.webassembly.js");
    script.src = src;
    script.async = false;

    if (isBlazor)
      script.setAttribute("autostart", "false");

    script.addEventListener("load", async () => {
      script.dataset.loaded = "true";
      try {
        if (isBlazor) {
          log("Starting Blazor.");
          await window.Blazor.start();
          log("Blazor started.");
        }

        resolve();
      } catch (error) {
        reject(error);
      }
    }, { once: true });

    script.addEventListener("error", () => {
      reject(new Error(`Could not load script '${src}'.`));
    }, { once: true });

    document.body.appendChild(script);
  });
}

function waitForScript(script) {
  return new Promise((resolve, reject) => {
    script.addEventListener("load", () => resolve(), { once: true });
    script.addEventListener("error", () => reject(new Error(`Could not load script '${script.src}'.`)), { once: true });
  });
}

function setStatus(message) {
  statusText.textContent = message;
}

function log(message) {
  const timestamp = new Date().toLocaleTimeString();
  logOutput.textContent += `[${timestamp}] ${message}\n`;
  logOutput.scrollTop = logOutput.scrollHeight;
}
