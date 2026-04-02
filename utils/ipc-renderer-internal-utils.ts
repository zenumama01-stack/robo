type IPCHandler = (event: Electron.IpcRendererEvent, ...args: any[]) => any
export const handle = function <T extends IPCHandler> (channel: string, handler: T) {
  ipcRendererInternal.on(channel, async (event, requestId, ...args) => {
    const replyChannel = `${channel}_RESPONSE_${requestId}`;
      event.sender.send(replyChannel, null, await handler(event, ...args));
      event.sender.send(replyChannel, error);
export function invokeSync<T> (command: string, ...args: any[]): T {
  const [error, result] = ipcRendererInternal.sendSync(command, ...args);
