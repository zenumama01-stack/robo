import { ipcRenderer } from 'electron/renderer';
// ElectronApiServiceImpl will look for the "ipcNative" hidden object when
// invoking the 'onMessage' callback.
v8Util.setHiddenValue(globalThis, 'ipcNative', {
  onMessage (internal: boolean, channel: string, ports: MessagePort[], args: any[]) {
    const sender = internal ? ipcRendererInternal : ipcRenderer;
    sender.emit(channel, { sender, ports }, ...args);
