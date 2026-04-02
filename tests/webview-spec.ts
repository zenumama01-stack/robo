import { BrowserWindow, session, ipcMain, app, WebContents } from 'electron/main';
import { ifit, ifdescribe, defer, itremote, useRemoteContext, listen } from './lib/spec-helpers';
declare let WebView: any;
async function loadWebView (w: WebContents, attributes: Record<string, string>, opts?: {openDevTools?: boolean}): Promise<void> {
  const { openDevTools } = {
    openDevTools: false,
    ...opts
      webview.id = 'webview'
      for (const [k, v] of Object.entries(${JSON.stringify(attributes)})) {
        webview.setAttribute(k, v)
      webview.addEventListener('dom-ready', () => {
        if (${openDevTools}) {
          webview.openDevTools()
async function loadWebViewAndWaitForEvent (w: WebContents, attributes: Record<string, string>, eventName: string): Promise<any> {
  return await w.executeJavaScript(`new Promise((resolve, reject) => {
    webview.addEventListener(${JSON.stringify(eventName)}, (e) => resolve({...e}), {once: true})
async function loadWebViewAndWaitForMessage (w: WebContents, attributes: Record<string, string>): Promise<string> {
  const { message } = await loadWebViewAndWaitForEvent(w, attributes, 'console-message');
describe('<webview> tag', function () {
  const blankPageUrl = url.pathToFileURL(path.join(fixtures, 'pages', 'blank.html')).toString();
  function hideChildWindows (e: any, wc: WebContents) {
    wc.setWindowOpenHandler(() => ({
    app.on('web-contents-created', hideChildWindows);
    app.off('web-contents-created', hideChildWindows);
    it('works without script tag in page', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'webview-no-script.html'));
      await once(ipcMain, 'pong');
    it('works with sandbox', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'webview-isolated.html'));
    it('works with contextIsolation', async () => {
    it('works with contextIsolation + sandbox', async () => {
    it('works with Trusted Types', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'webview-trusted-types.html'));
          preload: path.join(fixtures, 'module', 'preload-webview.js'),
      const webview = once(ipcMain, 'webview');
      const [, type] = await webview;
      expect(type).to.equal('undefined', 'WebView still exists');
  // FIXME(deepak1556): Ch69 follow up.
  xdescribe('document.visibilityState/hidden', () => {
    it('updates when the window is shown after the ready-to-show event', async () => {
      const readyToShowSignal = once(w, 'ready-to-show');
      const pongSignal1 = once(ipcMain, 'pong');
      w.loadFile(path.join(fixtures, 'pages', 'webview-visibilitychange.html'));
      await pongSignal1;
      const pongSignal2 = once(ipcMain, 'pong');
      await readyToShowSignal;
      const [, visibilityState, hidden] = await pongSignal2;
      expect(hidden).to.be.false();
    it('inherits the parent window visibility state and receives visibilitychange events', async () => {
      expect(hidden).to.be.true();
      // We have to start waiting for the event
      // before we ask the webContents to resize.
      const getResponse = once(ipcMain, 'pong');
      w.webContents.emit('-window-visibility-change', 'visible');
      return getResponse.then(([, visibilityState, hidden]) => {
  describe('did-attach-webview event', () => {
    it('is emitted when a webview has been attached', async () => {
  describe('did-attach event', () => {
      const message = await w.webContents.executeJavaScript(`new Promise((resolve, reject) => {
        webview.setAttribute('src', 'about:blank')
        webview.addEventListener('did-attach', (e) => {
          resolve('ok')
      expect(message).to.equal('ok');
    it('emits when theme color changes', async () => {
        pathname: `${fixtures.replaceAll('\\', '/')}/pages/theme-color.html`,
        webview.addEventListener('did-change-theme-color', (e) => {
    // FIXME: This test is flaky on WOA, so skip it there.
    ifit(process.platform !== 'win32' || process.arch !== 'arm64')('loads devtools extensions registered on the parent window', async () => {
      w.webContents.session.removeExtension('foo');
      const extensionPath = path.join(__dirname, 'fixtures', 'devtools-extensions', 'foo');
      await w.webContents.session.loadExtension(extensionPath, {
        allowFileAccess: true
      w.loadFile(path.join(__dirname, 'fixtures', 'pages', 'webview-devtools.html'));
      loadWebView(w.webContents, {
        nodeintegration: 'on',
        webpreferences: 'contextIsolation=no',
        src: `file://${path.join(__dirname, 'fixtures', 'blank.html')}`
      }, { openDevTools: true });
      let childWebContentsId = 0;
      app.once('web-contents-created', (e, webContents) => {
        childWebContentsId = webContents.id;
        webContents.on('devtools-opened', function () {
          const showPanelIntervalId = setInterval(function () {
            if (!webContents.isDestroyed() && webContents.devToolsWebContents) {
              webContents.devToolsWebContents.executeJavaScript('(' + function () {
                const lastPanelId: any = tabs[tabs.length - 1].id;
              }.toString() + ')()');
              clearInterval(showPanelIntervalId);
      const [, { runtimeId, tabId }] = await once(ipcMain, 'answer');
      expect(runtimeId).to.match(/^[a-z]{32}$/);
      expect(tabId).to.equal(childWebContentsId);
      await w.webContents.executeJavaScript('webview.closeDevTools()');
  describe('zoom behavior', () => {
    const zoomScheme = standardScheme;
    const webviewSession = session.fromPartition('webview-temp');
      const protocol = webviewSession.protocol;
      protocol.registerStringProtocol(zoomScheme, (request, respond) => {
        respond('hello');
      protocol.unregisterProtocol(zoomScheme);
    it('inherits the zoomFactor of the parent window', async () => {
          zoomFactor: 1.2,
      const zoomEventPromise = once(ipcMain, 'webview-parent-zoom-level');
      w.loadFile(path.join(fixtures, 'pages', 'webview-zoom-factor.html'));
      const [, zoomFactor, zoomLevel] = await zoomEventPromise;
      expect(zoomFactor).to.equal(1.2);
      expect(zoomLevel).to.equal(1);
    it('maintains the zoom level for a given host in the same session after navigation', () => {
      const zoomPromise = new Promise<void>((resolve) => {
        ipcMain.on('webview-zoom-persist-level', (_event, values) => {
          resolve(values);
      w.loadFile(path.join(fixtures, 'pages', 'webview-zoom-change-persist-host.html'));
      expect(zoomPromise).to.eventually.deep.equal({
        initialZoomLevel: 2,
        switchZoomLevel: 3,
        finalZoomLevel: 2
    it('maintains zoom level on navigation', async () => {
      const promise = new Promise<void>((resolve) => {
        ipcMain.on('webview-zoom-level', (event, zoomLevel, zoomFactor, newHost, final) => {
          if (!newHost) {
            expect(zoomFactor).to.equal(1.44);
          if (final) {
      w.loadFile(path.join(fixtures, 'pages', 'webview-custom-zoom-level.html'));
    it('maintains zoom level when navigating within same page', async () => {
        ipcMain.on('webview-zoom-in-page', (event, zoomLevel, zoomFactor, final) => {
      w.loadFile(path.join(fixtures, 'pages', 'webview-in-page-navigate.html'));
    it('inherits zoom level for the origin when available', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'webview-origin-zoom-level.html'));
      const [, zoomLevel] = await once(ipcMain, 'webview-origin-zoom-level');
    it('does not crash when navigating with zoom level inherited from parent', async () => {
          session: webviewSession,
      const attachPromise = once(w.webContents, 'did-attach-webview') as Promise<[any, WebContents]>;
      const readyPromise = once(ipcMain, 'dom-ready');
      w.loadFile(path.join(fixtures, 'pages', 'webview-zoom-inherited.html'));
      const [, webview] = await attachPromise;
      await readyPromise;
      expect(webview.getZoomFactor()).to.equal(1.2);
      await w.loadURL(`${zoomScheme}://host1`);
    it('does not crash when changing zoom level after webview is destroyed', async () => {
      await w.loadFile(path.join(fixtures, 'pages', 'webview-zoom-inherited.html'));
      await attachPromise;
      await w.webContents.executeJavaScript('view.remove()');
  describe('requestFullscreen from webview', () => {
    async function loadWebViewWindow (): Promise<[BrowserWindow, WebContents]> {
      const readyPromise = once(ipcMain, 'webview-ready');
      w.loadFile(path.join(__dirname, 'fixtures', 'webview', 'fullscreen', 'main.html'));
      await Promise.all([readyPromise, loadPromise]);
      return [w, webview];
      // The leaving animation is un-observable but can interfere with future tests
      // Specifically this is async on macOS but can be on other platforms too
    ifit(process.platform !== 'darwin')('should make parent frame element fullscreen too (non-macOS)', async () => {
      const [w, webview] = await loadWebViewWindow();
      expect(await w.webContents.executeJavaScript('isIframeFullscreen()')).to.be.false();
      const parentFullscreen = once(ipcMain, 'fullscreenchange');
      await webview.executeJavaScript('document.getElementById("div").requestFullscreen()', true);
      await parentFullscreen;
      expect(await w.webContents.executeJavaScript('isIframeFullscreen()')).to.be.true();
      const close = once(w, 'closed');
    ifit(process.platform === 'darwin')('should make parent frame element fullscreen too (macOS)', async () => {
      const enterHTMLFS = once(w.webContents, 'enter-html-full-screen');
      const leaveHTMLFS = once(w.webContents, 'leave-html-full-screen');
      await webview.executeJavaScript('document.exitFullscreen()');
      await Promise.all([enterHTMLFS, leaveHTMLFS, parentFullscreen]);
    // FIXME(zcbenz): Fullscreen events do not work on Linux.
    ifit(process.platform !== 'linux')('exiting fullscreen should unfullscreen window', async () => {
      await webview.executeJavaScript('document.exitFullscreen()', true);
    it('pressing ESC should unfullscreen window', async () => {
      webview.sendInputEvent({ type: 'keyDown', keyCode: 'Escape' });
    it('pressing ESC should emit the leave-html-full-screen event', async () => {
      const enterFSWindow = once(w, 'enter-html-full-screen');
      const enterFSWebview = once(webContents, 'enter-html-full-screen');
      await webContents.executeJavaScript('document.getElementById("div").requestFullscreen()', true);
      await enterFSWindow;
      await enterFSWebview;
      const leaveFSWindow = once(w, 'leave-html-full-screen');
      const leaveFSWebview = once(webContents, 'leave-html-full-screen');
      webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Escape' });
      await leaveFSWebview;
      await leaveFSWindow;
    it('should support user gesture', async () => {
      const waitForEnterHtmlFullScreen = once(webview, 'enter-html-full-screen');
      const jsScript = "document.querySelector('video').webkitRequestFullscreen()";
      webview.executeJavaScript(jsScript, true);
      await waitForEnterHtmlFullScreen;
      w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true, webviewTag: true, contextIsolation: false } });
      // Don't wait for loading to finish.
        allowpopups: 'on',
        src: `file://${path.join(fixtures, 'api', 'native-window-open-blank.html')}`
        src: `file://${path.join(fixtures, 'api', 'native-window-open-file.html')}`
    it('returns null from window.open when allowpopups is not set', async () => {
        src: `file://${path.join(fixtures, 'api', 'native-window-open-no-allowpopups.html')}`
      const [, { windowOpenReturnedNull }] = await once(ipcMain, 'answer');
      expect(windowOpenReturnedNull).to.be.true();
        src: `file://${path.join(fixtures, 'api', 'native-window-open-cross-origin.html')}`
      const expectedContent =
          /Failed to read a named property 'toString' from 'Location': Blocked a frame with origin "(.*?)" from accessing a cross-origin frame./;
      expect(content).to.match(expectedContent);
    it('emits a browser-window-created event', async () => {
        src: `file://${fixtures}/pages/window-open.html`
      await once(app, 'browser-window-created');
    it('emits a web-contents-created event', async () => {
      const webContentsCreated = emittedUntil(app, 'web-contents-created',
        (event: Electron.Event, contents: Electron.WebContents) => contents.getType() === 'window');
      await webContentsCreated;
    it('does not crash when creating window with noopener', async () => {
        src: `file://${path.join(fixtures, 'api', 'native-window-open-noopener.html')}`
  describe('webpreferences attribute', () => {
      w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true, webviewTag: true } });
    it('can enable context isolation', async () => {
        allowpopups: 'yes',
        preload: `file://${fixtures}/api/isolated-preload.js`,
        src: `file://${fixtures}/api/isolated.html`,
        webpreferences: 'contextIsolation=yes'
      const [, data] = await once(ipcMain, 'isolated-world');
      expect(data).to.deep.equal({
      await w.loadURL(`file://${fixtures}/pages/flex-webview.html`);
      await w.webContents.executeJavaScript(`{
        for (const el of document.querySelectorAll('webview')) el.remove();
    ifit(hasCapturableScreen())('is transparent by default', async () => {
      await loadWebView(w.webContents, {
        src: 'data:text/html,foo'
      const screenCapture = new ScreenCapture();
    ifit(hasCapturableScreen())('remains transparent when set', async () => {
        src: 'data:text/html,foo',
        webpreferences: 'transparent=yes'
    ifit(hasCapturableScreen())('can disable transparency', async () => {
        webpreferences: 'transparent=no'
      await screenCapture.expectColorAtCenterMatches(HexColors.WHITE);
  describe('permission request handlers', () => {
    const partition = 'permissionTest';
    function setUpRequestHandler (webContentsId: number, requestedPermission: string) {
        session.fromPartition(partition).setPermissionRequestHandler(function (webContents, permission, allow) {
          if (webContents.id === webContentsId) {
            // All midi permission requests are blocked or allowed as midiSysex permissions
            // since https://chromium-review.googlesource.com/c/chromium/src/+/5154368
            if (permission === 'midiSysex') {
              const allowed = requestedPermission === 'midi' || requestedPermission === 'midiSysex';
              return allow(!allowed);
              expect(permission).to.equal(requestedPermission);
              return reject(e);
            allow(false);
      session.fromPartition(partition).setPermissionRequestHandler(null);
    // This is disabled because CI machines don't have cameras or microphones,
    // so Chrome responds with "NotFoundError" instead of
    // "PermissionDeniedError". It should be re-enabled if we find a way to mock
    // the presence of a microphone & camera.
    xit('emits when using navigator.getUserMedia api', async () => {
      const errorFromRenderer = once(ipcMain, 'message');
        src: `file://${fixtures}/pages/permissions/media.html`,
        partition,
        nodeintegration: 'on'
      const [, webViewContents] = await once(app, 'web-contents-created') as [any, WebContents];
      setUpRequestHandler(webViewContents.id, 'media');
      const [, errorName] = await errorFromRenderer;
      expect(errorName).to.equal('PermissionDeniedError');
    it('emits when using navigator.geolocation api', async () => {
        src: `file://${fixtures}/pages/permissions/geolocation.html`,
        webpreferences: 'contextIsolation=no'
      setUpRequestHandler(webViewContents.id, 'geolocation');
      const [, error] = await errorFromRenderer;
      expect(error).to.equal('User denied Geolocation');
    it('emits when using navigator.requestMIDIAccess without sysex api', async () => {
        src: `file://${fixtures}/pages/permissions/midi.html`,
      setUpRequestHandler(webViewContents.id, 'midi');
      expect(error).to.equal('NotAllowedError');
    it('emits when using navigator.requestMIDIAccess with sysex api', async () => {
        src: `file://${fixtures}/pages/permissions/midi-sysex.html`,
      setUpRequestHandler(webViewContents.id, 'midiSysex');
    it('emits when accessing external protocol', async () => {
        src: 'magnet:test',
        partition
      await setUpRequestHandler(webViewContents.id, 'openExternal');
    it('emits when using Notification.requestPermission', async () => {
        src: `file://${fixtures}/pages/permissions/notification.html`,
      await setUpRequestHandler(webViewContents.id, 'notifications');
      expect(error).to.equal('denied');
  describe('DOM events', () => {
    it('receives extra properties on DOM events when contextIsolation is enabled', async () => {
        webview.setAttribute('src', 'data:text/html,<script>console.log("hi")</script>')
        webview.addEventListener('console-message', (e) => {
          resolve(e.message)
      expect(message).to.equal('hi');
    it('emits focus event when contextIsolation is enabled', async () => {
      await w.webContents.executeJavaScript(`new Promise((resolve, reject) => {
          webview.focus()
        webview.addEventListener('focus', () => {
  describe('attributes', () => {
      await window.loadURL(`file://${fixtures}/pages/blank.html`);
      w = window.webContents;
      await w.executeJavaScript(`{
    describe('src attribute', () => {
      it('specifies the page to load', async () => {
        const message = await loadWebViewAndWaitForMessage(w, {
          src: `file://${fixtures}/pages/a.html`
        expect(message).to.equal('a');
      it('navigates to new page when changed', async () => {
        await loadWebView(w, {
        const { message } = await w.executeJavaScript(`new Promise(resolve => {
          webview.addEventListener('console-message', e => resolve({message: e.message}))
          webview.src = ${JSON.stringify(`file://${fixtures}/pages/b.html`)}
        expect(message).to.equal('b');
      it('resolves relative URLs', async () => {
          src: './e.html'
        expect(message).to.equal('Window script is loaded before preload script');
      it('ignores empty values', async () => {
        loadWebView(w, {});
        for (const emptyValue of ['""', 'null', 'undefined']) {
          const src = await w.executeJavaScript(`webview.src = ${emptyValue}, webview.src`);
          expect(src).to.equal('');
      it('does not wait until loadURL is resolved', async () => {
        await loadWebView(w, { src: 'about:blank' });
        const delay = await w.executeJavaScript(`new Promise(resolve => {
          webview.src = 'file://${fixtures}/pages/blank.html';
          resolve(now - before);
        // Setting src is essentially sending a sync IPC message, which should
        // not exceed more than a few ms.
        // This is for testing #18638.
        expect(delay).to.be.below(100);
    describe('nodeintegration attribute', () => {
      it('inserts no node symbols when not set', async () => {
          src: `file://${fixtures}/pages/c.html`
        const types = JSON.parse(message);
        expect(types).to.include({
          require: 'undefined',
          module: 'undefined',
          process: 'undefined',
          global: 'undefined'
      it('inserts node symbols when set', async () => {
          src: `file://${fixtures}/pages/d.html`
          require: 'function',
          module: 'object',
          process: 'object'
      it('loads node symbols after POST navigation when set', async function () {
          src: `file://${fixtures}/pages/post.html`
      it('disables node integration on child windows when it is disabled on the webview', async () => {
          pathname: `${fixtures}/pages/webview-opener-no-node-integration.html`,
            p: `${fixtures}/pages/window-opener-node.html`
          src
        expect(JSON.parse(message).isProcessGlobalUndefined).to.be.true();
      ifit(!process.env.ELECTRON_SKIP_NATIVE_MODULE_TESTS)('loads native modules when navigation happens', async function () {
          src: `file://${fixtures}/pages/native-module.html`
        const message = await w.executeJavaScript(`new Promise(resolve => {
          webview.addEventListener('console-message', e => resolve(e.message))
          webview.reload();
        expect(message).to.equal('function');
    describe('preload attribute', () => {
      useRemoteContext({ webPreferences: { webviewTag: true } });
          preload: `${fixtures}/module/preload.js`,
          src: `file://${fixtures}/pages/e.html`
        expect(message).to.be.a('string');
        expect(message).to.be.not.equal('Window script is loaded before preload script');
      it('preload script can still use "process" and "Buffer" when nodeintegration is off', async () => {
          preload: `${fixtures}/module/preload-node-off.js`,
          src: `file://${fixtures}/api/blank.html`
          process: 'object',
          Buffer: 'function'
      it('runs in the correct scope when sandboxed', async () => {
          preload: `${fixtures}/module/preload-context.js`,
          src: `file://${fixtures}/api/blank.html`,
          webpreferences: 'sandbox=yes'
          require: 'function', // arguments passed to it should be available
          electron: 'undefined', // objects from the scope it is called from should not be available
          window: 'object', // the window object should be available
          localVar: 'undefined' // but local variables should not be exposed to the window
      it('preload script can require modules that still use "process" and "Buffer" when nodeintegration is off', async () => {
          preload: `${fixtures}/module/preload-node-off-wrapper.js`,
          webpreferences: 'sandbox=no',
      it('receives ipc message in preload script', async () => {
          preload: `${fixtures}/module/preload-ipc.js`,
        const message = 'boom!';
        const { channel, args } = await w.executeJavaScript(`new Promise(resolve => {
          webview.send('ping', ${JSON.stringify(message)})
          webview.addEventListener('ipc-message', ({channel, args}) => resolve({channel, args}))
        expect(channel).to.equal('pong');
        expect(args).to.deep.equal([message]);
      itremote('<webview>.sendToFrame()', async (fixtures: string) => {
        const w = new WebView();
        w.setAttribute('nodeintegration', 'on');
        w.setAttribute('webpreferences', 'contextIsolation=no');
        w.setAttribute('preload', `file://${fixtures}/module/preload-ipc.js`);
        w.setAttribute('src', `file://${fixtures}/pages/ipc-message.html`);
        document.body.appendChild(w);
        const { frameId } = await new Promise<any>(resolve => w.addEventListener('ipc-message', resolve, { once: true }));
        w.sendToFrame(frameId, 'ping', message);
        const { channel, args } = await new Promise<any>(resolve => w.addEventListener('ipc-message', resolve, { once: true }));
          src: `file://${fixtures}/pages/base-page.html`
          preload: '../module/preload.js',
      itremote('ignores empty values', async () => {
        for (const emptyValue of ['', null, undefined]) {
          webview.preload = emptyValue;
          expect(webview.preload).to.equal('');
    describe('httpreferrer attribute', () => {
      it('sets the referrer url', async () => {
        const referrer = 'http://github.com/';
        const received = await new Promise<string | undefined>((resolve, reject) => {
              resolve(req.headers.referer);
            loadWebView(w, {
              httpreferrer: referrer,
              src: url
        expect(received).to.equal(referrer);
    describe('useragent attribute', () => {
      it('sets the user agent', async () => {
        const referrer = 'Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko';
          src: `file://${fixtures}/pages/useragent.html`,
          useragent: referrer
        expect(message).to.equal(referrer);
    describe('disablewebsecurity attribute', () => {
      it('does not disable web security when not set', async () => {
        const result = await w.executeJavaScript(`webview.executeJavaScript(\`fetch(${JSON.stringify(blankPageUrl)}).then(() => 'ok', () => 'failed')\`)`);
      it('disables web security when set', async () => {
        await loadWebView(w, { src: 'about:blank', disablewebsecurity: '' });
        expect(result).to.equal('ok');
      it('does not break node integration', async () => {
          disablewebsecurity: '',
      it('does not break preload script', async () => {
    describe('partition attribute', () => {
          partition: 'test1',
          partition: 'test2',
      it('isolates storage for different id', async () => {
        await w.executeJavaScript('localStorage.setItem(\'test\', \'one\')');
          partition: 'test3',
          src: `file://${fixtures}/pages/partition/one.html`
        const parsedMessage = JSON.parse(message);
        expect(parsedMessage).to.include({
          numberOfEntries: 0,
          testValue: null
      it('uses current session storage when no id is provided', async () => {
        await w.executeJavaScript('localStorage.setItem(\'test\', \'two\')');
        const testValue = 'two';
          testValue
    describe('allowpopups attribute', () => {
      const generateSpecs = (description: string, webpreferences = '') => {
          it('can not open new window when not set', async () => {
              webpreferences,
              src: `file://${fixtures}/pages/window-open-hide.html`
            expect(message).to.equal('null');
          it('can open new window when set', async () => {
            expect(message).to.equal('window');
      generateSpecs('without sandbox');
      generateSpecs('with sandbox', 'sandbox=yes');
      it('can enable nodeintegration', async () => {
          src: `file://${fixtures}/pages/d.html`,
          webpreferences: 'nodeIntegration,contextIsolation=no'
      it('can disable web security and enable nodeintegration', async () => {
        await loadWebView(w, { src: 'about:blank', webpreferences: 'webSecurity=no, nodeIntegration=yes, contextIsolation=no' });
        const type = await w.executeJavaScript('webview.executeJavaScript("typeof require")');
        expect(type).to.equal('function');
  describe('events', () => {
      it('emits when guest sends an ipc message to browser', async () => {
        const { frameId, channel, args } = await loadWebViewAndWaitForEvent(w, {
          src: `file://${fixtures}/pages/ipc-message.html`,
        }, 'ipc-message');
        expect(frameId).to.be.an('array').that.has.lengthOf(2);
        expect(channel).to.equal('channel');
        expect(args).to.deep.equal(['arg1', 'arg2']);
      it('emits when title is set', async () => {
        const { title, explicitSet } = await loadWebViewAndWaitForEvent(w, {
        }, 'page-title-updated');
        expect(title).to.equal('test');
        expect(explicitSet).to.be.true();
    describe('page-favicon-updated event', () => {
      it('emits when favicon urls are received', async () => {
        const { favicons } = await loadWebViewAndWaitForEvent(w, {
        }, 'page-favicon-updated');
        expect(favicons).to.be.an('array').of.length(2);
          expect(favicons[0]).to.match(/^file:\/\/\/[A-Z]:\/favicon.png$/i);
          expect(favicons[0]).to.equal('file:///favicon.png');
    describe('did-redirect-navigation event', () => {
        const event = await loadWebViewAndWaitForEvent(w, {
          src: `${url}/302`
        }, 'did-redirect-navigation');
        expect(event.url).to.equal(`${url}/200`);
        expect(event.isInPlace).to.be.false();
        expect(event.isMainFrame).to.be.true();
        expect(event.frameProcessId).to.be.a('number');
        expect(event.frameRoutingId).to.be.a('number');
      it('emits when a url that leads to outside of the page is loaded', async () => {
        const { url } = await loadWebViewAndWaitForEvent(w, {
          src: `file://${fixtures}/pages/webview-will-navigate.html`
        }, 'will-navigate');
      it('emits when a link that leads to outside of the page is loaded', async () => {
        const { url, isMainFrame } = await loadWebViewAndWaitForEvent(w, {
        }, 'will-frame-navigate');
        expect(isMainFrame).to.be.true();
      it('emits when a link within an iframe, which leads to outside of the page, is loaded', async () => {
          src: `file://${fixtures}/pages/webview-will-navigate-in-frame.html`,
          nodeIntegration: ''
        const { url, frameProcessId, frameRoutingId } = await w.executeJavaScript(`
            let hasFrameNavigatedOnce = false;
            const webview = document.getElementById('webview');
            webview.addEventListener('will-frame-navigate', ({url, isMainFrame, frameProcessId, frameRoutingId}) => {
              if (hasFrameNavigatedOnce) resolve({
                isMainFrame,
                frameProcessId,
                frameRoutingId,
              // First navigation is the initial iframe load within the <webview>
              hasFrameNavigatedOnce = true;
            webview.executeJavaScript('loadSubframe()');
        expect(frameProcessId).to.be.a('number');
        expect(frameRoutingId).to.be.a('number');
    describe('did-navigate event', () => {
      it('emits when a url that leads to outside of the page is clicked', async () => {
        const pageUrl = url.pathToFileURL(path.join(fixtures, 'pages', 'webview-will-navigate.html')).toString();
        const event = await loadWebViewAndWaitForEvent(w, { src: pageUrl }, 'did-navigate');
        expect(event.url).to.equal(pageUrl);
    describe('did-navigate-in-page event', () => {
      it('emits when an anchor link is clicked', async () => {
        const pageUrl = url.pathToFileURL(path.join(fixtures, 'pages', 'webview-did-navigate-in-page.html')).toString();
        const event = await loadWebViewAndWaitForEvent(w, { src: pageUrl }, 'did-navigate-in-page');
        expect(event.url).to.equal(`${pageUrl}#test_content`);
      it('emits when window.history.replaceState is called', async () => {
          src: `file://${fixtures}/pages/webview-did-navigate-in-page-with-history.html`
        }, 'did-navigate-in-page');
      it('emits when window.location.hash is changed', async () => {
        const pageUrl = url.pathToFileURL(path.join(fixtures, 'pages', 'webview-did-navigate-in-page-with-hash.html')).toString();
        expect(event.url).to.equal(`${pageUrl}#test`);
      it('should fire when interior page calls window.close', async () => {
        await loadWebViewAndWaitForEvent(w, { src: `file://${fixtures}/pages/close.html` }, 'close');
    describe('devtools-opened event', () => {
      it('should fire when webview.openDevTools() is called', async () => {
        await loadWebViewAndWaitForEvent(w, {
        }, 'dom-ready');
        await w.executeJavaScript(`new Promise((resolve) => {
          webview.addEventListener('devtools-opened', () => resolve(), {once: true})
        await w.executeJavaScript('webview.closeDevTools()');
    describe('devtools-closed event', () => {
      itremote('should fire when webview.closeDevTools() is called', async (fixtures: string) => {
        webview.src = `file://${fixtures}/pages/base-page.html`;
        await new Promise(resolve => webview.addEventListener('dom-ready', resolve, { once: true }));
        webview.openDevTools();
        await new Promise(resolve => webview.addEventListener('devtools-opened', resolve, { once: true }));
        webview.closeDevTools();
        await new Promise(resolve => webview.addEventListener('devtools-closed', resolve, { once: true }));
    describe('devtools-focused event', () => {
      itremote('should fire when webview.openDevTools() is called', async (fixtures: string) => {
        const waitForDevToolsFocused = new Promise(resolve => webview.addEventListener('devtools-focused', resolve, { once: true }));
        await waitForDevToolsFocused;
    describe('dom-ready event', () => {
      it('emits when document is loaded', async () => {
        const server = http.createServer(() => {});
          src: `file://${fixtures}/pages/dom-ready.html?port=${port}`
      itremote('throws a custom error when an API method is called before the event is emitted', () => {
        const expectedErrorMessage =
            'The WebView must be attached to the DOM ' +
            'and the dom-ready event emitted before this method can be called.';
        expect(() => { webview.stop(); }).to.throw(expectedErrorMessage);
        const { params, url } = await w.executeJavaScript(`new Promise(resolve => {
          webview.addEventListener('context-menu', (e) => resolve({...e, url: webview.getURL() }), {once: true})
          const opts = { x: 0, y: 0, button: 'right' };
          webview.sendInputEvent({ ...opts, type: 'mouseDown' });
          webview.sendInputEvent({ ...opts, type: 'mouseUp' });
        expect(params.pageURL).to.equal(url);
        expect(params.frame).to.be.undefined();
    describe('found-in-page event', () => {
      itremote('emits when a request is made', async (fixtures: string) => {
        const didFinishLoad = new Promise(resolve => webview.addEventListener('did-finish-load', resolve, { once: true }));
        webview.src = `file://${fixtures}/pages/content.html`;
        // TODO(deepak1556): With https://codereview.chromium.org/2836973002
        // focus of the webContents is required when triggering the api.
        // Remove this workaround after determining the cause for
        // incorrect focus.
        webview.focus();
        await didFinishLoad;
        const activeMatchOrdinal = [];
          const foundInPage = new Promise<any>(resolve => webview.addEventListener('found-in-page', resolve, { once: true }));
          const requestId = webview.findInPage('virtual');
          const event = await foundInPage;
          expect(event.result.requestId).to.equal(requestId);
          expect(event.result.matches).to.equal(3);
          activeMatchOrdinal.push(event.result.activeMatchOrdinal);
          if (event.result.activeMatchOrdinal === event.result.matches) {
        expect(activeMatchOrdinal).to.deep.equal([1, 2, 3]);
        webview.stopFindInPage('clearSelection');
    describe('will-attach-webview event', () => {
      itremote('does not emit when src is not changed', async () => {
        const expectedErrorMessage = 'The WebView must be attached to the DOM and the dom-ready event emitted before this method can be called.';
      it('supports changing the web preferences', async () => {
        w.once('will-attach-webview', (event, webPreferences, params) => {
          params.src = `file://${path.join(fixtures, 'pages', 'c.html')}`;
          webPreferences.nodeIntegration = false;
          nodeintegration: 'yes',
      it('handler modifying params.instanceId does not break <webview>', async () => {
          params.instanceId = null as any;
        await loadWebViewAndWaitForMessage(w, {
      it('supports preventing a webview from being created', async () => {
        w.once('will-attach-webview', event => event.preventDefault());
        }, 'destroyed');
      it('supports removing the preload script', async () => {
          params.src = url.pathToFileURL(path.join(fixtures, 'pages', 'webview-stripped-preload.html')).toString();
          delete webPreferences.preload;
          preload: path.join(fixtures, 'module', 'preload-set-global.js'),
        expect(message).to.equal('undefined');
    describe('media-started-playing and media-paused events', () => {
      it('emits when audio starts and stops playing', async function () {
        if (!await w.executeJavaScript('document.createElement(\'audio\').canPlayType(\'audio/wav\')')) {
        await loadWebView(w, { src: blankPageUrl });
        // With the new autoplay policy, audio elements must be unmuted
        // see https://goo.gl/xX8pDD.
        await w.executeJavaScript(`new Promise(resolve => {
          webview.executeJavaScript(\`
            const audio = document.createElement("audio")
            audio.src = "../assets/tone.wav"
            document.body.appendChild(audio);
            audio.play()
          \`, true)
          webview.addEventListener('media-started-playing', () => resolve(), {once: true})
            document.querySelector("audio").pause()
          webview.addEventListener('media-paused', () => resolve(), {once: true})
  describe('methods', () => {
    describe('<webview>.reload()', () => {
          src: `file://${fixtures}/pages/beforeunload-false.html`
        // Event handler has to be added before reload.
        const channel = await w.executeJavaScript(`new Promise(resolve => {
          webview.addEventListener('ipc-message', e => resolve(e.channel))
        expect(channel).to.equal('onbeforeunload');
      it('does not crash when renderer process crashes', async function () {
        // It takes more time to wait for the rendering process to crash
          src: blankPageUrl
        // Create a crash in the rendering process of a webview
        await w.executeJavaScript(`new Promise((resolve, reject) => {
          webview.addEventListener('render-process-gone', (e) => resolve({...e}), {once: true})
          webview.executeJavaScript('process.crash()', true)
        // Reload the webview and the main process will not crash.
          webview.reload()
    describe('<webview>.goForward()', () => {
      itremote('should work after a replaced history entry', async (fixtures: string) => {
        function waitForEvent (target: EventTarget, event: string) {
          return new Promise<any>(resolve => target.addEventListener(event, resolve, { once: true }));
        function waitForEvents (target: EventTarget, ...events: string[]) {
          return Promise.all(events.map(event => waitForEvent(webview, event)));
        webview.setAttribute('nodeintegration', 'on');
        webview.src = `file://${fixtures}/pages/history-replace.html`;
          const [e] = await waitForEvents(webview, 'ipc-message', 'did-stop-loading');
          expect(e.channel).to.equal('history');
          expect(e.args[0]).to.equal(1);
          expect(webview.canGoBack()).to.be.false();
          expect(webview.canGoForward()).to.be.false();
        await new Promise<void>(resolve => webview.addEventListener('did-stop-loading', resolve, { once: true }));
        expect(webview.canGoBack()).to.be.true();
        webview.goBack();
          expect(e.args[0]).to.equal(2);
          expect(webview.canGoForward()).to.be.true();
        webview.goForward();
    describe('<webview>.clearHistory()', () => {
      it('should clear the navigation history', async () => {
        // Navigation must be triggered by a user gesture to make canGoBack() return true
        await w.executeJavaScript('webview.executeJavaScript(`history.pushState(null, "", "foo.html")`, true)');
        expect(await w.executeJavaScript('webview.canGoBack()')).to.be.true();
        await w.executeJavaScript('webview.clearHistory()');
        expect(await w.executeJavaScript('webview.canGoBack()')).to.be.false();
      it('can return the result of the executed script', async () => {
          src: 'about:blank'
        const jsScript = "'4'+2";
        const expectedResult = '42';
        const result = await w.executeJavaScript(`webview.executeJavaScript(${JSON.stringify(jsScript)})`);
        expect(result).to.equal(expectedResult);
      await loadWebView(w, { src: `file://${fixtures}/pages/base-page.html` });
      await w.executeJavaScript('webview.insertCSS(\'body { background-repeat: round; }\')');
      const result = await w.executeJavaScript('webview.executeJavaScript(\'window.getComputedStyle(document.body).getPropertyValue("background-repeat")\')');
      const key = await w.executeJavaScript('webview.insertCSS(\'body { background-repeat: round; }\')');
      await w.executeJavaScript(`webview.removeInsertedCSS(${JSON.stringify(key)})`);
    describe('sendInputEvent', () => {
      it('can send keyboard event', async () => {
          src: `file://${fixtures}/pages/onkeyup.html`
        const waitForIpcMessage = w.executeJavaScript('new Promise(resolve => webview.addEventListener("ipc-message", e => resolve({...e})), {once: true})');
        w.executeJavaScript(`webview.sendInputEvent({
          type: 'keyup',
          keyCode: 'c',
          modifiers: ['shift']
        const { channel, args } = await waitForIpcMessage;
        expect(channel).to.equal('keyup');
        expect(args).to.deep.equal(['C', 'KeyC', 67, true, false]);
      it('can send mouse event', async () => {
          src: `file://${fixtures}/pages/onmouseup.html`
          type: 'mouseup',
          modifiers: ['ctrl'],
          y: 20
        expect(channel).to.equal('mouseup');
        expect(args).to.deep.equal([10, 20, false, true]);
    describe('<webview>.getWebContentsId', () => {
      it('can return the WebContents ID', async () => {
        expect(await w.executeJavaScript('webview.getWebContentsId()')).to.be.a('number');
    ifdescribe(features.isPrintingEnabled())('<webview>.printToPDF()', () => {
          preferCSSPageSize: 'no'
          const src = 'data:text/html,%3Ch1%3EHello%2C%20World!%3C%2Fh1%3E';
          await loadWebView(w, { src });
          await expect(w.executeJavaScript(`webview.printToPDF(${JSON.stringify(param)})`)).to.eventually.be.rejected();
      it('can print to PDF', async () => {
        const data = await w.executeJavaScript('webview.printToPDF({})');
        expect(data).to.be.an.instanceof(Uint8Array).that.is.not.empty();
      for (const [description, sandbox] of [
        ['without sandbox', false] as const,
        ['with sandbox', true] as const
          it('emits focus event', async () => {
              src: `file://${fixtures}/pages/a.html`,
              webpreferences: `sandbox=${sandbox ? 'yes' : 'no'}`
            // If this test fails, check if webview.focus() still works.
              webview.addEventListener('focus', () => resolve(), {once: true});
    // FIXME: This test is flaking constantly on Linux and macOS.
    xdescribe('<webview>.capturePage()', () => {
      it('returns a Promise with a NativeImage', async function () {
        this.retries(5);
        await loadWebViewAndWaitForEvent(w, { src }, 'did-stop-loading');
        const image = await w.executeJavaScript('webview.capturePage()');
      it('returns a Promise with a NativeImage in the renderer', async function () {
        const byte = await w.executeJavaScript(`new Promise(resolve => {
          webview.capturePage().then(image => {
            resolve(image.toPNG()[25])
        expect(byte).to.equal(6);
    // FIXME(zcbenz): Disabled because of moving to OOPIF webview.
    xdescribe('setDevToolsWebContents() API', () => {
      it('sets webContents of webview as devtools', async () => {
        const webview2 = new WebView();
        loadWebView(webview2);
        // Setup an event handler for further usage.
        const waitForDomReady = waitForEvent(webview2, 'dom-ready');
        loadWebView(webview, { src: 'about:blank' });
        await waitForEvent(webview, 'dom-ready');
        webview.getWebContents().setDevToolsWebContents(webview2.getWebContents());
        webview.getWebContents().openDevTools();
        await waitForDomReady;
        // Its WebContents should be a DevTools.
        const devtools = webview2.getWebContents();
        expect(devtools.getURL().startsWith('devtools://devtools')).to.be.true();
        const name = await devtools.executeJavaScript('InspectorFrontendHost.constructor.name');
        document.body.removeChild(webview2);
        expect(name).to.be.equal('InspectorFrontendHostImpl');
  describe('basic auth', () => {
    it('should authenticate with correct credentials', async () => {
      const message = 'Authenticated';
        const credentials = auth(req)!;
        if (credentials.name === 'test' && credentials.pass === 'test') {
          res.end(message);
          res.end('failed');
      const e = await loadWebViewAndWaitForEvent(w, {
        src: `file://${fixtures}/pages/basic-auth.html?port=${port}`
      expect(e.channel).to.equal(message);
