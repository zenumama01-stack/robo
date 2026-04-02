import { BrowserWindow, ipcMain, webContents, session, app, BrowserView, WebContents, BaseWindow, WebContentsView } from 'electron/main';
import { ifdescribe, defer, waitUntil, listen, ifit } from './lib/spec-helpers';
import { cleanupWebContents, closeAllWindows } from './lib/window-helpers';
const features = process._linkedBinding('electron_common_features');
describe('webContents module', () => {
  describe('getAllWebContents() API', () => {
    it('returns an array of web contents', async () => {
        webPreferences: { webviewTag: true }
      w.loadFile(path.join(fixturesPath, 'pages', 'webview-zoom-factor.html'));
      await once(w.webContents, 'did-attach-webview') as [any, WebContents];
      await once(w.webContents, 'devtools-opened');
      const all = webContents.getAllWebContents().sort((a, b) => {
        return a.id - b.id;
      expect(all).to.have.length(3);
      expect(all[0].getType()).to.equal('window');
      expect(all[all.length - 2].getType()).to.equal('webview');
      expect(all[all.length - 1].getType()).to.equal('remote');
  describe('webContents properties', () => {
    it('has expected additional enumerable properties', () => {
      const properties = Object.getOwnPropertyNames(w.webContents);
      expect(properties).to.include('ipc');
      expect(properties).to.include('navigationHistory');
  describe('fromId()', () => {
    it('returns undefined for an unknown id', () => {
      expect(webContents.fromId(12345)).to.be.undefined();
  describe('fromFrame()', () => {
    afterEach(cleanupWebContents);
    it('returns WebContents for mainFrame', () => {
      expect(webContents.fromFrame(contents.mainFrame)).to.equal(contents);
    it('returns undefined for disposed frame', async () => {
      const { mainFrame } = contents;
      await waitUntil(() => typeof webContents.fromFrame(mainFrame) === 'undefined');
    it('throws when passing invalid argument', async () => {
      let errored = false;
        webContents.fromFrame({} as any);
        errored = true;
      expect(errored).to.be.true();
  describe('fromDevToolsTargetId()', () => {
    it('returns WebContents for attached DevTools target', async () => {
        await w.webContents.debugger.attach('1.3');
        const { targetInfo } = await w.webContents.debugger.sendCommand('Target.getTargetInfo');
        expect(webContents.fromDevToolsTargetId(targetInfo.targetId)).to.equal(w.webContents);
        await w.webContents.debugger.detach();
      expect(webContents.fromDevToolsTargetId('nope')).to.be.undefined();
  describe('will-prevent-unload event', function () {
      await cleanupWebContents();
    it('does not emit if beforeunload returns undefined in a BrowserWindow', async () => {
      w.webContents.once('will-prevent-unload', () => {
        expect.fail('should not have fired');
    it('does not emit if beforeunload returns undefined in a BrowserView', async () => {
      view.setBounds(w.getBounds());
      view.webContents.once('will-prevent-unload', () => {
      await view.webContents.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-undefined.html'));
    it('emits if beforeunload returns false in a BrowserWindow', async () => {
      await once(w.webContents, 'will-prevent-unload');
    it('emits if beforeunload returns false in a BrowserView', async () => {
      await view.webContents.loadFile(path.join(__dirname, 'fixtures', 'api', 'beforeunload-false.html'));
      await once(view.webContents, 'will-prevent-unload');
    it('supports calling preventDefault on will-prevent-unload events in a BrowserWindow', async () => {
      w.webContents.once('will-prevent-unload', event => event.preventDefault());
    it('fails loading a subsequent page after beforeunload is not prevented', async () => {
      await w.webContents.executeJavaScript('console.log(\'gesture\')', true);
      w.loadFile(path.join(__dirname, 'fixtures', 'pages', 'a.html'));
      const [, code, , validatedURL] = await didFailLoad;
      expect(code).to.equal(-3); // ERR_ABORTED
      const { href: expectedURL } = url.pathToFileURL(path.join(__dirname, 'fixtures', 'pages', 'a.html'));
      expect(validatedURL).to.equal(expectedURL);
    it('allows loading a subsequent page after beforeunload is prevented', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'pages', 'a.html'));
      const pageTitle = await w.webContents.executeJavaScript('document.title');
      expect(pageTitle).to.equal('test');
  describe('webContents.send(channel, args...)', () => {
    it('throws an error when the channel is missing', () => {
        (w.webContents.send as any)();
      }).to.throw('Missing required channel argument');
        w.webContents.send(null as any);
    it('does not block node async APIs when sent before document is ready', (done) => {
      // Please reference https://github.com/electron/electron/issues/19368 if
      // this test fails.
      ipcMain.once('async-node-api-done', () => {
      w.loadFile(path.join(fixturesPath, 'pages', 'send-after-node.html'));
      setTimeout(50).then(() => {
        w.webContents.send('test');
  ifdescribe(features.isPrintingEnabled())('webContents.print()', () => {
    it('does not throw when options are not passed', () => {
        w.webContents.print();
        w.webContents.print(undefined);
    it('does not throw when options object is empty', () => {
        w.webContents.print({});
    it('throws when invalid settings are passed', () => {
        w.webContents.print(true);
      }).to.throw('webContents.print(): Invalid print settings specified.');
        w.webContents.print(null);
    it('throws when an invalid pageSize is passed', () => {
      const badSize = 5;
        w.webContents.print({ pageSize: badSize });
      }).to.throw(`Unsupported pageSize: ${badSize}`);
    it('throws when a user passes both pageSize and usePrinterDefaultPageSize', () => {
        w.webContents.print({ pageSize: 'A4', usePrinterDefaultPageSize: true });
      }).to.throw('usePrinterDefaultPageSize cannot be combined with pageSize');
    it('throws when an invalid callback is passed', () => {
        w.webContents.print({}, true);
      }).to.throw('webContents.print(): Invalid optional callback provided.');
    it('fails when an invalid deviceName is passed', (done) => {
      w.webContents.print({ deviceName: 'i-am-a-nonexistent-printer' }, (success, reason) => {
        expect(success).to.equal(false);
        expect(reason).to.match(/Invalid deviceName provided/);
        w.webContents.print({ pageSize: 'i-am-a-bad-pagesize' }, () => {});
      }).to.throw('Unsupported pageSize: i-am-a-bad-pagesize');
    it('throws when an invalid custom pageSize is passed', () => {
        w.webContents.print({
          pageSize: {
      }).to.throw('height and width properties must be minimum 352 microns.');
    it('does not crash with custom margins', () => {
          margins: {
            marginType: 'custom',
            top: 1,
            bottom: 1,
            left: 1,
            right: 1
  describe('webContents.executeJavaScript', () => {
    describe('in about:blank', () => {
      const expected = 'hello, world!';
      const expectedErrorMsg = 'woops!';
      const code = `(() => "${expected}")()`;
      const asyncCode = `(() => new Promise(r => setTimeout(() => r("${expected}"), 500)))()`;
      const badAsyncCode = `(() => new Promise((r, e) => setTimeout(() => e("${expectedErrorMsg}"), 500)))()`;
      const errorTypes = new Set([
        ReferenceError,
        EvalError,
        RangeError,
        SyntaxError,
        TypeError,
        URIError
        w = new BrowserWindow({ show: false, webPreferences: { contextIsolation: false } });
      it('resolves the returned promise with the result', async () => {
        const result = await w.webContents.executeJavaScript(code);
        expect(result).to.equal(expected);
      it('resolves the returned promise with the result if the code returns an asynchronous promise', async () => {
        const result = await w.webContents.executeJavaScript(asyncCode);
      it('rejects the returned promise if an async error is thrown', async () => {
        await expect(w.webContents.executeJavaScript(badAsyncCode)).to.eventually.be.rejectedWith(expectedErrorMsg);
      it('rejects the returned promise with an error if an Error.prototype is thrown', async () => {
        for (const error of errorTypes) {
          await expect(w.webContents.executeJavaScript(`Promise.reject(new ${error.name}("Wamp-wamp"))`))
            .to.eventually.be.rejectedWith(error);
    describe('on a real page', () => {
      it('works after page load and during subframe load', async () => {
        // initiate a sub-frame load, then try and execute script during it
          iframe.src = '${serverUrl}/slow'
          null // don't return the iframe
        await w.webContents.executeJavaScript('console.log(\'hello\')');
      it('executes after page load', async () => {
        const executeJavaScript = w.webContents.executeJavaScript('(() => "test")()');
        const result = await executeJavaScript;
        expect(result).to.equal('test');
  describe('webContents.executeJavaScriptInIsolatedWorld', () => {
      w = new BrowserWindow({ show: false, webPreferences: { contextIsolation: true } });
    after(() => w.close());
      await w.webContents.executeJavaScriptInIsolatedWorld(999, [{ code: 'window.X = 123' }]);
      const isolatedResult = await w.webContents.executeJavaScriptInIsolatedWorld(999, [{ code: 'window.X' }]);
      const mainWorldResult = await w.webContents.executeJavaScript('window.X');
      expect(isolatedResult).to.equal(123);
      expect(mainWorldResult).to.equal(undefined);
  describe('loadURL() promise API', () => {
    let s: http.Server;
      session.fromPartition('loadurl-webcontents-spec').setPermissionRequestHandler((webContents, permission, callback) => {
        if (permission === 'openExternal') {
          return callback(false);
      if (s) {
        s.close();
        s = null as unknown as http.Server;
          partition: 'loadurl-webcontents-spec'
      session.fromPartition('loadurl-webcontents-spec').setPermissionRequestHandler(null);
    it('resolves when done loading', async () => {
    it('resolves when done loading a file URL', async () => {
      await expect(w.loadFile(path.join(fixturesPath, 'pages', 'base-page.html'))).to.eventually.be.fulfilled();
    it('resolves when navigating within the page', async () => {
      await w.loadFile(path.join(fixturesPath, 'pages', 'base-page.html'));
      await expect(w.loadURL(w.getURL() + '#foo')).to.eventually.be.fulfilled();
    it('resolves after browser initiated navigation', async () => {
      let finishedLoading = false;
      w.webContents.on('did-finish-load', function () {
        finishedLoading = true;
      await w.loadFile(path.join(fixturesPath, 'pages', 'navigate_in_page_and_wait.html'));
      expect(finishedLoading).to.be.true();
    it('rejects when failing to load a file URL', async () => {
      await expect(w.loadURL('file:non-existent')).to.eventually.be.rejected()
        .and.have.property('code', 'ERR_FILE_NOT_FOUND');
    // FIXME: Temporarily disable on WOA until
    // https://github.com/electron/electron/issues/20008 is resolved
    ifit(!(process.platform === 'win32' && process.arch === 'arm64'))('rejects when loading fails due to DNS not resolved', async () => {
      await expect(w.loadURL('https://err.name.not.resolved')).to.eventually.be.rejected()
        .and.have.property('code', 'ERR_NAME_NOT_RESOLVED');
    it('rejects when navigation is cancelled due to a bad scheme', async () => {
      await expect(w.loadURL('bad-scheme://foo')).to.eventually.be.rejected()
        .and.have.property('code', 'ERR_FAILED');
    it('does not crash when loading a new URL with emulation settings set', async () => {
      const setEmulation = async () => {
        if (w.webContents) {
          w.webContents.debugger.attach('1.3');
          const deviceMetrics = {
            deviceScaleFactor: 2,
            mobile: true,
            dontSetVisibleSize: true
          await w.webContents.debugger.sendCommand(
            'Emulation.setDeviceMetricsOverride',
            deviceMetrics
        await setEmulation();
        await w.loadURL('data:text/html,<h1>HELLO</h1>');
        expect((e as Error).message).to.match(/Debugger is already attached to the target/);
    it('fails if loadURL is called inside did-start-loading', (done) => {
      w.webContents.once('did-fail-load', (_event, _errorCode, _errorDescription, validatedURL) => {
        expect(validatedURL).to.contain('blank.html');
      w.webContents.once('did-start-loading', () => {
        w.loadURL(`file://${fixturesPath}/pages/blank.html`);
      w.loadURL('data:text/html,<h1>HELLO</h1>');
    it('fails if loadurl is called after the navigation is ready to commit', () => {
      // @ts-expect-error internal-only event.
      w.webContents.once('-ready-to-commit-navigation', () => {
    it('fails if loadURL is called inside did-redirect-navigation', (done) => {
        } else if (req.url === '/200') {
          res.end('ok');
      listen(server).then(({ url }) => {
        w.webContents.once('did-redirect-navigation', () => {
    it('sets appropriate error information on rejection', async () => {
      let err: any;
        await w.loadURL('file:non-existent');
        err = e;
      expect(err).not.to.be.null();
      expect(err.code).to.eql('ERR_FILE_NOT_FOUND');
      expect(err.errno).to.eql(-6);
      expect(err.url).to.eql(process.platform === 'win32' ? 'file://non-existent/' : 'file:///non-existent');
    it('rejects if the load is aborted', async () => {
      s = http.createServer(() => { /* never complete the request */ });
      const { port } = await listen(s);
      const p = expect(w.loadURL(`http://127.0.0.1:${port}`)).to.eventually.be.rejectedWith(Error, /ERR_ABORTED/);
      // load a different file before the first load completes, causing the
      // first load to be aborted.
    it("doesn't reject when a subframe fails to load", async () => {
      let resp = null as unknown as http.ServerResponse;
      s = http.createServer((req, res) => {
        res.writeHead(200, { 'Content-Type': 'text/html' });
        res.write('<iframe src="http://err.name.not.resolved"></iframe>');
        resp = res;
        // don't end the response yet
      const p = new Promise<void>(resolve => {
        w.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL, isMainFrame) => {
          if (!isMainFrame) {
      const main = w.loadURL(`http://127.0.0.1:${port}`);
      resp.end();
      await main;
    it("doesn't resolve when a subframe loads", async () => {
        res.write('<iframe src="about:blank"></iframe>');
        w.webContents.on('did-frame-finish-load', (event, isMainFrame) => {
      resp.destroy(); // cause the main request to fail
      await expect(main).to.eventually.be.rejected()
        .and.have.property('errno', -355); // ERR_INCOMPLETE_CHUNKED_ENCODING
    it('subsequent load failures reject each time', async () => {
      await expect(w.loadURL('file:non-existent')).to.eventually.be.rejected();
    it('invalid URL load rejects', async () => {
      await expect(w.loadURL('invalidURL')).to.eventually.be.rejected();
  describe('navigationHistory', () => {
    const urlPage1 = 'data:text/html,<html><head><script>document.title = "Page 1";</script></head><body></body></html>';
    const urlPage2 = 'data:text/html,<html><head><script>document.title = "Page 2";</script></head><body></body></html>';
    const urlPage3 = 'data:text/html,<html><head><script>document.title = "Page 3";</script></head><body></body></html>';
    describe('navigationHistory.removeEntryAtIndex(index) API', () => {
      it('should remove a navigation entry given a valid index', async () => {
        await w.loadURL(urlPage1);
        await w.loadURL(urlPage2);
        await w.loadURL(urlPage3);
        const initialLength = w.webContents.navigationHistory.length();
        const wasRemoved = w.webContents.navigationHistory.removeEntryAtIndex(1); // Attempt to remove the second entry
        const newLength = w.webContents.navigationHistory.length();
        expect(wasRemoved).to.be.true();
        expect(newLength).to.equal(initialLength - 1);
      it('should not remove the current active navigation entry', async () => {
        const activeIndex = w.webContents.navigationHistory.getActiveIndex();
        const wasRemoved = w.webContents.navigationHistory.removeEntryAtIndex(activeIndex);
        expect(wasRemoved).to.be.false();
      it('should return false given an invalid index larger than history length', async () => {
        const wasRemoved = w.webContents.navigationHistory.removeEntryAtIndex(5); // Index larger than history length
      it('should return false given an invalid negative index', async () => {
        const wasRemoved = w.webContents.navigationHistory.removeEntryAtIndex(-1); // Negative index
    describe('navigationHistory.canGoBack and navigationHistory.goBack API', () => {
      it('should not be able to go back if history is empty', async () => {
        expect(w.webContents.navigationHistory.canGoBack()).to.be.false();
      it('should be able to go back if history is not empty', async () => {
        expect(w.webContents.navigationHistory.getActiveIndex()).to.equal(1);
        expect(w.webContents.navigationHistory.canGoBack()).to.be.true();
        w.webContents.navigationHistory.goBack();
        expect(w.webContents.navigationHistory.getActiveIndex()).to.equal(0);
      it('should have the same window title if navigating back within the page', async () => {
        const title = 'Test';
          w.setTitle(title);
          w.loadURL(`file://${fixturesPath}/pages/navigation-history-anchor-in-page.html#next`);
        await w.loadURL(`file://${fixturesPath}/pages/navigation-history-anchor-in-page.html`);
        expect(w.getTitle()).to.equal(title);
    describe('navigationHistory.canGoForward and navigationHistory.goForward API', () => {
      it('should not be able to go forward if history is empty', async () => {
        expect(w.webContents.navigationHistory.canGoForward()).to.be.false();
      it('should not be able to go forward if current index is same as history length', async () => {
      it('should be able to go forward if history is not empty and active index is less than history length', async () => {
        expect(w.webContents.navigationHistory.canGoForward()).to.be.true();
        w.webContents.navigationHistory.goForward();
      it('should have the same window title if navigating forward within the page', async () => {
    describe('navigationHistory.canGoToOffset(index) and navigationHistory.goToOffset(index) API', () => {
      it('should not be able to go to invalid offset', async () => {
        expect(w.webContents.navigationHistory.canGoToOffset(-1)).to.be.false();
        expect(w.webContents.navigationHistory.canGoToOffset(10)).to.be.false();
      it('should be able to go to valid negative offset', async () => {
        expect(w.webContents.navigationHistory.canGoToOffset(-2)).to.be.true();
        expect(w.webContents.navigationHistory.getActiveIndex()).to.equal(2);
        w.webContents.navigationHistory.goToOffset(-2);
      it('should be able to go to valid positive offset', async () => {
        expect(w.webContents.navigationHistory.canGoToOffset(1)).to.be.true();
        w.webContents.navigationHistory.goToOffset(1);
    describe('navigationHistory.clear API', () => {
      it('should be able clear history', async () => {
        expect(w.webContents.navigationHistory.length()).to.equal(3);
        w.webContents.navigationHistory.clear();
        expect(w.webContents.navigationHistory.length()).to.equal(1);
    describe('navigationHistory.getEntryAtIndex(index) API ', () => {
      it('should fetch default navigation entry when no urls are loaded', async () => {
        const result = w.webContents.navigationHistory.getEntryAtIndex(0);
        expect(result.url).to.equal('');
        expect(result.title).to.equal('');
      it('should fetch navigation entry given a valid index', async () => {
        expect(result.url).to.equal(urlPage1);
        expect(result.title).to.equal('Page 1');
      it('should return null given an invalid index larger than history length', async () => {
        const result = w.webContents.navigationHistory.getEntryAtIndex(5);
        expect(result).to.be.null();
      it('should return null given an invalid negative index', async () => {
        const result = w.webContents.navigationHistory.getEntryAtIndex(-1);
    describe('navigationHistory.getActiveIndex() API', () => {
      it('should return valid active index after a single page visit', async () => {
      it('should return valid active index after a multiple page visits', async () => {
      it('should return valid active index given no page visits', async () => {
    describe('navigationHistory.length() API', () => {
      it('should return valid history length after a single page visit', async () => {
      it('should return valid history length after a multiple page visits', async () => {
      it('should return valid history length given no page visits', async () => {
        // Note: Even if no navigation has committed, the history list will always start with an initial navigation entry
        // Ref: https://source.chromium.org/chromium/chromium/src/+/main:ceontent/public/browser/navigation_controller.h;l=381
    describe('navigationHistory.getAllEntries() API', () => {
      it('should return all navigation entries as an array of NavigationEntry objects', async () => {
        const entries = w.webContents.navigationHistory.getAllEntries().map(entry => ({
          url: entry.url,
          title: entry.title
        expect(entries.length).to.equal(3);
        expect(entries[0]).to.deep.equal({ url: urlPage1, title: 'Page 1' });
        expect(entries[1]).to.deep.equal({ url: urlPage2, title: 'Page 2' });
        expect(entries[2]).to.deep.equal({ url: urlPage3, title: 'Page 3' });
      it('should return an empty array when there is no navigation history', async () => {
        const entries = w.webContents.navigationHistory.getAllEntries();
        expect(entries.length).to.equal(0);
      it('should create a NavigationEntry with PageState that can be serialized/deserialized with JSON', async () => {
        const serialized = JSON.stringify(entries);
        const deserialized = JSON.parse(serialized);
        expect(deserialized).to.deep.equal(entries);
    describe('navigationHistory.restore({ index, entries }) API', () => {
          res.end('<html><head><title>Form</title></head><body><form><input type="text" value="value" /></form></body></html>');
      it('should restore navigation history with PageState', async () => {
        // Fill out the form on the page
        await w.webContents.executeJavaScript('document.querySelector("input").value = "Hi!";');
        // PageState is committed:
        // 1) When the page receives an unload event
        // 2) During periodic serialization of page state
        // To not wait randomly for the second option, we'll trigger another load
        // Save the navigation state
        // Close the window, make a new one
        w = new BrowserWindow();
        const formValue = await new Promise<string>(resolve => {
          w.webContents.once('dom-ready', () => resolve(w.webContents.executeJavaScript('document.querySelector("input").value')));
          // Restore the navigation history
          return w.webContents.navigationHistory.restore({ index: 2, entries });
        await waitUntil(() => formValue === 'Hi!');
      it('should handle invalid base64 pageState', async () => {
        const brokenEntries = w.webContents.navigationHistory.getAllEntries().map(entry => ({
          pageState: 'invalid base64'
        await w.webContents.navigationHistory.restore({ index: 2, entries: brokenEntries });
        // Check that we used the original url and titles but threw away the broken
        // pageState
        entries.forEach((entry, index) => {
          expect(entry.url).to.equal(brokenEntries[index].url);
          expect(entry.title).to.equal(brokenEntries[index].title);
          expect(entry.pageState?.length).to.be.greaterThanOrEqual(100);
    it('should restore an overridden user agent', async () => {
      const partition = 'persist:wcvtest';
      const testUA = 'MyCustomUA';
      const ses = session.fromPartition(partition);
      ses.setUserAgent(testUA);
      const wcv = new WebContentsView({
        webPreferences: { partition }
      wcv.webContents.navigationHistory.restore({
        entries: [{
          url: urlPage1,
          title: 'url1'
      const ua = wcv.webContents.getUserAgent();
      const wcvua = await wcv.webContents.executeJavaScript('navigator.userAgent');
      expect(ua).to.equal(wcvua);
  describe('getFocusedWebContents() API', () => {
    // FIXME
    ifit(!(process.platform === 'win32' && process.arch === 'arm64'))('returns the focused web contents', async () => {
      expect(webContents.getFocusedWebContents()?.id).to.equal(w.webContents.id);
      const devToolsOpened = once(w.webContents, 'devtools-opened');
      await devToolsOpened;
      expect(webContents.getFocusedWebContents()?.id).to.equal(w.webContents.devToolsWebContents!.id);
      const devToolsClosed = once(w.webContents, 'devtools-closed');
      w.webContents.closeDevTools();
      await devToolsClosed;
    it('does not crash when called on a detached dev tools window', async () => {
      w.webContents.openDevTools({ mode: 'detach' });
      w.webContents.inspectElement(100, 100);
      // For some reason we have to wait for two focused events...?
      expect(() => { webContents.getFocusedWebContents(); }).to.not.throw();
      // Work around https://github.com/electron/electron/issues/19985
    it('Inspect activates detached devtools window', async () => {
      const window = new BrowserWindow({ show: true });
      await window.loadURL('about:blank');
      const webContentsBeforeOpenedDevtools = webContents.getAllWebContents();
      const windowWasBlurred = once(window, 'blur');
      window.webContents.openDevTools({ mode: 'detach' });
      await windowWasBlurred;
      let devToolsWebContents = null;
      for (const newWebContents of webContents.getAllWebContents()) {
        const oldWebContents = webContentsBeforeOpenedDevtools.find(
          oldWebContents => {
            return newWebContents.id === oldWebContents.id;
        if (oldWebContents !== null) {
          devToolsWebContents = newWebContents;
      assert(devToolsWebContents !== null);
      const windowFocused = once(window, 'focus');
      const devToolsBlurred = once(devToolsWebContents, 'blur');
      await Promise.all([windowFocused, devToolsBlurred]);
      expect(devToolsWebContents.isFocused()).to.be.false();
      const devToolsWebContentsFocused = once(devToolsWebContents, 'focus');
      const windowBlurred = once(window, 'blur');
      window.webContents.inspectElement(100, 100);
      await Promise.all([devToolsWebContentsFocused, windowBlurred]);
      expect(devToolsWebContents.isFocused()).to.be.true();
      expect(window.isFocused()).to.be.false();
  describe('setDevToolsWebContents() API', () => {
    it('sets arbitrary webContents as devtools', async () => {
      const devtools = new BrowserWindow({ show: false });
      const promise = once(devtools.webContents, 'dom-ready');
      w.webContents.setDevToolsWebContents(devtools.webContents);
      expect(devtools.webContents.getURL().startsWith('devtools://devtools')).to.be.true();
      const result = await devtools.webContents.executeJavaScript('InspectorFrontendHost.constructor.name');
      expect(result).to.equal('InspectorFrontendHostImpl');
      devtools.destroy();
  describe('isFocused() API', () => {
    it('returns false when the window is hidden', async () => {
      expect(w.isVisible()).to.be.false();
      expect(w.webContents.isFocused()).to.be.false();
  describe('isCurrentlyAudible() API', () => {
    it('returns whether audio is playing', async () => {
        window.context = new AudioContext
        // Start in suspended state, because of the
        // new web audio api policy.
        context.suspend()
        window.oscillator = context.createOscillator()
        oscillator.connect(context.destination)
        oscillator.start()
      let p = once(w.webContents, 'audio-state-changed');
      w.webContents.executeJavaScript('context.resume()');
      expect(w.webContents.isCurrentlyAudible()).to.be.true();
      p = once(w.webContents, 'audio-state-changed');
      w.webContents.executeJavaScript('oscillator.stop()');
      expect(w.webContents.isCurrentlyAudible()).to.be.false();
  describe('openDevTools() API', () => {
    it('can show window with activation', async () => {
      const focused = once(w, 'focus');
      await focused;
      const blurred = once(w, 'blur');
      w.webContents.openDevTools({ mode: 'detach', activate: true });
        once(w.webContents, 'devtools-opened'),
        once(w.webContents, 'devtools-focused')
      await blurred;
    it('can show window without activation', async () => {
      const devtoolsOpened = once(w.webContents, 'devtools-opened');
      w.webContents.openDevTools({ mode: 'detach', activate: false });
      await devtoolsOpened;
      expect(w.webContents.isDevToolsOpened()).to.be.true();
    it('can show a DevTools window with custom title', async () => {
      w.webContents.openDevTools({ mode: 'detach', activate: false, title: 'myTitle' });
      expect(w.webContents.getDevToolsTitle()).to.equal('myTitle');
    it('can re-open devtools', async () => {
      const devtoolsClosed = once(w.webContents, 'devtools-closed');
      await devtoolsClosed;
      expect(w.webContents.isDevToolsOpened()).to.be.false();
      const devtoolsOpened2 = once(w.webContents, 'devtools-opened');
      await devtoolsOpened2;
  describe('setDevToolsTitle() API', () => {
    it('can set devtools title with function', async () => {
      w.webContents.setDevToolsTitle('newTitle');
      expect(w.webContents.getDevToolsTitle()).to.equal('newTitle');
  describe('before-mouse-event event', () => {
    it('can prevent document mouse events', async () => {
      await w.loadFile(path.join(fixturesPath, 'pages', 'mouse-events.html'));
      const mouseDown = new Promise(resolve => {
        ipcMain.once('mousedown', (event, button) => resolve(button));
      w.webContents.once('before-mouse-event', (event, input) => {
        if (input.button === 'left') event.preventDefault();
      w.webContents.sendInputEvent({ type: 'mouseDown', button: 'left', x: 100, y: 100 });
      w.webContents.sendInputEvent({ type: 'mouseDown', button: 'right', x: 100, y: 100 });
      expect(await mouseDown).to.equal(2); // Right button is 2
    it('has the correct properties', async () => {
      const testBeforeMouse = async (opts: Electron.MouseInputEvent) => {
        const p = once(w.webContents, 'before-mouse-event');
        w.webContents.sendInputEvent({
          type: opts.type,
          button: opts.button,
          x: opts.x,
          y: opts.y,
          globalX: opts.globalX,
          globalY: opts.globalY,
          clickCount: opts.clickCount
        const [, input] = await p;
        expect(input.type).to.equal(opts.type);
        expect(input.button).to.equal(opts.button);
        expect(input.x).to.equal(opts.x);
        expect(input.y).to.equal(opts.y);
        expect(input.globalX).to.equal(opts.globalX);
        expect(input.globalY).to.equal(opts.globalY);
        expect(input.clickCount).to.equal(opts.clickCount);
      await testBeforeMouse({
        type: 'mouseDown',
        button: 'left',
        y: 100,
        globalX: 200,
        globalY: 200,
        clickCount: 1
        type: 'mouseUp',
        button: 'right',
        x: 150,
        y: 150,
        globalX: 250,
        globalY: 250,
        clickCount: 2
        type: 'mouseMove',
        button: 'middle',
        y: 200,
        globalX: 300,
        globalY: 300,
        clickCount: 0
  describe('before-input-event event', () => {
    it('can prevent document keyboard events', async () => {
      await w.loadFile(path.join(fixturesPath, 'pages', 'key-events.html'));
      const keyDown = new Promise(resolve => {
        ipcMain.once('keydown', (event, key) => resolve(key));
      w.webContents.once('before-input-event', (event, input) => {
        if (input.key === 'a') event.preventDefault();
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'a' });
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'b' });
      expect(await keyDown).to.equal('b');
      const testBeforeInput = async (opts: any) => {
        const modifiers = [];
        if (opts.shift) modifiers.push('shift');
        if (opts.control) modifiers.push('control');
        if (opts.alt) modifiers.push('alt');
        if (opts.meta) modifiers.push('meta');
        if (opts.isAutoRepeat) modifiers.push('isAutoRepeat');
        const p = once(w.webContents, 'before-input-event') as Promise<[any, Electron.Input]>;
          keyCode: opts.keyCode,
          modifiers: modifiers as any
        expect(input.key).to.equal(opts.key);
        expect(input.code).to.equal(opts.code);
        expect(input.isAutoRepeat).to.equal(opts.isAutoRepeat);
        expect(input.shift).to.equal(opts.shift);
        expect(input.control).to.equal(opts.control);
        expect(input.alt).to.equal(opts.alt);
        expect(input.meta).to.equal(opts.meta);
      await testBeforeInput({
        type: 'keyDown',
        key: 'A',
        code: 'KeyA',
        keyCode: 'a',
        shift: true,
        control: true,
        alt: true,
        meta: true,
        isAutoRepeat: true
        type: 'keyUp',
        key: '.',
        code: 'Period',
        keyCode: '.',
        shift: false,
        meta: false,
        isAutoRepeat: false
        key: '!',
        code: 'Digit1',
        keyCode: '1',
        control: false,
        alt: false,
        key: 'Tab',
        code: 'Tab',
        keyCode: 'Tab',
  // On Mac, zooming isn't done with the mouse wheel.
  ifdescribe(process.platform !== 'darwin')('zoom-changed', () => {
    it('is emitted with the correct zoom-in info', async () => {
      const testZoomChanged = async () => {
          type: 'mouseWheel',
          x: 300,
          y: 300,
          deltaX: 0,
          deltaY: 1,
          wheelTicksX: 0,
          wheelTicksY: 1,
          modifiers: ['control', 'meta']
        const [, zoomDirection] = await once(w.webContents, 'zoom-changed') as [any, string];
        expect(zoomDirection).to.equal('in');
      await testZoomChanged();
    it('is emitted with the correct zoom-out info', async () => {
          deltaY: -1,
          wheelTicksY: -1,
        expect(zoomDirection).to.equal('out');
  describe('sendInputEvent(event)', () => {
    it('can send keydown events', async () => {
      const keydown = once(ipcMain, 'keydown');
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'A' });
      const [, key, code, keyCode, shiftKey, ctrlKey, altKey] = await keydown;
      expect(key).to.equal('a');
      expect(code).to.equal('KeyA');
      expect(keyCode).to.equal(65);
      expect(shiftKey).to.be.false();
      expect(ctrlKey).to.be.false();
      expect(altKey).to.be.false();
    it('can send keydown events with modifiers', async () => {
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Z', modifiers: ['shift', 'ctrl'] });
      expect(key).to.equal('Z');
      expect(code).to.equal('KeyZ');
      expect(keyCode).to.equal(90);
      expect(shiftKey).to.be.true();
      expect(ctrlKey).to.be.true();
    it('can send keydown events with special keys', async () => {
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Tab', modifiers: ['alt'] });
      expect(key).to.equal('Tab');
      expect(code).to.equal('Tab');
      expect(keyCode).to.equal(9);
      expect(altKey).to.be.true();
    it('can send char events', async () => {
      const keypress = once(ipcMain, 'keypress');
      w.webContents.sendInputEvent({ type: 'char', keyCode: 'A' });
      const [, key, code, keyCode, shiftKey, ctrlKey, altKey] = await keypress;
    it('can correctly convert accelerators to key codes', async () => {
      const keyup = once(ipcMain, 'keyup');
      w.webContents.sendInputEvent({ keyCode: 'Plus', type: 'char' });
      w.webContents.sendInputEvent({ keyCode: 'Space', type: 'char' });
      w.webContents.sendInputEvent({ keyCode: 'Plus', type: 'keyUp' });
      await keyup;
      const inputText = await w.webContents.executeJavaScript('document.getElementById("input").value');
      expect(inputText).to.equal('+ + +');
    it('can send char events with modifiers', async () => {
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Z' });
      w.webContents.sendInputEvent({ type: 'char', keyCode: 'Z', modifiers: ['shift', 'ctrl'] });
  describe('insertCSS', () => {
    it('supports inserting CSS', async () => {
      await w.webContents.insertCSS('body { background-repeat: round; }');
      const result = await w.webContents.executeJavaScript('window.getComputedStyle(document.body).getPropertyValue("background-repeat")');
      expect(result).to.equal('round');
    it('supports removing inserted CSS', async () => {
      const key = await w.webContents.insertCSS('body { background-repeat: round; }');
      await w.webContents.removeInsertedCSS(key);
      expect(result).to.equal('repeat');
  describe('inspectElement()', () => {
    it('supports inspecting an element in the devtools', async () => {
      const event = once(w.webContents, 'devtools-opened');
      w.webContents.inspectElement(10, 10);
  describe('startDrag({file, icon})', () => {
    it('throws errors for a missing file or a missing/empty icon', () => {
        w.webContents.startDrag({ icon: path.join(fixturesPath, 'assets', 'logo.png') } as any);
      }).to.throw('Must specify either \'file\' or \'files\' option');
        w.webContents.startDrag({ file: __filename } as any);
      }).to.throw('\'icon\' parameter is required');
        w.webContents.startDrag({ file: __filename, icon: path.join(fixturesPath, 'blank.png') });
  describe('focus APIs', () => {
    describe('focus()', () => {
      it('does not blur the focused window when the web contents is hidden', async () => {
        const child = new BrowserWindow({ show: false });
        child.loadURL('about:blank');
        child.webContents.focus();
        const currentFocused = w.isFocused();
        const childFocused = child.isFocused();
        expect(currentFocused).to.be.true();
        expect(childFocused).to.be.false();
      it('does not crash when focusing a WebView webContents', async () => {
        await w.loadURL('data:text/html,<webview src="data:text/html,hi"></webview>');
        const wc = webContents.getAllWebContents().find((wc) => wc.getType() === 'webview')!;
        expect(() => wc.focus()).to.not.throw();
    const moveFocusToDevTools = async (win: BrowserWindow) => {
      const devToolsOpened = once(win.webContents, 'devtools-opened');
      win.webContents.openDevTools({ mode: 'right' });
      win.webContents.devToolsWebContents!.focus();
    describe('focus event', () => {
      it('is triggered when web contents is focused', async () => {
        await moveFocusToDevTools(w);
        const focusPromise = once(w.webContents, 'focus');
        w.webContents.focus();
        await expect(focusPromise).to.eventually.be.fulfilled();
    describe('blur event', () => {
      it('is triggered when web contents is blurred', async () => {
        const blurPromise = once(w.webContents, 'blur');
        await expect(blurPromise).to.eventually.be.fulfilled();
    describe('focusOnNavigation webPreference', () => {
      it('focuses the webContents on navigation by default', async () => {
        await w.loadURL('data:text/html,<body>test</body>');
        expect(w.webContents.isFocused()).to.be.true();
      it('does not focus the webContents on navigation when focusOnNavigation is false', async () => {
            focusOnNavigation: false
  describe('getOSProcessId()', () => {
    it('returns a valid process id', async () => {
      expect(w.webContents.getOSProcessId()).to.equal(0);
      expect(w.webContents.getOSProcessId()).to.be.above(0);
  describe('getMediaSourceId()', () => {
    it('returns a valid stream id', () => {
      expect(w.webContents.getMediaSourceId(w.webContents)).to.be.a('string').that.is.not.empty();
  describe('getOrCreateDevToolsTargetId()', () => {
    it('returns the devtools target id', async () => {
      const devToolsId = w.webContents.getOrCreateDevToolsTargetId();
      expect(devToolsId).to.be.a('string').that.is.not.empty();
      // Verify it's the inverse of fromDevToolsTargetId
      expect(webContents.fromDevToolsTargetId(devToolsId)).to.equal(w.webContents);
  describe('userAgent APIs', () => {
    it('is not empty by default', () => {
      const userAgent = w.webContents.getUserAgent();
      expect(userAgent).to.be.a('string').that.is.not.empty();
    it('can set the user agent (functions)', () => {
      w.webContents.setUserAgent('my-user-agent');
      expect(w.webContents.getUserAgent()).to.equal('my-user-agent');
      w.webContents.setUserAgent(userAgent);
      expect(w.webContents.getUserAgent()).to.equal(userAgent);
    it('can set the user agent (properties)', () => {
      const userAgent = w.webContents.userAgent;
      w.webContents.userAgent = 'my-user-agent';
      expect(w.webContents.userAgent).to.equal('my-user-agent');
      w.webContents.userAgent = userAgent;
      expect(w.webContents.userAgent).to.equal(userAgent);
  describe('audioMuted APIs', () => {
    it('can set the audio mute level (functions)', () => {
      w.webContents.setAudioMuted(true);
      expect(w.webContents.isAudioMuted()).to.be.true();
      w.webContents.setAudioMuted(false);
      expect(w.webContents.isAudioMuted()).to.be.false();
      w.webContents.audioMuted = true;
      expect(w.webContents.audioMuted).to.be.true();
      w.webContents.audioMuted = false;
      expect(w.webContents.audioMuted).to.be.false();
  describe('zoom api', () => {
    const hostZoomMap: Record<string, number> = {
      host1: 0.3,
      host2: 0.7,
      host3: 0.2
      protocol.registerStringProtocol(standardScheme, (request, callback) => {
        const response = `<script>
                            ipcRenderer.send('set-zoom', window.location.hostname)
                            ipcRenderer.on(window.location.hostname + '-zoom-set', () => {
                              ipcRenderer.send(window.location.hostname + '-zoom-level')
                          </script>`;
        callback({ data: response, mimeType: 'text/html' });
      protocol.unregisterProtocol(standardScheme);
    it('throws on an invalid zoomFactor', async () => {
        w.webContents.setZoomFactor(0.0);
      }).to.throw(/'zoomFactor' must be a double greater than 0.0/);
        w.webContents.setZoomFactor(-2.0);
    it('can set the correct zoom level (functions)', async () => {
        const zoomLevel = w.webContents.getZoomLevel();
        expect(zoomLevel).to.eql(0.0);
        w.webContents.setZoomLevel(0.5);
        const newZoomLevel = w.webContents.getZoomLevel();
        expect(newZoomLevel).to.eql(0.5);
        w.webContents.setZoomLevel(0);
    it('can set the correct zoom level (properties)', async () => {
        const zoomLevel = w.webContents.zoomLevel;
        w.webContents.zoomLevel = 0.5;
        const newZoomLevel = w.webContents.zoomLevel;
        w.webContents.zoomLevel = 0;
    it('can set the correct zoom factor (functions)', async () => {
        const zoomFactor = w.webContents.getZoomFactor();
        expect(zoomFactor).to.eql(1.0);
        w.webContents.setZoomFactor(0.5);
        const newZoomFactor = w.webContents.getZoomFactor();
        expect(newZoomFactor).to.eql(0.5);
        w.webContents.setZoomFactor(1.0);
    it('can set the correct zoom factor (properties)', async () => {
        const zoomFactor = w.webContents.zoomFactor;
        w.webContents.zoomFactor = 0.5;
        const newZoomFactor = w.webContents.zoomFactor;
        w.webContents.zoomFactor = 1.0;
    it('can persist zoom level across navigation', (done) => {
      let finalNavigation = false;
      ipcMain.on('set-zoom', (e, host) => {
        const zoomLevel = hostZoomMap[host];
        if (!finalNavigation) w.webContents.zoomLevel = zoomLevel;
        e.sender.send(`${host}-zoom-set`);
      ipcMain.on('host1-zoom-level', (e) => {
          const zoomLevel = e.sender.getZoomLevel();
          const expectedZoomLevel = hostZoomMap.host1;
          expect(zoomLevel).to.equal(expectedZoomLevel);
          if (finalNavigation) {
            w.loadURL(`${standardScheme}://host2`);
      ipcMain.once('host2-zoom-level', (e) => {
          const expectedZoomLevel = hostZoomMap.host2;
          finalNavigation = true;
          w.webContents.goBack();
      w.loadURL(`${standardScheme}://host1`);
    it('can propagate zoom level across same session', async () => {
        w2.setClosable(true);
      await w.loadURL(`${standardScheme}://host3`);
      w.webContents.zoomLevel = hostZoomMap.host3;
      await w2.loadURL(`${standardScheme}://host3`);
      const zoomLevel1 = w.webContents.zoomLevel;
      expect(zoomLevel1).to.equal(hostZoomMap.host3);
      const zoomLevel2 = w2.webContents.zoomLevel;
      expect(zoomLevel1).to.equal(zoomLevel2);
    it('cannot propagate zoom level across different session', async () => {
          partition: 'temp'
      const protocol = w2.webContents.session.protocol;
        callback('hello');
      expect(zoomLevel2).to.equal(0);
      expect(zoomLevel1).to.not.equal(zoomLevel2);
    it('can persist when it contains iframe', (done) => {
        setTimeout(200).then(() => {
        const content = `<iframe src=${url}></iframe>`;
        w.webContents.on('did-frame-finish-load', (e, isMainFrame) => {
              expect(zoomLevel).to.equal(2.0);
          w.webContents.zoomLevel = 2.0;
        w.loadURL(`data:text/html,${content}`);
    it('cannot propagate when used with webframe', async () => {
      const temporaryZoomSet = once(ipcMain, 'temporary-zoom-set');
      w.loadFile(path.join(fixturesPath, 'pages', 'webframe-zoom.html'));
      await temporaryZoomSet;
      const finalZoomLevel = w.webContents.getZoomLevel();
      await w2.loadFile(path.join(fixturesPath, 'pages', 'c.html'));
      expect(zoomLevel1).to.equal(finalZoomLevel);
    describe('with unique domains', () => {
          setTimeout().then(() => res.end('hey'));
      it('cannot persist zoom level after navigation with webFrame', async () => {
          const {ipcRenderer, webFrame} = require('electron')
          webFrame.setZoomLevel(0.6)
          ipcRenderer.send('zoom-level-set', webFrame.getZoomLevel())
        const zoomLevelPromise = once(ipcMain, 'zoom-level-set');
        await w.webContents.executeJavaScript(source);
        let [, zoomLevel] = await zoomLevelPromise;
        expect(zoomLevel).to.equal(0.6);
        const loadPromise = once(w.webContents, 'did-finish-load');
        await w.loadURL(crossSiteUrl);
        zoomLevel = w.webContents.zoomLevel;
        expect(zoomLevel).to.equal(0);
  describe('webrtc ip policy api', () => {
    it('can set and get webrtc ip policies', () => {
      const policies = [
        'default_public_interface_only',
        'default_public_and_private_interfaces',
        'disable_non_proxied_udp'
      for (const policy of policies) {
        w.webContents.setWebRTCIPHandlingPolicy(policy);
        expect(w.webContents.getWebRTCIPHandlingPolicy()).to.equal(policy);
  describe('webrtc udp port range policy api', () => {
    it('check default webrtc udp port range is { min: 0, max: 0 }', () => {
      const settings = w.webContents.getWebRTCUDPPortRange();
      expect(settings).to.deep.equal({ min: 0, max: 0 });
    it('can set and get webrtc udp port range policy with correct arguments', () => {
      w.webContents.setWebRTCUDPPortRange({ min: 1, max: 65535 });
      expect(settings).to.deep.equal({ min: 1, max: 65535 });
    it('can not set webrtc udp port range policy with invalid arguments', () => {
        w.webContents.setWebRTCUDPPortRange({ min: 0, max: 65535 });
      }).to.throw("'min' and 'max' must be in the (0, 65535] range or [0, 0]");
        w.webContents.setWebRTCUDPPortRange({ min: 1, max: 65536 });
        w.webContents.setWebRTCUDPPortRange({ min: 60000, max: 56789 });
      }).to.throw("'max' must be greater than or equal to 'min'");
    it('can reset webrtc udp port range policy to default with { min: 0, max: 0 }', () => {
      w.webContents.setWebRTCUDPPortRange({ min: 0, max: 0 });
      const defaultSetting = w.webContents.getWebRTCUDPPortRange();
      expect(defaultSetting).to.deep.equal({ min: 0, max: 0 });
  describe('opener api', () => {
    it('can get opener with window.open()', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { sandbox: true } });
      const childPromise = once(w.webContents, 'did-create-window') as Promise<[BrowserWindow, Electron.DidCreateWindowDetails]>;
      w.webContents.executeJavaScript('window.open("about:blank")', true);
      const [childWindow] = await childPromise;
      expect(childWindow.webContents.opener).to.equal(w.webContents.mainFrame);
    it('has no opener when using "noopener"', async () => {
      w.webContents.executeJavaScript('window.open("about:blank", undefined, "noopener")', true);
      expect(childWindow.webContents.opener).to.be.null();
    it('can get opener with a[target=_blank][rel=opener]', async () => {
      w.webContents.executeJavaScript(`(function() {
        a.target = '_blank';
        a.rel = 'opener';
        a.href = 'about:blank';
      }())`, true);
    it('has no opener with a[target=_blank][rel=noopener]', async () => {
        a.rel = 'noopener';
  describe('focusedFrame api', () => {
    const focusFrame = (frame: Electron.WebFrameMain) => {
      // There has to be a better way to do this...
      return frame.executeJavaScript(`(${() => {
        document.body.appendChild(input);
        input.onfocus = () => input.remove();
      }})()`, true);
    it('is null before a url is committed', () => {
      expect(w.webContents.focusedFrame).to.be.null();
    it('is set when main frame is focused', async () => {
      await waitUntil(() => w.webContents.focusedFrame === w.webContents.mainFrame);
    it('is set to child frame when focused', async () => {
      await w.loadFile(path.join(fixturesPath, 'sub-frames', 'frame-with-frame-container.html'));
      const childFrame = w.webContents.mainFrame.frames[0];
      await focusFrame(childFrame);
      await waitUntil(() => w.webContents.focusedFrame === childFrame);
  describe('render view deleted events', () => {
        const respond = () => {
          if (req.url === '/redirect-cross-site') {
            res.setHeader('Location', `${crossSiteUrl}/redirected`);
          } else if (req.url === '/redirected') {
            res.end('<html><script>window.localStorage</script></html>');
          } else if (req.url === '/first-window-open') {
            res.end(`<html><script>window.open('${serverUrl}/second-window-open', 'first child');</script></html>`);
          } else if (req.url === '/second-window-open') {
            res.end('<html><script>window.open(\'wrong://url\', \'second child\');</script></html>');
        setTimeout().then(respond);
    it('does not emit current-render-view-deleted when speculative RVHs are deleted', async () => {
      let currentRenderViewDeletedEmitted = false;
      const renderViewDeletedHandler = () => {
        currentRenderViewDeletedEmitted = true;
      w.webContents.on('current-render-view-deleted' as any, renderViewDeletedHandler);
        w.webContents.removeListener('current-render-view-deleted' as any, renderViewDeletedHandler);
      w.loadURL(`${serverUrl}/redirect-cross-site`);
      expect(currentRenderViewDeletedEmitted).to.be.false('current-render-view-deleted was emitted');
      const parentWindow = new BrowserWindow({ show: false });
      let childWindow: BrowserWindow | null = null;
      const destroyed = once(parentWindow.webContents, 'destroyed');
      const childWindowCreated = new Promise<void>((resolve) => {
        app.once('browser-window-created', (event, window) => {
          childWindow = window;
          window.webContents.on('current-render-view-deleted' as any, renderViewDeletedHandler);
      parentWindow.loadURL(`${serverUrl}/first-window-open`);
      await childWindowCreated;
      childWindow!.webContents.removeListener('current-render-view-deleted' as any, renderViewDeletedHandler);
      parentWindow.close();
      expect(currentRenderViewDeletedEmitted).to.be.false('child window was destroyed');
    it('emits current-render-view-deleted if the current RVHs are deleted', async () => {
      w.webContents.on('current-render-view-deleted' as any, () => {
      expect(currentRenderViewDeletedEmitted).to.be.true('current-render-view-deleted wasn\'t emitted');
    it('emits render-view-deleted if any RVHs are deleted', async () => {
      let rvhDeletedCount = 0;
      w.webContents.on('render-view-deleted' as any, () => {
        rvhDeletedCount++;
      const expectedRenderViewDeletedEventCount = 1;
      expect(rvhDeletedCount).to.equal(expectedRenderViewDeletedEventCount, 'render-view-deleted wasn\'t emitted the expected nr. of times');
  describe('setIgnoreMenuShortcuts(ignore)', () => {
        w.webContents.setIgnoreMenuShortcuts(true);
        w.webContents.setIgnoreMenuShortcuts(false);
  const crashPrefs = [
  const nicePrefs = (o: any) => {
    let s = '';
    for (const key of Object.keys(o)) {
      s += `${key}=${o[key]}, `;
    return `(${s.slice(0, s.length - 2)})`;
  for (const prefs of crashPrefs) {
    describe(`crash  with webPreferences ${nicePrefs(prefs)}`, () => {
      it('isCrashed() is false by default', () => {
        expect(w.webContents.isCrashed()).to.equal(false);
      it('forcefullyCrashRenderer() crashes the process with reason=killed||crashed', async () => {
        const crashEvent = once(w.webContents, 'render-process-gone') as Promise<[any, Electron.RenderProcessGoneDetails]>;
        w.webContents.forcefullyCrashRenderer();
        const [, details] = await crashEvent;
        expect(details.reason === 'killed' || details.reason === 'crashed').to.equal(true, 'reason should be killed || crashed');
        expect(w.webContents.isCrashed()).to.equal(true);
      it('a crashed process is recoverable with reload()', async () => {
        w.webContents.reload();
  // Destroying webContents in its event listener is going to crash when
  // Electron is built in Debug mode.
  describe('destroy()', () => {
    before((done) => {
            done(new Error('unsupported endpoint'));
        serverUrl = url;
      { name: 'did-start-loading', url: '/200' },
      { name: 'dom-ready', url: '/200' },
      { name: 'did-stop-loading', url: '/200' },
      { name: 'did-finish-load', url: '/200' },
      // FIXME: Multiple Emit calls inside an observer assume that object
      // will be alive till end of the observer. Synchronous `destroy` api
      // violates this contract and crashes.
      { name: 'did-frame-finish-load', url: '/200' },
      { name: 'did-fail-load', url: '/net-error' }
    for (const e of events) {
      it(`should not crash when invoked synchronously inside ${e.name} handler`, async function () {
        // This test is flaky on Windows CI and we don't know why, but the
        // purpose of this test is to make sure Electron does not crash so it
        // is fine to retry this test for a few times.
        this.retries(3);
        const originalEmit = contents.emit.bind(contents);
        contents.emit = (...args) => { return originalEmit(...args); };
        contents.once(e.name as any, () => contents.destroy());
        const destroyed = once(contents, 'destroyed');
        contents.loadURL(serverUrl + e.url);
  describe('did-change-theme-color event', () => {
    it('is triggered with correct theme color', (done) => {
      w.webContents.on('did-change-theme-color', (e, color) => {
            expect(color).to.equal('#FFEEDD');
            w.loadFile(path.join(fixturesPath, 'pages', 'base-page.html'));
          } else if (count === 1) {
            expect(color).to.be.null();
      w.loadFile(path.join(fixturesPath, 'pages', 'theme-color.html'));
    it('is triggered with correct log message', (done) => {
      w.webContents.on('console-message', (e) => {
        // Don't just assert as Chromium might emit other logs that we should ignore.
        if (e.message === 'a') {
      w.loadFile(path.join(fixturesPath, 'pages', 'a.html'));
  describe('ipc-message event', () => {
    it('emits when the renderer process sends an asynchronous message', async () => {
      const w = new BrowserWindow({ show: true, webPreferences: { nodeIntegration: true, contextIsolation: false } });
        require('electron').ipcRenderer.send('message', 'Hello World!')
      const [, channel, message] = await once(w.webContents, 'ipc-message');
      expect(channel).to.equal('message');
      expect(message).to.equal('Hello World!');
  describe('ipc-message-sync event', () => {
    it('emits when the renderer process sends a synchronous message', async () => {
      const promise: Promise<[string, string]> = new Promise(resolve => {
        w.webContents.once('ipc-message-sync', (event, channel, arg) => {
          event.returnValue = 'foobar';
          resolve([channel, arg]);
        require('electron').ipcRenderer.sendSync('message', 'Hello World!')
      const [channel, message] = await promise;
      expect(result).to.equal('foobar');
  describe('referrer', () => {
    it('propagates referrer information to new target=_blank windows', (done) => {
        if (req.url === '/should_have_referrer') {
            expect(req.headers.referer).to.equal(`http://127.0.0.1:${(server.address() as AddressInfo).port}/`);
            return done();
            return done(e);
        res.end('<a id="a" href="/should_have_referrer" target="_blank">link</a>');
        w.webContents.once('did-finish-load', () => {
          w.webContents.setWindowOpenHandler(details => {
            expect(details.referrer.url).to.equal(url + '/');
            expect(details.referrer.policy).to.equal('strict-origin-when-cross-origin');
            return { action: 'allow' };
          w.webContents.executeJavaScript('a.click()');
    it('propagates referrer information to windows opened with window.open', (done) => {
          w.webContents.executeJavaScript('window.open(location.href + "should_have_referrer")');
  describe('webframe messages in sandboxed contents', () => {
    it('responds to executeJavaScript', async () => {
      const result = await w.webContents.executeJavaScript('37 + 5');
      expect(result).to.equal(42);
  describe('preload-error event', () => {
        it('is triggered when unhandled exception is thrown', async () => {
          const preload = path.join(fixturesPath, 'module', 'preload-error-exception.js');
          const promise = once(w.webContents, 'preload-error') as Promise<[any, string, Error]>;
          const [, preloadPath, error] = await promise;
          expect(preloadPath).to.equal(preload);
          expect(error.message).to.equal('Hello World!');
        it('is triggered on syntax errors', async () => {
          const preload = path.join(fixturesPath, 'module', 'preload-error-syntax.js');
          expect(error.message).to.equal('foobar is not defined');
        it('is triggered when preload script loading fails', async () => {
          const preload = path.join(fixturesPath, 'module', 'preload-invalid.js');
          expect(error.message).to.contain('preload-invalid.js');
  describe('takeHeapSnapshot()', () => {
    it('works with sandboxed renderers', async () => {
        await w.webContents.takeHeapSnapshot(filePath);
    it('fails with invalid file path', async () => {
      const badPath = path.join('i', 'am', 'a', 'super', 'bad', 'path');
      const promise = w.webContents.takeHeapSnapshot(badPath);
      return expect(promise).to.be.eventually.rejectedWith(Error, `Failed to take heap snapshot with invalid file path ${badPath}`);
    it('fails with invalid render process', async () => {
      const promise = w.webContents.takeHeapSnapshot(filePath);
      return expect(promise).to.be.eventually.rejectedWith(Error, 'Failed to take heap snapshot with nonexistent render frame');
  describe('setBackgroundThrottling()', () => {
    it('does not crash when allowing', () => {
      w.webContents.setBackgroundThrottling(true);
    it('does not crash when called via BrowserWindow', () => {
      w.setBackgroundThrottling(true);
    it('does not crash when disallowing', () => {
      const w = new BrowserWindow({ show: false, webPreferences: { backgroundThrottling: true } });
      w.webContents.setBackgroundThrottling(false);
  describe('getBackgroundThrottling()', () => {
    it('works via getter', () => {
      expect(w.webContents.getBackgroundThrottling()).to.equal(false);
      expect(w.webContents.getBackgroundThrottling()).to.equal(true);
    it('works via property', () => {
      w.webContents.backgroundThrottling = false;
      expect(w.webContents.backgroundThrottling).to.equal(false);
      w.webContents.backgroundThrottling = true;
      expect(w.webContents.backgroundThrottling).to.equal(true);
    it('works via BrowserWindow', () => {
      w.setBackgroundThrottling(false);
      expect(w.getBackgroundThrottling()).to.equal(false);
      expect(w.getBackgroundThrottling()).to.equal(true);
  ifdescribe(features.isPrintingEnabled())('getPrintersAsync()', () => {
    it('can get printer list', async () => {
      const printers = await w.webContents.getPrintersAsync();
      expect(printers).to.be.an('array');
  ifdescribe(features.isPrintingEnabled())('printToPDF()', () => {
    let server: http.Server | null;
    const readPDF = async (data: any) => {
      const tmpDir = await fs.promises.mkdtemp(path.resolve(os.tmpdir(), 'e-spec-printtopdf-'));
      const pdfPath = path.resolve(tmpDir, 'test.pdf');
      await fs.promises.writeFile(pdfPath, data);
      const pdfReaderPath = path.resolve(fixturesPath, 'api', 'pdf-reader.mjs');
      const result = cp.spawn(process.execPath, [pdfReaderPath, pdfPath], {
      const stdout: Buffer[] = [];
      const stderr: Buffer[] = [];
      result.stdout.on('data', (chunk) => stdout.push(chunk));
      result.stderr.on('data', (chunk) => stderr.push(chunk));
      const [code, signal] = await new Promise<[number | null, NodeJS.Signals | null]>((resolve) => {
        result.on('close', (code, signal) => {
          resolve([code, signal]);
        const errMsg = Buffer.concat(stderr).toString().trim();
        console.error(`Error parsing PDF file, exit code was ${code}; signal was ${signal}, error: ${errMsg}`);
        return JSON.parse(Buffer.concat(stdout).toString().trim());
        console.error('Error parsing PDF file:', err);
        console.error('Raw output:', Buffer.concat(stdout).toString().trim());
    const containsText = (items: any[], text: RegExp) => {
      return items.some(({ str }: { str: string }) => str.match(text));
    it('rejects on incorrectly typed parameters', async () => {
      const badTypes = {
        landscape: [],
        displayHeaderFooter: '123',
        printBackground: 2,
        scale: 'not-a-number',
        pageSize: 'IAmAPageSize',
        margins: 'terrible',
        pageRanges: { oops: 'im-not-the-right-key' },
        headerTemplate: [1, 2, 3],
        footerTemplate: [4, 5, 6],
        preferCSSPageSize: 'no',
        generateTaggedPDF: 'wtf',
        generateDocumentOutline: [7, 8, 9]
      await w.loadURL('data:text/html,<h1>Hello, World!</h1>');
      // These will hard crash in Chromium unless we type-check
      for (const [key, value] of Object.entries(badTypes)) {
        const param = { [key]: value };
        await expect(w.webContents.printToPDF(param)).to.eventually.be.rejected();
    it('rejects when margins exceed physical page size', async () => {
      await expect(w.webContents.printToPDF({
        pageSize: 'Letter',
          top: 100,
          bottom: 100,
          left: 5,
          right: 5
      })).to.eventually.be.rejectedWith('margins must be less than or equal to pageSize');
    it('does not crash when called multiple times in parallel', async () => {
      for (let i = 0; i < 3; i++) {
        promises.push(w.webContents.printToPDF({}));
      for (const data of results) {
        expect(data).to.be.an.instanceof(Buffer).that.is.not.empty();
    it('does not crash when called multiple times in sequence', async () => {
        const result = await w.webContents.printToPDF({});
    it('can print a PDF with default settings', async () => {
      const data = await w.webContents.printToPDF({});
    type PageSizeString = Exclude<Required<Electron.PrintToPDFOptions>['pageSize'], Electron.Size>;
    it('with custom page sizes', async () => {
      const paperFormats: Record<PageSizeString, ElectronInternal.PageSize> = {
        Letter: { width: 8.5, height: 11 },
        Legal: { width: 8.5, height: 14 },
        Tabloid: { width: 11, height: 17 },
        Ledger: { width: 17, height: 11 },
        A0: { width: 33.1, height: 46.8 },
        A1: { width: 23.4, height: 33.1 },
        A2: { width: 16.54, height: 23.4 },
        A3: { width: 11.7, height: 16.54 },
        A4: { width: 8.27, height: 11.7 },
        A5: { width: 5.83, height: 8.27 },
        A6: { width: 4.13, height: 5.83 }
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'print-to-pdf-small.html'));
      for (const format of Object.keys(paperFormats) as PageSizeString[]) {
        const data = await w.webContents.printToPDF({ pageSize: format });
        const pdfInfo = await readPDF(data);
        // page.view is [top, left, width, height].
        const width = pdfInfo.view[2] / 72;
        const height = pdfInfo.view[3] / 72;
        const approxEq = (a: number, b: number, epsilon = 0.01) => Math.abs(a - b) <= epsilon;
        expect(approxEq(width, paperFormats[format].width)).to.be.true();
        expect(approxEq(height, paperFormats[format].height)).to.be.true();
    it('with custom header and footer', async () => {
      await w.loadFile(path.join(fixturesPath, 'api', 'print-to-pdf-small.html'));
      const data = await w.webContents.printToPDF({
        displayHeaderFooter: true,
        headerTemplate: '<div>I\'m a PDF header</div>',
        footerTemplate: '<div>I\'m a PDF footer</div>'
      expect(containsText(pdfInfo.textContent, /I'm a PDF header/)).to.be.true();
      expect(containsText(pdfInfo.textContent, /I'm a PDF footer/)).to.be.true();
    it('in landscape mode', async () => {
      const data = await w.webContents.printToPDF({ landscape: true });
      const width = pdfInfo.view[2];
      const height = pdfInfo.view[3];
      expect(width).to.be.greaterThan(height);
    it('with custom page ranges', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'print-to-pdf-large.html'));
        pageRanges: '1-3',
        landscape: true
      // Check that correct # of pages are rendered.
      expect(pdfInfo.numPages).to.equal(3);
    it('does not tag PDFs by default', async () => {
      expect(pdfInfo.markInfo).to.be.null();
    it('can print same-origin iframes', async () => {
      await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'print-to-pdf-same-origin.html'));
      expect(containsText(pdfInfo.textContent, /Virtual member functions/)).to.be.true();
    // TODO(codebytere): OOPIF printing is disabled on Linux at the moment due to crashes.
    ifit(process.platform !== 'linux')('can print cross-origin iframes', async () => {
      server = http.createServer((_, res) => {
          <title>cross-origin iframe</title>
          <p>This page is displayed in an iframe.</p>
      expect(containsText(pdfInfo.textContent, /This page is displayed in an iframe./)).to.be.true();
    it('can generate tag data for PDFs', async () => {
      const data = await w.webContents.printToPDF({ generateTaggedPDF: true });
      expect(pdfInfo.markInfo).to.deep.equal({
        Marked: true,
        UserProperties: false,
        Suspects: false
    it('from an existing pdf document', async () => {
      const pdfPath = path.join(fixturesPath, 'cat.pdf');
      const readyToPrint = once(w.webContents, '-pdf-ready-to-print');
      await w.loadFile(pdfPath);
      await readyToPrint;
      expect(pdfInfo.numPages).to.equal(2);
      expect(containsText(pdfInfo.textContent, /Cat: The Ideal Pet/)).to.be.true();
    it('from an existing pdf document in a WebView', async () => {
      await win.loadURL('about:blank');
      const webContentsCreated = once(app, 'web-contents-created') as Promise<[any, WebContents]>;
      const src = url.format({
        pathname: `${fixturesPath.replaceAll('\\', '/')}/cat.pdf`,
        slashes: true
      await win.webContents.executeJavaScript(`
          const webview = new WebView()
          webview.setAttribute('src', '${src}')
          document.body.appendChild(webview)
            resolve()
      const [, webContents] = await webContentsCreated;
      await once(webContents, '-pdf-ready-to-print');
      const data = await webContents.printToPDF({});
  describe('PictureInPicture video', () => {
    it('works as expected', async function () {
      const w = new BrowserWindow({ webPreferences: { sandbox: true } });
      // TODO(codebytere): figure out why this workaround is needed and remove.
      // It is not germane to the actual test.
      await w.loadFile(path.join(fixturesPath, 'blank.html'));
      await w.loadFile(path.join(fixturesPath, 'api', 'picture-in-picture.html'));
      await w.webContents.executeJavaScript('document.createElement(\'video\').canPlayType(\'video/webm; codecs="vp8.0"\')', true);
      const result = await w.webContents.executeJavaScript('runTest(true)', true);
  describe('Shared Workers', () => {
    it('can get multiple shared workers', async () => {
      const ready = once(ipcMain, 'ready');
      w.loadFile(path.join(fixturesPath, 'api', 'shared-worker', 'shared-worker.html'));
      await ready;
      const sharedWorkers = w.webContents.getAllSharedWorkers();
      expect(sharedWorkers).to.have.lengthOf(2);
      expect(sharedWorkers[0].url).to.contain('shared-worker');
      expect(sharedWorkers[1].url).to.contain('shared-worker');
    it('can inspect a specific shared worker', async () => {
      w.webContents.inspectSharedWorkerById(sharedWorkers[0].id);
    let serverPort: number;
    let proxyServer: http.Server;
    let proxyServerPort: number;
        if (request.url === '/no-auth') {
          response.writeHead(200, { 'Content-type': 'text/plain' });
          return response.end(request.headers.authorization);
          .end('401');
      ({ port: serverPort, url: serverUrl } = await listen(server));
      proxyServer = http.createServer((request, response) => {
        if (request.headers['proxy-authorization']) {
          return response.end(request.headers['proxy-authorization']);
          .writeHead(407, { 'Proxy-Authenticate': 'Basic realm="Foo"' })
      proxyServerPort = (await listen(proxyServer)).port;
      await session.defaultSession.clearAuthCache();
      proxyServer.close();
    it('is emitted when navigating', async () => {
      let eventRequest: any;
      let eventAuthInfo: any;
      w.webContents.on('login', (event, request, authInfo, cb) => {
        eventRequest = request;
        eventAuthInfo = authInfo;
      const body = await w.webContents.executeJavaScript('document.documentElement.textContent');
      expect(body).to.equal(`Basic ${Buffer.from(`${user}:${pass}`).toString('base64')}`);
      expect(eventRequest.url).to.equal(serverUrl + '/');
      expect(eventAuthInfo.isProxy).to.be.false();
      expect(eventAuthInfo.scheme).to.equal('basic');
      expect(eventAuthInfo.host).to.equal('127.0.0.1');
      expect(eventAuthInfo.port).to.equal(serverPort);
      expect(eventAuthInfo.realm).to.equal('Foo');
    it('is emitted when a proxy requests authorization', async () => {
      const customSession = session.fromPartition(`${Math.random()}`);
      await customSession.setProxy({ proxyRules: `127.0.0.1:${proxyServerPort}`, proxyBypassRules: '<-loopback>' });
      const w = new BrowserWindow({ show: false, webPreferences: { session: customSession } });
      await w.loadURL(`${serverUrl}/no-auth`);
      expect(eventRequest.url).to.equal(`${serverUrl}/no-auth`);
      expect(eventAuthInfo.isProxy).to.be.true();
      expect(eventAuthInfo.port).to.equal(proxyServerPort);
    it('cancels authentication when callback is called with no arguments', async () => {
      expect(body).to.equal('401');
  describe('page-title-updated event', () => {
    it('is emitted with a full title for pages with no navigation', async () => {
      await bw.loadURL('about:blank');
      bw.webContents.executeJavaScript('child = window.open("", "", "show=no"); null');
      const [, child] = await once(app, 'web-contents-created') as [any, WebContents];
      bw.webContents.executeJavaScript('child.document.title = "new title"');
      const [, title] = await once(child, 'page-title-updated') as [any, string];
      expect(title).to.equal('new title');
  describe('context-menu event', () => {
    it('emits when right-clicked in page', async () => {
      const promise = once(w.webContents, 'context-menu') as Promise<[any, Electron.ContextMenuParams]>;
      // Simulate right-click to create context-menu event.
      const opts = { x: 0, y: 0, button: 'right' as const };
      w.webContents.sendInputEvent({ ...opts, type: 'mouseDown' });
      w.webContents.sendInputEvent({ ...opts, type: 'mouseUp' });
      const [, params] = await promise;
      expect(params.pageURL).to.equal(w.webContents.getURL());
      expect(params.frame).to.be.an('object');
      expect(params.x).to.be.a('number');
      expect(params.y).to.be.a('number');
    // Skipping due to lack of native click support.
    it.skip('emits the correct number of times when right-clicked in page', async () => {
      let contextMenuEmitCount = 0;
      w.webContents.on('context-menu', () => {
        contextMenuEmitCount++;
      // TODO(samuelmaddock): Perform native right-click. We've tried then
      // dropped robotjs and nutjs so for now this is a manual test.
      await once(w.webContents, 'context-menu');
      expect(contextMenuEmitCount).to.equal(1);
    it('emits when right-clicked in page in a draggable region', async () => {
        w.on('system-context-menu', (event) => { event.preventDefault(); });
      await w.loadFile(path.join(fixturesPath, 'pages', 'draggable-page.html'));
      const midPoint = w.getBounds().width / 2;
      const opts = { x: midPoint, y: midPoint, button: 'right' as const };
    it('emits when right clicked in a WebContentsView', async () => {
      const mainView = new WebContentsView({
      const draggablePage = path.join(fixturesPath, 'pages', 'draggable-page.html');
      await mainView.webContents.loadFile(draggablePage);
      w.contentView.addChildView(mainView);
      const { width, height } = w.getContentBounds();
      mainView.setBounds({ x: 0, y: 0, width, height });
      const promise = once(mainView.webContents, 'context-menu') as Promise<[any, Electron.ContextMenuParams]>;
      mainView.webContents.sendInputEvent({ ...opts, type: 'mouseDown' });
      mainView.webContents.sendInputEvent({ ...opts, type: 'mouseUp' });
      expect(params.pageURL).to.equal(mainView.webContents.getURL());
    it('emits when right clicked in a BrowserWindow with vibrancy', async () => {
      const w = new BrowserWindow({ show: false, vibrancy: 'titlebar' });
  describe('close() method', () => {
    it('closes when close() is called', async () => {
      const w = (webContents as typeof ElectronInternal.WebContents).create();
      const destroyed = once(w, 'destroyed');
      expect(w.isDestroyed()).to.be.true();
    it('closes when close() is called after loading a page', async () => {
    it('can be GCed before loading a page', async () => {
      let registry: FinalizationRegistry<unknown> | null = null;
      const cleanedUp = new Promise<number>(resolve => {
        registry = new FinalizationRegistry(resolve as any);
        registry!.register(w, 42);
      const i = setInterval(() => v8Util.requestGarbageCollectionForTesting(), 100);
      defer(() => clearInterval(i));
      expect(await cleanedUp).to.equal(42);
    it('causes its parent browserwindow to be closed', async () => {
      w.webContents.close();
    it('ignores beforeunload if waitForBeforeUnload not specified', async () => {
      await w.executeJavaScript('window.onbeforeunload = () => "hello"; null');
      w.on('will-prevent-unload', () => { throw new Error('unexpected will-prevent-unload'); });
    it('runs beforeunload if waitForBeforeUnload is specified', async () => {
      const willPreventUnload = once(w, 'will-prevent-unload');
      w.close({ waitForBeforeUnload: true });
      await willPreventUnload;
      expect(w.isDestroyed()).to.be.false();
    it('overriding beforeunload prevention results in webcontents close', async () => {
      w.once('will-prevent-unload', e => e.preventDefault());
  describe('content-bounds-updated event', () => {
    it('emits when moveTo is called', async () => {
      w.webContents.executeJavaScript('window.moveTo(50, 50)', true);
      const [, rect] = await once(w.webContents, 'content-bounds-updated') as [any, Electron.Rectangle];
      const { width, height } = w.getBounds();
      expect(rect).to.deep.equal({
        x: 50,
        y: 50,
      expect(w.getBounds().x).to.equal(50);
      expect(w.getBounds().y).to.equal(50);
    it('emits when resizeTo is called', async () => {
      w.webContents.executeJavaScript('window.resizeTo(100, 100)', true);
      const { x, y } = w.getBounds();
        x,
      expect({
        width: w.getBounds().width,
        height: w.getBounds().height
      }).to.deep.equal(process.platform === 'win32'
            // The width is reported as being larger on Windows? I'm not sure why
            // this is.
            width: 136,
    it('does not change window bounds if cancelled', async () => {
      w.webContents.once('content-bounds-updated', e => e.preventDefault());
      await w.webContents.executeJavaScript('window.resizeTo(100, 100)', true);
      expect(w.getBounds().width).to.equal(width);
      expect(w.getBounds().height).to.equal(height);
