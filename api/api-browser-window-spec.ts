import { app, BrowserWindow, BrowserView, dialog, ipcMain, OnBeforeSendHeadersListenerDetails, net, protocol, screen, webContents, webFrameMain, session, systemPreferences, WebContents, WebFrameMain } from 'electron/main';
import * as qs from 'node:querystring';
import { setTimeout as syncSetTimeout } from 'node:timers';
import * as nodeUrl from 'node:url';
import { emittedUntil, emittedNTimes } from './lib/events-helpers';
import { randomString } from './lib/net-helpers';
import { HexColors, hasCapturableScreen, ScreenCapture } from './lib/screen-helpers';
import { ifit, ifdescribe, defer, listen, waitUntil } from './lib/spec-helpers';
const mainFixtures = path.resolve(__dirname, 'fixtures');
// Is the display's scale factor possibly causing rounding of pixel coordinate
// values?
const isScaleFactorRounding = () => {
  const { scaleFactor } = screen.getPrimaryDisplay();
  // Return true if scale factor is non-integer value
  if (Math.round(scaleFactor) !== scaleFactor) return true;
  // Return true if scale factor is odd number above 2
  return scaleFactor > 2 && scaleFactor % 2 === 1;
const expectBoundsEqual = (actual: any, expected: any) => {
  if (!isScaleFactorRounding()) {
    expect(actual).to.deep.equal(expected);
    expect(actual[0]).to.be.closeTo(expected[0], 1);
    expect(actual[1]).to.be.closeTo(expected[1], 1);
    expect(actual.x).to.be.closeTo(expected.x, 1);
    expect(actual.y).to.be.closeTo(expected.y, 1);
    expect(actual.width).to.be.closeTo(expected.width, 1);
    expect(actual.height).to.be.closeTo(expected.height, 1);
const isBeforeUnload = (event: Event, level: number, message: string) => {
  return (message === 'beforeunload');
describe('BrowserWindow module', () => {
    expect(BrowserWindow.prototype.constructor.name).to.equal('BrowserWindow');
  describe('BrowserWindow constructor', () => {
    it('allows passing void 0 as the webContents', async () => {
          // apparently void 0 had different behaviour from undefined in the
          // issue that this test is supposed to catch.
          webContents: void 0 // eslint-disable-line no-void
        w.destroy();
    ifit(process.platform === 'linux')('does not crash when setting large window icons', async () => {
      const appPath = path.join(fixtures, 'apps', 'xwindow-icon');
      const appProcess = childProcess.spawn(process.execPath, [appPath]);
    it('does not crash or throw when passed an invalid icon', async () => {
          icon: undefined
  describe('garbage collection', () => {
    it('window does not get garbage collected when opened', async () => {
      // Keep a weak reference to the window.
      const wr = new WeakRef(w);
      await setTimeout();
      // Do garbage collection, since |w| is not referenced in this closure
      // it would be gone after next call if there is no other reference.
      expect(wr.deref()).to.not.be.undefined();
  describe('BrowserWindow.close()', () => {
      w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true, contextIsolation: false } });
      w = null as unknown as BrowserWindow;
    it('should work if called when a messageBox is showing', async () => {
      dialog.showMessageBox(w, { message: 'Hello Error' });
      await closed;
    it('should work if called when multiple messageBoxes are showing', async () => {
    it('closes window without rounded corners', async () => {
      w = new BrowserWindow({ show: false, frame: false, roundedCorners: false });
    it('should not crash if called after webContents is destroyed', () => {
      w.webContents.destroy();
      w.webContents.on('destroyed', () => w.close());
    it('should allow access to id after destruction', async () => {
      expect(w.id).to.be.a('number');
    it('should emit unload handler', async () => {
      await w.loadFile(path.join(fixtures, 'api', 'unload.html'));
      const test = path.join(fixtures, 'api', 'unload');
      const content = fs.readFileSync(test);
      fs.unlinkSync(test);
      expect(String(content)).to.equal('unload');
    it('should emit beforeunload handler', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-false.html'));
      await once(w.webContents, '-before-unload-fired');
    it('should not crash when keyboard event is sent before closing', async () => {
      await w.loadURL('data:text/html,pls no crash');
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Escape' });
    describe('when invoked synchronously inside navigation observer', () => {
      let url: string;
          switch (request.url) {
            case '/net-error':
              response.destroy();
            case '/301':
              response.statusCode = 301;
              response.setHeader('Location', '/200');
              response.end();
            case '/200':
              response.statusCode = 200;
              response.end('hello');
            case '/title':
              response.end('<title>Hello</title>');
              throw new Error(`unsupported endpoint: ${request.url}`);
        url = (await listen(server)).url;
      const events = [
        { name: 'did-start-loading', path: '/200' },
        { name: 'dom-ready', path: '/200' },
        { name: 'page-title-updated', path: '/title' },
        { name: 'did-stop-loading', path: '/200' },
        { name: 'did-finish-load', path: '/200' },
        { name: 'did-frame-finish-load', path: '/200' },
        { name: 'did-fail-load', path: '/net-error' }
      for (const { name, path } of events) {
        it(`should not crash when closed during ${name}`, async () => {
          w.webContents.once((name as any), () => {
          const destroyed = once(w.webContents, 'destroyed');
          w.webContents.loadURL(url + path);
  describe('window.accessibleTitle', () => {
    const title = 'Window Title';
      w = new BrowserWindow({ show: false, title, webPreferences: { nodeIntegration: true, contextIsolation: false } });
    it('should default to the window title', async () => {
      expect(w.accessibleTitle).to.equal(title);
    it('should be mutable', async () => {
      const accessibleTitle = randomString(20);
      w.accessibleTitle = accessibleTitle;
      expect(w.accessibleTitle).to.equal(accessibleTitle);
    it('should be clearable', async () => {
      w.accessibleTitle = '';
  describe('window.close()', () => {
    it('should emit unload event', async () => {
      w.loadFile(path.join(fixtures, 'api', 'close.html'));
      await once(w, 'closed');
      const test = path.join(fixtures, 'api', 'close');
      const content = fs.readFileSync(test).toString();
      expect(content).to.equal('close');
    it('should emit beforeunload event', async function () {
      w.webContents.executeJavaScript('window.close()', true);
  describe('BrowserWindow.destroy()', () => {
      w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true } });
    it('prevents users to access methods of webContents', async () => {
      const contents = w.webContents;
      await new Promise(setImmediate);
        contents.getProcessId();
      }).to.throw('Object has been destroyed');
    it('should not crash when destroying windows with pending events', () => {
      const focusListener = () => { };
      app.on('browser-window-focus', focusListener);
      const windowCount = 3;
      const windows = Array.from(Array(windowCount)).map(() => new BrowserWindow(windowOptions));
      for (const win of windows) win.show();
      for (const win of windows) win.focus();
      for (const win of windows) win.destroy();
      app.removeListener('browser-window-focus', focusListener);
  ifdescribe(process.platform !== 'linux')('BrowserWindow.getContentProtection', () => {
    it('can set content protection', async () => {
      expect(w.isContentProtected()).to.equal(false);
      const shown = once(w, 'show');
      await shown;
      w.setContentProtection(true);
      expect(w.isContentProtected()).to.equal(true);
    it('does not remove content protection after the window is hidden and shown', async () => {
      const hidden = once(w, 'hide');
      w.hide();
      await hidden;
  describe('BrowserWindow.loadURL(url)', () => {
    const scheme = 'other';
    const srcPath = path.join(fixtures, 'api');
      protocol.handle(scheme, (req) => {
        const reqURL = new URL(req.url);
        return net.fetch(nodeUrl.pathToFileURL(path.join(srcPath, reqURL.pathname)).toString());
      protocol.unhandle(scheme);
    let postData = null as any;
      const filePath = path.join(fixtures, 'pages', 'a.html');
      const fileStats = fs.statSync(filePath);
      postData = [
          type: 'rawData',
          bytes: Buffer.from('username=test&file=')
          length: fileStats.size,
          modificationTime: fileStats.mtime.getTime() / 1000
        function respond () {
          if (req.method === 'POST') {
            req.on('data', (data) => {
              if (data) body += data;
            req.on('end', () => {
              const parsedData = qs.parse(body);
              fs.readFile(filePath, (err, data) => {
                if (err) return;
                if (parsedData.username === 'test' &&
                  parsedData.file === data.toString()) {
          } else if (req.url === '/302') {
            res.setHeader('Location', '/200');
            res.statusCode = 302;
        setTimeout(req.url && req.url.includes('slow') ? 200 : 0).then(respond);
    it('should emit did-start-loading event', async () => {
      const didStartLoading = once(w.webContents, 'did-start-loading');
      w.loadURL('about:blank');
      await didStartLoading;
    it('should emit ready-to-show event', async () => {
      const readyToShow = once(w, 'ready-to-show');
      await readyToShow;
    // DISABLED-FIXME(deepak1556): The error code now seems to be `ERR_FAILED`, verify what
    // changed and adjust the test.
    it('should emit did-fail-load event for files that do not exist', async () => {
      const didFailLoad = once(w.webContents, 'did-fail-load');
      w.loadURL('file://a.txt');
      const [, code, desc, , isMainFrame] = await didFailLoad;
      expect(code).to.equal(-6);
      expect(desc).to.equal('ERR_FILE_NOT_FOUND');
      expect(isMainFrame).to.equal(true);
    it('should emit did-fail-load event for invalid URL', async () => {
      w.loadURL('http://example:port');
      expect(desc).to.equal('ERR_INVALID_URL');
      expect(code).to.equal(-300);
    it('should not emit did-fail-load for a successfully loaded media file', async () => {
      w.webContents.on('did-fail-load', () => {
        expect.fail('did-fail-load should not emit on media file loads');
      const mediaStarted = once(w.webContents, 'media-started-playing');
      w.loadFile(path.join(fixtures, 'cat-spin.mp4'));
      await mediaStarted;
    it('should set `mainFrame = false` on did-fail-load events in iframes', async () => {
      w.loadFile(path.join(fixtures, 'api', 'did-fail-load-iframe.html'));
      const [, , , , isMainFrame] = await didFailLoad;
      expect(isMainFrame).to.equal(false);
    it('does not crash in did-fail-provisional-load handler', (done) => {
      w.webContents.once('did-fail-provisional-load', () => {
        w.loadURL('http://127.0.0.1:11111');
    it('should emit did-fail-load event for URL exceeding character limit', async () => {
      const data = Buffer.alloc(2 * 1024 * 1024).toString('base64');
      w.loadURL(`data:image/png;base64,${data}`);
    it('should return a promise', () => {
      const p = w.loadURL('about:blank');
      expect(p).to.have.property('then');
    it('should return a promise that resolves', async () => {
      await expect(w.loadURL('about:blank')).to.eventually.be.fulfilled();
    it('should return a promise that rejects on a load failure', async () => {
      const p = w.loadURL(`data:image/png;base64,${data}`);
      await expect(p).to.eventually.be.rejected;
    it('should return a promise that resolves even if pushState occurs during navigation', async () => {
      const p = w.loadURL('data:text/html,<script>window.history.pushState({}, "/foo")</script>');
      await expect(p).to.eventually.be.fulfilled;
    describe('POST navigations', () => {
      afterEach(() => { w.webContents.session.webRequest.onBeforeSendHeaders(null); });
      it('supports specifying POST data', async () => {
        await w.loadURL(url, { postData });
      it('sets the content type header on URL encoded forms', async () => {
        await w.loadURL(url);
        const requestDetails: Promise<OnBeforeSendHeadersListenerDetails> = new Promise(resolve => {
          w.webContents.session.webRequest.onBeforeSendHeaders((details) => {
            resolve(details);
        w.webContents.executeJavaScript(`
          form = document.createElement('form')
          document.body.appendChild(form)
          form.method = 'POST'
          form.submit()
        const details = await requestDetails;
        expect(details.requestHeaders['Content-Type']).to.equal('application/x-www-form-urlencoded');
      it('sets the content type header on multi part forms', async () => {
          form.enctype = 'multipart/form-data'
          file = document.createElement('input')
          file.type = 'file'
          file.name = 'file'
          form.appendChild(file)
        expect(details.requestHeaders['Content-Type'].startsWith('multipart/form-data; boundary=----WebKitFormBoundary')).to.equal(true);
    // FIXME(#43730): fix underlying bug and re-enable asap
    it.skip('should support base url for data urls', async () => {
      await w
        .loadURL('data:text/html,<script src="loaded-from-dataurl.js"></script>', { baseURLForDataURL: 'other://' })
        .catch((e) => console.log(e));
      expect(await w.webContents.executeJavaScript('window.ping')).to.equal('pong');
    describe('webRequest', () => {
        session.defaultSession.webRequest.onBeforeRequest(null);
      it('triggers webRequest handlers for https', async () => {
        session.defaultSession.webRequest.onBeforeRequest((_, cb) => {
          cb({ cancel: true });
        await expect(w.loadURL('https://foo')).to.eventually.be.rejectedWith(/^ERR_BLOCKED_BY_CLIENT/);
      it('triggers webRequest handlers for intercepted https', async () => {
        session.defaultSession.protocol.handle('https', () => new Response());
        defer(() => {
          session.defaultSession.protocol.unhandle('https');
      it('triggers webRequest handlers for file urls', async () => {
        await expect(w.loadURL('file://foo')).to.eventually.be.rejectedWith(/^ERR_BLOCKED_BY_CLIENT/);
      it('triggers webRequest handlers for intercepted file urls', async () => {
        session.defaultSession.protocol.handle('file', () => new Response());
          session.defaultSession.protocol.unhandle('file');
      it('triggers webRequest handlers for registered protocols', async () => {
        session.defaultSession.protocol.handle('custom-protocol', () => new Response());
          session.defaultSession.protocol.unhandle('custom-protocol');
        await expect(w.loadURL('custom-protocol://foo')).to.eventually.be.rejectedWith(/^ERR_BLOCKED_BY_CLIENT/);
  for (const sandbox of [false, true]) {
    describe(`navigation events${sandbox ? ' with sandbox' : ''}`, () => {
        w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: false, sandbox } });
      describe('will-navigate event', () => {
            if (req.url === '/navigate-top') {
              res.end('<a target=_top href="/">navigate _top</a>');
              res.end('');
        it('allows the window to be closed from the event listener', async () => {
          const event = once(w.webContents, 'will-navigate');
          w.loadFile(path.join(fixtures, 'pages', 'will-navigate.html'));
          await event;
        it('can be prevented', (done) => {
          let willNavigate = false;
          w.webContents.once('will-navigate', (e) => {
            willNavigate = true;
          w.webContents.on('did-stop-loading', () => {
            if (willNavigate) {
              // i.e. it shouldn't have had '?navigated' appended to it.
                expect(w.webContents.getURL().endsWith('will-navigate.html')).to.be.true();
                done(e);
        it('is triggered when navigating from file: to http:', async () => {
          await w.loadFile(path.join(fixtures, 'api', 'blank.html'));
          w.webContents.executeJavaScript(`location.href = ${JSON.stringify(url)}`);
          const navigatedTo = await new Promise(resolve => {
            w.webContents.once('will-navigate', (e, url) => {
              resolve(url);
          expect(navigatedTo).to.equal(url + '/');
          expect(w.webContents.getURL()).to.match(/^file:/);
        it('is triggered when navigating from about:blank to http:', async () => {
          expect(w.webContents.getURL()).to.equal('about:blank');
        it('is triggered when a cross-origin iframe navigates _top', async () => {
          w.loadURL(`data:text/html,<iframe src="http://127.0.0.1:${(server.address() as AddressInfo).port}/navigate-top"></iframe>`);
          await emittedUntil(w.webContents, 'did-frame-finish-load', (e: any, isMainFrame: boolean) => !isMainFrame);
          let initiator: WebFrameMain | null | undefined;
          w.webContents.on('will-navigate', (e) => {
            initiator = e.initiator;
          const subframe = w.webContents.mainFrame.frames[0];
          subframe.executeJavaScript('document.getElementsByTagName("a")[0].click()', true);
          await once(w.webContents, 'did-navigate');
          expect(initiator).not.to.be.undefined();
          expect(initiator).to.equal(subframe);
        it('is triggered when navigating from chrome: to http:', async () => {
          let hasEmittedWillNavigate = false;
          const willNavigatePromise = new Promise((resolve) => {
            w.webContents.once('will-navigate', e => {
              hasEmittedWillNavigate = true;
              resolve(e.url);
          await w.loadURL('chrome://gpu');
          // shouldn't emit for browser-initiated request via loadURL
          expect(hasEmittedWillNavigate).to.equal(false);
          const navigatedTo = await willNavigatePromise;
          expect(w.webContents.getURL()).to.equal('chrome://gpu/');
      describe('will-frame-navigate event', () => {
        let server = null as unknown as http.Server;
        let url = null as unknown as string;
            } else if (req.url === '/navigate-iframe') {
              res.end('<a href="/test">navigate iframe</a>');
            } else if (req.url === '/navigate-iframe?navigated') {
              res.end('Successfully navigated');
            } else if (req.url === '/navigate-iframe-immediately') {
              res.end(`
                <script type="text/javascript" charset="utf-8">
                  location.href += '?navigated'
            } else if (req.url === '/navigate-iframe-immediately?navigated') {
        it('allows the window to be closed from the event listener', (done) => {
          w.webContents.once('will-frame-navigate', () => {
          w.webContents.once('will-frame-navigate', (e) => {
        it('can be prevented when navigating subframe', (done) => {
          w.webContents.on('did-frame-navigate', (_event, _url, _httpResponseCode, _httpStatusText, isMainFrame, frameProcessId, frameRoutingId) => {
            if (isMainFrame) return;
              const frame = webFrameMain.fromId(frameProcessId, frameRoutingId);
              expect(frame).to.not.be.undefined();
                  expect(frame!.url.endsWith('/navigate-iframe-immediately')).to.be.true();
          w.loadURL(`data:text/html,<iframe src="http://127.0.0.1:${(server.address() as AddressInfo).port}/navigate-iframe-immediately"></iframe>`);
          await w.loadURL(`data:text/html,<iframe src="http://127.0.0.1:${(server.address() as AddressInfo).port}/navigate-top"></iframe>`);
          await setTimeout(1000);
          let willFrameNavigateEmitted = false;
          let isMainFrameValue;
          w.webContents.on('will-frame-navigate', (event) => {
            willFrameNavigateEmitted = true;
            isMainFrameValue = event.isMainFrame;
          const didNavigatePromise = once(w.webContents, 'did-navigate');
          w.webContents.debugger.attach('1.1');
          const targets = await w.webContents.debugger.sendCommand('Target.getTargets');
          const iframeTarget = targets.targetInfos.find((t: any) => t.type === 'iframe');
          const { sessionId } = await w.webContents.debugger.sendCommand('Target.attachToTarget', {
            targetId: iframeTarget.targetId,
            flatten: true
          await w.webContents.debugger.sendCommand('Input.dispatchMouseEvent', {
            type: 'mousePressed',
            x: 10,
            y: 10,
            clickCount: 1,
            button: 'left'
          }, sessionId);
            type: 'mouseReleased',
          await didNavigatePromise;
          expect(willFrameNavigateEmitted).to.be.true();
          expect(isMainFrameValue).to.be.true();
        it('is triggered when a cross-origin iframe navigates itself', async () => {
          await w.loadURL(`data:text/html,<iframe src="http://127.0.0.1:${(server.address() as AddressInfo).port}/navigate-iframe"></iframe>`);
          let willNavigateEmitted = false;
            willNavigateEmitted = true;
          const didNavigatePromise = once(w.webContents, 'did-frame-navigate');
          expect(willNavigateEmitted).to.be.true();
          expect(isMainFrameValue).to.be.false();
        it('can cancel when a cross-origin iframe navigates itself', async () => {
      describe('will-redirect event', () => {
            if (req.url === '/302') {
            } else if (req.url === '/navigate-302') {
              res.end(`<html><body><script>window.location='${url}/302'</script></body></html>`);
        it('is emitted on redirects', async () => {
          const willRedirect = once(w.webContents, 'will-redirect');
          w.loadURL(`${url}/302`);
          await willRedirect;
        it('is emitted after will-navigate on redirects', async () => {
          let navigateCalled = false;
          w.webContents.on('will-navigate', () => {
            navigateCalled = true;
          w.loadURL(`${url}/navigate-302`);
          expect(navigateCalled).to.equal(true, 'should have called will-navigate first');
        it('is emitted before did-stop-loading on redirects', async () => {
          let stopCalled = false;
          expect(stopCalled).to.equal(false, 'should not have called did-stop-loading first');
          const event = once(w.webContents, 'will-redirect');
          w.webContents.once('will-redirect', (event) => {
          w.webContents.on('will-navigate', (e, u) => {
            expect(u).to.equal(`${url}/302`);
              expect(w.webContents.getURL()).to.equal(
                `${url}/navigate-302`,
                'url should not have changed after navigation event'
          w.webContents.on('will-redirect', (e, u) => {
              expect(u).to.equal(`${url}/200`);
      describe('ordering', () => {
        const navigationEvents = [
          'did-start-navigation',
          'did-navigate-in-page',
          'will-frame-navigate',
          'will-navigate',
          'will-redirect',
          'did-redirect-navigation',
          'did-frame-navigate',
          'did-navigate'
            if (req.url === '/navigate') {
              res.end('<a href="/">navigate</a>');
            } else if (req.url === '/redirect') {
              res.end('<a href="/redirect2">redirect</a>');
            } else if (req.url === '/redirect2') {
              res.setHeader('location', url);
            } else if (req.url === '/in-page') {
              res.end('<a href="#in-page">redirect</a><div id="in-page"></div>');
        it('for initial navigation, event order is consistent', async () => {
          const firedEvents: string[] = [];
          const expectedEventOrder = [
          const allEvents = Promise.all(expectedEventOrder.map(event =>
            once(w.webContents, event).then(() => firedEvents.push(event))
          w.loadURL(url);
          await allEvents;
          expect(firedEvents).to.deep.equal(expectedEventOrder);
        it('for second navigation, event order is consistent', async () => {
          w.loadURL(url + '/navigate');
          Promise.all(navigationEvents.map(event =>
          const navigationFinished = once(w.webContents, 'did-navigate');
          const pageTarget = targets.targetInfos.find((t: any) => t.type === 'page');
            targetId: pageTarget.targetId,
          await navigationFinished;
        it('when navigating with redirection, event order is consistent', async () => {
          w.loadURL(url + '/redirect');
        it('when navigating in-page, event order is consistent', async () => {
            'did-navigate-in-page'
          w.loadURL(url + '/in-page');
          const navigationFinished = once(w.webContents, 'did-navigate-in-page');
  describe('focus and visibility', () => {
    describe('BrowserWindow.show()', () => {
      it('should focus on window', async () => {
        const p = once(w, 'focus');
        expect(w.isFocused()).to.equal(true);
      it('should make the window visible', async () => {
        expect(w.isVisible()).to.equal(true);
      it('emits when window is shown', async () => {
        const show = once(w, 'show');
        await show;
    describe('BrowserWindow.hide()', () => {
      it('should defocus on window', () => {
        expect(w.isFocused()).to.equal(false);
      it('should make the window not visible', () => {
        expect(w.isVisible()).to.equal(false);
      it('emits when window is hidden', async () => {
    describe('BrowserWindow.minimize()', () => {
      // TODO(codebytere): Enable for Linux once maximize/minimize events work in CI.
      ifit(process.platform !== 'linux')('should not be visible when the window is minimized', async () => {
        const minimize = once(w, 'minimize');
        w.minimize();
        await minimize;
        expect(w.isMinimized()).to.equal(true);
    describe('BrowserWindow.showInactive()', () => {
      it('should not focus on window', () => {
        w.showInactive();
      // TODO(dsanders11): Enable for Linux once CI plays nice with these kinds of tests
      ifit(process.platform !== 'linux')('should not restore maximized windows', async () => {
        const maximize = once(w, 'maximize');
        w.maximize();
        // TODO(dsanders11): The maximize event isn't firing on macOS for a window initially hidden
          await maximize;
        expect(w.isMaximized()).to.equal(true);
      ifit(process.platform === 'darwin')('should attach child window to parent', async () => {
        const wShow = once(w, 'show');
        await wShow;
        const c = new BrowserWindow({ show: false, parent: w });
        const cShow = once(c, 'show');
        c.showInactive();
        await cShow;
        // verifying by checking that the child tracks the parent's visibility
        const minimized = once(w, 'minimize');
        await minimized;
        expect(w.isVisible()).to.be.false('parent is visible');
        expect(c.isVisible()).to.be.false('child is visible');
        const restored = once(w, 'restore');
        w.restore();
        await restored;
        expect(w.isVisible()).to.be.true('parent is visible');
        expect(c.isVisible()).to.be.true('child is visible');
        closeWindow(c);
    describe('BrowserWindow.focus()', () => {
      it('does not make the window become visible', () => {
        w.focus();
      ifit(process.platform !== 'win32')('focuses a blurred window', async () => {
          const isBlurred = once(w, 'blur');
          const isShown = once(w, 'show');
          w.blur();
          await isShown;
          await isBlurred;
      ifit(process.platform !== 'linux')('acquires focus status from the other windows', async () => {
        const w1 = new BrowserWindow({ show: false });
        const w3 = new BrowserWindow({ show: false });
          const isFocused3 = once(w3, 'focus');
          const isShown1 = once(w1, 'show');
          const isShown2 = once(w2, 'show');
          const isShown3 = once(w3, 'show');
          w1.show();
          w2.show();
          w3.show();
          await isShown1;
          await isShown2;
          await isShown3;
          await isFocused3;
        // TODO(RaisinTen): Investigate why this assertion fails only on Linux.
        expect(w1.isFocused()).to.equal(false);
        expect(w2.isFocused()).to.equal(false);
        expect(w3.isFocused()).to.equal(true);
        w1.focus();
        expect(w1.isFocused()).to.equal(true);
        expect(w3.isFocused()).to.equal(false);
        w2.focus();
        expect(w2.isFocused()).to.equal(true);
        w3.focus();
          const isClosed1 = once(w1, 'closed');
          const isClosed2 = once(w2, 'closed');
          const isClosed3 = once(w3, 'closed');
          w1.destroy();
          w3.destroy();
          await isClosed1;
          await isClosed2;
          await isClosed3;
      ifit(process.platform === 'darwin')('it does not activate the app if focusing an inactive panel', async () => {
        // Show to focus app, then remove existing window
        // We first need to resign app focus for this test to work
        const isInactive = once(app, 'did-resign-active');
        childProcess.execSync('osascript -e \'tell application "Finder" to activate\'');
        defer(() => childProcess.execSync('osascript -e \'tell application "Finder" to quit\''));
        await isInactive;
        // Create new window
          type: 'panel',
          center: true,
        const isShow = once(w, 'show');
        const isFocus = once(w, 'focus');
        await isShow;
        await isFocus;
        const getActiveAppOsa = 'tell application "System Events" to get the name of the first process whose frontmost is true';
        const activeApp = childProcess.execSync(`osascript -e '${getActiveAppOsa}'`).toString().trim();
        expect(activeApp).to.equal('Finder');
    // TODO(RaisinTen): Make this work on Windows too.
    // Refs: https://github.com/electron/electron/issues/20464.
    ifdescribe(process.platform !== 'win32')('BrowserWindow.blur()', () => {
      it('removes focus from window', async () => {
          const isFocused = once(w, 'focus');
          await isFocused;
      ifit(process.platform !== 'linux')('transfers focus status to the next window', async () => {
        w3.blur();
        w2.blur();
        w1.blur();
    describe('BrowserWindow.getFocusedWindow()', () => {
      it('returns the opener window when dev tools window is focused', async () => {
        w.webContents.openDevTools({ mode: 'undocked' });
        await once(w.webContents, 'devtools-focused');
        expect(BrowserWindow.getFocusedWindow()).to.equal(w);
    describe('BrowserWindow.moveTop()', () => {
      it('should not steal focus', async () => {
        const posDelta = 50;
        const wShownInactive = once(w, 'show');
        await wShownInactive;
        const otherWindow = new BrowserWindow({ show: false, title: 'otherWindow' });
        const otherWindowShown = once(otherWindow, 'show');
        const otherWindowFocused = once(otherWindow, 'focus');
        otherWindow.show();
        await otherWindowShown;
        await otherWindowFocused;
        expect(otherWindow.isFocused()).to.equal(true);
        w.moveTop();
        const wPos = w.getPosition();
        const wMoving = once(w, 'move');
        w.setPosition(wPos[0] + posDelta, wPos[1] + posDelta);
        await wMoving;
        const wFocused = once(w, 'focus');
        const otherWindowBlurred = once(otherWindow, 'blur');
        await wFocused;
        await otherWindowBlurred;
        otherWindow.moveTop();
        const otherWindowPos = otherWindow.getPosition();
        const otherWindowMoving = once(otherWindow, 'move');
        otherWindow.setPosition(otherWindowPos[0] + posDelta, otherWindowPos[1] + posDelta);
        await otherWindowMoving;
        expect(otherWindow.isFocused()).to.equal(false);
        await closeWindow(otherWindow, { assertNotWindows: false });
        expect(BrowserWindow.getAllWindows()).to.have.lengthOf(1);
      it('should not crash when called on a modal child window', async () => {
        const child = new BrowserWindow({ modal: true, parent: w });
        expect(() => { child.moveTop(); }).to.not.throw();
    describe('BrowserWindow.moveAbove(mediaSourceId)', () => {
      it('should throw an exception if wrong formatting', async () => {
        const fakeSourceIds = [
          'none', 'screen:0', 'window:fake', 'window:1234', 'foobar:1:2'
        for (const sourceId of fakeSourceIds) {
            w.moveAbove(sourceId);
          }).to.throw(/Invalid media source id/);
      it('should throw an exception if wrong type', async () => {
        const fakeSourceIds = [null as any, 123 as any];
          }).to.throw(/Error processing argument at index 0 */);
      it('should throw an exception if invalid window', async () => {
        // It is very unlikely that these window id exist.
        const fakeSourceIds = ['window:99999999:0', 'window:123456:1',
          'window:123456:9'];
      it('should not throw an exception', async () => {
        const w2 = new BrowserWindow({ show: false, title: 'window2' });
        const w2Shown = once(w2, 'show');
        await w2Shown;
          w.moveAbove(w2.getMediaSourceId());
        await closeWindow(w2, { assertNotWindows: false });
    describe('BrowserWindow.setFocusable()', () => {
      it('can set unfocusable window to focusable', async () => {
        const w2 = new BrowserWindow({ focusable: false });
        const w2Focused = once(w2, 'focus');
        w2.setFocusable(true);
        await w2Focused;
    describe('BrowserWindow.isFocusable()', () => {
      it('correctly returns whether a window is focusable', async () => {
        expect(w2.isFocusable()).to.be.false();
        expect(w2.isFocusable()).to.be.true();
  describe('sizing', () => {
      w = new BrowserWindow({ show: false, width: 400, height: 400 });
    describe('BrowserWindow.setBounds(bounds[, animate])', () => {
      it('sets the window bounds with full bounds', () => {
        const fullBounds = { x: 440, y: 225, width: 500, height: 400 };
        w.setBounds(fullBounds);
        expectBoundsEqual(w.getBounds(), fullBounds);
      it('rounds non-integer bounds', () => {
        w.setBounds({ x: 440.5, y: 225.1, width: 500.4, height: 400.9 });
        const bounds = w.getBounds();
        expect(bounds).to.deep.equal({ x: 441, y: 225, width: 500, height: 401 });
      it('sets the window bounds with partial bounds', () => {
        const boundsUpdate = { width: 200 };
        w.setBounds(boundsUpdate as any);
        const expectedBounds = { ...fullBounds, ...boundsUpdate };
        expectBoundsEqual(w.getBounds(), expectedBounds);
      ifit(process.platform === 'darwin')('on macOS', () => {
        it('emits \'resized\' event after animating', async () => {
          w.setBounds(fullBounds, true);
          await expect(once(w, 'resized')).to.eventually.be.fulfilled();
      it('does not emits the resize event for move-only changes', async () => {
        const [x, y] = w.getPosition();
        w.once('resize', () => {
          expect.fail('resize event should not be emitted');
        w.setBounds({ x: x + 10, y: y + 10 });
    describe('BrowserWindow.setSize(width, height)', () => {
      it('sets the window size', async () => {
        const size = [300, 400];
        const resized = once(w, 'resize');
        w.setSize(size[0], size[1]);
        await resized;
        expectBoundsEqual(w.getSize(), size);
      it('emits the resize event for single-pixel size changes', async () => {
        const size = [width + 1, height - 1];
          w.setSize(size[0], size[1], true);
    describe('BrowserWindow.setMinimum/MaximumSize(width, height)', () => {
      it('sets the maximum and minimum size of the window', () => {
        expect(w.getMinimumSize()).to.deep.equal([0, 0]);
        expect(w.getMaximumSize()).to.deep.equal([0, 0]);
        w.setMinimumSize(100, 100);
        expectBoundsEqual(w.getMinimumSize(), [100, 100]);
        expectBoundsEqual(w.getMaximumSize(), [0, 0]);
        w.setMaximumSize(900, 600);
        expectBoundsEqual(w.getMaximumSize(), [900, 600]);
      it('enforces minimum size', async () => {
        w.setMinimumSize(300, 300);
        const resize = once(w, 'resize');
        w.setSize(100, 100);
        await resize;
        const size = w.getSize();
        expect(size[0]).to.be.at.least(300);
        expect(size[1]).to.be.at.least(300);
      it('enforces maximum size', async () => {
        w.setMaximumSize(200, 200);
        w.setSize(500, 500);
        expect(size[0]).to.be.at.most(200);
        expect(size[1]).to.be.at.most(200);
    describe('BrowserWindow.setAspectRatio(ratio)', () => {
      it('resets the behaviour when passing in 0', async () => {
        w.setAspectRatio(1 / 2);
        w.setAspectRatio(0);
      it('doesn\'t change bounds when maximum size is set', () => {
        w.setMenu(null);
        w.setMaximumSize(400, 400);
        // Without https://github.com/electron/electron/pull/29101
        // following call would shrink the window to 384x361.
        // There would be also DCHECK in resize_utils.cc on
        // debug build.
        w.setAspectRatio(1.0);
        expectBoundsEqual(w.getSize(), [400, 400]);
    describe('BrowserWindow.setPosition(x, y)', () => {
      it('sets the window position', async () => {
        const pos = [10, 10];
        const move = once(w, 'move');
        w.setPosition(pos[0], pos[1]);
        await move;
        expect(w.getPosition()).to.deep.equal(pos);
    describe('BrowserWindow.setContentSize(width, height)', () => {
      it('sets the content size', async () => {
        // NB. The CI server has a very small screen. Attempting to size the window
        // larger than the screen will limit the window's size to the screen and
        // cause the test to fail.
        const size = [456, 567];
        w.setContentSize(size[0], size[1]);
        const after = w.getContentSize();
        expect(after).to.deep.equal(size);
      it('works for a frameless window', async () => {
    describe('BrowserWindow.setContentBounds(bounds)', () => {
      it('sets the content size and position', async () => {
        const bounds = { x: 10, y: 10, width: 250, height: 250 };
        w.setContentBounds(bounds);
        expectBoundsEqual(w.getContentBounds(), bounds);
    describe('BrowserWindow.getBackgroundColor()', () => {
      it('returns default value if no backgroundColor is set', () => {
        w = new BrowserWindow({});
        expect(w.getBackgroundColor()).to.equal('#FFFFFF');
      it('returns correct value if backgroundColor is set', () => {
        const backgroundColor = '#BBAAFF';
          backgroundColor
        expect(w.getBackgroundColor()).to.equal(backgroundColor);
      it('returns correct value from setBackgroundColor()', () => {
        const backgroundColor = '#AABBFF';
        w.setBackgroundColor(backgroundColor);
      it('returns correct color with multiple passed formats', async () => {
        const colors = new Map([
          ['blueviolet', '#8A2BE2'],
          ['rgb(255, 0, 185)', '#FF00B9'],
          ['hsl(155, 100%, 50%)', '#00FF95'],
          ['#355E3B', '#355E3B']
        for (const [color, hex] of colors) {
          w.setBackgroundColor(color);
          expect(w.getBackgroundColor()).to.equal(hex);
      it('can set the background color with transparency', async () => {
          ['rgba(245, 40, 145, 0.8)', '#F52891'],
          ['#1D1F21d9', '#1F21D9']
    describe('BrowserWindow.getNormalBounds()', () => {
      describe('Normal state', () => {
        it('checks normal bounds after resize', async () => {
          expectBoundsEqual(w.getNormalBounds(), w.getBounds());
        it('checks normal bounds after move', async () => {
      ifdescribe(process.platform !== 'linux')('Maximized state', () => {
        it('checks normal bounds when maximized', async () => {
          expectBoundsEqual(w.getNormalBounds(), bounds);
        it('updates normal bounds after resize and maximize', async () => {
          const original = w.getBounds();
          const normal = w.getNormalBounds();
          expect(normal).to.deep.equal(original);
          expect(normal).to.not.deep.equal(bounds);
          const close = once(w, 'close');
          await close;
        it('updates normal bounds after move and maximize', async () => {
        it('checks normal bounds when unmaximized', async () => {
          w.once('maximize', () => {
            w.unmaximize();
          const unmaximize = once(w, 'unmaximize');
          await unmaximize;
        it('correctly reports maximized state after maximizing then minimizing', async () => {
          expect(w.isMaximized()).to.equal(false);
        it('correctly reports maximized state after maximizing then fullscreening', async () => {
          const enterFS = once(w, 'enter-full-screen');
          w.setFullScreen(true);
          await enterFS;
          expect(w.isFullScreen()).to.equal(true);
        it('does not crash if maximized, minimized, then restored to maximized state', (done) => {
          w.on('maximize', () => {
            if (count === 0) syncSetTimeout(() => { w.minimize(); });
          w.on('minimize', () => {
            if (count === 1) syncSetTimeout(() => { w.restore(); });
          w.on('restore', () => {
              throw new Error('hey!');
              expect(e.message).to.equal('hey!');
        it('checks normal bounds for maximized transparent window', async () => {
          const bounds = w.getNormalBounds();
        it('does not change size for a frameless window with min size', async () => {
            minWidth: 300,
        it('correctly checks transparent window maximization state', async () => {
        it('returns the correct value for windows with an aspect ratio', async () => {
            fullscreenable: false
          w.setAspectRatio(16 / 11);
          w.resizable = false;
      ifdescribe(process.platform !== 'linux')('Minimized state', () => {
        it('checks normal bounds when minimized', async () => {
        it('updates normal bounds after move and minimize', async () => {
          expect(original).to.deep.equal(normal);
          expectBoundsEqual(normal, w.getBounds());
        it('updates normal bounds after resize and minimize', async () => {
        it('checks normal bounds when restored', async () => {
          w.once('minimize', () => {
          const restore = once(w, 'restore');
          await restore;
      ifdescribe(process.platform === 'win32')('Fullscreen state', () => {
          it('can be set with the fullscreen constructor option', () => {
            w = new BrowserWindow({ fullscreen: true });
            expect(w.fullScreen).to.be.true();
            w.fullScreen = false;
            expect(w.fullScreen).to.be.false();
            w.fullScreen = true;
          it('checks normal bounds when fullscreen\'ed', async () => {
            const enterFullScreen = once(w, 'enter-full-screen');
            await enterFullScreen;
          it('updates normal bounds after resize and fullscreen', async () => {
            const fsc = once(w, 'enter-full-screen');
            await fsc;
          it('updates normal bounds after move and fullscreen', async () => {
          it('checks normal bounds when unfullscreen\'ed', async () => {
            w.once('enter-full-screen', () => {
            const leaveFullScreen = once(w, 'leave-full-screen');
            await leaveFullScreen;
            expect(w.isFullScreen()).to.be.true();
            w.setFullScreen(false);
            expect(w.isFullScreen()).to.be.false();
  ifdescribe(process.platform === 'darwin')('tabbed windows', () => {
    describe('BrowserWindow.selectPreviousTab()', () => {
      it('does not throw', () => {
          w.selectPreviousTab();
    describe('BrowserWindow.selectNextTab()', () => {
          w.selectNextTab();
    describe('BrowserWindow.showAllTabs()', () => {
          w.showAllTabs();
    describe('BrowserWindow.mergeAllWindows()', () => {
          w.mergeAllWindows();
    describe('BrowserWindow.moveTabToNewWindow()', () => {
          w.moveTabToNewWindow();
    describe('BrowserWindow.toggleTabBar()', () => {
          w.toggleTabBar();
    describe('BrowserWindow.addTabbedWindow()', () => {
      it('does not throw', async () => {
        const tabbedWindow = new BrowserWindow({});
          w.addTabbedWindow(tabbedWindow);
        expect(BrowserWindow.getAllWindows()).to.have.lengthOf(2); // w + tabbedWindow
        await closeWindow(tabbedWindow, { assertNotWindows: false });
        expect(BrowserWindow.getAllWindows()).to.have.lengthOf(1); // w
      it('throws when called on itself', () => {
          w.addTabbedWindow(w);
        }).to.throw('AddTabbedWindow cannot be called by a window on itself.');
    describe('BrowserWindow.tabbingIdentifier', () => {
      it('is undefined if no tabbingIdentifier was set', () => {
        expect(w.tabbingIdentifier).to.be.undefined('tabbingIdentifier');
      it('returns the window tabbingIdentifier', () => {
        const w = new BrowserWindow({ show: false, tabbingIdentifier: 'group1' });
        expect(w.tabbingIdentifier).to.equal('group1');
  ifdescribe(process.platform !== 'darwin')('autoHideMenuBar state', () => {
    describe('for properties', () => {
      it('can be set with autoHideMenuBar constructor option', () => {
        const w = new BrowserWindow({ show: false, autoHideMenuBar: true });
        expect(w.autoHideMenuBar).to.be.true('autoHideMenuBar');
        expect(w.autoHideMenuBar).to.be.false('autoHideMenuBar');
        w.autoHideMenuBar = true;
        w.autoHideMenuBar = false;
    describe('for functions', () => {
        expect(w.isMenuBarAutoHide()).to.be.true('autoHideMenuBar');
        expect(w.isMenuBarAutoHide()).to.be.false('autoHideMenuBar');
        w.setAutoHideMenuBar(true);
        w.setAutoHideMenuBar(false);
  describe('BrowserWindow.capturePage(rect)', () => {
      const image = await w.capturePage({
    ifit(process.platform === 'darwin')('honors the stayHidden argument', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'visibilitychange.html'));
        const [, visibilityState, hidden] = await once(ipcMain, 'pong');
        expect(visibilityState).to.equal('visible');
        expect(hidden).to.be.false('hidden');
        expect(visibilityState).to.equal('hidden');
        expect(hidden).to.be.true('hidden');
      await w.capturePage({ x: 0, y: 0, width: 0, height: 0 }, { stayHidden: true });
      const visible = await w.webContents.executeJavaScript('document.visibilityState');
      expect(visible).to.equal('hidden');
    it('resolves when the window is occluded', async () => {
      w1.loadFile(path.join(fixtures, 'pages', 'a.html'));
      await once(w1, 'ready-to-show');
      w2.loadFile(path.join(fixtures, 'pages', 'a.html'));
      await once(w2, 'ready-to-show');
      const visibleImage = await w1.capturePage();
      expect(visibleImage.isEmpty()).to.equal(false);
    it('resolves when the window is not visible', async () => {
      w.loadFile(path.join(fixtures, 'pages', 'a.html'));
      await once(w, 'ready-to-show');
      const visibleImage = await w.capturePage();
      const hiddenImage = await w.capturePage();
      expect(hiddenImage.isEmpty()).to.equal(false);
    it('preserves transparency', async () => {
      const w = new BrowserWindow({ show: false, transparent: true });
      w.loadFile(path.join(fixtures, 'pages', 'theme-color.html'));
      const image = await w.capturePage();
      const imgBuffer = image.toPNG();
      // Check the 25th byte in the PNG.
      // Values can be 0,2,3,4, or 6. We want 6, which is RGB + Alpha
      expect(imgBuffer[25]).to.equal(6);
  describe('BrowserWindow.setProgressBar(progress)', () => {
    it('sets the progress', () => {
          app.dock?.setIcon(path.join(fixtures, 'assets', 'logo.png'));
        w.setProgressBar(0.5);
          app.dock?.setIcon(null as any);
        w.setProgressBar(-1);
    it('sets the progress using "paused" mode', () => {
        w.setProgressBar(0.5, { mode: 'paused' });
    it('sets the progress using "error" mode', () => {
        w.setProgressBar(0.5, { mode: 'error' });
    it('sets the progress using "normal" mode', () => {
        w.setProgressBar(0.5, { mode: 'normal' });
  ifdescribe(process.platform === 'win32')('BrowserWindow.{get|set}AccentColor', () => {
    it('throws if called with an invalid parameter', () => {
        // @ts-ignore this is wrong on purpose.
        w.setAccentColor([1, 2, 3]);
      }).to.throw('Invalid accent color value - must be null, hex string, or boolean');
        w.setAccentColor(new Date());
    it('can be reset with null', () => {
      w.setAccentColor('#FF0000');
      expect(w.getAccentColor()).to.equal('#FF0000');
      w.setAccentColor(null);
      expect(w.getAccentColor()).to.not.equal('#FF0000');
    it('returns the accent color after setting it to a string', () => {
      const testColor = '#FF0000';
      w.setAccentColor(testColor);
      const accentColor = w.getAccentColor();
      expect(accentColor).to.be.a('string');
      expect(accentColor).to.equal(testColor);
    it('returns the accent color after setting it to false', () => {
      w.setAccentColor(false);
      expect(accentColor).to.be.a('boolean');
      expect(accentColor).to.equal(false);
    it('returns a system color when set to true', () => {
      w.setAccentColor(true);
      expect(accentColor).to.match(/^#[0-9A-F]{6}$/i);
    it('matches the systemPreferences system color when true', () => {
      const accentColor = w.getAccentColor() as string;
      const systemColor = systemPreferences.getAccentColor().slice(0, 6);
      expect(accentColor).to.equal(`#${systemColor}`);
    it('returns the correct accent color after multiple changes', () => {
      const testColor1 = '#00FF00';
      w.setAccentColor(testColor1);
      expect(w.getAccentColor()).to.equal(testColor1);
      expect(w.getAccentColor()).to.equal(false);
      const testColor2 = '#0000FF';
      w.setAccentColor(testColor2);
      expect(w.getAccentColor()).to.equal(testColor2);
      const systemColor = w.getAccentColor();
      expect(systemColor).to.be.a('string');
      expect(systemColor).to.match(/^#[0-9A-F]{6}$/i);
    it('handles CSS color names correctly', () => {
      const testColor = 'red';
      expect(accentColor).to.equal('#FF0000');
    it('handles RGB color values correctly', () => {
      const testColor = 'rgb(255, 128, 0)';
      expect(accentColor).to.equal('#FF8000');
    it('persists accent color across window operations', () => {
      const testColor = '#ABCDEF';
      expect(w.getAccentColor()).to.equal(testColor);
  describe('BrowserWindow.setAlwaysOnTop(flag, level)', () => {
      w = new BrowserWindow({ show: true });
    it('sets the window as always on top', () => {
      expect(w.isAlwaysOnTop()).to.be.false('is alwaysOnTop');
      w.setAlwaysOnTop(true, 'screen-saver');
      expect(w.isAlwaysOnTop()).to.be.true('is not alwaysOnTop');
      w.setAlwaysOnTop(false);
      w.setAlwaysOnTop(true);
    ifit(process.platform === 'darwin')('resets the windows level on minimize', async () => {
    it('causes the right value to be emitted on `always-on-top-changed`', async () => {
      const alwaysOnTopChanged = once(w, 'always-on-top-changed') as Promise<[any, boolean]>;
      const [, alwaysOnTop] = await alwaysOnTopChanged;
      expect(alwaysOnTop).to.be.true('is not alwaysOnTop');
    ifit(process.platform === 'darwin')('honors the alwaysOnTop level of a child window', () => {
      const c = new BrowserWindow({ parent: w });
      c.setAlwaysOnTop(true, 'screen-saver');
      expect(w.isAlwaysOnTop()).to.be.false();
      expect(c.isAlwaysOnTop()).to.be.true('child is not always on top');
      expect(c._getAlwaysOnTopLevel()).to.equal('screen-saver');
    it('works when called prior to show', async () => {
    it('works when called prior to showInactive', async () => {
  describe('preconnect feature', () => {
    let connections = 0;
      connections = 0;
        if (req.url === '/link') {
          res.setHeader('Content-type', 'text/html');
          res.end('<head><link rel="preconnect" href="//example.com" /></head><body>foo</body>');
      server.on('connection', () => { connections++; });
      server = null as unknown as http.Server;
    it('calling preconnect() connects to the server', async () => {
      w.webContents.on('did-start-navigation', (event, url) => {
        w.webContents.session.preconnect({ url, numSockets: 4 });
      expect(connections).to.equal(4);
    it('does not preconnect unless requested', async () => {
      expect(connections).to.equal(1);
    it('parses <link rel=preconnect>', async () => {
      const p = once(w.webContents.session, 'preconnect');
      w.loadURL(url + '/link');
      const [, preconnectUrl, allowCredentials] = await p;
      expect(preconnectUrl).to.equal('http://example.com/');
      expect(allowCredentials).to.be.true('allowCredentials');
  describe('BrowserWindow.setAutoHideCursor(autoHide)', () => {
      it('allows changing cursor auto-hiding', () => {
          w.setAutoHideCursor(false);
          w.setAutoHideCursor(true);
    ifit(process.platform !== 'darwin')('on non-macOS platforms', () => {
      it('is not available', () => {
        expect(w.setAutoHideCursor).to.be.undefined('setAutoHideCursor function');
  ifdescribe(process.platform === 'darwin')('BrowserWindow.setWindowButtonVisibility()', () => {
        w.setWindowButtonVisibility(true);
        w.setWindowButtonVisibility(false);
    it('changes window button visibility for normal window', () => {
      expect(w._getWindowButtonVisibility()).to.equal(true);
      expect(w._getWindowButtonVisibility()).to.equal(false);
    it('changes window button visibility for frameless window', () => {
      const w = new BrowserWindow({ show: false, frame: false });
    it('changes window button visibility for hiddenInset window', () => {
      const w = new BrowserWindow({ show: false, frame: false, titleBarStyle: 'hiddenInset' });
    // Buttons of customButtonsOnHover are always hidden unless hovered.
    it('does not change window button visibility for customButtonsOnHover window', () => {
      const w = new BrowserWindow({ show: false, frame: false, titleBarStyle: 'customButtonsOnHover' });
    it('correctly updates when entering/exiting fullscreen for hidden style', async () => {
      const w = new BrowserWindow({ show: false, frame: false, titleBarStyle: 'hidden' });
      const leaveFS = once(w, 'leave-full-screen');
      await leaveFS;
    it('correctly updates when entering/exiting fullscreen for hiddenInset style', async () => {
  ifdescribe(process.platform === 'darwin')('BrowserWindow.setVibrancy(type)', () => {
    it('allows setting, changing, and removing the vibrancy', () => {
        w.setVibrancy('titlebar');
        w.setVibrancy('selection');
        w.setVibrancy(null);
        w.setVibrancy('menu');
        w.setVibrancy('' as any);
    it('does not crash if vibrancy is set to an invalid value', () => {
        w.setVibrancy('i-am-not-a-valid-vibrancy-type' as any);
  ifdescribe(process.platform === 'darwin')('trafficLightPosition', () => {
    const pos = { x: 10, y: 10 };
    describe('BrowserWindow.getWindowButtonPosition(pos)', () => {
      it('returns null when there is no custom position', () => {
        expect(w.getWindowButtonPosition()).to.be.null('getWindowButtonPosition');
      it('gets position property for "hidden" titleBarStyle', () => {
        const w = new BrowserWindow({ show: false, titleBarStyle: 'hidden', trafficLightPosition: pos });
        expect(w.getWindowButtonPosition()).to.deep.equal(pos);
      it('gets position property for "customButtonsOnHover" titleBarStyle', () => {
        const w = new BrowserWindow({ show: false, titleBarStyle: 'customButtonsOnHover', trafficLightPosition: pos });
    describe('BrowserWindow.setWindowButtonPosition(pos)', () => {
      it('resets the position when null is passed', () => {
        w.setWindowButtonPosition(null);
        expect(w.getWindowButtonPosition()).to.be.null('setWindowButtonPosition');
      it('sets position property for "hidden" titleBarStyle', () => {
        const newPos = { x: 20, y: 20 };
        w.setWindowButtonPosition(newPos);
        expect(w.getWindowButtonPosition()).to.deep.equal(newPos);
      it('sets position property for "customButtonsOnHover" titleBarStyle', () => {
  ifdescribe(process.platform === 'win32')('BrowserWindow.setAppDetails(options)', () => {
    it('supports setting the app details', () => {
      const iconPath = path.join(fixtures, 'assets', 'icon.ico');
        w.setAppDetails({ appId: 'my.app.id' });
        w.setAppDetails({ appIconPath: iconPath, appIconIndex: 0 });
        w.setAppDetails({ appIconPath: iconPath });
        w.setAppDetails({ relaunchCommand: 'my-app.exe arg1 arg2', relaunchDisplayName: 'My app name' });
        w.setAppDetails({ relaunchCommand: 'my-app.exe arg1 arg2' });
        w.setAppDetails({ relaunchDisplayName: 'My app name' });
        w.setAppDetails({
          appId: 'my.app.id',
          appIconPath: iconPath,
          appIconIndex: 0,
          relaunchCommand: 'my-app.exe arg1 arg2',
          relaunchDisplayName: 'My app name'
        w.setAppDetails({});
        (w.setAppDetails as any)();
      }).to.throw('Insufficient number of arguments.');
  describe('BrowserWindow.fromId(id)', () => {
    it('returns the window with id', () => {
      expect(BrowserWindow.fromId(w.id)!.id).to.equal(w.id);
  describe('Opening a BrowserWindow from a link', () => {
    let appProcess: childProcess.ChildProcessWithoutNullStreams | undefined;
      if (appProcess && !appProcess.killed) {
        appProcess.kill();
        appProcess = undefined;
    it('can properly open and load a new window from a link', async () => {
      const appPath = path.join(__dirname, 'fixtures', 'apps', 'open-new-window-from-link');
      appProcess = childProcess.spawn(process.execPath, [appPath]);
  describe('BrowserWindow.fromWebContents(webContents)', () => {
    it('returns the window with the webContents', () => {
      const found = BrowserWindow.fromWebContents(w.webContents);
      expect(found!.id).to.equal(w.id);
    it('returns null for webContents without a BrowserWindow', () => {
      const contents = (webContents as typeof ElectronInternal.WebContents).create();
        expect(BrowserWindow.fromWebContents(contents)).to.be.null('BrowserWindow.fromWebContents(contents)');
        contents.destroy();
    it('returns the correct window for a BrowserView webcontents', async () => {
      w.setBrowserView(bv);
        w.removeBrowserView(bv);
        bv.webContents.destroy();
      await bv.webContents.loadURL('about:blank');
      expect(BrowserWindow.fromWebContents(bv.webContents)!.id).to.equal(w.id);
    it('returns the correct window for a WebView webcontents', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { webviewTag: true } });
      w.loadURL('data:text/html,<webview src="data:text/html,hi"></webview>');
      // NOTE(nornagon): Waiting for 'did-attach-webview' is a workaround for
      // https://github.com/electron/electron/issues/25413, and is not integral
      // to the test.
      const p = once(w.webContents, 'did-attach-webview');
      const [, webviewContents] = await once(app, 'web-contents-created') as [any, WebContents];
      expect(BrowserWindow.fromWebContents(webviewContents)!.id).to.equal(w.id);
    it('is usable immediately on browser-window-created', async () => {
      w.webContents.executeJavaScript('window.open(""); null');
      const [win, winFromWebContents] = await new Promise<any>((resolve) => {
        app.once('browser-window-created', (e, win) => {
          resolve([win, BrowserWindow.fromWebContents(win.webContents)]);
      expect(winFromWebContents).to.equal(win);
  describe('BrowserWindow.openDevTools()', () => {
    it('does not crash for frameless window', () => {
      w.webContents.openDevTools();
  describe('BrowserWindow.fromBrowserView(browserView)', () => {
    it('returns the window with the BrowserView', () => {
      expect(BrowserWindow.fromBrowserView(bv)!.id).to.equal(w.id);
    it('returns the window when there are multiple BrowserViews', () => {
      const bv1 = new BrowserView();
      w.addBrowserView(bv1);
      const bv2 = new BrowserView();
      w.addBrowserView(bv2);
        w.removeBrowserView(bv1);
        w.removeBrowserView(bv2);
        bv1.webContents.destroy();
        bv2.webContents.destroy();
      expect(BrowserWindow.fromBrowserView(bv1)!.id).to.equal(w.id);
      expect(BrowserWindow.fromBrowserView(bv2)!.id).to.equal(w.id);
    it('returns undefined if not attached', () => {
      expect(BrowserWindow.fromBrowserView(bv)).to.be.null('BrowserWindow associated with bv');
  describe('BrowserWindow.setOpacity(opacity)', () => {
    ifdescribe(process.platform !== 'linux')(('Windows and Mac'), () => {
      it('make window with initial opacity', () => {
        const w = new BrowserWindow({ show: false, opacity: 0.5 });
        expect(w.getOpacity()).to.equal(0.5);
      it('allows setting the opacity', () => {
          w.setOpacity(0.0);
          expect(w.getOpacity()).to.equal(0.0);
          w.setOpacity(0.5);
          w.setOpacity(1.0);
          expect(w.getOpacity()).to.equal(1.0);
      it('clamps opacity to [0.0...1.0]', () => {
        w.setOpacity(100);
        w.setOpacity(-100);
    ifdescribe(process.platform === 'linux')(('Linux'), () => {
      it('sets 1 regardless of parameter', () => {
        w.setOpacity(0);
  describe('BrowserWindow.setShape(rects)', () => {
    it('allows setting shape', () => {
        w.setShape([]);
        w.setShape([{ x: 0, y: 0, width: 100, height: 100 }]);
        w.setShape([{ x: 0, y: 0, width: 100, height: 100 }, { x: 0, y: 200, width: 1000, height: 100 }]);
  describe('"useContentSize" option', () => {
    it('make window created with content size when used', () => {
        useContentSize: true
      const contentSize = w.getContentSize();
      expect(contentSize).to.deep.equal([400, 400]);
    it('make window created with window size when not used', () => {
      expect(size).to.deep.equal([400, 400]);
    it('works for a frameless window', () => {
  describe('"titleBarStyle" option', () => {
    const testWindowsOverlay = async (style: any) => {
        titleBarStyle: style,
        titleBarOverlay: true
      const overlayHTML = path.join(__dirname, 'fixtures', 'pages', 'overlay.html');
        await w.loadFile(overlayHTML);
        const overlayReady = once(ipcMain, 'geometrychange');
        await overlayReady;
      const overlayEnabled = await w.webContents.executeJavaScript('navigator.windowControlsOverlay.visible');
      expect(overlayEnabled).to.be.true('overlayEnabled');
      const overlayRect = await w.webContents.executeJavaScript('getJSOverlayProperties()');
      expect(overlayRect.y).to.equal(0);
        expect(overlayRect.x).to.be.greaterThan(0);
        expect(overlayRect.x).to.equal(0);
      expect(overlayRect.width).to.be.greaterThan(0);
      expect(overlayRect.height).to.be.greaterThan(0);
      const cssOverlayRect = await w.webContents.executeJavaScript('getCssOverlayProperties();');
      expect(cssOverlayRect).to.deep.equal(overlayRect);
      const geometryChange = once(ipcMain, 'geometrychange');
      w.setBounds({ width: 800 });
      const [, newOverlayRect] = await geometryChange;
      expect(newOverlayRect.width).to.equal(overlayRect.width + 400);
      await closeAllWindows();
      ipcMain.removeAllListeners('geometrychange');
    it('creates browser window with hidden title bar', () => {
    ifit(process.platform === 'darwin')('creates browser window with hidden inset title bar', () => {
        titleBarStyle: 'hiddenInset'
    it('sets Window Control Overlay with hidden title bar', async () => {
      await testWindowsOverlay('hidden');
    ifit(process.platform === 'darwin')('sets Window Control Overlay with hidden inset title bar', async () => {
      await testWindowsOverlay('hiddenInset');
    ifdescribe(process.platform !== 'darwin')('when an invalid titleBarStyle is initially set', () => {
          titleBarOverlay: {
            color: '#0000f0',
            symbolColor: '#ffffff'
      it('does not crash changing minimizability ', () => {
          w.setMinimizable(false);
      it('does not crash changing maximizability', () => {
          w.setMaximizable(false);
  describe('"titleBarOverlay" option', () => {
    const testWindowsOverlayHeight = async (size: any) => {
          height: size
      const overlayRectPreMax = await w.webContents.executeJavaScript('getJSOverlayProperties()');
      expect(overlayRectPreMax.y).to.equal(0);
        expect(overlayRectPreMax.x).to.be.greaterThan(0);
        expect(overlayRectPreMax.x).to.equal(0);
      expect(overlayRectPreMax.width).to.be.greaterThan(0);
      expect(overlayRectPreMax.height).to.equal(size);
      // 'maximize' event is not emitted on Linux in CI.
      if (process.platform !== 'linux' && !w.isMaximized()) {
        expect(w.isMaximized()).to.be.true('not maximized');
        const overlayRectPostMax = await w.webContents.executeJavaScript('getJSOverlayProperties()');
        expect(overlayRectPostMax.height).to.equal(size);
    it('sets Window Control Overlay with title bar height of 40', async () => {
      await testWindowsOverlayHeight(40);
  ifdescribe(process.platform !== 'darwin')('BrowserWindow.setTitlebarOverlay', () => {
    it('throws when an invalid titleBarStyle is initially set', () => {
        win.setTitleBarOverlay({
          color: '#000000'
      }).to.throw('Titlebar overlay is not enabled');
    it('correctly updates the height of the overlay', async () => {
      const testOverlay = async (w: BrowserWindow, size: Number, firstRun: boolean) => {
        if (firstRun) {
        const { height: preMaxHeight } = await w.webContents.executeJavaScript('getJSOverlayProperties()');
        expect(preMaxHeight).to.equal(size);
          const { x, y, width, height } = await w.webContents.executeJavaScript('getJSOverlayProperties()');
          expect(x).to.equal(0);
          expect(y).to.equal(0);
          expect(width).to.be.greaterThan(0);
          expect(height).to.equal(size);
      const INITIAL_SIZE = 40;
          height: INITIAL_SIZE
      await testOverlay(w, INITIAL_SIZE, true);
      w.setTitleBarOverlay({
        height: INITIAL_SIZE + 10
      await testOverlay(w, INITIAL_SIZE + 10, false);
  ifdescribe(process.platform === 'darwin')('"enableLargerThanScreen" option', () => {
    it('can move the window out of screen', () => {
      const w = new BrowserWindow({ show: true, enableLargerThanScreen: true });
      w.setPosition(-10, 50);
      const after = w.getPosition();
      expect(after).to.deep.equal([-10, 50]);
    it('cannot move the window behind menu bar', () => {
      w.setPosition(-10, -10);
      expect(after[1]).to.be.at.least(0);
    it('can move the window behind menu bar if it has no frame', () => {
      const w = new BrowserWindow({ show: true, enableLargerThanScreen: true, frame: false });
      expect(after[0]).to.be.equal(-10);
      expect(after[1]).to.be.equal(-10);
    it('without it, cannot move the window out of screen', () => {
      const w = new BrowserWindow({ show: true, enableLargerThanScreen: false });
    it('can set the window larger than screen', () => {
      const size = screen.getPrimaryDisplay().size;
      size.width += 100;
      size.height += 100;
      w.setSize(size.width, size.height);
      expectBoundsEqual(w.getSize(), [size.width, size.height]);
    it('without it, cannot set the window larger than screen', () => {
      expect(w.getSize()[1]).to.at.most(screen.getPrimaryDisplay().size.height);
  ifdescribe(process.platform === 'darwin')('"zoomToPageWidth" option', () => {
    it('sets the window width to the page width when used', () => {
        zoomToPageWidth: true
      expect(w.getSize()[0]).to.equal(500);
  describe('"tabbingIdentifier" option', () => {
    it('can be set on a window', () => {
        /* eslint-disable-next-line no-new */
        new BrowserWindow({
          tabbingIdentifier: 'group1'
          tabbingIdentifier: 'group2',
  describe('"webPreferences" option', () => {
    afterEach(() => { ipcMain.removeAllListeners('answer'); });
    describe('"preload" option', () => {
      const doesNotLeakSpec = (name: string, webPrefs: { nodeIntegration: boolean, sandbox: boolean, contextIsolation: boolean }) => {
        it(name, async () => {
              ...webPrefs,
              preload: path.resolve(fixtures, 'module', 'empty.js')
          w.loadFile(path.join(fixtures, 'api', 'no-leak.html'));
          const [, result] = await once(ipcMain, 'leak-result');
          expect(result).to.have.property('require', 'undefined');
          expect(result).to.have.property('exports', 'undefined');
          expect(result).to.have.property('windowExports', 'undefined');
          expect(result).to.have.property('windowPreload', 'undefined');
          expect(result).to.have.property('windowRequire', 'undefined');
      doesNotLeakSpec('does not leak require', {
        sandbox: false,
      doesNotLeakSpec('does not leak require when sandbox is enabled', {
      doesNotLeakSpec('does not leak require when context isolation is enabled', {
      doesNotLeakSpec('does not leak require when context isolation and sandbox are enabled', {
      it('does not leak any node globals on the window object with nodeIntegration is disabled', async () => {
        let w = new BrowserWindow({
            contextIsolation: false,
        w.loadFile(path.join(fixtures, 'api', 'globals.html'));
        const [, notIsolated] = await once(ipcMain, 'leak-result');
        expect(notIsolated).to.have.property('globals');
        const [, isolated] = await once(ipcMain, 'leak-result');
        expect(isolated).to.have.property('globals');
        const notIsolatedGlobals = new Set(notIsolated.globals);
        for (const isolatedGlobal of isolated.globals) {
          notIsolatedGlobals.delete(isolatedGlobal);
        expect([...notIsolatedGlobals]).to.deep.equal([], 'non-isolated renderer should have no additional globals');
      it('loads the script before other scripts in window', async () => {
        const preload = path.join(fixtures, 'module', 'set-global.js');
            preload
        w.loadFile(path.join(fixtures, 'api', 'preload.html'));
        const [, test] = await once(ipcMain, 'answer');
        expect(test).to.eql('preload');
      it('has synchronous access to all eventual window APIs', async () => {
        const preload = path.join(fixtures, 'module', 'access-blink-apis.js');
        expect(test).to.be.an('object');
        expect(test.atPreload).to.be.an('array');
        expect(test.atLoad).to.be.an('array');
        expect(test.atPreload).to.deep.equal(test.atLoad, 'should have access to the same window APIs');
    describe('session preload scripts', function () {
      const preloads = [
        path.join(fixtures, 'module', 'set-global-preload-1.js'),
        path.join(fixtures, 'module', 'set-global-preload-2.js'),
        path.relative(process.cwd(), path.join(fixtures, 'module', 'set-global-preload-3.js'))
      const defaultSession = session.defaultSession;
        expect(defaultSession.getPreloads()).to.deep.equal([]);
        defaultSession.setPreloads(preloads);
        defaultSession.setPreloads([]);
      it('can set multiple session preload script', () => {
        expect(defaultSession.getPreloads()).to.deep.equal(preloads);
      const generateSpecs = (description: string, sandbox: boolean) => {
        describe(description, () => {
          it('loads the script before other scripts in window including normal preloads', async () => {
                sandbox,
                preload: path.join(fixtures, 'module', 'get-global-preload.js'),
            const [, preload1, preload2, preload3] = await once(ipcMain, 'vars');
            expect(preload1).to.equal('preload-1');
            expect(preload2).to.equal('preload-1-2');
            expect(preload3).to.be.undefined('preload 3');
      generateSpecs('without sandbox', false);
      generateSpecs('with sandbox', true);
    describe('"additionalArguments" option', () => {
      it('adds extra args to process.argv in the renderer process', async () => {
        const preload = path.join(fixtures, 'module', 'check-arguments.js');
            preload,
            additionalArguments: ['--my-magic-arg']
        w.loadFile(path.join(fixtures, 'api', 'blank.html'));
        const [, argv] = await once(ipcMain, 'answer');
        expect(argv).to.include('--my-magic-arg');
      it('adds extra value args to process.argv in the renderer process', async () => {
            additionalArguments: ['--my-magic-arg=foo']
        expect(argv).to.include('--my-magic-arg=foo');
    describe('"node-integration" option', () => {
      it('disables node integration by default', async () => {
        const preload = path.join(fixtures, 'module', 'send-later.js');
        const [, typeofProcess, typeofBuffer] = await once(ipcMain, 'answer');
        expect(typeofProcess).to.equal('undefined');
        expect(typeofBuffer).to.equal('undefined');
    describe('"sandbox" option', () => {
      const preload = path.join(path.resolve(__dirname, 'fixtures'), 'module', 'preload-sandbox.js');
            case '/cross-site':
              response.end(`<html><body><h1>${request.url}</h1></body></html>`);
      it('exposes ipcRenderer to preload script', async () => {
        expect(test).to.equal('preload');
      it('exposes ipcRenderer to preload script (path has special chars)', async () => {
        const preloadSpecialChars = path.join(fixtures, 'module', 'preload-sandboxæø åü.js');
            preload: preloadSpecialChars,
      it('exposes "loaded" event to preload script', async () => {
        await once(ipcMain, 'process-loaded');
      it('exposes "exit" event to preload script', async () => {
        const htmlPath = path.join(__dirname, 'fixtures', 'api', 'sandbox.html?exit-event');
        const pageUrl = 'file://' + htmlPath;
        w.loadURL(pageUrl);
        const [, url] = await once(ipcMain, 'answer');
        const expectedUrl = process.platform === 'win32'
          ? 'file:///' + htmlPath.replaceAll('\\', '/')
          : pageUrl;
        expect(url).to.equal(expectedUrl);
      it('exposes full EventEmitter object to preload script', async () => {
            preload: path.join(fixtures, 'module', 'preload-eventemitter.js')
        const [, rendererEventEmitterProperties] = await once(ipcMain, 'answer');
        const { EventEmitter } = require('node:events');
        const emitter = new EventEmitter();
        const browserEventEmitterProperties = [];
        let currentObj = emitter;
          browserEventEmitterProperties.push(...Object.getOwnPropertyNames(currentObj));
        } while ((currentObj = Object.getPrototypeOf(currentObj)));
        expect(rendererEventEmitterProperties).to.deep.equal(browserEventEmitterProperties);
      it('should open windows in same domain with cross-scripting enabled', async () => {
        w.webContents.setWindowOpenHandler(() => ({
          action: 'allow',
          overrideBrowserWindowOptions: {
        const htmlPath = path.join(__dirname, 'fixtures', 'api', 'sandbox.html?window-open');
        const answer = once(ipcMain, 'answer');
        const [, { url, frameName, options }] = await once(w.webContents, 'did-create-window') as [BrowserWindow, Electron.DidCreateWindowDetails];
        expect(frameName).to.equal('popup!');
        expect(options.width).to.equal(500);
        expect(options.height).to.equal(600);
        const [, html] = await answer;
        expect(html).to.equal('<h1>scripting from opener</h1>');
      it('should open windows in another domain with cross-scripting disabled', async () => {
        w.loadFile(
          path.join(__dirname, 'fixtures', 'api', 'sandbox.html'),
          { search: 'window-open-external' }
        // Wait for a message from the main window saying that it's ready.
        await once(ipcMain, 'opener-loaded');
        // Ask the opener to open a popup with window.opener.
        const expectedPopupUrl = `${serverUrl}/cross-site`; // Set in "sandbox.html".
        w.webContents.send('open-the-popup', expectedPopupUrl);
        // The page is going to open a popup that it won't be able to close.
        // We have to close it from here later.
        const [, popupWindow] = await once(app, 'browser-window-created') as [any, BrowserWindow];
        // Ask the popup window for details.
        const detailsAnswer = once(ipcMain, 'child-loaded');
        popupWindow.webContents.send('provide-details');
        const [, openerIsNull, , locationHref] = await detailsAnswer;
        expect(openerIsNull).to.be.false('window.opener is null');
        expect(locationHref).to.equal(expectedPopupUrl);
        // Ask the page to access the popup.
        const touchPopupResult = once(ipcMain, 'answer');
        w.webContents.send('touch-the-popup');
        const [, popupAccessMessage] = await touchPopupResult;
        // Ask the popup to access the opener.
        const touchOpenerResult = once(ipcMain, 'answer');
        popupWindow.webContents.send('touch-the-opener');
        const [, openerAccessMessage] = await touchOpenerResult;
        // We don't need the popup anymore, and its parent page can't close it,
        // so let's close it from here before we run any checks.
        await closeWindow(popupWindow, { assertNotWindows: false });
        const errorPattern = /Failed to read a named property 'document' from 'Window': Blocked a frame with origin "(.*?)" from accessing a cross-origin frame./;
        expect(popupAccessMessage).to.be.a('string',
          'child\'s .document is accessible from its parent window');
        expect(popupAccessMessage).to.match(errorPattern);
        expect(openerAccessMessage).to.be.a('string',
          'opener .document is accessible from a popup window');
        expect(openerAccessMessage).to.match(errorPattern);
      it('should inherit the sandbox setting in opened windows', async () => {
            sandbox: true
        const preloadPath = path.join(mainFixtures, 'api', 'new-window-preload.js');
        w.webContents.setWindowOpenHandler(() => ({ action: 'allow', overrideBrowserWindowOptions: { webPreferences: { preload: preloadPath } } }));
        w.loadFile(path.join(fixtures, 'api', 'new-window.html'));
        const [, { argv }] = await once(ipcMain, 'answer');
        expect(argv).to.include('--enable-sandbox');
      it('should open windows with the options configured via setWindowOpenHandler handlers', async () => {
        w.webContents.setWindowOpenHandler(() => ({ action: 'allow', overrideBrowserWindowOptions: { webPreferences: { preload: preloadPath, contextIsolation: false } } }));
        const [[, childWebContents]] = await Promise.all([
          once(app, 'web-contents-created') as Promise<[any, WebContents]>,
          once(ipcMain, 'answer')
        const webPreferences = childWebContents.getLastWebPreferences();
        expect(webPreferences!.contextIsolation).to.equal(false);
      it('should apply zoomFactor from setWindowOpenHandler overrideBrowserWindowOptions', async () => {
              zoomFactor: 2.0
        const [childWindow] = await once(w.webContents, 'did-create-window') as [BrowserWindow, any];
        await once(childWindow.webContents, 'did-finish-load');
        expect(childWindow.webContents.getZoomFactor()).to.be.closeTo(2.0, 0.1);
      it('should set ipc event sender correctly', async () => {
        let childWc: WebContents | null = null;
        w.webContents.setWindowOpenHandler(() => ({ action: 'allow', overrideBrowserWindowOptions: { webPreferences: { preload, contextIsolation: false } } }));
        w.webContents.on('did-create-window', (win) => {
          childWc = win.webContents;
          expect(w.webContents).to.not.equal(childWc);
        ipcMain.once('parent-ready', function (event) {
          expect(event.sender).to.equal(w.webContents, 'sender should be the parent');
          event.sender.send('verified');
        ipcMain.once('child-ready', function (event) {
          expect(childWc).to.not.be.null('child webcontents should be available');
          expect(event.sender).to.equal(childWc, 'sender should be the child');
        const done = Promise.all([
          'parent-answer',
          'child-answer'
        ].map(name => once(ipcMain, name)));
        w.loadFile(path.join(__dirname, 'fixtures', 'api', 'sandbox.html'), { search: 'verify-ipc-sender' });
        await done;
      describe('event handling', () => {
          w = new BrowserWindow({ show: false, webPreferences: { sandbox: true } });
        it('works for window events', async () => {
          const pageTitleUpdated = once(w, 'page-title-updated');
          const newTitle = 'changed';
          w.loadURL(`data:text/html,<script>document.title = '${newTitle}'</script>`);
          await pageTitleUpdated;
          // w.title should update after 'page-title-updated'.
          // It happens right *after* the event fires though,
          // so we have to waitUntil it changes
          waitUntil(() => w.title === newTitle);
        it('works for stop events', async () => {
            'did-navigate',
            'did-fail-load',
            'did-stop-loading'
          ].map(name => once(w.webContents, name)));
          w.loadURL('data:text/html,<script>stop()</script>');
        it('works for web contents events', async () => {
            'did-finish-load',
            'did-frame-finish-load',
            'did-start-loading',
            'did-stop-loading',
            'dom-ready'
          w.loadFile(path.join(__dirname, 'fixtures', 'api', 'sandbox.html'), { search: 'webcontents-events' });
      it('validates process APIs access in sandboxed renderer', async () => {
        w.webContents.once('preload-error', (event, preloadPath, error) => {
        process.env.sandboxmain = 'foo';
        expect(test.hasCrash).to.be.true('has crash');
        expect(test.hasHang).to.be.true('has hang');
        expect(test.heapStatistics).to.be.an('object');
        expect(test.blinkMemoryInfo).to.be.an('object');
        expect(test.processMemoryInfo).to.be.an('object');
        expect(test.systemVersion).to.be.a('string');
        expect(test.cpuUsage).to.be.an('object');
        expect(test.uptime).to.be.a('number');
        expect(test.arch).to.equal(process.arch);
        expect(test.platform).to.equal(process.platform);
        expect(test.env).to.deep.equal(process.env);
        expect(test.execPath).to.equal(process.helperExecPath);
        expect(test.sandboxed).to.be.true('sandboxed');
        expect(test.contextIsolated).to.be.false('contextIsolated');
        expect(test.type).to.equal('renderer');
        expect(test.version).to.equal(process.version);
        expect(test.versions).to.deep.equal(process.versions);
        expect(test.contextId).to.be.a('string');
        expect(test.nodeEvents).to.equal(true);
        expect(test.nodeTimers).to.equal(true);
        expect(test.nodeUrl).to.equal(true);
        if (process.platform === 'linux' && test.osSandbox) {
          expect(test.creationTime).to.be.null('creation time');
          expect(test.systemMemoryInfo).to.be.null('system memory info');
          expect(test.creationTime).to.be.a('number');
          expect(test.systemMemoryInfo).to.be.an('object');
      it('webview in sandbox renderer', async () => {
            webviewTag: true,
        const didAttachWebview = once(w.webContents, 'did-attach-webview') as Promise<[any, WebContents]>;
        const webviewDomReady = once(ipcMain, 'webview-dom-ready');
        w.loadFile(path.join(fixtures, 'pages', 'webview-did-attach-event.html'));
        const [, webContents] = await didAttachWebview;
        const [, id] = await webviewDomReady;
        expect(webContents.id).to.equal(id);
    describe('child windows', () => {
            // tests relies on preloads in opened windows
            nodeIntegrationInSubFrames: true,
      it('opens window of about:blank with cross-scripting enabled', async () => {
        w.loadFile(path.join(fixtures, 'api', 'native-window-open-blank.html'));
        const [, content] = await answer;
        expect(content).to.equal('Hello');
      it('opens window of same domain with cross-scripting enabled', async () => {
        w.loadFile(path.join(fixtures, 'api', 'native-window-open-file.html'));
      it('blocks accessing cross-origin frames', async () => {
        w.loadFile(path.join(fixtures, 'api', 'native-window-open-cross-origin.html'));
        expect(content).to.equal('Failed to read a named property \'toString\' from \'Location\': Blocked a frame with origin "file://" from accessing a cross-origin frame.');
      it('opens window from <iframe> tags', async () => {
        w.loadFile(path.join(fixtures, 'api', 'native-window-open-iframe.html'));
      it('opens window with cross-scripting enabled from isolated context', async () => {
            preload: path.join(fixtures, 'api', 'native-window-open-isolated-preload.js')
        w.loadFile(path.join(fixtures, 'api', 'native-window-open-isolated.html'));
        const [, content] = await once(ipcMain, 'answer');
      ifit(!process.env.ELECTRON_SKIP_NATIVE_MODULE_TESTS)('loads native addons correctly after reload', async () => {
        w.loadFile(path.join(__dirname, 'fixtures', 'api', 'native-window-open-native-addon.html'));
          expect(content).to.equal('function');
      it('<webview> works in a scriptable popup', async () => {
        const preload = path.join(fixtures, 'api', 'new-window-webview-preload.js');
        const webviewLoaded = once(ipcMain, 'webview-loaded');
        w.loadFile(path.join(fixtures, 'api', 'new-window-webview.html'));
        await webviewLoaded;
              preload: preloadPath,
      describe('window.location', () => {
          ['foo', path.join(fixtures, 'api', 'window-open-location-change.html')],
          ['bar', path.join(fixtures, 'api', 'window-open-location-final.html')]
          for (const [scheme, path] of protocols) {
            protocol.registerBufferProtocol(scheme, (request, callback) => {
              callback({
                mimeType: 'text/html',
                data: fs.readFileSync(path)
          for (const [scheme] of protocols) {
            protocol.unregisterProtocol(scheme);
        it('retains the original web preferences when window.location is changed to a new origin', async () => {
              // test relies on preloads in opened window
                preload: path.join(mainFixtures, 'api', 'window-open-preload.js'),
                nodeIntegrationInSubFrames: true
          w.loadFile(path.join(fixtures, 'api', 'window-open-location-open.html'));
          const [, { nodeIntegration, typeofProcess }] = await once(ipcMain, 'answer');
          expect(nodeIntegration).to.be.false();
          expect(typeofProcess).to.eql('undefined');
        it('window.opener is not null when window.location is changed to a new origin', async () => {
                preload: path.join(mainFixtures, 'api', 'window-open-preload.js')
          const [, { windowOpenerIsNull }] = await once(ipcMain, 'answer');
          expect(windowOpenerIsNull).to.be.false('window.opener is null');
    describe('"disableHtmlFullscreenWindowResize" option', () => {
      it('prevents window from resizing when set', async () => {
            disableHtmlFullscreenWindowResize: true
        const enterHtmlFullScreen = once(w.webContents, 'enter-html-full-screen');
        w.webContents.executeJavaScript('document.body.webkitRequestFullscreen()', true);
        await enterHtmlFullScreen;
        expect(w.getSize()).to.deep.equal(size);
    describe('"defaultFontFamily" option', () => {
      it('can change the standard font family', async () => {
            defaultFontFamily: {
              standard: 'Impact'
        await w.loadFile(path.join(fixtures, 'pages', 'content.html'));
        const fontFamily = await w.webContents.executeJavaScript("window.getComputedStyle(document.getElementsByTagName('p')[0])['font-family']", true);
        expect(fontFamily).to.equal('Impact');
  describe('beforeunload handler', function () {
    it('returning undefined would not prevent close', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-undefined.html'));
      const wait = once(w, 'closed');
      await wait;
    it('returning false would prevent close', async () => {
      const [, proceed] = await once(w.webContents, '-before-unload-fired');
      expect(proceed).to.equal(false);
    it('returning empty string would prevent close', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-empty-string.html'));
    it('emits for each close attempt', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-false-prevent3.html'));
      const destroyListener = () => { expect.fail('Close was not prevented'); };
      w.webContents.once('destroyed', destroyListener);
      w.webContents.executeJavaScript('installBeforeUnload(2)', true);
      // The renderer needs to report the status of beforeunload handler
      // back to main process, so wait for next console message, which means
      // the SuddenTerminationStatus message have been flushed.
      await once(w.webContents, 'console-message');
      w.webContents.removeListener('destroyed', destroyListener);
    it('emits for each reload attempt', async () => {
      const navigationListener = () => { expect.fail('Reload was not prevented'); };
      w.webContents.once('did-start-navigation', navigationListener);
      // Chromium does not emit '-before-unload-fired' on WebContents for
      // navigations, so we have to use other ways to know if beforeunload
      // is fired.
      await emittedUntil(w.webContents, 'console-message', isBeforeUnload);
      w.webContents.removeListener('did-start-navigation', navigationListener);
    it('emits for each navigation attempt', async () => {
  // TODO(codebytere): figure out how to make these pass in CI on Windows.
  ifdescribe(process.platform !== 'win32')('document.visibilityState/hidden', () => {
    it('visibilityState is initially visible despite window being hidden', async () => {
      let readyToShow = false;
      w.once('ready-to-show', () => {
        readyToShow = true;
      expect(readyToShow).to.be.false('ready to show');
    it('visibilityState changes when window is hidden', async () => {
    it('visibilityState changes when window is shown', async () => {
        // See https://github.com/electron/electron/issues/8664
        await once(w, 'show');
      const [, visibilityState] = await once(ipcMain, 'pong');
    it('visibilityState changes when window is shown inactive', async () => {
    ifit(process.platform === 'darwin')('visibilityState changes when window is minimized', async () => {
    it('visibilityState remains visible if backgroundThrottling is disabled', async () => {
      ipcMain.once('pong', (event, visibilityState, hidden) => {
        throw new Error(`Unexpected visibility change event. visibilityState: ${visibilityState} hidden: ${hidden}`);
        const shown1 = once(w, 'show');
        await shown1;
        const shown2 = once(w, 'show');
        await shown2;
        ipcMain.removeAllListeners('pong');
  ifdescribe(process.platform !== 'linux')('max/minimize events', () => {
    it('emits an event when window is maximized', async () => {
    it('emits an event when a transparent window is maximized', async () => {
    it('emits only one event when frameless window is maximized', () => {
      let emitted = 0;
      w.on('maximize', () => emitted++);
      expect(emitted).to.equal(1);
    it('emits an event when window is unmaximized', async () => {
    it('emits an event when a transparent window is unmaximized', async () => {
    it('emits an event when window is minimized', async () => {
  describe('beginFrameSubscription method', () => {
    it('does not crash when callback returns nothing', (done) => {
      let called = false;
      w.loadFile(path.join(fixtures, 'api', 'frame-subscriber.html'));
      w.webContents.on('dom-ready', () => {
        w.webContents.beginFrameSubscription(function () {
          // This callback might be called twice.
          if (called) return;
          called = true;
          // Pending endFrameSubscription to next tick can reliably reproduce
          // a crash which happens when nothing is returned in the callback.
          setTimeout().then(() => {
            w.webContents.endFrameSubscription();
    it('subscribes to frame updates', (done) => {
        w.webContents.beginFrameSubscription(function (data) {
            expect(data.constructor.name).to.equal('NativeImage');
            expect(data.isEmpty()).to.be.false('data is empty');
    it('subscribes to frame updates (only dirty rectangle)', (done) => {
      let gotInitialFullSizeFrame = false;
      const [contentWidth, contentHeight] = w.getContentSize();
      w.webContents.on('did-finish-load', () => {
        w.webContents.beginFrameSubscription(true, (image, rect) => {
          if (image.isEmpty()) {
            // Chromium sometimes sends a 0x0 frame at the beginning of the
            // page load.
          if (rect.height === contentHeight && rect.width === contentWidth &&
            !gotInitialFullSizeFrame) {
            // The initial frame is full-size, but we're looking for a call
            // with just the dirty-rect. The next frame should be a smaller
            // rect.
            gotInitialFullSizeFrame = true;
          // We asked for just the dirty rectangle, so we expect to receive a
          // rect smaller than the full size.
          // TODO(jeremy): this is failing on windows currently; investigate.
          // assert(rect.width < contentWidth || rect.height < contentHeight)
            const expectedSize = rect.width * rect.height * 4;
            expect(image.toBitmap()).to.be.an.instanceOf(Buffer).with.lengthOf(expectedSize);
    it('throws error when subscriber is not well defined', () => {
        w.webContents.beginFrameSubscription(true, true as any);
        // TODO(zcbenz): gin is weak at guessing parameter types, we should
        // upstream native_mate's implementation to gin.
      }).to.throw('Error processing argument at index 1, conversion failure from ');
  describe('savePage method', () => {
    const savePageDir = path.join(fixtures, 'save_page');
    const savePageHtmlPath = path.join(savePageDir, 'save_page.html');
    const savePageJsPath = path.join(savePageDir, 'save_page_files', 'test.js');
    const savePageCssPath = path.join(savePageDir, 'save_page_files', 'test.css');
      closeAllWindows();
        fs.unlinkSync(savePageCssPath);
        fs.unlinkSync(savePageJsPath);
        fs.unlinkSync(savePageHtmlPath);
        fs.rmdirSync(path.join(savePageDir, 'save_page_files'));
        fs.rmdirSync(savePageDir);
      } catch { }
    it('should throw when passing relative paths', async () => {
      await w.loadFile(path.join(fixtures, 'pages', 'save_page', 'index.html'));
        w.webContents.savePage('save_page.html', 'HTMLComplete')
      ).to.eventually.be.rejectedWith('Path must be absolute');
        w.webContents.savePage('save_page.html', 'HTMLOnly')
        w.webContents.savePage('save_page.html', 'MHTML')
    it('should save page to disk with HTMLOnly', async () => {
      await w.webContents.savePage(savePageHtmlPath, 'HTMLOnly');
      expect(fs.existsSync(savePageHtmlPath)).to.be.true('html path');
      expect(fs.existsSync(savePageJsPath)).to.be.false('js path');
      expect(fs.existsSync(savePageCssPath)).to.be.false('css path');
    it('should save page to disk with MHTML', async () => {
      /* Use temp directory for saving MHTML file since the write handle
       * gets passed to untrusted process and chromium will deny exec access to
       * the path. To perform this task, chromium requires that the path is one
       * of the browser controlled paths, refs https://chromium-review.googlesource.com/c/chromium/src/+/3774416
      const tmpDir = await fs.promises.mkdtemp(path.resolve(os.tmpdir(), 'electron-mhtml-save-'));
      const savePageMHTMLPath = path.join(tmpDir, 'save_page.html');
      await w.webContents.savePage(savePageMHTMLPath, 'MHTML');
      expect(fs.existsSync(savePageMHTMLPath)).to.be.true('html path');
        await fs.promises.unlink(savePageMHTMLPath);
        await fs.promises.rmdir(tmpDir);
    it('should save page to disk with HTMLComplete', async () => {
      await w.webContents.savePage(savePageHtmlPath, 'HTMLComplete');
      expect(fs.existsSync(savePageJsPath)).to.be.true('js path');
      expect(fs.existsSync(savePageCssPath)).to.be.true('css path');
  describe('BrowserWindow options argument is optional', () => {
    it('should create a window with default size (800x600)', () => {
      const w = new BrowserWindow();
      expect(w.getSize()).to.deep.equal([800, 600]);
  describe('BrowserWindow.restore()', () => {
    it('should restore the previous window size', () => {
        width: 800
      const initialSize = w.getSize();
      expectBoundsEqual(w.getSize(), initialSize);
    it('does not crash when restoring hidden minimized window', () => {
      const w = new BrowserWindow({});
    // TODO(zcbenz):
    // This test does not run on Linux CI. See:
    // https://github.com/electron/electron/issues/28699
    ifit(process.platform === 'linux' && !process.env.CI)('should bring a minimized maximized window back to maximized state', async () => {
    ifit(process.platform !== 'linux')('should not break fullscreen state', async () => {
      expect(w.isFullScreen()).to.be.true('not fullscreen');
      expect(w.isFullScreen()).to.be.true('not fullscreen after restore');
      expect(w.isMinimized()).to.be.false('should not be minimized');
      // Clean up fullscreen state.
  // TODO(dsanders11): Enable once maximize event works on Linux again on CI
  ifdescribe(process.platform !== 'linux')('BrowserWindow.maximize()', () => {
    it('should show the window if it is not currently shown', async () => {
      let shown = once(w, 'show');
      expect(w.isVisible()).to.be.false('visible');
      expect(w.isMaximized()).to.be.true('maximized');
      expect(w.isVisible()).to.be.true('visible');
      // Even if the window is already maximized
      shown = once(w, 'show');
  describe('BrowserWindow.unmaximize()', () => {
    it('should restore the previous window position', () => {
      const initialPosition = w.getPosition();
      expectBoundsEqual(w.getPosition(), initialPosition);
    // TODO(dsanders11): Enable once minimize event works on Linux again.
    //                   See https://github.com/electron/electron/issues/28699
    ifit(process.platform !== 'linux')('should not restore a minimized window', async () => {
      expect(w.isMinimized()).to.be.true();
    it('should not change the size or position of a normal window', async () => {
    ifit(process.platform === 'darwin')('should not change size or position of a window which is functionally maximized', async () => {
      const { workArea } = screen.getPrimaryDisplay();
      const bounds = {
        x: workArea.x,
        y: workArea.y,
        width: workArea.width,
        height: workArea.height
      const w = new BrowserWindow(bounds);
      expectBoundsEqual(w.getBounds(), bounds);
  describe('setFullScreen(false)', () => {
    // only applicable to windows: https://github.com/electron/electron/issues/6036
    ifdescribe(process.platform === 'win32')('on windows', () => {
      it('should restore a normal visible window from a fullscreen startup state', async () => {
        // start fullscreen and hidden
        const leftFullScreen = once(w, 'leave-full-screen');
        await leftFullScreen;
        expect(w.isFullScreen()).to.be.false('fullscreen');
      it('should keep window hidden if already in hidden state', async () => {
    ifdescribe(process.platform === 'darwin')('BrowserWindow.setFullScreen(false) when HTML fullscreen', () => {
      it('exits HTML fullscreen when window leaves fullscreen', async () => {
        await w.webContents.executeJavaScript('document.body.webkitRequestFullscreen()', true);
        await once(w, 'enter-full-screen');
        // Wait a tick for the full-screen state to 'stick'
        await once(w, 'leave-html-full-screen');
  describe('parent window', () => {
    ifit(process.platform === 'darwin')('sheet-begin event emits when window opens a sheet', async () => {
      const sheetBegin = once(w, 'sheet-begin');
        modal: true,
        parent: w
      await sheetBegin;
    ifit(process.platform === 'darwin')('sheet-end event emits when window has closed a sheet', async () => {
      const sheet = new BrowserWindow({
      const sheetEnd = once(w, 'sheet-end');
      sheet.close();
      await sheetEnd;
    describe('parent option', () => {
      it('sets parent window', () => {
        expect(c.getParentWindow()).to.equal(w);
      it('adds window to child windows of parent', () => {
        expect(w.getChildWindows()).to.deep.equal([c]);
      it('removes from child windows of parent when window is closed', async () => {
        const closed = once(c, 'closed');
        c.close();
        // The child window list is not immediately cleared, so wait a tick until it's ready.
        expect(w.getChildWindows().length).to.equal(0);
      it('can handle child window close and reparent multiple times', async () => {
        let c: BrowserWindow | null;
          c = new BrowserWindow({ show: false, parent: w });
      it('can handle parent window close with focus or blur events', (done) => {
        c.on('closed', () => {
      ifit(process.platform === 'darwin')('only shows the intended window when a child with siblings is shown', async () => {
        const childOne = new BrowserWindow({ show: false, parent: w });
        const childTwo = new BrowserWindow({ show: false, parent: w });
        const parentShown = once(w, 'show');
        await parentShown;
        expect(childOne.isVisible()).to.be.false('childOne is visible');
        expect(childTwo.isVisible()).to.be.false('childTwo is visible');
        const childOneShown = once(childOne, 'show');
        childOne.show();
        await childOneShown;
        expect(childOne.isVisible()).to.be.true('childOne is not visible');
      ifit(process.platform === 'darwin')('child matches parent visibility when parent visibility changes', async () => {
        c.show();
        await Promise.all([wShow, cShow]);
      ifit(process.platform === 'darwin')('parent matches child visibility when child visibility changes', async () => {
        const minimized = once(c, 'minimize');
        c.minimize();
        const restored = once(c, 'restore');
        c.restore();
      it('closes a grandchild window when a middle child window is destroyed', async () => {
        w.loadFile(path.join(fixtures, 'pages', 'base-page.html'));
        w.webContents.executeJavaScript('window.open("")');
        w.webContents.on('did-create-window', async (window) => {
          const childWindow = new BrowserWindow({ parent: window });
          const closed = once(childWindow, 'closed');
          expect(() => { BrowserWindow.getFocusedWindow(); }).to.not.throw();
      it('should not affect the show option', () => {
        expect(c.getParentWindow()!.isVisible()).to.be.false('parent is visible');
    describe('win.setParentWindow(parent)', () => {
        const c = new BrowserWindow({ show: false });
        expect(w.getParentWindow()).to.be.null('w.parent');
        expect(c.getParentWindow()).to.be.null('c.parent');
        c.setParentWindow(w);
        c.setParentWindow(null);
        expect(w.getChildWindows()).to.deep.equal([]);
      ifit(process.platform === 'darwin')('can reparent when the first parent is destroyed', async () => {
        c.setParentWindow(w1);
        expect(w1.getChildWindows().length).to.equal(1);
        const closed = once(w1, 'closed');
        c.setParentWindow(w2);
        const children = w2.getChildWindows();
        expect(children[0]).to.equal(c);
    describe('modal option', () => {
      it('does not freeze or crash', async () => {
        const parentWindow = new BrowserWindow();
        const createTwo = async () => {
          const two = new BrowserWindow({
            parent: parentWindow,
          const twoShown = once(two, 'show');
          two.show();
          await twoShown;
          setTimeout(500).then(() => two.close());
          await once(two, 'closed');
        const one = new BrowserWindow({
        const oneShown = once(one, 'show');
        one.show();
        await oneShown;
        setTimeout(500).then(() => one.destroy());
        await once(one, 'closed');
        await createTwo();
      ifit(process.platform !== 'darwin')('can disable and enable a window', () => {
        w.setEnabled(false);
        expect(w.isEnabled()).to.be.false('w.isEnabled()');
        w.setEnabled(true);
        expect(w.isEnabled()).to.be.true('!w.isEnabled()');
      ifit(process.platform !== 'darwin')('disables parent window', () => {
        const c = new BrowserWindow({ show: false, parent: w, modal: true });
        expect(w.isEnabled()).to.be.true('w.isEnabled');
        expect(w.isEnabled()).to.be.false('w.isEnabled');
      ifit(process.platform !== 'darwin')('re-enables an enabled parent window when closed', async () => {
      ifit(process.platform !== 'darwin')('does not re-enable a disabled parent window when closed', async () => {
      ifit(process.platform !== 'darwin')('disables parent window recursively', () => {
        const c2 = new BrowserWindow({ show: false, parent: w, modal: true });
        c2.show();
        c.destroy();
        c2.destroy();
  describe('window states', () => {
    it('does not resize frameless windows when states change', () => {
      w.minimizable = false;
      w.minimizable = true;
      expect(w.getSize()).to.deep.equal([300, 200]);
      w.resizable = true;
      w.maximizable = false;
      w.maximizable = true;
      w.fullScreenable = false;
      w.fullScreenable = true;
      w.closable = false;
      w.closable = true;
    describe('resizable state', () => {
      it('with properties', () => {
        it('can be set with resizable constructor option', () => {
          const w = new BrowserWindow({ show: false, resizable: false });
          expect(w.resizable).to.be.false('resizable');
            expect(w.maximizable).to.to.true('maximizable');
          expect(w.resizable).to.be.true('resizable');
      it('with functions', () => {
          expect(w.isResizable()).to.be.false('resizable');
            expect(w.isMaximizable()).to.to.true('maximizable');
          expect(w.isResizable()).to.be.true('resizable');
          w.setResizable(false);
          w.setResizable(true);
          const w = new BrowserWindow({ show: false, thickFrame: false });
      // On Linux there is no "resizable" property of a window.
      ifit(process.platform !== 'linux')('does affect maximizability when disabled and enabled', () => {
        expect(w.maximizable).to.be.true('maximizable');
        expect(w.maximizable).to.be.false('not maximizable');
      it('does not change window size when disabled and enabled', () => {
          frame: true
        expectBoundsEqual(w.getSize(), [400, 300]);
      ifit(process.platform !== 'darwin')('works for a window smaller than 64x64', () => {
        w.setContentSize(60, 60);
        expectBoundsEqual(w.getContentSize(), [60, 60]);
        w.setContentSize(30, 30);
        expectBoundsEqual(w.getContentSize(), [30, 30]);
        w.setContentSize(10, 10);
        expectBoundsEqual(w.getContentSize(), [10, 10]);
      ifit(process.platform === 'win32')('do not change window with frame bounds when maximized', () => {
          frame: true,
          thickFrame: true
      ifit(process.platform === 'win32')('do not change window without frame bounds when maximized', () => {
      ifit(process.platform === 'win32')('do not change window transparent without frame bounds when maximized', () => {
          thickFrame: true,
    describe('loading main frame state', () => {
      it('is true when the main frame is loading', async () => {
        w.webContents.loadURL(serverUrl);
        expect(w.webContents.isLoadingMainFrame()).to.be.true('isLoadingMainFrame');
      it('is false when only a subframe is loading', async () => {
        const didStopLoading = once(w.webContents, 'did-stop-loading');
        await didStopLoading;
        expect(w.webContents.isLoadingMainFrame()).to.be.false('isLoadingMainFrame');
          var iframe = document.createElement('iframe')
          iframe.src = '${serverUrl}/page2'
          document.body.appendChild(iframe)
      it('is true when navigating to pages from the same origin', async () => {
        w.webContents.loadURL(`${serverUrl}/page2`);
    ifdescribe(process.platform !== 'win32')('visibleOnAllWorkspaces state', () => {
          expect(w.visibleOnAllWorkspaces).to.be.false();
          w.visibleOnAllWorkspaces = true;
          expect(w.visibleOnAllWorkspaces).to.be.true();
          expect(w.isVisibleOnAllWorkspaces()).to.be.false();
          w.setVisibleOnAllWorkspaces(true);
          expect(w.isVisibleOnAllWorkspaces()).to.be.true();
    describe('native window title', () => {
        it('can be set with title constructor option', () => {
          const w = new BrowserWindow({ show: false, title: 'mYtItLe' });
          expect(w.title).to.eql('mYtItLe');
          expect(w.title).to.eql('Electron Test Main');
          w.title = 'NEW TITLE';
          expect(w.title).to.eql('NEW TITLE');
        it('can be set with minimizable constructor option', () => {
          expect(w.getTitle()).to.eql('mYtItLe');
          expect(w.getTitle()).to.eql('Electron Test Main');
          w.setTitle('NEW TITLE');
          expect(w.getTitle()).to.eql('NEW TITLE');
    describe('hasShadow state', () => {
        it('returns a boolean on all platforms', () => {
          expect(w.shadow).to.be.a('boolean');
        // On Windows there's no shadow by default & it can't be changed dynamically.
        it('can be changed with hasShadow option', () => {
          const hasShadow = process.platform !== 'darwin';
          const w = new BrowserWindow({ show: false, hasShadow });
          expect(w.shadow).to.equal(hasShadow);
        it('can be changed with setHasShadow method', () => {
          w.shadow = false;
          expect(w.shadow).to.be.false('hasShadow');
          w.shadow = true;
          expect(w.shadow).to.be.true('hasShadow');
          const hasShadow = w.hasShadow();
          expect(hasShadow).to.be.a('boolean');
          expect(w.hasShadow()).to.equal(hasShadow);
          w.setHasShadow(false);
          expect(w.hasShadow()).to.be.false('hasShadow');
          w.setHasShadow(true);
          expect(w.hasShadow()).to.be.true('hasShadow');
  ifdescribe(process.platform !== 'linux')('window states (excluding Linux)', () => {
    // Not implemented on Linux.
    describe('movable state', () => {
        it('can be set with movable constructor option', () => {
          const w = new BrowserWindow({ show: false, movable: false });
          expect(w.movable).to.be.false('movable');
          expect(w.movable).to.be.true('movable');
          w.movable = false;
          w.movable = true;
          expect(w.isMovable()).to.be.false('movable');
          expect(w.isMovable()).to.be.true('movable');
          w.setMovable(false);
          w.setMovable(true);
    ifdescribe(process.platform === 'darwin')('documentEdited state', () => {
          expect(w.documentEdited).to.be.false();
          w.documentEdited = true;
          expect(w.documentEdited).to.be.true();
          expect(w.isDocumentEdited()).to.be.false();
          w.setDocumentEdited(true);
          expect(w.isDocumentEdited()).to.be.true();
    ifdescribe(process.platform === 'darwin')('representedFilename', () => {
          expect(w.representedFilename).to.eql('');
          w.representedFilename = 'a name';
          expect(w.representedFilename).to.eql('a name');
          expect(w.getRepresentedFilename()).to.eql('');
          w.setRepresentedFilename('a name');
          expect(w.getRepresentedFilename()).to.eql('a name');
    describe('minimizable state', () => {
          const w = new BrowserWindow({ show: false, minimizable: false });
          expect(w.minimizable).to.be.false('minimizable');
          expect(w.minimizable).to.be.true('minimizable');
          expect(w.isMinimizable()).to.be.false('movable');
          expect(w.isMinimizable()).to.be.true('isMinimizable');
          expect(w.isMinimizable()).to.be.false('isMinimizable');
          w.setMinimizable(true);
    describe('maximizable state (property)', () => {
        it('can be set with maximizable constructor option', () => {
          const w = new BrowserWindow({ show: false, maximizable: false });
          expect(w.maximizable).to.be.false('maximizable');
        it('is not affected when changing other states', () => {
          expect(w.isMaximizable()).to.be.false('isMaximizable');
          expect(w.isMaximizable()).to.be.true('isMaximizable');
          w.setMaximizable(true);
          w.setClosable(false);
          w.setClosable(true);
          w.setFullScreenable(false);
    ifdescribe(process.platform === 'win32')('maximizable state', () => {
        it('is reset to its former state', () => {
    ifdescribe(process.platform !== 'darwin')('menuBarVisible state', () => {
          expect(w.menuBarVisible).to.be.true();
          w.menuBarVisible = false;
          expect(w.menuBarVisible).to.be.false();
          w.menuBarVisible = true;
          expect(w.isMenuBarVisible()).to.be.true('isMenuBarVisible');
          w.setMenuBarVisibility(false);
          expect(w.isMenuBarVisible()).to.be.false('isMenuBarVisible');
          w.setMenuBarVisibility(true);
    ifdescribe(process.platform !== 'darwin')('when fullscreen state is changed', () => {
      it('correctly remembers state prior to fullscreen change', async () => {
        // This should do nothing.
        w.setFullScreen(true); // This should do nothing.
        expect(w.fullScreen).to.be.true('not fullscreen');
        const exitFS = once(w, 'leave-full-screen');
        w.setFullScreen(false); // This should do nothing.
        await exitFS;
        expect(w.fullScreen).to.be.false('not fullscreen');
      it('correctly remembers state prior to fullscreen change with autoHide', async () => {
    ifdescribe(process.platform !== 'darwin')('fullscreen state', () => {
      it('correctly remembers state prior to HTML fullscreen transition', async () => {
        await w.loadFile(path.join(fixtures, 'pages', 'a.html'));
        expect(w.isFullScreen()).to.be.false('is fullscreen');
        await w.webContents.executeJavaScript('document.getElementById("div").requestFullscreen()', true);
        await w.webContents.executeJavaScript('document.exitFullscreen()', true);
    ifdescribe(process.platform === 'darwin')('fullscreenable state', () => {
        it('can be set with fullscreenable constructor option', () => {
          const w = new BrowserWindow({ show: false, fullscreenable: false });
          expect(w.isFullScreenable()).to.be.false('isFullScreenable');
          expect(w.isFullScreenable()).to.be.true('isFullScreenable');
          w.setFullScreenable(true);
      it('does not open non-fullscreenable child windows in fullscreen if parent is fullscreen', async () => {
        const child = new BrowserWindow({ parent: w, resizable: false, fullscreenable: false });
        const shown = once(child, 'show');
        expect(child.resizable).to.be.false('resizable');
        expect(child.fullScreen).to.be.false('fullscreen');
        expect(child.fullScreenable).to.be.false('fullscreenable');
      it('is set correctly with different resizable values', async () => {
        const w1 = new BrowserWindow({
        const w2 = new BrowserWindow({
        const w3 = new BrowserWindow({
        expect(w1.isFullScreenable()).to.be.false('isFullScreenable');
        expect(w2.isFullScreenable()).to.be.false('isFullScreenable');
        expect(w3.isFullScreenable()).to.be.false('isFullScreenable');
      it('does not disable maximize button if window is resizable', () => {
    ifdescribe(process.platform === 'darwin')('isHiddenInMissionControl state', () => {
        it('can be set with ignoreMissionControl constructor option', () => {
          const w = new BrowserWindow({ show: false, hiddenInMissionControl: true });
          expect(w.isHiddenInMissionControl()).to.be.true('isHiddenInMissionControl');
          expect(w.isHiddenInMissionControl()).to.be.false('isHiddenInMissionControl');
          w.setHiddenInMissionControl(true);
          w.setHiddenInMissionControl(false);
    // fullscreen events are dispatched eagerly and twiddling things too fast can confuse poor Electron
    ifdescribe(process.platform === 'darwin')('kiosk state', () => {
        it('can be set with a constructor property', () => {
          const w = new BrowserWindow({ kiosk: true });
          expect(w.kiosk).to.be.true();
        it('can be changed ', async () => {
          w.kiosk = true;
          expect(w.isKiosk()).to.be.true('isKiosk');
          w.kiosk = false;
          expect(w.isKiosk()).to.be.false('isKiosk');
          expect(w.isKiosk()).to.be.true();
          w.setKiosk(true);
          w.setKiosk(false);
    ifdescribe(process.platform === 'darwin')('fullscreen state with resizable set', () => {
      it('resizable flag should be set to false and restored', async () => {
        const w = new BrowserWindow({ resizable: false });
      it('default resizable flag should be restored after entering/exiting fullscreen', async () => {
    ifdescribe(process.platform === 'darwin')('fullscreen state', () => {
      it('should not cause a crash if called when exiting fullscreen', async () => {
      it('should not crash if rounded corners are disabled', async () => {
          roundedCorners: false
      it('should not crash if opening a borderless child window from fullscreen parent', async () => {
        const parent = new BrowserWindow();
        const parentFS = once(parent, 'enter-full-screen');
        parent.setFullScreen(true);
        await parentFS;
        const child = new BrowserWindow({
        const childFS = once(child, 'enter-full-screen');
        child.show();
        await childFS;
        const leaveFullScreen = once(child, 'leave-full-screen');
        child.setFullScreen(false);
      it('should be able to load a URL while transitioning to fullscreen', async () => {
        const w = new BrowserWindow({ fullscreen: true });
        w.loadFile(path.join(fixtures, 'pages', 'c.html'));
        const load = once(w.webContents, 'did-finish-load');
        await Promise.all([enterFS, load]);
      it('can be changed with setFullScreen method', async () => {
        expect(w.isFullScreen()).to.be.true('isFullScreen');
        expect(w.isFullScreen()).to.be.false('isFullScreen');
      it('handles several transitions starting with fullscreen', async () => {
        const w = new BrowserWindow({ fullscreen: true, show: true });
        const enterFullScreen = emittedNTimes(w, 'enter-full-screen', 2);
      it('handles several HTML fullscreen transitions', async () => {
      it('handles several transitions in close proximity', async () => {
        const enterFS = emittedNTimes(w, 'enter-full-screen', 2);
        const leaveFS = emittedNTimes(w, 'leave-full-screen', 2);
        await Promise.all([enterFS, leaveFS]);
        expect(w.isFullScreen()).to.be.false('not fullscreen');
      it('handles several chromium-initiated transitions in close proximity', async () => {
        let enterCount = 0;
        let exitCount = 0;
        const done = new Promise<void>(resolve => {
          const checkDone = () => {
            if (enterCount === 2 && exitCount === 2) resolve();
          w.webContents.on('enter-html-full-screen', () => {
            enterCount++;
            checkDone();
          w.webContents.on('leave-html-full-screen', () => {
            exitCount++;
        await w.webContents.executeJavaScript('document.exitFullscreen()');
      it('handles HTML fullscreen transitions when fullscreenable is false', async () => {
        const w = new BrowserWindow({ fullscreenable: false });
        const done = new Promise<void>((resolve, reject) => {
          w.webContents.on('enter-html-full-screen', async () => {
            if (w.isFullScreen()) reject(new Error('w.isFullScreen should be false'));
            await waitUntil(async () => {
              const isFS = await w.webContents.executeJavaScript('!!document.fullscreenElement');
              return isFS === true;
        await expect(done).to.eventually.be.fulfilled();
      it('does not crash when exiting simpleFullScreen (properties)', async () => {
        w.setSimpleFullScreen(true);
        w.setFullScreen(!w.isFullScreen());
      it('does not crash when exiting simpleFullScreen (functions)', async () => {
        w.simpleFullScreen = true;
      it('should not be changed by setKiosk method', async () => {
      it('should stay fullscreen if fullscreen before kiosk', async () => {
        // Wait enough time for a fullscreen change to take effect.
      it('multiple windows inherit correct fullscreen state', async () => {
        const enterFullScreen2 = once(w2, 'enter-full-screen');
        await enterFullScreen2;
        expect(w2.isFullScreen()).to.be.true('isFullScreen');
    describe('closable state', () => {
        it('can be set with closable constructor option', () => {
          const w = new BrowserWindow({ show: false, closable: false });
          expect(w.closable).to.be.false('closable');
          expect(w.closable).to.be.true('closable');
          expect(w.isClosable()).to.be.false('isClosable');
          expect(w.isClosable()).to.be.true('isClosable');
  describe('window.getMediaSourceId()', () => {
    it('returns valid source id', async () => {
      // Check format 'window:1234:0'.
      const sourceId = w.getMediaSourceId();
      expect(sourceId).to.match(/^window:\d+:\d+$/);
  ifdescribe(!process.env.ELECTRON_SKIP_NATIVE_MODULE_TESTS)('window.getNativeWindowHandle()', () => {
    it('returns valid handle', () => {
      const isValidWindow = require('@electron-ci/is-valid-window');
      expect(isValidWindow(w.getNativeWindowHandle())).to.be.true('is valid window');
  ifdescribe(process.platform === 'darwin')('previewFile', () => {
    it('opens the path in Quick Look on macOS', () => {
        w.previewFile(__filename);
        w.closeFilePreview();
    it('should not call BrowserWindow show event', async () => {
      let showCalled = false;
      w.on('show', () => {
        showCalled = true;
      await setTimeout(500);
      expect(showCalled).to.equal(false, 'should not have called show twice');
  // TODO (jkleinsc) renable these tests on mas arm64
  ifdescribe(!process.mas || process.arch !== 'arm64')('contextIsolation option with and without sandbox option', () => {
    const expectedContextData = {
      preloadContext: {
        preloadProperty: 'number',
        pageProperty: 'undefined',
        typeofRequire: 'function',
        typeofProcess: 'object',
        typeofArrayPush: 'function',
        typeofFunctionApply: 'function',
        typeofPreloadExecuteJavaScriptProperty: 'undefined'
      pageContext: {
        preloadProperty: 'undefined',
        pageProperty: 'string',
        typeofRequire: 'undefined',
        typeofProcess: 'undefined',
        typeofArrayPush: 'number',
        typeofFunctionApply: 'boolean',
        typeofPreloadExecuteJavaScriptProperty: 'number',
        typeofOpenedWindow: 'object'
    it('separates the page context from the Electron/preload context', async () => {
      const iw = new BrowserWindow({
          preload: path.join(fixtures, 'api', 'isolated-preload.js')
      const p = once(ipcMain, 'isolated-world');
      iw.loadFile(path.join(fixtures, 'api', 'isolated.html'));
      const [, data] = await p;
      expect(data).to.deep.equal(expectedContextData);
    it('recreates the contexts on reload', async () => {
      await iw.loadFile(path.join(fixtures, 'api', 'isolated.html'));
      const isolatedWorld = once(ipcMain, 'isolated-world');
      iw.webContents.reload();
      const [, data] = await isolatedWorld;
    it('enables context isolation on child windows', async () => {
      const browserWindowCreated = once(app, 'browser-window-created') as Promise<[any, BrowserWindow]>;
      iw.loadFile(path.join(fixtures, 'pages', 'window-open.html'));
      const [, window] = await browserWindowCreated;
      expect(window.webContents.getLastWebPreferences()!.contextIsolation).to.be.true('contextIsolation');
    it('separates the page context from the Electron/preload context with sandbox on', async () => {
      const ws = new BrowserWindow({
      ws.loadFile(path.join(fixtures, 'api', 'isolated.html'));
    it('recreates the contexts on reload with sandbox on', async () => {
      await ws.loadFile(path.join(fixtures, 'api', 'isolated.html'));
      ws.webContents.reload();
    it('supports fetch api', async () => {
      const fetchWindow = new BrowserWindow({
          preload: path.join(fixtures, 'api', 'isolated-fetch-preload.js')
      const p = once(ipcMain, 'isolated-fetch-error');
      fetchWindow.loadURL('about:blank');
      const [, error] = await p;
      expect(error).to.equal('Failed to fetch');
    it('doesn\'t break ipc serialization', async () => {
      iw.loadURL('about:blank');
      iw.webContents.executeJavaScript(`
        const opened = window.open()
        openedLocation = opened.location.href
        opened.close()
        window.postMessage({openedLocation}, '*')
      expect(data.pageContext.openedLocation).to.equal('about:blank');
    it('reports process.contextIsolated', async () => {
          preload: path.join(fixtures, 'api', 'isolated-process.js')
      const p = once(ipcMain, 'context-isolation');
      const [, contextIsolation] = await p;
      expect(contextIsolation).to.be.true('contextIsolation');
  it('reloading does not cause Node.js module API hangs after reload', (done) => {
    ipcMain.on('async-node-api-done', () => {
      if (count === 3) {
        ipcMain.removeAllListeners('async-node-api-done');
    w.loadFile(path.join(fixtures, 'pages', 'send-after-node.html'));
  // TODO(codebytere): fix on Windows and Linux too
  ifdescribe(process.platform === 'darwin')('window.webContents initial paint', () => {
    it('paints when a window is initially hidden', async () => {
      const entries = await w.webContents.executeJavaScript(`
        new Promise((resolve) => {
          const observer = new PerformanceObserver((performance) => {
            resolve(performance.getEntries());
          observer.observe({ entryTypes: ['paint'] });
        const header = document.createElement('h1');
        header.innerText = 'Paint me!!';
        document.getElementById('div').appendChild(header);
      expect(JSON.stringify(entries)).to.eq('{}');
  describe('window.webContents.focus()', () => {
    it('focuses window', async () => {
      const w1 = new BrowserWindow({ x: 100, y: 300, width: 300, height: 200 });
      w1.loadURL('about:blank');
      const w2 = new BrowserWindow({ x: 300, y: 300, width: 300, height: 200 });
      w2.loadURL('about:blank');
      const w1Focused = once(w1, 'focus');
      w1.webContents.focus();
      await w1Focused;
      expect(w1.webContents.isFocused()).to.be.true('focuses window');
  describe('offscreen rendering', () => {
    it('creates offscreen window with correct size', async () => {
      const paint = once(w.webContents, 'paint') as Promise<[any, Electron.Rectangle, Electron.NativeImage]>;
      w.loadFile(path.join(fixtures, 'api', 'offscreen-rendering.html'));
      const [, , data] = await paint;
      const size = data.getSize();
      const scaleFactor = 1;
      expect(size.width).to.be.closeTo(100 * scaleFactor, 2);
      expect(size.height).to.be.closeTo(100 * scaleFactor, 2);
    it('does not crash after navigation', () => {
    describe('window.webContents.isOffscreen()', () => {
      it('is true for offscreen type', () => {
        expect(w.webContents.isOffscreen()).to.be.true('isOffscreen');
      it('is false for regular window', () => {
        expect(c.webContents.isOffscreen()).to.be.false('isOffscreen');
    describe('window.webContents.isPainting()', () => {
      it('returns whether is currently painting', async () => {
        await paint;
        expect(w.webContents.isPainting()).to.be.true('isPainting');
    describe('window.webContents.stopPainting()', () => {
      it('stops painting', async () => {
        const domReady = once(w.webContents, 'dom-ready');
        await domReady;
        w.webContents.stopPainting();
        expect(w.webContents.isPainting()).to.be.false('isPainting');
    describe('window.webContents.startPainting()', () => {
      it('starts painting', async () => {
        w.webContents.startPainting();
        await once(w.webContents, 'paint') as [any, Electron.Rectangle, Electron.NativeImage];
    describe('frameRate APIs', () => {
      it('has default frame rate (function)', async () => {
        expect(w.webContents.getFrameRate()).to.equal(60);
      it('has default frame rate (property)', async () => {
        expect(w.webContents.frameRate).to.equal(60);
      it('sets custom frame rate (function)', async () => {
        w.webContents.setFrameRate(30);
        expect(w.webContents.getFrameRate()).to.equal(30);
      it('sets custom frame rate (property)', async () => {
        w.webContents.frameRate = 30;
        expect(w.webContents.frameRate).to.equal(30);
    describe('shared texture', () => {
      it('does not crash when release() is called after the texture is garbage collected', async () => {
        const sw = new BrowserWindow({
            offscreen: {
              useSharedTexture: true
        const paint = once(sw.webContents, 'paint') as Promise<[any, Electron.Rectangle, Electron.NativeImage]>;
        sw.loadFile(path.join(fixtures, 'api', 'offscreen-rendering.html'));
        const [event] = await paint;
        sw.webContents.stopPainting();
        if (!event.texture) {
          // GPU shared texture not available on this host; skip.
          sw.destroy();
        // Keep only the release closure and drop the owning texture object.
        const staleRelease = event.texture.release;
        const weakTexture = new WeakRef(event.texture);
        event.texture = undefined;
        // Force GC until the texture object is collected.
        let collected = false;
        for (let i = 0; i < 30 && !collected; ++i) {
          collected = weakTexture.deref() === undefined;
        expect(collected).to.be.true('texture should be garbage collected');
        // This should return safely and not crash the main process.
        expect(() => staleRelease()).to.not.throw();
  describe('offscreen rendering with device scale factor', () => {
    const scaleFactor = 1.5;
            deviceScaleFactor: scaleFactor
    it('creates offscreen window with correct size considering device scale factor', async () => {
    it('has correct screen and window sizes', async () => {
      await once(w.webContents, 'dom-ready');
      const sizes = await w.webContents.executeJavaScript(`
          const screenSize = [screen.width, screen.height];
          const outerSize = [window.outerWidth, window.outerHeight];
          const dpr = window.devicePixelRatio;
          resolve({ screenSize, outerSize, dpr });
      expect(sizes.screenSize).to.deep.equal([100, 100]);
      expect(sizes.outerSize).to.deep.equal([100, 100]);
      expect(sizes.dpr).to.be.equal(scaleFactor);
    it('has correct device screen size media query result', async () => {
      const query = `(device-width: ${100}px)`;
      const matches = await w.webContents.executeJavaScript(`
          const mediaQuery = window.matchMedia('${query}');
          resolve(mediaQuery.matches);
      expect(matches).to.be.true();
  describe('"transparent" option', () => {
    ifit(process.platform !== 'linux')('correctly returns isMaximized() when the window is maximized then minimized', async () => {
      expect(w.isMaximized()).to.be.false();
    // Only applicable on Windows where transparent windows can't be maximized.
    ifit(process.platform === 'win32')('can show maximized frameless window', async () => {
        ...display.bounds,
      expect(w.isMaximized()).to.be.true();
      // Fails when the transparent HWND is in an invalid maximized state.
      expect(w.getBounds()).to.deep.equal(display.workArea);
      const newBounds = { width: 256, height: 256, x: 0, y: 0 };
      w.setBounds(newBounds);
      expect(w.getBounds()).to.deep.equal(newBounds);
    // FIXME(codebytere): figure out why these are failing on MAS arm64.
    ifit(hasCapturableScreen() && !(process.mas && process.arch === 'arm64'))('should not display a visible background', async () => {
      const backgroundWindow = new BrowserWindow({
        backgroundColor: HexColors.GREEN,
        hasShadow: false
      await backgroundWindow.loadURL('data:text/html,<html></html>');
      const foregroundWindow = new BrowserWindow({
      const colorFile = path.join(__dirname, 'fixtures', 'pages', 'half-background-color.html');
      await foregroundWindow.loadFile(colorFile);
      await screenCapture.expectColorAtPointOnDisplayMatches(
        HexColors.GREEN,
        (size) => ({
          x: size.width / 4,
          y: size.height / 2
        HexColors.RED,
          x: size.width * 3 / 4,
    ifit(hasCapturableScreen() && !(process.mas && process.arch === 'arm64'))('Allows setting a transparent window via CSS', async () => {
        backgroundColor: HexColors.PURPLE,
        hasShadow: false,
          nodeIntegration: true
      foregroundWindow.loadFile(path.join(__dirname, 'fixtures', 'pages', 'css-transparent.html'));
      await once(ipcMain, 'set-transparent');
      await screenCapture.expectColorAtCenterMatches(HexColors.PURPLE);
    ifit(hasCapturableScreen())('should not make background transparent if falsy', async () => {
      for (const transparent of [false, undefined]) {
          transparent
        await once(window, 'show');
        await window.webContents.loadURL('data:text/html,<head><meta name="color-scheme" content="dark"></head>');
        // color-scheme is set to dark so background should not be white
        await screenCapture.expectColorAtCenterDoesNotMatch(HexColors.WHITE);
  describe('"backgroundColor" option', () => {
    ifit(hasCapturableScreen())('should display the set color', async () => {
        backgroundColor: HexColors.BLUE
      w.loadURL('data:text/html,<html></html>');
      await screenCapture.expectColorAtCenterMatches(HexColors.BLUE);
