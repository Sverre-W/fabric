//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

var e=!1;const t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,4,1,96,0,0,3,2,1,0,10,8,1,6,0,6,64,25,11,11])),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),n=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),r=Symbol.for("wasm promise_control");function i(e,t){let o=null;const n=new Promise((function(n,r){o={isDone:!1,promise:null,resolve:t=>{o.isDone||(o.isDone=!0,n(t),e&&e())},reject:e=>{o.isDone||(o.isDone=!0,r(e),t&&t())}}}));o.promise=n;const i=n;return i[r]=o,{promise:i,promise_control:o}}function s(e){return e[r]}function a(e){e&&function(e){return void 0!==e[r]}(e)||Be(!1,"Promise is not controllable")}const l="__mono_message__",c=["debug","log","trace","warn","info","error"],d="MONO_WASM: ";let u,f,m,g,p,h;function w(e){g=e}function b(e){if(Pe.diagnosticTracing){const t="function"==typeof e?e():e;console.debug(d+t)}}function y(e,...t){console.info(d+e,...t)}function v(e,...t){console.info(e,...t)}function E(e,...t){console.warn(d+e,...t)}function _(e,...t){if(t&&t.length>0&&t[0]&&"object"==typeof t[0]){if(t[0].silent)return;if(t[0].toString)return void console.error(d+e,t[0].toString())}console.error(d+e,...t)}function x(e,t,o){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}t(o?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){m.error(`proxyConsole failed: ${e}`)}}}function j(e,t,o){f=t,g=e,m={...t};const n=`${o}/console`.replace("https://","wss://").replace("http://","ws://");u=new WebSocket(n),u.addEventListener("error",A),u.addEventListener("close",S),function(){for(const e of c)f[e]=x(`console.${e}`,T,!0)}()}function R(e){let t=30;const o=()=>{u?0==u.bufferedAmount||0==t?(e&&v(e),function(){for(const e of c)f[e]=x(`console.${e}`,m.log,!1)}(),u.removeEventListener("error",A),u.removeEventListener("close",S),u.close(1e3,e),u=void 0):(t--,globalThis.setTimeout(o,100)):e&&m&&m.log(e)};o()}function T(e){u&&u.readyState===WebSocket.OPEN?u.send(e):m.log(e)}function A(e){m.error(`[${g}] proxy console websocket error: ${e}`,e)}function S(e){m.debug(`[${g}] proxy console websocket closed: ${e}`,e)}function D(){Pe.preferredIcuAsset=O(Pe.config);let e="invariant"==Pe.config.globalizationMode;if(!e)if(Pe.preferredIcuAsset)Pe.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===Pe.config.globalizationMode||"all"===Pe.config.globalizationMode||"sharded"===Pe.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}Pe.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,Pe.preferredIcuAsset=null}const t="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",o=Pe.config.environmentVariables;if(void 0===o[t]&&e&&(o[t]="1"),void 0===o.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(o.TZ=e)}catch(e){y("failed to detect timezone, will fallback to UTC")}}function O(e){var t;if((null===(t=e.resources)||void 0===t?void 0:t.icu)&&"invariant"!=e.globalizationMode){const t=e.applicationCulture||(ke?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale),o=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(o.length>=1)return o[0].name}else t&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const t=e.split("-")[0];return"en"===t||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(t)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(t)):n="icudt.dat";if(n)for(let e=0;e<o.length;e++){const t=o[e];if(t.virtualPath===n)return t.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const C=class{constructor(e){this.url=e}toString(){return this.url}};async function k(e,t){try{const o="function"==typeof globalThis.fetch;if(Se){const n=e.startsWith("file://");if(!n&&o)return globalThis.fetch(e,t||{credentials:"same-origin"});p||(h=Ne.require("url"),p=Ne.require("fs")),n&&(e=h.fileURLToPath(e));const r=await p.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(o)return globalThis.fetch(e,t||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(t){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t},text:()=>{throw t}}}throw new Error("No fetch implementation available")}function I(e){return"string"!=typeof e&&Be(!1,"url must be a string"),!M(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}const U=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,P=/[a-zA-Z]:[\\/]/;function M(e){return Se||Ie?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||P.test(e):U.test(e)}let L,N=0;const $=[],z=[],W=new Map,F={"js-module-threads":!0,"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},B={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},q={...B,manifest:!0},H={...B,dotnetwasm:!0},J={dotnetwasm:!0,symbols:!0},Z={...B,dotnetwasm:!0,symbols:!0},Q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=Pe.preferredIcuAsset)}function K(e,t,o){null!=t||(t=[]),Be(1==t.length,`Expect to have one ${o} asset in resources`);const n=t[0];return n.behavior=o,X(n),e.push(n),n}function X(e){V[e.behavior]&&W.set(e.behavior,e)}function Y(e){Be(V[e],`Unknown single asset behavior ${e}`);const t=W.get(e);if(t&&!t.resolvedUrl)if(t.resolvedUrl=Pe.locateFile(t.name),F[t.behavior]){const e=ge(t);e?("string"!=typeof e&&Be(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),t.resolvedUrl=e):t.resolvedUrl=ce(t.resolvedUrl,t.behavior)}else if("dotnetwasm"!==t.behavior)throw new Error(`Unknown single asset behavior ${e}`);return t}function ee(e){const t=Y(e);return Be(t,`Single asset for ${e} not found`),t}let te=!1;async function oe(){if(!te){te=!0,Pe.diagnosticTracing&&b("mono_download_assets");try{const e=[],t=[],o=(e,t)=>{!Z[e.behavior]&&G(e)&&Pe.expected_instantiated_assets_count++,!H[e.behavior]&&G(e)&&(Pe.expected_downloaded_assets_count++,t.push(se(e)))};for(const t of $)o(t,e);for(const e of z)o(e,t);Pe.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...t]).then((()=>{Pe.allDownloadsFinished.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),await Pe.runtimeModuleLoaded.promise;const n=async e=>{const t=await e;if(t.buffer){if(!Z[t.behavior]){t.buffer&&"object"==typeof t.buffer||Be(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof t.resolvedUrl&&Be(!1,"resolvedUrl must be string");const e=t.resolvedUrl,o=await t.buffer,n=new Uint8Array(o);pe(t),await Ue.beforeOnRuntimeInitialized.promise,Ue.instantiate_asset(t,e,n)}}else J[t.behavior]?("symbols"===t.behavior&&(await Ue.instantiate_symbols_asset(t),pe(t)),J[t.behavior]&&++Pe.actual_downloaded_assets_count):(t.isOptional||Be(!1,"Expected asset to have the downloaded buffer"),!H[t.behavior]&&G(t)&&Pe.expected_downloaded_assets_count--,!Z[t.behavior]&&G(t)&&Pe.expected_instantiated_assets_count--)},r=[],i=[];for(const t of e)r.push(n(t));for(const e of t)i.push(n(e));Promise.all(r).then((()=>{Ce||Ue.coreAssetsInMemory.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),Promise.all(i).then((async()=>{Ce||(await Ue.coreAssetsInMemory.promise,Ue.allAssetsInMemory.promise_control.resolve())})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e}))}catch(e){throw Pe.err("Error in mono_download_assets: "+e),e}}}let ne=!1;function re(){if(ne)return;ne=!0;const e=Pe.config,t=[];if(e.assets)for(const t of e.assets)"object"!=typeof t&&Be(!1,`asset must be object, it was ${typeof t} : ${t}`),"string"!=typeof t.behavior&&Be(!1,"asset behavior must be known string"),"string"!=typeof t.name&&Be(!1,"asset name must be string"),t.resolvedUrl&&"string"!=typeof t.resolvedUrl&&Be(!1,"asset resolvedUrl could be string"),t.hash&&"string"!=typeof t.hash&&Be(!1,"asset resolvedUrl could be string"),t.pendingDownload&&"object"!=typeof t.pendingDownload&&Be(!1,"asset pendingDownload could be object"),t.isCore?$.push(t):z.push(t),X(t);else if(e.resources){const o=e.resources;o.wasmNative||Be(!1,"resources.wasmNative must be defined"),o.jsModuleNative||Be(!1,"resources.jsModuleNative must be defined"),o.jsModuleRuntime||Be(!1,"resources.jsModuleRuntime must be defined"),K(z,o.wasmNative,"dotnetwasm"),K(t,o.jsModuleNative,"js-module-native"),K(t,o.jsModuleRuntime,"js-module-runtime"),o.jsModuleDiagnostics&&K(t,o.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,t,o)=>{const n=e;n.behavior=t,o?(n.isCore=!0,$.push(n)):z.push(n)};if(o.coreAssembly)for(let e=0;e<o.coreAssembly.length;e++)n(o.coreAssembly[e],"assembly",!0);if(o.assembly)for(let e=0;e<o.assembly.length;e++)n(o.assembly[e],"assembly",!o.coreAssembly);if(0!=e.debugLevel&&Pe.isDebuggingSupported()){if(o.corePdb)for(let e=0;e<o.corePdb.length;e++)n(o.corePdb[e],"pdb",!0);if(o.pdb)for(let e=0;e<o.pdb.length;e++)n(o.pdb[e],"pdb",!o.corePdb)}if(e.loadAllSatelliteResources&&o.satelliteResources)for(const e in o.satelliteResources)for(let t=0;t<o.satelliteResources[e].length;t++){const r=o.satelliteResources[e][t];r.culture=e,n(r,"resource",!o.coreAssembly)}if(o.coreVfs)for(let e=0;e<o.coreVfs.length;e++)n(o.coreVfs[e],"vfs",!0);if(o.vfs)for(let e=0;e<o.vfs.length;e++)n(o.vfs[e],"vfs",!o.coreVfs);const r=O(e);if(r&&o.icu)for(let e=0;e<o.icu.length;e++){const t=o.icu[e];t.name===r&&n(t,"icu",!1)}if(o.wasmSymbols)for(let e=0;e<o.wasmSymbols.length;e++)n(o.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let t=0;t<e.appsettings.length;t++){const o=e.appsettings[t],n=he(o);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||z.push({name:o,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...$,...z,...t]}async function ie(e){const t=await se(e);return await t.pendingDownloadInternal.response,t.buffer}async function se(e){try{return await ae(e)}catch(t){if(!Pe.enableDownloadRetry)throw t;if(Ie||Se)throw t;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw t;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw t;if(t&&404==t.status)throw t;e.pendingDownloadInternal=void 0,await Pe.allDownloadsQueued.promise;try{return Pe.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ae(e)}catch(t){return e.pendingDownloadInternal=void 0,await new Promise((e=>globalThis.setTimeout(e,100))),Pe.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ae(e)}}}async function ae(e){for(;L;)await L.promise;try{++N,N==Pe.maxParallelDownloads&&(Pe.diagnosticTracing&&b("Throttling further parallel downloads"),L=i());const t=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const t=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>t,json:()=>JSON.parse(new TextDecoder("utf-8").decode(t)),text:()=>{throw new Error("NotImplementedException")},headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const t=e.loadRemote&&Pe.config.remoteSources?Pe.config.remoteSources:[""];let o;for(let n of t){n=n.trim(),"./"===n&&(n="");const t=le(e,n);e.name===t?Pe.diagnosticTracing&&b(`Attempting to download '${t}'`):Pe.diagnosticTracing&&b(`Attempting to download '${t}' for ${e.name}`);try{e.resolvedUrl=t;const n=fe(e);if(e.pendingDownloadInternal=n,o=await n.response,!o||!o.ok)continue;return o}catch(e){o||(o={ok:!1,url:t,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&Pe.config.ignorePdbLoadErrors;if(o||Be(!1,`Response undefined ${e.name}`),!n){const t=new Error(`download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`);throw t.status=o.status,t}y(`optional download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`)}(e);return t?(J[e.behavior]||(e.buffer=await t.arrayBuffer(),++Pe.actual_downloaded_assets_count),e):e}finally{if(--N,L&&N==Pe.maxParallelDownloads-1){Pe.diagnosticTracing&&b("Resuming more parallel downloads");const e=L;L=void 0,e.promise_control.resolve()}}}function le(e,t){let o;return null==t&&Be(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?o=e.resolvedUrl:(o=""===t?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:t+e.name,o=ce(Pe.locateFile(o),e.behavior)),o&&"string"==typeof o||Be(!1,"attemptUrl need to be path or url string"),o}function ce(e,t){return Pe.modulesUniqueQuery&&q[t]&&(e+=Pe.modulesUniqueQuery),e}let de=0;const ue=new Set;function fe(e){try{e.resolvedUrl||Be(!1,"Request's resolvedUrl must be set");const t=function(e){let t=e.resolvedUrl;if(Pe.loadBootResource){const o=ge(e);if(o instanceof Promise)return o;"string"==typeof o&&(t=o)}const o={};return e.cache?o.cache=e.cache:Pe.config.disableNoCacheFetch||(o.cache="no-cache"),e.useCredentials?o.credentials="include":!Pe.config.disableIntegrityCheck&&e.hash&&(o.integrity=e.hash),Pe.fetch_like(t,o)}(e),o={name:e.name,url:e.resolvedUrl,response:t};return ue.add(e.name),o.response.then((()=>{"assembly"==e.behavior&&Pe.loadedAssemblies.push(e.name),de++,Pe.onDownloadResourceProgress&&Pe.onDownloadResourceProgress(de,ue.size)})),o}catch(t){const o={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(o)}}}const me={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs","js-module-threads":"dotnetjs"};function ge(e){var t;if(Pe.loadBootResource){const o=null!==(t=e.hash)&&void 0!==t?t:"",n=e.resolvedUrl,r=me[e.behavior];if(r){const t=Pe.loadBootResource(r,e.name,n,o,e.behavior);return"string"==typeof t?I(t):t}}}function pe(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function he(e){let t=e.lastIndexOf("/");return t>=0&&t++,e.substring(t)}async function we(e){e&&await Promise.all((null!=e?e:[]).map((e=>async function(e){try{const t=e.name;if(!e.moduleExports){const o=ce(Pe.locateFile(t),"js-module-library-initializer");Pe.diagnosticTracing&&b(`Attempting to import '${o}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */o)}Pe.libraryInitializers.push({scriptName:t,exports:e.moduleExports})}catch(t){E(`Failed to import library initializer '${e}': ${t}`)}}(e))))}async function be(e,t){if(!Pe.libraryInitializers)return;const o=[];for(let n=0;n<Pe.libraryInitializers.length;n++){const r=Pe.libraryInitializers[n];r.exports[e]&&o.push(ye(r.scriptName,e,(()=>r.exports[e](...t))))}await Promise.all(o)}async function ye(e,t,o){try{await o()}catch(o){throw E(`Failed to invoke '${t}' on library initializer '${e}': ${o}`),Xe(1,o),o}}function ve(e,t){if(e===t)return e;const o={...t};return void 0!==o.assets&&o.assets!==e.assets&&(o.assets=[...e.assets||[],...o.assets||[]]),void 0!==o.resources&&(o.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},o.resources)),void 0!==o.environmentVariables&&(o.environmentVariables={...e.environmentVariables||{},...o.environmentVariables||{}}),void 0!==o.runtimeOptions&&o.runtimeOptions!==e.runtimeOptions&&(o.runtimeOptions=[...e.runtimeOptions||[],...o.runtimeOptions||[]]),Object.assign(e,o)}function Ee(e,t){if(e===t)return e;const o={...t};return o.config&&(e.config||(e.config={}),o.config=ve(e.config,o.config)),Object.assign(e,o)}function _e(e,t){if(e===t)return e;const o={...t};return void 0!==o.coreAssembly&&(o.coreAssembly=[...e.coreAssembly||[],...o.coreAssembly||[]]),void 0!==o.assembly&&(o.assembly=[...e.assembly||[],...o.assembly||[]]),void 0!==o.lazyAssembly&&(o.lazyAssembly=[...e.lazyAssembly||[],...o.lazyAssembly||[]]),void 0!==o.corePdb&&(o.corePdb=[...e.corePdb||[],...o.corePdb||[]]),void 0!==o.pdb&&(o.pdb=[...e.pdb||[],...o.pdb||[]]),void 0!==o.jsModuleWorker&&(o.jsModuleWorker=[...e.jsModuleWorker||[],...o.jsModuleWorker||[]]),void 0!==o.jsModuleNative&&(o.jsModuleNative=[...e.jsModuleNative||[],...o.jsModuleNative||[]]),void 0!==o.jsModuleDiagnostics&&(o.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...o.jsModuleDiagnostics||[]]),void 0!==o.jsModuleRuntime&&(o.jsModuleRuntime=[...e.jsModuleRuntime||[],...o.jsModuleRuntime||[]]),void 0!==o.wasmSymbols&&(o.wasmSymbols=[...e.wasmSymbols||[],...o.wasmSymbols||[]]),void 0!==o.wasmNative&&(o.wasmNative=[...e.wasmNative||[],...o.wasmNative||[]]),void 0!==o.icu&&(o.icu=[...e.icu||[],...o.icu||[]]),void 0!==o.satelliteResources&&(o.satelliteResources=function(e,t){if(e===t)return e;for(const o in t)e[o]=[...e[o]||[],...t[o]||[]];return e}(e.satelliteResources||{},o.satelliteResources||{})),void 0!==o.modulesAfterConfigLoaded&&(o.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...o.modulesAfterConfigLoaded||[]]),void 0!==o.modulesAfterRuntimeReady&&(o.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...o.modulesAfterRuntimeReady||[]]),void 0!==o.extensions&&(o.extensions={...e.extensions||{},...o.extensions||{}}),void 0!==o.vfs&&(o.vfs=[...e.vfs||[],...o.vfs||[]]),Object.assign(e,o)}function xe(){const e=Pe.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleWorker:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){Pe.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const t of e.assets){const o={};switch(t.behavior){case"assembly":o.assembly=[t];break;case"pdb":o.pdb=[t];break;case"resource":o.satelliteResources={},o.satelliteResources[t.culture]=[t];break;case"icu":o.icu=[t];break;case"symbols":o.wasmSymbols=[t];break;case"vfs":o.vfs=[t];break;case"dotnetwasm":o.wasmNative=[t];break;case"js-module-threads":o.jsModuleWorker=[t];break;case"js-module-runtime":o.jsModuleRuntime=[t];break;case"js-module-native":o.jsModuleNative=[t];break;case"js-module-diagnostics":o.jsModuleDiagnostics=[t];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${t.behavior} of asset ${t.name}`)}_e(e.resources,o)}}e.debugLevel,e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Ue.diagnosticTracing=Pe.diagnosticTracing=!!e.diagnosticTracing,Ue.waitForDebugger=e.waitForDebugger,Pe.maxParallelDownloads=e.maxParallelDownloads||Pe.maxParallelDownloads,Pe.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:Pe.enableDownloadRetry}let je=!1;async function Re(e){var t;if(je)return void await Pe.afterConfigLoaded.promise;let o;try{if(e.configSrc||Pe.config&&0!==Object.keys(Pe.config).length&&(Pe.config.assets||Pe.config.resources)||(e.configSrc="dotnet.boot.js"),o=e.configSrc,je=!0,o&&(Pe.diagnosticTracing&&b("mono_wasm_load_config"),await async function(e){const t=e.configSrc,o=Pe.locateFile(t);let n=null;void 0!==Pe.loadBootResource&&(n=Pe.loadBootResource("manifest",t,o,"","manifest"));let r,i=null;if(n)if("string"==typeof n)n.includes(".json")?(i=await s(I(n)),r=await Ae(i)):r=(await import(I(n))).config;else{const e=await n;"function"==typeof e.json?(i=e,r=await Ae(i)):r=e.config}else o.includes(".json")?(i=await s(ce(o,"manifest")),r=await Ae(i)):r=(await import(ce(o,"manifest"))).config;function s(e){return Pe.fetch_like(e,{method:"GET",credentials:"include",cache:"no-cache"})}Pe.config.applicationEnvironment&&(r.applicationEnvironment=Pe.config.applicationEnvironment),ve(Pe.config,r)}(e)),xe(),await we(null===(t=Pe.config.resources)||void 0===t?void 0:t.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[Pe.config]),e.onConfigLoaded)try{await e.onConfigLoaded(Pe.config,Le),xe()}catch(e){throw _("onConfigLoaded() failed",e),e}xe(),Pe.afterConfigLoaded.promise_control.resolve(Pe.config)}catch(t){const n=`Failed to load config file ${o} ${t} ${null==t?void 0:t.stack}`;throw Pe.config=e.config=Object.assign(Pe.config,{message:n,error:t,isError:!0}),Xe(1,new Error(n)),t}}function Te(){return!!globalThis.navigator&&(Pe.isChromium||Pe.isFirefox)}async function Ae(e){const t=Pe.config,o=await e.json();t.applicationEnvironment||o.applicationEnvironment||(o.applicationEnvironment=e.headers.get("Blazor-Environment")||e.headers.get("DotNet-Environment")||void 0),o.environmentVariables||(o.environmentVariables={});const n=e.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");n&&(o.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES=n);const r=e.headers.get("ASPNETCORE-BROWSER-TOOLS");return r&&(o.environmentVariables.__ASPNETCORE_BROWSER_TOOLS=r),o}"function"!=typeof importScripts||globalThis.onmessage||(globalThis.dotnetSidecar=!0);const Se="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,De="function"==typeof importScripts,Oe=De&&"undefined"!=typeof dotnetSidecar,Ce=De&&!Oe,ke="object"==typeof window||De&&!Se,Ie=!ke&&!Se;let Ue={},Pe={},Me={},Le={},Ne={},$e=!1;const ze={},We={config:ze},Fe={mono:{},binding:{},internal:Ne,module:We,loaderHelpers:Pe,runtimeHelpers:Ue,diagnosticHelpers:Me,api:Le};function Be(e,t){if(e)return;const o="Assert failed: "+("function"==typeof t?t():t),n=new Error(o);_(o,n),Ue.nativeAbort(n)}function Ve(){return void 0!==Pe.exitCode}function qe(){return Ue.runtimeReady&&!Ve()}function He(){Ve()&&Be(!1,`.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`),Ue.runtimeReady||Be(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function Je(){ke&&(globalThis.addEventListener("unhandledrejection",et),globalThis.addEventListener("error",tt))}let Ze,Qe;function Ge(e){Qe&&Qe(e),Xe(e,Pe.exitReason)}function Ke(e){Ze&&Ze(e||Pe.exitReason),Xe(1,e||Pe.exitReason)}function Xe(t,o){var n,r;const i=o&&"object"==typeof o;t=i&&"number"==typeof o.status?o.status:void 0===t?-1:t;const s=i&&"string"==typeof o.message?o.message:""+o;(o=i?o:Ue.ExitStatus?function(e,t){const o=new Ue.ExitStatus(e);return o.message=t,o.toString=()=>t,o}(t,s):new Error("Exit with code "+t+" "+s)).status=t,o.message||(o.message=s);const a=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>a})}catch(e){}const l=!!o.silent;if(o.silent=!0,Ve())Pe.diagnosticTracing&&b("mono_exit called after exit");else{try{We.onAbort==Ke&&(We.onAbort=Ze),We.onExit==Ge&&(We.onExit=Qe),ke&&(globalThis.removeEventListener("unhandledrejection",et),globalThis.removeEventListener("error",tt)),Ue.runtimeReady?(Ue.jiterpreter_dump_stats&&Ue.jiterpreter_dump_stats(!1),0===t&&(null===(n=Pe.config)||void 0===n?void 0:n.interopCleanupOnExit)&&Ue.forceDisposeProxies(!0,!0),e&&0!==t&&(null===(r=Pe.config)||void 0===r||r.dumpThreadsOnNonZeroExit)):(Pe.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){Pe.allDownloadsQueued.promise_control.reject(e),Pe.allDownloadsFinished.promise_control.reject(e),Pe.afterConfigLoaded.promise_control.reject(e),Pe.wasmCompilePromise.promise_control.reject(e),Pe.runtimeModuleLoaded.promise_control.reject(e),Ue.dotnetReady&&(Ue.dotnetReady.promise_control.reject(e),Ue.afterInstantiateWasm.promise_control.reject(e),Ue.beforePreInit.promise_control.reject(e),Ue.afterPreInit.promise_control.reject(e),Ue.afterPreRun.promise_control.reject(e),Ue.beforeOnRuntimeInitialized.promise_control.reject(e),Ue.afterOnRuntimeInitialized.promise_control.reject(e),Ue.afterPostRun.promise_control.reject(e))}(o))}catch(e){E("mono_exit A failed",e)}try{l||(function(e,t){if(0!==e&&t){const e=Ue.ExitStatus&&t instanceof Ue.ExitStatus?b:_;"string"==typeof t?e(t):(void 0===t.stack&&(t.stack=(new Error).stack+""),t.message?e(Ue.stringify_as_error_with_stack?Ue.stringify_as_error_with_stack(t.message+"\n"+t.stack):t.message+"\n"+t.stack):e(JSON.stringify(t)))}!Ce&&Pe.config&&(Pe.config.logExitCode?Pe.config.forwardConsoleLogsToWS?R("WASM EXIT "+e):v("WASM EXIT "+e):Pe.config.forwardConsoleLogsToWS&&R())}(t,o),function(e){if(ke&&!Ce&&Pe.config&&Pe.config.appendElementOnExit&&document){const t=document.createElement("label");t.id="tests_done",0!==e&&(t.style.background="red"),t.innerHTML=""+e,document.body.appendChild(t)}}(t))}catch(e){E("mono_exit B failed",e)}Pe.exitCode=t,Pe.exitReason||(Pe.exitReason=o),!Ce&&Ue.runtimeReady&&We.runtimeKeepalivePop()}if(Pe.config&&Pe.config.asyncFlushOnExit&&0===t)throw(async()=>{try{await async function(){try{const e=await import(/*! webpackIgnore: true */"process"),t=e=>new Promise(((t,o)=>{e.on("error",o),e.end("","utf8",t)})),o=t(e.stderr),n=t(e.stdout);let r;const i=new Promise((e=>{r=setTimeout((()=>e("timeout")),1e3)}));await Promise.race([Promise.all([n,o]),i]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(t,o)}})(),o;Ye(t,o)}function Ye(e,t){if(Ue.runtimeReady&&Ue.nativeExit)try{Ue.nativeExit(e)}catch(e){!Ue.ExitStatus||e instanceof Ue.ExitStatus||E("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!ke)throw Se&&Ne.process?Ne.process.exit(e):Ue.quit&&Ue.quit(e,t),t}function et(e){ot(e,e.reason,"rejection")}function tt(e){ot(e,e.error,"error")}function ot(e,t,o){e.preventDefault();try{t||(t=new Error("Unhandled "+o)),void 0===t.stack&&(t.stack=(new Error).stack),t.stack=t.stack+"",t.silent||(_("Unhandled error:",t),Xe(1,t))}catch(e){}}!function(e){if($e)throw new Error("Loader module already loaded");$e=!0,Ue=e.runtimeHelpers,Pe=e.loaderHelpers,Me=e.diagnosticHelpers,Le=e.api,Ne=e.internal,Object.assign(Le,{INTERNAL:Ne,invokeLibraryInitializers:be}),Object.assign(e.module,{config:ve(ze,{environmentVariables:{}})});const r={mono_wasm_bindings_is_ready:!1,config:e.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"44525024595742ebe09023abe709df51de65009b",config:e.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:i(),allDownloadsQueued:i(),allDownloadsFinished:i(),wasmCompilePromise:i(),runtimeModuleLoaded:i(),loadingWorkers:i(),is_exited:Ve,is_runtime_running:qe,assert_runtime_running:He,mono_exit:Xe,createPromiseController:i,getPromiseController:s,assertIsControllablePromise:a,mono_download_assets:oe,resolve_single_asset_path:ee,setup_proxy_console:j,set_thread_prefix:w,installUnhandledErrorHandler:Je,retrieve_asset_download:ie,invokeLibraryInitializers:be,isDebuggingSupported:Te,exceptions:t,simd:n,relaxedSimd:o};Object.assign(Ue,r),Object.assign(Pe,l)}(Fe);let nt,rt,it,st=!1,at=!1;async function lt(e){if(!at){if(at=!0,ke&&Pe.config.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&j("main",globalThis.console,globalThis.location.origin),We||Be(!1,"Null moduleConfig"),Pe.config||Be(!1,"Null moduleConfig.config"),"function"==typeof e){const t=e(Fe.api);if(t.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(We,t),Ee(We,t)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");Ee(We,e)}await async function(e){if(Se){const e=await import(/*! webpackIgnore: true */"process"),t=14;if(e.versions.node.split(".")[0]<t)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${t}. See also https://aka.ms/dotnet-wasm-features`)}const t=/*! webpackIgnore: true */import.meta.url,o=t.indexOf("?");var n;if(o>0&&(Pe.modulesUniqueQuery=t.substring(o)),Pe.scriptUrl=t.replace(/\\/g,"/").replace(/[?#].*/,""),Pe.scriptDirectory=(n=Pe.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",Pe.locateFile=e=>"URL"in globalThis&&globalThis.URL!==C?new URL(e,Pe.scriptDirectory).toString():M(e)?e:Pe.scriptDirectory+e,Pe.fetch_like=k,Pe.out=console.log,Pe.err=console.error,Pe.onDownloadResourceProgress=e.onDownloadResourceProgress,ke&&globalThis.navigator){const e=globalThis.navigator,t=e.userAgentData&&e.userAgentData.brands;t&&t.length>0?Pe.isChromium=t.some((e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand)):e.userAgent&&(Pe.isChromium=e.userAgent.includes("Chrome"),Pe.isFirefox=e.userAgent.includes("Firefox"))}Ne.require=Se?await import(/*! webpackIgnore: true */"module").then((e=>e.createRequire(/*! webpackIgnore: true */import.meta.url))):Promise.resolve((()=>{throw new Error("require not supported")})),void 0===globalThis.URL&&(globalThis.URL=C)}(We)}}async function ct(e){return await lt(e),Ze=We.onAbort,Qe=We.onExit,We.onAbort=Ke,We.onExit=Ge,We.ENVIRONMENT_IS_PTHREAD?async function(){(function(){const e=new MessageChannel,t=e.port1,o=e.port2;t.addEventListener("message",(e=>{var n,r;n=JSON.parse(e.data.config),r=JSON.parse(e.data.monoThreadInfo),st?Pe.diagnosticTracing&&b("mono config already received"):(ve(Pe.config,n),Ue.monoThreadInfo=r,xe(),Pe.diagnosticTracing&&b("mono config received"),st=!0,Pe.afterConfigLoaded.promise_control.resolve(Pe.config),ke&&n.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&Pe.setup_proxy_console("worker-idle",console,globalThis.location.origin)),t.close(),o.close()}),{once:!0}),t.start(),self.postMessage({[l]:{monoCmd:"preload",port:o}},[o])})(),await Pe.afterConfigLoaded.promise,function(){const e=Pe.config;e.assets||Be(!1,"config.assets must be defined");for(const t of e.assets)X(t),Q[t.behavior]&&z.push(t)}(),setTimeout((async()=>{try{await oe()}catch(e){Xe(1,e)}}),0);const e=dt(),t=await Promise.all(e);return await ut(t),We}():async function(){var e;await Re(We),re();const t=dt();(async function(){try{const e=ee("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||Be(!1,"Can't load dotnet.native.wasm");const t=await e.pendingDownloadInternal.response,o=t.headers&&t.headers.get?t.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===o)n=await WebAssembly.compileStreaming(t);else{ke&&"application/wasm"!==o&&E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await t.arrayBuffer();Pe.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Ie?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,Pe.wasmCompilePromise.promise_control.resolve(n)}catch(e){Pe.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout((async()=>{try{D(),await oe()}catch(e){Xe(1,e)}}),0);const o=await Promise.all(t);return await ut(o),await Ue.dotnetReady.promise,await we(null===(e=Pe.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Le}()}function dt(){const e=ee("js-module-runtime"),t=ee("js-module-native");if(nt&&rt)return[nt,rt,it];"object"==typeof e.moduleExports?nt=e.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),nt=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof t.moduleExports?rt=t.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),rt=import(/*! webpackIgnore: true */t.resolvedUrl));const o=Y("js-module-diagnostics");return o&&("object"==typeof o.moduleExports?it=o.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),it=import(/*! webpackIgnore: true */o.resolvedUrl))),[nt,rt,it]}async function ut(e){const{initializeExports:t,initializeReplacements:o,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:i,setRuntimeGlobals:s,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];s(Fe),t(Fe),c&&c.setRuntimeGlobals(Fe),await n(We),Pe.runtimeModuleLoaded.promise_control.resolve(),l((e=>(Object.assign(We,{ready:e.ready,__dotnet_runtime:{initializeReplacements:o,configureEmscriptenStartup:r,configureWorkerStartup:i,passEmscriptenInternals:a}}),We))).catch((e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");throw e}))}const ft=new class{withModuleConfig(e){try{return Ee(We,e),this}catch(e){throw Xe(1,e),e}}withOnConfigLoaded(e){try{return Ee(We,{onConfigLoaded:e}),this}catch(e){throw Xe(1,e),e}}withConsoleForwarding(){try{return ve(ze,{forwardConsoleLogsToWS:!0}),this}catch(e){throw Xe(1,e),e}}withExitOnUnhandledError(){try{return ve(ze,{exitOnUnhandledError:!0}),Je(),this}catch(e){throw Xe(1,e),e}}withAsyncFlushOnExit(){try{return ve(ze,{asyncFlushOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withExitCodeLogging(){try{return ve(ze,{logExitCode:!0}),this}catch(e){throw Xe(1,e),e}}withElementOnExit(){try{return ve(ze,{appendElementOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withInteropCleanupOnExit(){try{return ve(ze,{interopCleanupOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withDumpThreadsOnNonZeroExit(){try{return ve(ze,{dumpThreadsOnNonZeroExit:!0}),this}catch(e){throw Xe(1,e),e}}withWaitingForDebugger(e){try{return ve(ze,{waitForDebugger:e}),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,t){try{return ve(ze,{interpreterPgo:e,interpreterPgoSaveDelay:t}),ze.runtimeOptions?ze.runtimeOptions.push("--interp-pgo-recording"):ze.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){try{return e&&"string"==typeof e||Be(!1,"must be file path or URL"),Ee(We,{configSrc:e}),this}catch(e){throw Xe(1,e),e}}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||Be(!1,"must be directory path"),ve(ze,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,t){try{const o={};return o[e]=t,ve(ze,{environmentVariables:o}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||Be(!1,"must be dictionary object"),ve(ze,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&Be(!1,"must be boolean"),ve(ze,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||Be(!1,"must be number"),ve(ze,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ve(ze,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ze.runtimeOptions?ze.runtimeOptions.push(...e):ze.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ve(ze,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ve(ze,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ve(ze,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return Pe.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lt(We),await Re(We),re(),D(),oe(),await Pe.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await ct(We),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}async run(){try{return We.config||Be(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}},mt=Xe,gt=ct;Ie||"function"==typeof globalThis.URL||Be(!1,"This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),"function"!=typeof globalThis.BigInt64Array&&Be(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),ft.withConfig(/*json-start*/{
  "mainAssemblyName": "Elsa.Studio.Host.CustomElements",
  "resources": {
    "hash": "sha256-2zQ0N++nUa9eYBSmGzvJVrSF4QqLDKelxFelG9EFa7M=",
    "jsModuleNative": [
      {
        "name": "dotnet.native.87vtjjdetb.js"
      }
    ],
    "jsModuleRuntime": [
      {
        "name": "dotnet.runtime.2tx45g8lli.js"
      }
    ],
    "wasmNative": [
      {
        "name": "dotnet.native.befq3iek54.wasm",
        "integrity": "sha256-cxtEpYwNaw5SZcxjGX5684Bzda4TyKmrK7bSsnG0NtA=",
        "cache": "force-cache"
      }
    ],
    "icu": [
      {
        "virtualPath": "icudt.dat",
        "name": "icudt.oh1zvcfom8.dat",
        "integrity": "sha256-tO5O5YzMTVSaKBboxAqezOQL9ewmupzV2JrB5Rkc8a4=",
        "cache": "force-cache"
      }
    ],
    "coreAssembly": [
      {
        "virtualPath": "System.Runtime.InteropServices.JavaScript.wasm",
        "name": "System.Runtime.InteropServices.JavaScript.tt6l0p3aft.wasm",
        "integrity": "sha256-LzaIRVBPgVLNRqmGw2jHYF3H+3EPN+YUXJU6QnSULY0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.CoreLib.wasm",
        "name": "System.Private.CoreLib.hco3yeuc29.wasm",
        "integrity": "sha256-AdjTQzoXE0YVc9owBAQ+G+3m3g0pl9YeNvZRUi2EQLs=",
        "cache": "force-cache"
      }
    ],
    "assembly": [
      {
        "virtualPath": "Blazored.FluentValidation.wasm",
        "name": "Blazored.FluentValidation.x0xsecpmtx.wasm",
        "integrity": "sha256-MSQuUWmw/uBMui2/n4kdEKi0h5j0G+EuO1sM6KK6ltM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazored.LocalStorage.wasm",
        "name": "Blazored.LocalStorage.12n6dz54qr.wasm",
        "integrity": "sha256-OaMAAd5n7ORfyur5e3QIyEVKJ76MKIvwbg7/icnnYcU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "BlazorMonaco.wasm",
        "name": "BlazorMonaco.98tzo6dc2v.wasm",
        "integrity": "sha256-R8FyBgovIXHO+Iv0CZyrVhS+T30U6xRRX4tNBwjDfI8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "CodeBeam.MudBlazor.Extensions.wasm",
        "name": "CodeBeam.MudBlazor.Extensions.bydg23m35f.wasm",
        "integrity": "sha256-XQfaBBhYf6CLg5AOKUqffYPVyVZ6UZ9fDR7fs83XOhQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Api.Client.wasm",
        "name": "Elsa.Api.Client.19uxhxtwlk.wasm",
        "integrity": "sha256-m0MjtcYBdECkRBB/LiM1mEBjU+LvrsjLMHR03IE5e1g=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "FluentValidation.wasm",
        "name": "FluentValidation.yuptatg8bv.wasm",
        "integrity": "sha256-1vkL1fNCyvLkWYtavCvKdUZS0BPY392ec7LutoPTps4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Humanizer.wasm",
        "name": "Humanizer.eggg0i7lf1.wasm",
        "integrity": "sha256-Tu9yMfVipRZC2mXDz8EkDZ9hzbRfvg/gRWzEMyp6neI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Authorization.185e59rzqg.wasm",
        "integrity": "sha256-D5XidqmXLxGw/v+ZC1Uzam+c9C1isciGfRn0coN09Sc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.wasm",
        "name": "Microsoft.AspNetCore.Components.7wkw9fbef3.wasm",
        "integrity": "sha256-OVlKw+t4a+Hs80ZJuhjZSvG8givq03634Q3LII9HrRg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Components.Authorization.7tplfk6or1.wasm",
        "integrity": "sha256-6xREBPkYMeRCXcStmkkazoHqSBZCrmTL2OLSG/Lv8uM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.CustomElements.wasm",
        "name": "Microsoft.AspNetCore.Components.CustomElements.9q3munff2o.wasm",
        "integrity": "sha256-YQa/sop2y6cMSvmP1CNOcRoVRiSaONVJwgruJVYNFdU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.wasm",
        "name": "Microsoft.AspNetCore.Components.Forms.5y64gmei0y.wasm",
        "integrity": "sha256-03vdkTZTzHzTrOdpPTXki9mu2sdOpo2WD/Qz3q+ipVg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Web.wasm",
        "name": "Microsoft.AspNetCore.Components.Web.1dxmaxcdvl.wasm",
        "integrity": "sha256-mz+/kbElrprmQciO6+2YfAMUSiRN001Urckj5tPS7Vs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.wasm",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.vfhvuk5ckb.wasm",
        "integrity": "sha256-9Zf9l9CYzlvQ28sv/m+Om6ac3vj2LIycYssNnFOc7v4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Connections.Abstractions.wasm",
        "name": "Microsoft.AspNetCore.Connections.Abstractions.5h8d1ve7n5.wasm",
        "integrity": "sha256-13YBWI/XIVujZRkM+GJErMTFyfBOKqf23oxjOgvuWWg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Http.Connections.Client.wasm",
        "name": "Microsoft.AspNetCore.Http.Connections.Client.77zevojlki.wasm",
        "integrity": "sha256-TQfm3YkciArhaln7JmbdUvmFZycUU+gw0LcqT2GnKEU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Http.Connections.Common.wasm",
        "name": "Microsoft.AspNetCore.Http.Connections.Common.9c25ro3znw.wasm",
        "integrity": "sha256-Gea96aorcuhUor2d+6jc7NM2+DwTqhS8LPBxQmfNkvA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Metadata.wasm",
        "name": "Microsoft.AspNetCore.Metadata.3tj1rw598p.wasm",
        "integrity": "sha256-hyVhdjUpyHZZbSg8L54mTM1ZMzP8qbsRT1GFPfuKAW4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.SignalR.Client.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Client.6v87gadgle.wasm",
        "integrity": "sha256-Xo+Rp+RZWNiRtpWqLu5yz1sndJuo1bP7dDESox43Qtw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.SignalR.Client.Core.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Client.Core.3i3vc5egw6.wasm",
        "integrity": "sha256-e8Od1Fd41pws2ENZx0KEmsQL5KpuasEN9hsPwy7VW4w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.SignalR.Common.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Common.7k0mpde2yi.wasm",
        "integrity": "sha256-Vxm0qRig9Y19qsZA5jMKfelGAwC0CJqjy/z29qECc4g=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.SignalR.Protocols.Json.wasm",
        "name": "Microsoft.AspNetCore.SignalR.Protocols.Json.34kx473cem.wasm",
        "integrity": "sha256-7euoZ3ZNT0rCiqWfIu4ViuetCh0+8iI721yvXsb7fG0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.WebUtilities.wasm",
        "name": "Microsoft.AspNetCore.WebUtilities.zqdwnht5z8.wasm",
        "integrity": "sha256-m3ionXv0H45Qtdntdeep8ugiviloBXQg/lDuiWsbmok=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.wasm",
        "name": "Microsoft.Extensions.Configuration.2ighdhd1um.wasm",
        "integrity": "sha256-yo4mXQQl/ydl7MB6Mv1BgbHLGRM8LU9qFvGn4/+0fiM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.wasm",
        "name": "Microsoft.Extensions.Configuration.Abstractions.6o6eit7sus.wasm",
        "integrity": "sha256-upBN5/bJpuCbmjE3Ho9YTBJ/WBDkgo59rnWE5M6/flc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Binder.wasm",
        "name": "Microsoft.Extensions.Configuration.Binder.88atjg19g7.wasm",
        "integrity": "sha256-+KPOvU2tnVXqr+FQkrZZS/hJNIDA4iLrLpRy2qQvQmA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Json.wasm",
        "name": "Microsoft.Extensions.Configuration.Json.dtdoolwb2w.wasm",
        "integrity": "sha256-D1jnvoQnv2aAi2ps5NzM3lqE5jEy+XJit2tS6neJnXM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.79qy5ms8qg.wasm",
        "integrity": "sha256-lUlrBksBkQaAt6ES1aYJuSgqEycaq9un03HjojneMNU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.quxx8eldpx.wasm",
        "integrity": "sha256-XkZTHGMY1ePr7OTxepNynEL7Na73cccr/QAUtQ3DWN4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.wasm",
        "name": "Microsoft.Extensions.Diagnostics.47qgfhx9eb.wasm",
        "integrity": "sha256-oIu8yLUtJP4EGU5Om1fDSAzpY3ffMjl9rKZnwII0XTs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.wasm",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.isypqwhob8.wasm",
        "integrity": "sha256-npldTut68mqzQngvA3K/GSRCbYWhEVEjs1SJ18ArUuo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Features.wasm",
        "name": "Microsoft.Extensions.Features.xeuqs8zk69.wasm",
        "integrity": "sha256-eQisN26qr0dy/Hdj3clB44fHYJJAAiiqTH7f+qcLjgA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.wasm",
        "name": "Microsoft.Extensions.Http.0f9fvouf0r.wasm",
        "integrity": "sha256-bFxKAXSThrME2TTBRhHNVfCY3tWgHff6eIk860d1+B8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.Polly.wasm",
        "name": "Microsoft.Extensions.Http.Polly.mhsxpbhz5b.wasm",
        "integrity": "sha256-hlxuxBpGYwzyPDI5hjHGLreDA++rSR/mzQt9duvQ3B4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Localization.wasm",
        "name": "Microsoft.Extensions.Localization.xcslyy3nju.wasm",
        "integrity": "sha256-L2P/tLhZ6FSR1KG27vIE/jer8JBjOAPRMf7D9eFEUNs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Localization.Abstractions.wasm",
        "name": "Microsoft.Extensions.Localization.Abstractions.n3xzr1mcfu.wasm",
        "integrity": "sha256-U5E1N/0VF+NL2SQ736sE5KFmUNdmbxzWIWaNFYzoKSU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.wasm",
        "name": "Microsoft.Extensions.Logging.rdg544eyp4.wasm",
        "integrity": "sha256-L1bF5n+9gpBxrjwDtqRbuzHX6GpEkWbSB2dZzuI8c8U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.wasm",
        "name": "Microsoft.Extensions.Logging.Abstractions.5gd4kr2cr5.wasm",
        "integrity": "sha256-FR1LHHYqW1kfz+fbE2W9SGEoqGzxcj9ofvH5YOwX184=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.wasm",
        "name": "Microsoft.Extensions.Options.9w9x65j0r5.wasm",
        "integrity": "sha256-pyWsUcPd3YLorRQE70Uke2ADx3z5FztEUqfU+lnWTo4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Primitives.wasm",
        "name": "Microsoft.Extensions.Primitives.boh3vzuzj5.wasm",
        "integrity": "sha256-xfjQqoHMPmyq5Vcf6A18PVWfjE0639VdEzDrueLJtSY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.wasm",
        "name": "Microsoft.JSInterop.8flfc3htr9.wasm",
        "integrity": "sha256-NOL6UggjZCsxssWRVEd6NBMooO7dipxYqHzR7bG+Y+U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.WebAssembly.wasm",
        "name": "Microsoft.JSInterop.WebAssembly.mrus3axuke.wasm",
        "integrity": "sha256-wmWkUz8XauAbA+eb0N0RvClESqaj+ct9V3xtGILLnp8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "MudBlazor.wasm",
        "name": "MudBlazor.drinqkvgcc.wasm",
        "integrity": "sha256-Ryetfnvdgszpsoq5gxyTfsOLbOP7eYB3a/WhqrTiAMQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "MudBlazor.Translations.wasm",
        "name": "MudBlazor.Translations.b5s38r9i21.wasm",
        "integrity": "sha256-werSFKKycufTBDhrF9MYnxEv1KelZ9gJNiaf/1cpGis=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Polly.wasm",
        "name": "Polly.edmxorbv1n.wasm",
        "integrity": "sha256-fjy1ix52+QqFaLZHRRfrWeTC3HzOC2RidJ1mm0syTbI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Polly.Core.wasm",
        "name": "Polly.Core.limuqwgaoj.wasm",
        "integrity": "sha256-g9jXAvuNN0GAfVm4ti+RF6ANEL7q4skfrt8aymlUxto=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Polly.Extensions.Http.wasm",
        "name": "Polly.Extensions.Http.aamf4ift6v.wasm",
        "integrity": "sha256-bqX9l2dLJ0m0IilNsXvbO14i9Lx0oDeudAGIsS3kEAw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Radzen.Blazor.wasm",
        "name": "Radzen.Blazor.hjuhjeexw9.wasm",
        "integrity": "sha256-I0Bqcvo7A1TKtolmEc91FYXeacVtBqFXIhXi13ZvLss=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Refit.wasm",
        "name": "Refit.pvpyi8nr27.wasm",
        "integrity": "sha256-hVn0zcJrCqcNR6EOofdX775hYG69LFPTpvvHr9dIOSc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Refit.HttpClientFactory.wasm",
        "name": "Refit.HttpClientFactory.u7tj2sv9yi.wasm",
        "integrity": "sha256-VoOnpUJhSbWOE+ZBE5MNTerzaBzMQn7hv+AeRRrSEXI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DEDrake.ShortGuid.wasm",
        "name": "DEDrake.ShortGuid.xvdfruyxwj.wasm",
        "integrity": "sha256-DYdQGjRgO7IhiqcKqcnznHqwzOObOJgoIY3wI+Tn1X4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "ThrottleDebounce.wasm",
        "name": "ThrottleDebounce.w3nnnu2hkc.wasm",
        "integrity": "sha256-vONRG66VipFAlp0Kj4sQLQ5tvkreqRefDgANCKnr+LU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.CSharp.wasm",
        "name": "Microsoft.CSharp.0irawbxmoj.wasm",
        "integrity": "sha256-SJoQSXwyiNWuZ7yBXiSEdkFYmG32LDLxF0kaXkk1Ek0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Concurrent.wasm",
        "name": "System.Collections.Concurrent.lzeu0xgd0o.wasm",
        "integrity": "sha256-YQs/AdkUGcQhP4bmWt+m4IhNnqqyWtv7FtRSmxQ1xD0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Immutable.wasm",
        "name": "System.Collections.Immutable.552jzz136h.wasm",
        "integrity": "sha256-Zy8JQ34sUouoysYNjNKL9xL4LccV/t8HJEazeqzVBw0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.NonGeneric.wasm",
        "name": "System.Collections.NonGeneric.2p95c2ud0n.wasm",
        "integrity": "sha256-VrRli9mnt7xY3pNJWuvPsxQ5u9ELFFcnBT3JguWKpho=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Specialized.wasm",
        "name": "System.Collections.Specialized.ffampignv4.wasm",
        "integrity": "sha256-tSDoxBO+xlfhhXynp08khmaySEDjJdbjVdHn1Mem8qE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.wasm",
        "name": "System.Collections.s56iv0wzrj.wasm",
        "integrity": "sha256-sC7zehQMtJQ4YZ2l7YAzOJCR0gSO7i43Hee+gieyJS4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.Annotations.wasm",
        "name": "System.ComponentModel.Annotations.h8vnfgjj7o.wasm",
        "integrity": "sha256-H2PgQNJ11DTNafCDEx50qbC/5Rk2lwLpMXb/GpvyTzU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.Primitives.wasm",
        "name": "System.ComponentModel.Primitives.5tn63wavih.wasm",
        "integrity": "sha256-937D+RHJ0mjtEVQJFuRMofu0tuvyD3BEeBYybRvlIz8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.TypeConverter.wasm",
        "name": "System.ComponentModel.TypeConverter.z5jlc8fo2v.wasm",
        "integrity": "sha256-R/CtsJb08qowzis2j0Z1SQfnZPV/wb0gASbPx0cqsr4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.wasm",
        "name": "System.ComponentModel.to7xi487g5.wasm",
        "integrity": "sha256-0ziwZjBchC21l/Xxwu8RK/GtO5Mx4LcNziMfzEhQUn8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Console.wasm",
        "name": "System.Console.8mff40hc47.wasm",
        "integrity": "sha256-MG9TOLQkhPCONRsDs/ilwfl6C1LG0Yp99tYku6eQf1c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Core.wasm",
        "name": "System.Core.mszblqghkw.wasm",
        "integrity": "sha256-ClkRunIKwPDnYS7DtqeoRFyo/qZYK1qcdDs/LQyVngU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Data.Common.wasm",
        "name": "System.Data.Common.0idnb8i8j7.wasm",
        "integrity": "sha256-GM9Pe1W2c6UBG/fseAjZNnlqF1sTUtULll68EuuON1Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.DiagnosticSource.wasm",
        "name": "System.Diagnostics.DiagnosticSource.843tlkhejg.wasm",
        "integrity": "sha256-CEreTY3L0ht+mtiBzFi2yV35AqbhOYZiCSJ9jDhsU0A=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.Primitives.wasm",
        "name": "System.Drawing.Primitives.xhkwm166db.wasm",
        "integrity": "sha256-xYl+klCpZxUOShdaByTfB9SUN52ad9QwSceu6rXRdVg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.wasm",
        "name": "System.Drawing.sfv6afh671.wasm",
        "integrity": "sha256-WKUq/8/T1LPe2Fyy4LLvvyl4FHtt6T7zCREMzthqeDs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Compression.wasm",
        "name": "System.IO.Compression.wg5u0dmzid.wasm",
        "integrity": "sha256-MOuThtHukZsv67cPoNn9eaoVrp8zcg+OuXdVr+nhnXk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Pipelines.wasm",
        "name": "System.IO.Pipelines.18inh8hrcf.wasm",
        "integrity": "sha256-/6Jt7y/8owvxzl+atCBtUQFudpS9Ht3TYfrpSNKWwOY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Expressions.wasm",
        "name": "System.Linq.Expressions.6ab1o4s8yf.wasm",
        "integrity": "sha256-oqj3ICl7XCX+AjUk3lLKZBXDWhBAZzdnmMXomKKI5Mk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Queryable.wasm",
        "name": "System.Linq.Queryable.0my7lmgu33.wasm",
        "integrity": "sha256-+HdFvS348OpUIiDDMHjGnRaYctRdYVgjL2Ufz29RagM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.wasm",
        "name": "System.Linq.j5k5nfvo4o.wasm",
        "integrity": "sha256-uQdy6/9q3qXQs7l/NgR082kSV96MPMOrfdj7Fv/CHN0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Memory.wasm",
        "name": "System.Memory.7c4ztmqftl.wasm",
        "integrity": "sha256-t7XIMWi2HTl7PE7Zzt85dJH8CiO6LQS5Zfc6WmZ4sOQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.Json.wasm",
        "name": "System.Net.Http.Json.28qbzaor4j.wasm",
        "integrity": "sha256-ggTuXvTmOascmKtuF9Xn5ZchWMDavql7CHOcwxYAtuE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.wasm",
        "name": "System.Net.Http.v3sp9asdjw.wasm",
        "integrity": "sha256-dZTq+/+ZbiatjRE7axcgII0pgcOFifcBV0ZpFieBN1U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Primitives.wasm",
        "name": "System.Net.Primitives.v9j66rdnee.wasm",
        "integrity": "sha256-E0m/py++E1U11risCEgUTguD98pDk+R1oRIvEBxH4UY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Requests.wasm",
        "name": "System.Net.Requests.h1ynrktn9t.wasm",
        "integrity": "sha256-65hMf1L+Q2fcfOT8wQ1F4IBXqp9xFbTn7yWhbN+FTCg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Security.wasm",
        "name": "System.Net.Security.sa1o5k85zw.wasm",
        "integrity": "sha256-bC4H86CWTcO4g8kQHWBo/iNfwoKTTwJMiCyJf7De28Y=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.ServerSentEvents.wasm",
        "name": "System.Net.ServerSentEvents.jtk45g1pp7.wasm",
        "integrity": "sha256-Z/d1eZPdsxk1sP46MunpkAOuQScoGxUo8Cz+sRmoTzI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebSockets.Client.wasm",
        "name": "System.Net.WebSockets.Client.1kgf68iw86.wasm",
        "integrity": "sha256-b/iOozPwVjrpgmf2VnCln9g/0U20JdIDDycc3UuiptQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebSockets.wasm",
        "name": "System.Net.WebSockets.t5i7b0inez.wasm",
        "integrity": "sha256-sSLQy3jGNMJb1uYzmP2fVDsTbYIgGOwP5jTetsD1Oi4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ObjectModel.wasm",
        "name": "System.ObjectModel.c8jbp7v18b.wasm",
        "integrity": "sha256-HJvIg7MVBEpRkzm/NFbtKPJ1dD0A29L/iKzOCdAoxUo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Uri.wasm",
        "name": "System.Private.Uri.mongvehtqg.wasm",
        "integrity": "sha256-5qKS23ku2W1WEEtWiVQ7zwBxNe3RtxBHbfFiRTzIXcU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.Linq.wasm",
        "name": "System.Private.Xml.Linq.5j6qt4gcjo.wasm",
        "integrity": "sha256-kXCzX9on61om2E6kJQPRhw/MPC7GBNzZRobX8J7afqQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.wasm",
        "name": "System.Private.Xml.k983lq9szd.wasm",
        "integrity": "sha256-mLl+A6V6cvxfXKwqWOch1GY1apN1MGUImEBtEIOnKEY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.DispatchProxy.wasm",
        "name": "System.Reflection.DispatchProxy.zmcg3px7we.wasm",
        "integrity": "sha256-+1oPKye1AkUvWlePtehWZJP78f+MWDsP7pJGFP8B2/4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Emit.ILGeneration.wasm",
        "name": "System.Reflection.Emit.ILGeneration.zjo6tpjogz.wasm",
        "integrity": "sha256-zzKmsHdaNhMGnM5w4f5fg7FYAz19YxdwrIffTKPq7lE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Emit.wasm",
        "name": "System.Reflection.Emit.z99eoq970s.wasm",
        "integrity": "sha256-icm+num+un7JFZ/EbKOf7jUj6MJh822txIFFtt3s+aY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Primitives.wasm",
        "name": "System.Reflection.Primitives.fwq0oxl9pf.wasm",
        "integrity": "sha256-0as3GpS5RofTsrGMXBcS34uPilKSzARn71mLryBe+bo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.InteropServices.wasm",
        "name": "System.Runtime.InteropServices.czq9qmgm70.wasm",
        "integrity": "sha256-KB0TE1OfMUz076x8jL3nYYLNyocTKdVnLtN6CB5wRMk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Numerics.wasm",
        "name": "System.Runtime.Numerics.n54lqjx14q.wasm",
        "integrity": "sha256-S97eLfi/fFJfYNJke6yOcpfoAeyulEyzKOISGQOoNTk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Serialization.Primitives.wasm",
        "name": "System.Runtime.Serialization.Primitives.hwtju2wvk5.wasm",
        "integrity": "sha256-3/s316/8UJr3rbvlQP2l5uHFvBfGmrNCDgG+ezlaUQw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.wasm",
        "name": "System.Runtime.b8zn6k52ej.wasm",
        "integrity": "sha256-5YdutvPwW2iWLUYTo2IxTDC45/I9jp9NltXKWeQD/ew=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Claims.wasm",
        "name": "System.Security.Claims.722bdyajqy.wasm",
        "integrity": "sha256-0cv5d5gV+xWwUo4TzY7aePXzlQ09RIdsIiGiN0F9ngw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.wasm",
        "name": "System.Security.Cryptography.hkfdarqn3x.wasm",
        "integrity": "sha256-73kgtNOZFPvoXXWM7qjYQUnaIYxWwIkrLedy92XBW0c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encodings.Web.wasm",
        "name": "System.Text.Encodings.Web.thoudobrm6.wasm",
        "integrity": "sha256-gdSWhoehbaR6sKjCczE3wKmEVW0tE4tjxM7ftn1IrAE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Json.wasm",
        "name": "System.Text.Json.xvs775nupu.wasm",
        "integrity": "sha256-A+eaA3/V0BmQtxm4rx1cUXWPtGIOmQUer9YjTQHq/6A=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.RegularExpressions.wasm",
        "name": "System.Text.RegularExpressions.ad9rzit9zh.wasm",
        "integrity": "sha256-0vwPPObX1daLvZNxWKYwoOZJNBa6dM0zc3aWUDOQWlQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.Channels.wasm",
        "name": "System.Threading.Channels.5p7c1s430j.wasm",
        "integrity": "sha256-B71wC6U6c7LlIIu5H0C6Yi3oBXqBU5QEwJ6v3d2qs4Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.wasm",
        "name": "System.Threading.wtzdkcyqqv.wasm",
        "integrity": "sha256-XCU9nObOGjjv0hX8Tpp0oOopAJ2wXZL2M+kKwvMGuzI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Web.HttpUtility.wasm",
        "name": "System.Web.HttpUtility.5olguwlgg6.wasm",
        "integrity": "sha256-tX/JbNS/m1t9QwF5G1uCbsMVasI7qr9K4iqR1KMSUAY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.Linq.wasm",
        "name": "System.Xml.Linq.bno46imr5t.wasm",
        "integrity": "sha256-x8MB2DkAOXhOCsJ/AzpobuVUqt727s2bp6fDdDQ1d8A=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.XDocument.wasm",
        "name": "System.Xml.XDocument.guz2zhmw0p.wasm",
        "integrity": "sha256-+Gif8+ZkUfFHYl2jBZ2iUkO0Q7IKlJXxUhQ8ow2uSuo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.wasm",
        "name": "System.l9jolnojrw.wasm",
        "integrity": "sha256-d1dcH2nxptL8QLqZqtRI5G+lrjrbUKrUOnC/yloVB98=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "netstandard.wasm",
        "name": "netstandard.gn9e9griw9.wasm",
        "integrity": "sha256-pL5p1uXbElWuQ7rh+xnA5CBDp+aOuGMsg2YOBhn9xxo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.ActivityPortProviders.wasm",
        "name": "Elsa.Studio.ActivityPortProviders.mbpvhf08rp.wasm",
        "integrity": "sha256-DiHFZd6CHUT9Ls0kFTdedf548VVM3sIDLPs7jAkZtok=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Authentication.Abstractions.wasm",
        "name": "Elsa.Studio.Authentication.Abstractions.7ba9n92yvs.wasm",
        "integrity": "sha256-jYgq0ZOejfXjvUdaabJVqycTJkHiWoSkguI2aJuxq/o=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Core.BlazorWasm.wasm",
        "name": "Elsa.Studio.Core.BlazorWasm.rmply4vym7.wasm",
        "integrity": "sha256-/Sg1eGoCNseJA42wXO8W0twgx9Dp5kQhXkCdiDIdFmg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Core.wasm",
        "name": "Elsa.Studio.Core.6rp0jdw6xg.wasm",
        "integrity": "sha256-nra1e9wJJr3R0HmAJyaOAnSOwhZPeATJBeByMmVQOOA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.DomInterop.wasm",
        "name": "Elsa.Studio.DomInterop.reoswnbxd3.wasm",
        "integrity": "sha256-kzay0lViysxkfy1FHAUZ7JG9mx+Bz3wM5Zvdutw9KSg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Localization.BlazorWasm.wasm",
        "name": "Elsa.Studio.Localization.BlazorWasm.art9fzrfwc.wasm",
        "integrity": "sha256-GrZXDb+cRzv0jmkjNT6v+fYUR09KtIUZ3vBaXkZVppU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Localization.wasm",
        "name": "Elsa.Studio.Localization.j1yfydpjcj.wasm",
        "integrity": "sha256-6Z8z/cxQ/j36L7dbZOT0J001Ts0/NXMusrOrOh7R7tM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Login.wasm",
        "name": "Elsa.Studio.Login.zr4rp5heqb.wasm",
        "integrity": "sha256-ETJZmvW300yTTEdYOY+YvCNttDdo1w86xsJ3AGsNHac=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Shared.wasm",
        "name": "Elsa.Studio.Shared.xd6uxqe9y8.wasm",
        "integrity": "sha256-1QS9rjV3ty84s3213Q4oS0SSvT4WanYNaJzNWuJN2aI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Shell.wasm",
        "name": "Elsa.Studio.Shell.rb2nskmkdc.wasm",
        "integrity": "sha256-echntgduUSD1UfBJelaqbidGMQxd1cO9sv/aWBNWE04=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.UIHints.wasm",
        "name": "Elsa.Studio.UIHints.4p6hbb9y2s.wasm",
        "integrity": "sha256-KEO5M9bUcQp7B6jjruSzinwhqM/GtqNdL3/MrAWP1tY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Workflows.Core.wasm",
        "name": "Elsa.Studio.Workflows.Core.mojapmm0k1.wasm",
        "integrity": "sha256-JrMV3b4qj9Qm/kK/KFuc/N2pwNNSTU4UkRb7Bt0wK/I=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Workflows.Designer.wasm",
        "name": "Elsa.Studio.Workflows.Designer.l5e6iwdx1z.wasm",
        "integrity": "sha256-KW+9ccHLue5xeVRObb/6QjsB/dTBatO9cpIJyNOaGQ8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Workflows.wasm",
        "name": "Elsa.Studio.Workflows.eqgjl0ptk1.wasm",
        "integrity": "sha256-duB1biOsbHJXTI98yWqKajb3dI6e9h6z9KiqxkIK47g=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Elsa.Studio.Host.CustomElements.wasm",
        "name": "Elsa.Studio.Host.CustomElements.wlg6vuljtu.wasm",
        "integrity": "sha256-bJ9BLvZP/WaiTFxXZ+rX+t/A/JEVRNiBDtpv3nbRWNE=",
        "cache": "force-cache"
      }
    ],
    "satelliteResources": {
      "af": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.e37hecxxff.wasm",
          "integrity": "sha256-Kgxl2vNjLdog27j0Ujc2eS7mgaAfnKFc6BlTDNWF/oM=",
          "cache": "force-cache"
        }
      ],
      "ar": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.3p8s72qmhs.wasm",
          "integrity": "sha256-+RXhv6e4DjLomC1BbAXuEnz764onkIqBEOhEiv6+RxE=",
          "cache": "force-cache"
        }
      ],
      "ca": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.7ym819qckn.wasm",
          "integrity": "sha256-0dRtLLQgdYDWwktTfeXTtRl6F8el3d5TchjeWS94XPU=",
          "cache": "force-cache"
        }
      ],
      "ckb": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.mjs3b4find.wasm",
          "integrity": "sha256-Qp4ETCwsV0B8fNcA6We9HqcOlEfyvGsZrUIvVcqjE2w=",
          "cache": "force-cache"
        }
      ],
      "cs": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.tahj933od7.wasm",
          "integrity": "sha256-q1zPKAjzg7fc6HU6nA52vwT5+2ATIGJL8Cp9LCj2rDw=",
          "cache": "force-cache"
        }
      ],
      "da": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.v8o5pxi6nn.wasm",
          "integrity": "sha256-/4YrKUMWXxMzvwuMWVOVNfVT8M7v6TlZZnx05iRZFAw=",
          "cache": "force-cache"
        }
      ],
      "de": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.xeodso3e0r.wasm",
          "integrity": "sha256-2Wj3QVAueDQ19CkKzfg9c5fULn2tyPEOINfH6EjrL30=",
          "cache": "force-cache"
        }
      ],
      "es": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.6f65m7e15j.wasm",
          "integrity": "sha256-wiEUFS8770kBQ4tiYbjtGvSRE6deLYoCtNMui1H8H2o=",
          "cache": "force-cache"
        }
      ],
      "fa": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.9dys92ppvu.wasm",
          "integrity": "sha256-A6LbK4LCuDatYV3KAG2Bu9G1flj4IV1VhumWUF4T6FU=",
          "cache": "force-cache"
        }
      ],
      "fi": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.ycuon2epvp.wasm",
          "integrity": "sha256-TnFBtZfHYqXwaRy55tYwPdQgeGqBorKFqrCYsuuYm38=",
          "cache": "force-cache"
        }
      ],
      "fr": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.82wtij424x.wasm",
          "integrity": "sha256-cHpyIpC6KpPWNwN2cSH1LzM8BiheGiS012mHJ9ILleY=",
          "cache": "force-cache"
        }
      ],
      "hr": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.b3067q0e57.wasm",
          "integrity": "sha256-99R0Sbvp0GNkecJ0qJ4qGVkNBxtyerN2UZJz5jerQEc=",
          "cache": "force-cache"
        }
      ],
      "hu": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.axnv1vsovl.wasm",
          "integrity": "sha256-Iji3mGXCxkjSSBeJNX3FrNgg6aP91fJTR0VOGUBUqcg=",
          "cache": "force-cache"
        }
      ],
      "it": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.ofgj3381w1.wasm",
          "integrity": "sha256-DJeaLQpaEzeqtStyxqOUQcgDcXdWe5iEBaS0wtZPVdc=",
          "cache": "force-cache"
        }
      ],
      "ja": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.dol4pckjwh.wasm",
          "integrity": "sha256-Q1A84ukZxLicdo76RclFqgZrHE/N1ItdyF0RgQUhSls=",
          "cache": "force-cache"
        }
      ],
      "ko": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.cvu41h9rq4.wasm",
          "integrity": "sha256-SErEbEjHvzJAApmfKHQhab4wuVPH7fmrbwCWgyT1/aQ=",
          "cache": "force-cache"
        }
      ],
      "lb": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.2qmicqursj.wasm",
          "integrity": "sha256-dujd5Q+8a26xh6NJydWzQEq/PVXgITw8uXFLAFOE5cs=",
          "cache": "force-cache"
        }
      ],
      "lv": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.f505yesnqy.wasm",
          "integrity": "sha256-/ZXgfwsPo5DBq3VTjS7+VbFtRt60TNsZ8jbVFTZby+I=",
          "cache": "force-cache"
        }
      ],
      "ms": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.rc2it0spgs.wasm",
          "integrity": "sha256-lebR7TvLOp/XVApyhw65GRmAGUmNZowAYEva51wcHYE=",
          "cache": "force-cache"
        }
      ],
      "nb-NO": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.iea3k6koxp.wasm",
          "integrity": "sha256-agMbHBgK9X0LUcLjaZviKltlOMzeiwAbk9bDMmcIE1Y=",
          "cache": "force-cache"
        }
      ],
      "ne": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.bg26eivkdf.wasm",
          "integrity": "sha256-65eDKDz9G0NhI1Gsuz1pdyEEcI0QXUIfKhdXc39iyyU=",
          "cache": "force-cache"
        }
      ],
      "nl": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.0cw3zitwcd.wasm",
          "integrity": "sha256-cCGAO+kU9adQ+4Pwnptwig4Nw+TJjlamIcVsd9oARxE=",
          "cache": "force-cache"
        }
      ],
      "nn-NO": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.gdhhoxd7se.wasm",
          "integrity": "sha256-ND6OhVeCx8RQWVQJwJcSUcohzYgHIWEU2UUmAvQ6oNs=",
          "cache": "force-cache"
        }
      ],
      "pl": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.jx7b73ep0k.wasm",
          "integrity": "sha256-52/C39PoTmI2/NGQwSDSPck/CkRFbiVdtySzc69Sj1A=",
          "cache": "force-cache"
        }
      ],
      "pt-BR": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.3akjbjwt6r.wasm",
          "integrity": "sha256-lbssc2Bd0Vi1vZ62lFQhcwyx7PY8tVBCFfP8j7TcXQM=",
          "cache": "force-cache"
        }
      ],
      "pt": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.ecqgu4qpk1.wasm",
          "integrity": "sha256-Xjdb3BxBaJR50vlV3wWOUJKiEmgQhozihmjcoQJ0/1Q=",
          "cache": "force-cache"
        }
      ],
      "ro": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.ucigqsj8io.wasm",
          "integrity": "sha256-EsVd3L9pAUuFHcV17v020wRA6z6+j6qyOtGWolj2w+U=",
          "cache": "force-cache"
        }
      ],
      "ru": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.rz412wo4lf.wasm",
          "integrity": "sha256-ee7jEDTIWvHrhuIPeVj+fSNDHV3pSXKsVZGiMAQYdYM=",
          "cache": "force-cache"
        }
      ],
      "sk": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.spf9js2var.wasm",
          "integrity": "sha256-0Hq1C09GTwYTPM38FdustHHjQwW7QYbM0LN/5GGzOKo=",
          "cache": "force-cache"
        }
      ],
      "sv": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.e94i0w39do.wasm",
          "integrity": "sha256-Etbbt3YHWX9Kz+ZoooHvxpJBmBFtOg1MefC8uCrRBZk=",
          "cache": "force-cache"
        }
      ],
      "ta": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.dylhhc9pix.wasm",
          "integrity": "sha256-ZbrBXk5hSnkgDyF3EEQ1O2935DtxVx49q7jxD3z4nF8=",
          "cache": "force-cache"
        }
      ],
      "tr": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.qs8t30vb73.wasm",
          "integrity": "sha256-Npv76RsL2rje6febcV/dctwHDqF9CG65pRNEsMXupAA=",
          "cache": "force-cache"
        }
      ],
      "uk": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.xid4xhiszp.wasm",
          "integrity": "sha256-hx1ltLZTkii/OIY73K04eD4Hcz9DL1mEAU0aFdMVg+k=",
          "cache": "force-cache"
        }
      ],
      "ur": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.ojca8jbvbw.wasm",
          "integrity": "sha256-BALJDAE5BA41EOr9cf78sGkT2fhpMFHI3K4uWnjXdlw=",
          "cache": "force-cache"
        }
      ],
      "uz": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.dnvfhsew1j.wasm",
          "integrity": "sha256-+c7QgPQeq/kue3oIB62bk0BA80FxwSEt333tJvEKg8I=",
          "cache": "force-cache"
        }
      ],
      "vi": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.lgm6rsu0kf.wasm",
          "integrity": "sha256-y31pOaIvY7esMddnkzuovzCUbwTvMVa7QCHjMh6TKaM=",
          "cache": "force-cache"
        }
      ],
      "zh-Hans": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.3tm5kwg6m5.wasm",
          "integrity": "sha256-t5Z5jVxsftx64zqfrEcKpmDcwQq9JGLRTgREqgjtoEY=",
          "cache": "force-cache"
        }
      ],
      "zh-Hant": [
        {
          "virtualPath": "MudBlazor.Translations.resources.wasm",
          "name": "MudBlazor.Translations.resources.kt6lqr5lun.wasm",
          "integrity": "sha256-yEtZg6/WnlZE1X+iH7uncHvvZv+I+NADktJz8MVxWpk=",
          "cache": "force-cache"
        }
      ]
    },
    "libraryInitializers": [
      {
        "name": "_content/Microsoft.AspNetCore.Components.CustomElements/Microsoft.AspNetCore.Components.CustomElements.lib.module.js"
      }
    ],
    "modulesAfterRuntimeReady": [
      {
        "name": "../_content/Microsoft.AspNetCore.Components.CustomElements/Microsoft.AspNetCore.Components.CustomElements.lib.module.js"
      }
    ]
  },
  "debugLevel": 0,
  "linkerEnabled": true,
  "appsettings": [
    "../appsettings.json"
  ],
  "globalizationMode": "all",
  "extensions": {
    "blazor": {}
  },
  "runtimeConfig": {
    "runtimeOptions": {
      "configProperties": {
        "Microsoft.AspNetCore.Components.Routing.RegexConstraintSupport": false,
        "Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability": true,
        "System.ComponentModel.DefaultValueAttribute.IsSupported": false,
        "System.ComponentModel.Design.IDesignerHost.IsSupported": false,
        "System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization": false,
        "System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported": false,
        "System.Data.DataSet.XmlSerializationIsSupported": false,
        "System.Diagnostics.Debugger.IsSupported": false,
        "System.Diagnostics.Metrics.Meter.IsSupported": false,
        "System.Diagnostics.Tracing.EventSource.IsSupported": false,
        "System.GC.Server": true,
        "System.Globalization.Invariant": false,
        "System.TimeZoneInfo.Invariant": false,
        "System.Linq.Enumerable.IsSizeOptimized": true,
        "System.Net.Http.EnableActivityPropagation": false,
        "System.Net.Http.WasmEnableStreamingResponse": true,
        "System.Net.SocketsHttpHandler.Http3Support": false,
        "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
        "System.Resources.ResourceManager.AllowCustomResourceTypes": false,
        "System.Resources.UseSystemResourceKeys": true,
        "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": true,
        "System.Runtime.InteropServices.BuiltInComInterop.IsSupported": false,
        "System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting": false,
        "System.Runtime.InteropServices.EnableCppCLIHostActivation": false,
        "System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop": false,
        "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false,
        "System.StartupHookProvider.IsSupported": false,
        "System.Text.Encoding.EnableUnsafeUTF7Encoding": false,
        "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault": true,
        "System.Threading.Thread.EnableAutoreleasePool": false,
        "Microsoft.AspNetCore.Components.Endpoints.NavigationManager.DisableThrowNavigationException": false
      }
    }
  }
}/*json-end*/);export{gt as default,ft as dotnet,mt as exit};
