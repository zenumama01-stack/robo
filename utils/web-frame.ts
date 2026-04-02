const { mainFrame, WebFrame } = process._linkedBinding('electron_renderer_web_frame');
// @ts-expect-error - WebFrame types are cursed. It's an instanced class, but
// the docs define it as a static module.
// TODO(smaddock): Fix web-frame.md to define it as an instance class.
const WebFramePrototype: Electron.WebFrame = WebFrame.prototype;
const routingIdDeprecated = deprecate.warnOnce('webFrame.routingId', 'webFrame.frameToken');
Object.defineProperty(WebFramePrototype, 'routingId', {
  get: function (this: Electron.WebFrame) {
    routingIdDeprecated();
    return ipcRendererUtils.invokeSync<number>(
      IPC_MESSAGES.BROWSER_GET_FRAME_ROUTING_ID_SYNC,
      this.frameToken
const findFrameByRoutingIdDeprecated = deprecate.warnOnce('webFrame.findFrameByRoutingId', 'webFrame.findFrameByToken');
WebFramePrototype.findFrameByRoutingId = function (
  routingId: number
): Electron.WebFrame | null {
  findFrameByRoutingIdDeprecated();
  const frameToken = ipcRendererUtils.invokeSync<string | undefined>(
    IPC_MESSAGES.BROWSER_GET_FRAME_TOKEN_SYNC,
    routingId
  return frameToken ? this.findFrameByToken(frameToken) : null;
export default mainFrame;
