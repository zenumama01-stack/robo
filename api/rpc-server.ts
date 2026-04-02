import { clipboard } from 'electron/common';
import { webFrameMain } from 'electron/main';
// Implements window.close()
ipcMainInternal.on(IPC_MESSAGES.BROWSER_WINDOW_CLOSE, function (event) {
  const window = event.sender.getOwnerBrowserWindow();
  if (window) {
    window.close();
  event.returnValue = null;
ipcMainInternal.handle(IPC_MESSAGES.BROWSER_GET_LAST_WEB_PREFERENCES, function (event) {
  return event.sender.getLastWebPreferences();
ipcMainInternal.handle(IPC_MESSAGES.BROWSER_GET_PROCESS_MEMORY_INFO, function (event) {
  return event.sender._getProcessMemoryInfo();
// Methods not listed in this set are called directly in the renderer process.
const allowedClipboardMethods = (() => {
  switch (process.platform) {
    case 'darwin':
      return new Set(['readFindText', 'writeFindText']);
    case 'linux':
      return new Set(Object.keys(clipboard));
      return new Set();
ipcMainUtils.handleSync(IPC_MESSAGES.BROWSER_CLIPBOARD_SYNC, function (event, method: string, ...args: any[]) {
  if (!allowedClipboardMethods.has(method)) {
  return (clipboard as any)[method](...args);
const getPreloadScriptsFromEvent = (event: ElectronInternal.IpcMainInternalEvent) => {
  const session: Electron.Session = event.type === 'service-worker' ? event.session : event.sender.session;
  let preloadScripts = session.getPreloadScripts();
    preloadScripts = preloadScripts.filter(script => script.type === 'frame');
    const webPrefPreload = event.sender._getPreloadScript();
    if (webPrefPreload) preloadScripts.push(webPrefPreload);
    preloadScripts = preloadScripts.filter(script => script.type === 'service-worker');
    throw new Error(`getPreloadScriptsFromEvent: event.type is invalid (${(event as any).type})`);
  // TODO(samuelmaddock): Remove filter after Session.setPreloads is fully
  // deprecated. The new API will prevent relative paths from being registered.
  return preloadScripts.filter(script => path.isAbsolute(script.filePath));
const readPreloadScript = async function (script: Electron.PreloadScript): Promise<ElectronInternal.PreloadScript> {
  let contents;
  let error;
    contents = await fs.promises.readFile(script.filePath, 'utf8');
      error = err;
    ...script,
    contents,
ipcMainUtils.handleSync(IPC_MESSAGES.BROWSER_SANDBOX_LOAD, async function (event) {
  const preloadScripts = getPreloadScriptsFromEvent(event);
    preloadScripts: await Promise.all(preloadScripts.map(readPreloadScript)),
    process: {
      arch: process.arch,
      platform: process.platform,
      env: { ...process.env },
      version: process.version,
      versions: process.versions,
      execPath: process.helperExecPath
ipcMainUtils.handleSync(IPC_MESSAGES.BROWSER_NONSANDBOX_LOAD, function (event) {
  return { preloadPaths: preloadScripts.map(script => script.filePath) };
ipcMainInternal.on(IPC_MESSAGES.BROWSER_PRELOAD_ERROR, function (event, preloadPath: string, error: Error) {
  event.sender?.emit('preload-error', event, preloadPath, error);
ipcMainUtils.handleSync(IPC_MESSAGES.BROWSER_GET_FRAME_ROUTING_ID_SYNC, function (event, frameToken: string) {
  const senderFrame = event.senderFrame;
  if (!senderFrame || senderFrame.isDestroyed()) return;
  return webFrameMain.fromFrameToken(senderFrame.processId, frameToken)?.routingId;
ipcMainUtils.handleSync(IPC_MESSAGES.BROWSER_GET_FRAME_TOKEN_SYNC, function (event, routingId: number) {
  return webFrameMain.fromId(senderFrame.processId, routingId)?.frameToken;
