const clipboard = process._linkedBinding('electron_common_clipboard');
export default clipboard;
const clipboard = {} as Electron.Clipboard;
const originalClipboard = process._linkedBinding('electron_common_clipboard');
const warnDeprecatedAccess = function (method: keyof Electron.Clipboard) {
  return deprecate.warnOnceMessage(`Accessing 'clipboard.${method}' from the renderer process is
     deprecated and will be removed. Please use the 'contextBridge' API to access
     the clipboard API from the renderer.`);
const makeDeprecatedMethod = function (method: keyof Electron.Clipboard): any {
  const warnDeprecated = warnDeprecatedAccess(method);
  return (...args: any[]) => {
    warnDeprecated();
    return (originalClipboard[method] as any)(...args);
const makeRemoteMethod = function (method: keyof Electron.Clipboard): any {
    return ipcRendererUtils.invokeSync(IPC_MESSAGES.BROWSER_CLIPBOARD_SYNC, method, ...args);
  // On Linux we could not access clipboard in renderer process.
  for (const method of Object.keys(originalClipboard) as (keyof Electron.Clipboard)[]) {
    clipboard[method] = makeRemoteMethod(method);
    if (process.platform === 'darwin' && (method === 'readFindText' || method === 'writeFindText')) {
      clipboard[method] = makeDeprecatedMethod(method);
