const { WebFrameMain, fromId, fromFrameToken } = process._linkedBinding('electron_browser_web_frame_main');
Object.defineProperty(WebFrameMain.prototype, 'ipc', {
WebFrameMain.prototype.send = function (channel, ...args) {
    console.error('Error sending from webFrameMain: ', e);
WebFrameMain.prototype._sendInternal = function (channel, ...args) {
    return this._send(true /* internal */, channel, args);
WebFrameMain.prototype.postMessage = function (...args) {
  if (Array.isArray(args[2])) {
    args[2] = args[2].map(o => o instanceof MessagePortMain ? o._internalPort : o);
  this._postMessage(...args);
  fromId,
  fromFrameToken
