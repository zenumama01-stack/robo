import { MessagePortMain } from '@electron/internal/browser/message-port-main';
import type { ServiceWorkerMain } from 'electron/main';
import { ipcMain } from 'electron/main';
const webFrameMainBinding = process._linkedBinding('electron_browser_web_frame_main');
const addReplyToEvent = (event: Electron.IpcMainEvent) => {
  const { processId, frameId } = event;
  event.reply = (channel: string, ...args: any[]) => {
    event.sender.sendToFrame([processId, frameId], channel, ...args);
const addReturnValueToEvent = (event: Electron.IpcMainEvent | Electron.IpcMainServiceWorkerEvent) => {
  Object.defineProperty(event, 'returnValue', {
    set: (value) => event._replyChannel.sendReply(value),
    get: () => {}
const getServiceWorkerFromEvent = (event: Electron.IpcMainServiceWorkerEvent | Electron.IpcMainServiceWorkerInvokeEvent): ServiceWorkerMain | undefined => {
  return event.session.serviceWorkers._getWorkerFromVersionIDIfExists(event.versionId);
const addServiceWorkerPropertyToEvent = (event: Electron.IpcMainServiceWorkerEvent | Electron.IpcMainServiceWorkerInvokeEvent) => {
  Object.defineProperty(event, 'serviceWorker', {
    get: () => event.session.serviceWorkers.getWorkerFromVersionID(event.versionId)
 * Cached IPC emitters sorted by dispatch priority.
 * Caching is used to avoid frequent array allocations.
const cachedIpcEmitters: (ElectronInternal.IpcMainInternal | undefined)[] = [
  undefined, // WebFrameMain ipc
  undefined, // WebContents ipc
  ipcMain
// Get list of relevant IPC emitters for dispatch.
const getIpcEmittersForFrameEvent = (event: Electron.IpcMainEvent | Electron.IpcMainInvokeEvent): (ElectronInternal.IpcMainInternal | undefined)[] => {
  // Lookup by FrameTreeNode ID to ensure IPCs received after a frame swap are
  // always received. This occurs when a RenderFrame sends an IPC while it's
  // unloading and its internal state is pending deletion.
  const { frameTreeNodeId } = event;
  const webFrameByFtn = frameTreeNodeId ? webFrameMainBinding._fromFtnIdIfExists(frameTreeNodeId) : undefined;
  cachedIpcEmitters[0] = webFrameByFtn?.ipc;
  cachedIpcEmitters[1] = event.sender.ipc;
  return cachedIpcEmitters;
 * Listens for IPC dispatch events on `api`.
export function addIpcDispatchListeners (api: NodeJS.EventEmitter) {
  api.on('-ipc-message' as any, function (event: Electron.IpcMainEvent | Electron.IpcMainServiceWorkerEvent, channel: string, args: any[]) {
    const internal = v8Util.getHiddenValue<boolean>(event, 'internal');
    if (internal) {
      ipcMainInternal.emit(channel, event, ...args);
    } else if (event.type === 'frame') {
      addReplyToEvent(event);
      event.sender.emit('ipc-message', event, channel, ...args);
      for (const ipcEmitter of getIpcEmittersForFrameEvent(event)) {
        ipcEmitter?.emit(channel, event, ...args);
    } else if (event.type === 'service-worker') {
      addServiceWorkerPropertyToEvent(event);
      getServiceWorkerFromEvent(event)?.ipc.emit(channel, event, ...args);
  api.on('-ipc-invoke' as any, async function (event: Electron.IpcMainInvokeEvent | Electron.IpcMainServiceWorkerInvokeEvent, channel: string, args: any[]) {
    const replyWithResult = (result: any) => event._replyChannel.sendReply({ result });
    const replyWithError = (error: Error) => {
      console.error(`Error occurred in handler for '${channel}':`, error);
      event._replyChannel.sendReply({ error: error.toString() });
    const targets: (Electron.IpcMainServiceWorker | ElectronInternal.IpcMainInternal | undefined)[] = [];
      targets.push(ipcMainInternal);
      targets.push(...getIpcEmittersForFrameEvent(event));
      const workerIpc = getServiceWorkerFromEvent(event)?.ipc;
      targets.push(workerIpc);
    const target = targets.find(target => (target as any)?._invokeHandlers.has(channel));
    if (target) {
      const handler = (target as any)._invokeHandlers.get(channel);
        replyWithResult(await Promise.resolve(handler(event, ...args)));
        replyWithError(err as Error);
      replyWithError(new Error(`No handler registered for '${channel}'`));
  api.on('-ipc-message-sync' as any, function (event: Electron.IpcMainEvent | Electron.IpcMainServiceWorkerEvent, channel: string, args: any[]) {
    addReturnValueToEvent(event);
      const webContents = event.sender;
      const ipcEmitters = getIpcEmittersForFrameEvent(event);
        webContents.listenerCount('ipc-message-sync') === 0 &&
        ipcEmitters.every(emitter => !emitter || emitter.listenerCount(channel) === 0)
        console.warn(`WebContents #${webContents.id} called ipcRenderer.sendSync() with '${channel}' channel without listeners.`);
      webContents.emit('ipc-message-sync', event, channel, ...args);
      for (const ipcEmitter of ipcEmitters) {
  api.on('-ipc-message-host', function (event: Electron.IpcMainEvent, channel: string, args: any[]) {
    event.sender.emit('-ipc-message-host', event, channel, args);
  api.on('-ipc-ports' as any, function (event: Electron.IpcMainEvent | Electron.IpcMainServiceWorkerEvent, channel: string, message: any, ports: any[]) {
    event.ports = ports.map(p => new MessagePortMain(p));
        ipcEmitter?.emit(channel, event, message);
    } if (event.type === 'service-worker') {
      getServiceWorkerFromEvent(event)?.ipc.emit(channel, event, message);
