const { ipcRenderer, contextBridge } = require('electron/renderer');
const policy = window.trustedTypes.createPolicy('electron-default-app', {
  // we trust the SVG contents
  createHTML: input => input
async function getOcticonSvg (name: string) {
    const response = await fetch(`octicon/${name}.svg`);
    div.innerHTML = policy.createHTML(await response.text());
async function loadSVG (element: HTMLSpanElement) {
  for (const cssClass of element.classList) {
    if (cssClass.startsWith('octicon-')) {
      const icon = await getOcticonSvg(cssClass.substr(8));
      if (icon) {
        for (const elemClass of element.classList) {
          icon.classList.add(elemClass);
        element.before(icon);
        element.remove();
async function initialize () {
  const electronPath = await ipcRenderer.invoke('bootstrap');
  function replaceText (selector: string, text: string, link?: string) {
    const element = document.querySelector<HTMLElement>(selector);
        const anchor = document.createElement('a');
        anchor.textContent = text;
        anchor.href = link;
        anchor.target = '_blank';
        element.appendChild(anchor);
        element.innerText = text;
  replaceText('.electron-version', `Electron v${process.versions.electron}`, 'https://electronjs.org/docs');
  replaceText('.chrome-version', `Chromium v${process.versions.chrome}`, 'https://developer.chrome.com/docs/chromium');
  replaceText('.node-version', `Node v${process.versions.node}`, `https://nodejs.org/docs/v${process.versions.node}/api`);
  replaceText('.v8-version', `v8 v${process.versions.v8}`, 'https://v8.dev/docs');
  replaceText('.command-example', `${electronPath} path-to-app`);
  for (const element of document.querySelectorAll<HTMLSpanElement>('.octicon')) {
    loadSVG(element);
contextBridge.exposeInMainWorld('electronDefaultApp', {
  initialize
interface PreloadContext {
  loadedModules: Map<string, any>;
  loadableModules: Map<string, any>;
  /** Process object to pass into preloads. */
  /** Globals to be exposed to preload context. */
  exposeGlobals: any;
export function createPreloadProcessObject (): NodeJS.Process {
  const preloadProcess: NodeJS.Process = new EventEmitter() as any;
  preloadProcess.getProcessMemoryInfo = () => {
  Object.defineProperty(preloadProcess, 'noDeprecation', {
      return process.noDeprecation;
    set (value) {
      process.noDeprecation = value;
  const { hasSwitch } = process._linkedBinding('electron_common_command_line');
  // Similar to nodes --expose-internals flag, this exposes _linkedBinding so
  // that tests can call it to get access to some test only bindings
  if (hasSwitch('unsafely-expose-electron-internals-for-testing')) {
    preloadProcess._linkedBinding = process._linkedBinding;
  return preloadProcess;
// This is the `require` function that will be visible to the preload script
function preloadRequire (context: PreloadContext, module: string) {
  if (context.loadedModules.has(module)) {
    return context.loadedModules.get(module);
  if (context.loadableModules.has(module)) {
    const loadedModule = context.loadableModules.get(module)!();
    context.loadedModules.set(module, loadedModule);
  throw new Error(`module not found: ${module}`);
// Wrap the script into a function executed in global scope. It won't have
// access to the current scope, so we'll expose a few objects as arguments:
// - `require`: The `preloadRequire` function
// - `process`: The `preloadProcess` object
// - `Buffer`: Shim of `Buffer` implementation
// - `global`: The window object, which is aliased to `global` by webpack.
function runPreloadScript (context: PreloadContext, preloadSrc: string) {
  const globalVariables = [];
  const fnParameters = [];
  for (const [key, value] of Object.entries(context.exposeGlobals)) {
    globalVariables.push(key);
    fnParameters.push(value);
  const preloadWrapperSrc = `(function(require, process, exports, module, ${globalVariables.join(', ')}) {
  ${preloadSrc}
  })`;
  // eval in window scope
  const preloadFn = context.createPreloadScript(preloadWrapperSrc);
  const exports = {};
  preloadFn(preloadRequire.bind(null, context), context.process, exports, { exports }, ...fnParameters);
 * Execute preload scripts within a sandboxed process.
export function executeSandboxedPreloadScripts (context: PreloadContext, preloadScripts: ElectronInternal.PreloadScript[]) {
  for (const { filePath, contents, error } of preloadScripts) {
      if (contents) {
        runPreloadScript(context, contents);
      console.error(`Unable to load preload script: ${filePath}`);
      ipcRendererInternal.send(IPC_MESSAGES.BROWSER_PRELOAD_ERROR, filePath, error);
