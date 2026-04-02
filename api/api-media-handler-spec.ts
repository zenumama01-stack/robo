import { BrowserWindow, session, desktopCapturer } from 'electron/main';
import { ifit, listen } from './lib/spec-helpers';
describe('setDisplayMediaRequestHandler', () => {
  // These tests are done on an http server because navigator.userAgentData
  // requires a secure context.
  ifit(process.platform !== 'darwin')('works when calling getDisplayMedia', async function () {
    if ((await desktopCapturer.getSources({ types: ['screen'] })).length === 0) {
      return this.skip();
    const ses = session.fromPartition('' + Math.random());
    let requestHandlerCalled = false;
    let mediaRequest: any = null;
    ses.setDisplayMediaRequestHandler((request, callback) => {
      requestHandlerCalled = true;
      mediaRequest = request;
      desktopCapturer.getSources({ types: ['screen'] }).then((sources) => {
        // Grant access to the first screen found.
        const { id, name } = sources[0];
          video: { id, name }
          // TODO: 'loopback' and 'loopbackWithMute' are currently only supported on Windows.
          // audio: { id: 'loopback', name: 'System Audio' }
    const w = new BrowserWindow({ show: false, webPreferences: { session: ses } });
    const { ok, message } = await w.webContents.executeJavaScript(`
      navigator.mediaDevices.getDisplayMedia({
        video: true,
        audio: false,
      }).then(x => ({ok: x instanceof MediaStream}), e => ({ok: false, message: e.message}))
    `, true);
    expect(requestHandlerCalled).to.be.true();
    expect(mediaRequest.videoRequested).to.be.true();
    expect(mediaRequest.audioRequested).to.be.false();
    expect(ok).to.be.true(message);
  it('does not crash when using a bogus ID', async () => {
        video: { id: 'bogus', name: 'whatever' }
        audio: true,
    expect(ok).to.be.false();
    expect(message).to.equal('Could not start video source');
  it('successfully returns a capture handle', async () => {
    let w: BrowserWindow | null = null;
      callback({ video: w?.webContents.mainFrame });
    w = new BrowserWindow({ show: false, webPreferences: { session: ses } });
    const { ok, handleID, captureHandle, message } = await w.webContents.executeJavaScript(`
      const handleID = crypto.randomUUID();
      navigator.mediaDevices.setCaptureHandleConfig({
        handle: handleID,
        exposeOrigin: true,
        permittedOrigins: ["*"],
        audio: false
      }).then(stream => {
        const [videoTrack] = stream.getVideoTracks();
        const captureHandle = videoTrack.getCaptureHandle();
        return { ok: true, handleID, captureHandle, message: null }
      }, e => ({ ok: false, message: e.message }))
    expect(ok).to.be.true();
    expect(captureHandle.handle).to.be.a('string');
    expect(handleID).to.eq(captureHandle.handle);
    expect(message).to.be.null();
  it('does not crash when providing only audio for a video request', async () => {
    let callbackError: any;
          audio: 'loopback'
        callbackError = e;
    const { ok } = await w.webContents.executeJavaScript(`
    expect(callbackError?.message).to.equal('Video was requested, but no video stream was provided');
  it('does not crash when providing only an audio stream for an audio+video request', async () => {
  it('does not crash when providing a non-loopback audio stream', async () => {
        video: w.webContents.mainFrame,
        audio: 'default' as any
  it('does not crash when providing no streams', async () => {
        callback({});
    expect(callbackError.message).to.equal('Video was requested, but no video stream was provided');
  it('does not crash when using a bogus web-contents-media-stream:// ID', async () => {
        video: { id: 'web-contents-media-stream://9999:9999', name: 'whatever' }
  it('is not called when calling getUserMedia', async () => {
    ses.setDisplayMediaRequestHandler(() => {
      throw new Error('bad');
      navigator.mediaDevices.getUserMedia({
  it('works when calling getDisplayMedia with preferCurrentTab', async () => {
      callback({ video: w.webContents.mainFrame });
        preferCurrentTab: true,
  it('returns a MediaStream with BrowserCaptureMediaStreamTrack when the current tab is selected', async () => {
        return { ok: videoTrack instanceof BrowserCaptureMediaStreamTrack, message: null };
      }, e => ({ok: false, message: e.message}))
  ifit(process.platform !== 'darwin')('can supply a screen response to preferCurrentTab', async () => {
    ses.setDisplayMediaRequestHandler(async (request, callback) => {
      callback({ video: sources[0] });
  it('can supply a frame response', async () => {
  it('is not called when calling legacy getUserMedia', async () => {
      new Promise((resolve, reject) => navigator.getUserMedia({
      }, x => resolve({ok: x instanceof MediaStream}), e => reject({ok: false, message: e.message})))
  it('is not called when calling legacy getUserMedia with desktop capture constraint', async () => {
        video: {
          mandatory: {
            chromeMediaSource: 'desktop'
  it('works when calling getUserMedia without a media request handler', async () => {
  it('works when calling legacy getUserMedia without a media request handler', async () => {
  it('throws an error when calling legacy getUserMedia with invalid chromeMediaSourceId', async () => {
            chromeMediaSource: 'desktop',
            chromeMediaSourceId: undefined,
      }, x => resolve({ok: x instanceof MediaStream}), e => resolve({ ok: false, message: e.message })))
    expect(message).to.equal('Invalid state');
  it('can remove a displayMediaRequestHandler', async () => {
    ses.setDisplayMediaRequestHandler(null);
    expect(message).to.equal('Not supported');
