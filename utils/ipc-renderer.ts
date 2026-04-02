const internal = false;
class IpcRenderer extends EventEmitter implements Electron.IpcRenderer {
  sendToHost (channel: string, ...args: any[]) {
    return ipc.sendToHost(channel, args);
  async invoke (channel: string, ...args: any[]) {
    const { error, result } = await ipc.invoke(internal, channel, args);
  postMessage (channel: string, message: any, transferables: any) {
    return ipc.postMessage(channel, message, transferables);
export default new IpcRenderer();
