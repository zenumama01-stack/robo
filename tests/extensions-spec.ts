import { app, session, webFrameMain, BrowserWindow, ipcMain, WebContents, Extension, Session } from 'electron/main';
import { spawn } from 'node:child_process';
import { emittedNTimes, emittedUntil } from './lib/events-helpers';
import { ifit, listen, waitUntil } from './lib/spec-helpers';
import { expectWarningMessages } from './lib/warning-helpers';
import { closeAllWindows, closeWindow, cleanupWebContents } from './lib/window-helpers';
describe('chrome extensions', () => {
  const emptyPage = '<html><body><h1>EMPTY PAGE</h1></body></html>';
  // NB. extensions are only allowed on http://, https:// and ftp:// (!) urls by default.
  let wss: WebSocket.Server;
      if (req.url === '/cors') {
        res.setHeader('Access-Control-Allow-Origin', 'http://example.com');
      res.end(emptyPage);
    wss = new WebSocket.Server({ noServer: true });
    ({ port, url } = await listen(server));
    for (const e of session.defaultSession.extensions.getAllExtensions()) {
      session.defaultSession.extensions.removeExtension(e.id);
  it('does not crash when using chrome.management', async () => {
    const customSession = session.fromPartition(`persist:${require('uuid').v4()}`);
    const w = new BrowserWindow({ show: false, webPreferences: { session: customSession, sandbox: true } });
    const promise = once(app, 'web-contents-created') as Promise<[any, WebContents]>;
    await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'persistent-background-page'));
    const args: any = await promise;
    const wc: Electron.WebContents = args[1];
    await expect(wc.executeJavaScript(`
          chrome.management.getSelf((info) => {
            resolve(info);
    `)).to.eventually.have.property('id');
  describe('host_permissions', async () => {
      customSession = session.fromPartition(`persist:${require('uuid').v4()}`);
    it('recognize malformed host permissions', async () => {
      await expectWarningMessages(
          const extPath = path.join(fixtures, 'extensions', 'host-permissions', 'malformed');
          await customSession.extensions.loadExtension(extPath);
        { name: 'ExtensionLoadWarning', message: /URL pattern 'malformed_host' is malformed/ }
    it('can grant special privileges to urls with host permissions', async () => {
      const extPath = path.join(fixtures, 'extensions', 'host-permissions', 'privileged-tab-info');
      const message = { method: 'query' };
      w.webContents.executeJavaScript(`window.postMessage('${JSON.stringify(message)}', '*')`);
      const [{ message: responseString }] = await once(w.webContents, 'console-message');
      const response = JSON.parse(responseString);
      expect(response).to.have.lengthOf(1);
      const tab = response[0];
      expect(tab).to.have.property('url').that.is.a('string');
      expect(tab).to.have.property('title').that.is.a('string');
      expect(tab).to.have.property('active').that.is.a('boolean');
      expect(tab).to.have.property('autoDiscardable').that.is.a('boolean');
      expect(tab).to.have.property('discarded').that.is.a('boolean');
      expect(tab).to.have.property('groupId').that.is.a('number');
      expect(tab).to.have.property('highlighted').that.is.a('boolean');
      expect(tab).to.have.property('id').that.is.a('number');
      expect(tab).to.have.property('incognito').that.is.a('boolean');
      expect(tab).to.have.property('index').that.is.a('number');
      expect(tab).to.have.property('pinned').that.is.a('boolean');
      expect(tab).to.have.property('selected').that.is.a('boolean');
      expect(tab).to.have.property('windowId').that.is.a('number');
  it('supports minimum_chrome_version manifest key', async () => {
    const extPath = path.join(fixtures, 'extensions', 'minimum-chrome-version');
    const load = customSession.extensions.loadExtension(extPath);
    await expect(load).to.eventually.be.rejectedWith(
      `Loading extension at ${extPath} failed with: This extension requires Chromium version 999 or greater.`
  function fetch (contents: WebContents, url: string) {
    return contents.executeJavaScript(`fetch(${JSON.stringify(url)})`);
  it('bypasses CORS in requests made from extensions', async () => {
    const extension = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'ui-page'));
    await w.loadURL(`${extension.url}bare-page.html`);
    await expect(fetch(w.webContents, `${url}/cors`)).to.not.be.rejectedWith(TypeError);
  it('loads an extension', async () => {
    // NB. we have to use a persist: session (i.e. non-OTR) because the
    // extension registry is redirected to the main session. so installing an
    // extension in an in-memory session results in it being installed in the
    // default session.
    const customSession = session.fromPartition(`persist:${uuid.v4()}`);
    await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'red-bg'));
    const bg = await w.webContents.executeJavaScript('document.documentElement.style.backgroundColor');
    expect(bg).to.equal('red');
  it('does not crash when loading an extension with missing manifest', async () => {
    const promise = customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'missing-manifest'));
    await expect(promise).to.eventually.be.rejectedWith(/Manifest file is missing or unreadable/);
  it('does not crash when failing to load an extension', async () => {
    const promise = customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'load-error'));
    await expect(promise).to.eventually.be.rejected();
  it('serializes a loaded extension', async () => {
    const extensionPath = path.join(fixtures, 'extensions', 'red-bg');
    const manifest = JSON.parse(await fs.readFile(path.join(extensionPath, 'manifest.json'), 'utf-8'));
    const extension = await customSession.extensions.loadExtension(extensionPath);
    expect(extension.id).to.be.a('string');
    expect(extension.name).to.be.a('string');
    expect(extension.path).to.be.a('string');
    expect(extension.version).to.be.a('string');
    expect(extension.url).to.be.a('string');
    expect(extension.manifest).to.deep.equal(manifest);
  it('removes an extension', async () => {
    const { id } = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'red-bg'));
    customSession.extensions.removeExtension(id);
      expect(bg).to.equal('');
  it('emits extension lifecycle events', async () => {
    const loadedPromise = once(customSession.extensions, 'extension-loaded');
    const readyPromise = emittedUntil(customSession.extensions, 'extension-ready', (event: Event, extension: Extension) => {
      return extension.name !== 'Chromium PDF Viewer';
    const extension = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'red-bg'));
    const [, loadedExtension] = await loadedPromise;
    const [, readyExtension] = await readyPromise;
    expect(loadedExtension).to.deep.equal(extension);
    expect(readyExtension).to.deep.equal(extension);
    const unloadedPromise = once(customSession.extensions, 'extension-unloaded');
    await customSession.extensions.removeExtension(extension.id);
    const [, unloadedExtension] = await unloadedPromise;
    expect(unloadedExtension).to.deep.equal(extension);
  it('lists loaded extensions in getAllExtensions', async () => {
    const e = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'red-bg'));
    expect(customSession.extensions.getAllExtensions()).to.deep.equal([e]);
    customSession.extensions.removeExtension(e.id);
    expect(customSession.extensions.getAllExtensions()).to.deep.equal([]);
  it('gets an extension by id', async () => {
    expect(customSession.extensions.getExtension(e.id)).to.deep.equal(e);
  it('confines an extension to the session it was loaded in', async () => {
    const w = new BrowserWindow({ show: false }); // not in the session
  it('loading an extension in a temporary session throws an error', async () => {
    const customSession = session.fromPartition(uuid.v4());
    await expect(customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'red-bg'))).to.eventually.be.rejectedWith('Extensions cannot be loaded in a temporary session');
  describe('chrome.i18n', () => {
    let extension: Extension;
    const exec = async (name: string) => {
      await w.webContents.executeJavaScript(`exec('${name}')`);
      const [, result] = await p;
      extension = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-i18n', 'v2'));
      w = new BrowserWindow({ show: false, webPreferences: { session: customSession, nodeIntegration: true, contextIsolation: false } });
    it('getAcceptLanguages()', async () => {
      const result = await exec('getAcceptLanguages');
      expect(result).to.be.an('array').and.deep.equal(['en-US', 'en']);
    it('getMessage()', async () => {
      const result = await exec('getMessage');
      expect(result.id).to.be.a('string').and.equal(extension.id);
      expect(result.name).to.be.a('string').and.equal('chrome-i18n');
  describe('chrome.runtime', () => {
      await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-runtime'));
    it('getManifest()', async () => {
      const result = await exec('getManifest');
      expect(result).to.be.an('object').with.property('name', 'chrome-runtime');
    it('id', async () => {
      const result = await exec('id');
      expect(result).to.be.a('string').with.lengthOf(32);
    it('getURL()', async () => {
      const result = await exec('getURL');
      expect(result).to.be.a('string').and.match(/^chrome-extension:\/\/.*main.js$/);
    it('getPlatformInfo()', async () => {
      const result = await exec('getPlatformInfo');
      expect(result.os).to.be.a('string');
      expect(result.arch).to.be.a('string');
  describe('chrome.storage', () => {
    it('stores and retrieves a key', async () => {
      await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-storage'));
      const w = new BrowserWindow({ show: false, webPreferences: { session: customSession, nodeIntegration: true, contextIsolation: false } });
        const p = once(ipcMain, 'storage-success');
        const [, v] = await p;
        expect(v).to.equal('value');
  describe('chrome.webRequest', () => {
      customSession = session.fromPartition(`persist:${uuid.v4()}`);
      w = new BrowserWindow({ show: false, webPreferences: { session: customSession, sandbox: true, contextIsolation: true } });
    describe('onBeforeRequest', () => {
      async function haveRejectedFetch () {
          await fetch(w.webContents, url);
        } catch (ex: any) {
          return ex.message === 'Failed to fetch';
      it('can cancel http requests', async () => {
        await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-webRequest'));
        await expect(waitUntil(haveRejectedFetch)).to.eventually.be.fulfilled();
      it('does not cancel http requests when no extension loaded', async () => {
        await expect(fetch(w.webContents, url)).to.not.be.rejectedWith('Failed to fetch');
    it('does not take precedence over Electron webRequest - http', async () => {
          fetch(w.webContents, url);
    it('does not take precedence over Electron webRequest - WebSocket', () => {
          customSession.webRequest.onBeforeSendHeaders(() => {
          await w.loadFile(path.join(fixtures, 'api', 'webrequest.html'), { query: { port: `${port}` } });
          await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-webRequest-wss'));
    describe('WebSocket', () => {
      it('can be proxied', async () => {
        customSession.webRequest.onSendHeaders((details) => {
  describe('chrome.tabs', () => {
      await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-api'));
    it('executeScript', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { session: customSession, nodeIntegration: true } });
      const message = { method: 'executeScript', args: ['1 + 2'] };
      expect(response).to.equal(3);
    it('connect', async () => {
      const portName = uuid.v4();
      const message = { method: 'connectTab', args: [portName] };
      const response = responseString.split(',');
      expect(response[0]).to.equal(portName);
      expect(response[1]).to.equal('howdy');
    it('sendMessage receives the response', async () => {
      const message = { method: 'sendMessage', args: ['Hello World!'] };
      expect(response.message).to.equal('Hello World!');
      expect(response.tabId).to.equal(w.webContents.id);
    it('update', async () => {
      const w2 = new BrowserWindow({ show: false, webPreferences: { session: customSession } });
      const w2Navigated = once(w2.webContents, 'did-navigate');
      const message = { method: 'update', args: [w2.webContents.id, { url }] };
      await w2Navigated;
      expect(new URL(w2.getURL()).toString()).to.equal(new URL(url).toString());
      expect(response.id).to.equal(w2.webContents.id);
  describe('background pages', () => {
    it('loads a lazy background page when sending a message', async () => {
      await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'lazy-background-page'));
        const [, resp] = await once(ipcMain, 'bg-page-message-response');
        expect(resp.message).to.deep.equal({ some: 'message' });
        expect(resp.sender.id).to.be.a('string');
        expect(resp.sender.origin).to.equal(url);
        expect(resp.sender.url).to.equal(url + '/');
    it('can use extension.getBackgroundPage from a ui page', async () => {
      const { id } = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'lazy-background-page'));
      await w.loadURL(`chrome-extension://${id}/page-get-background.html`);
      const receivedMessage = await w.webContents.executeJavaScript('window.completionPromise');
      expect(receivedMessage).to.deep.equal({ some: 'message' });
    it('can use runtime.getBackgroundPage from a ui page', async () => {
      await w.loadURL(`chrome-extension://${id}/page-runtime-get-background.html`);
    it('has session in background page', async () => {
      const { id } = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'persistent-background-page'));
      const [, bgPageContents] = await promise;
      expect(bgPageContents.getType()).to.equal('backgroundPage');
      await once(bgPageContents, 'did-finish-load');
      expect(bgPageContents.getURL()).to.equal(`chrome-extension://${id}/_generated_background_page.html`);
      expect(bgPageContents.session).to.not.equal(undefined);
    it('can open devtools of background page', async () => {
      bgPageContents.openDevTools();
      bgPageContents.closeDevTools();
  describe('devtools extensions', () => {
    let showPanelTimeoutId: any = null;
      if (showPanelTimeoutId) clearTimeout(showPanelTimeoutId);
    const showLastDevToolsPanel = (w: BrowserWindow) => {
        const show = () => {
          if (w == null || w.isDestroyed()) return;
          const { devToolsWebContents } = w as unknown as { devToolsWebContents: WebContents | undefined };
          if (devToolsWebContents == null || devToolsWebContents.isDestroyed()) {
          const showLastPanel = () => {
            // this is executed in the devtools context, where UI is a global
            const { EUI } = (window as any);
            const instance = EUI.InspectorView.InspectorView.instance();
            const tabs = instance.tabbedPane.tabs;
            const lastPanelId = tabs[tabs.length - 1].id;
            instance.showPanel(lastPanelId);
          devToolsWebContents.executeJavaScript(`(${showLastPanel})()`, false).then(() => {
            showPanelTimeoutId = setTimeout(show, 100);
    ifit(process.platform !== 'win32' || process.arch !== 'arm64')('loads a devtools extension', async () => {
      customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'devtools-extension'));
      const winningMessage = once(ipcMain, 'winning');
      const w = new BrowserWindow({ show: true, webPreferences: { session: customSession, nodeIntegration: true, contextIsolation: false } });
      showLastDevToolsPanel(w);
      await winningMessage;
  describe('chrome extension content scripts', () => {
    const extensionPath = path.resolve(fixtures, 'extensions');
    const addExtension = (name: string) => session.defaultSession.extensions.loadExtension(path.resolve(extensionPath, name));
    const removeAllExtensions = () => {
      Object.keys(session.defaultSession.extensions.getAllExtensions()).forEach(extName => {
        session.defaultSession.extensions.removeExtension(extName);
    let responseIdCounter = 0;
    const executeJavaScriptInFrame = (webContents: WebContents, frameToken: string, code: string) => {
        const responseId = responseIdCounter++;
        ipcMain.once(`executeJavaScriptInFrame_${responseId}`, (event, result) => {
        webContents.send('executeJavaScriptInFrame', frameToken, code, responseId);
    const generateTests = (sandboxEnabled: boolean, contextIsolationEnabled: boolean) => {
      describe(`with sandbox ${sandboxEnabled ? 'enabled' : 'disabled'} and context isolation ${contextIsolationEnabled ? 'enabled' : 'disabled'}`, () => {
        describe('supports "run_at" option', () => {
                contextIsolation: contextIsolationEnabled,
                sandbox: sandboxEnabled
            removeAllExtensions();
          it('should run content script at document_start', async () => {
            await addExtension('content-script-document-start');
            w.webContents.once('dom-ready', async () => {
              const result = await w.webContents.executeJavaScript('document.documentElement.style.backgroundColor');
              expect(result).to.equal('red');
          it('should run content script at document_idle', async () => {
            await addExtension('content-script-document-idle');
            const result = await w.webContents.executeJavaScript('document.body.style.backgroundColor');
          it('should run content script at document_end', async () => {
            await addExtension('content-script-document-end');
            w.webContents.once('did-finish-load', async () => {
        describe('supports "all_frames" option', () => {
          const contentScript = path.resolve(fixtures, 'extensions/content-script');
          const contentPath = path.join(contentScript, 'frame-with-frame.html');
          // Computed style values
          const COLOR_RED = 'rgb(255, 0, 0)';
          const COLOR_BLUE = 'rgb(0, 0, 255)';
          const COLOR_TRANSPARENT = 'rgba(0, 0, 0, 0)';
            server = http.createServer(async (_, res) => {
                const content = await fs.readFile(contentPath, 'utf-8');
                res.end(content, 'utf-8');
                res.writeHead(500);
                res.end(`Failed to load ${contentPath} : ${(error as NodeJS.ErrnoException).code}`);
            ({ port } = await listen(server));
            session.defaultSession.extensions.loadExtension(contentScript);
            session.defaultSession.extensions.removeExtension('content-script-test');
                // enable content script injection in subframes
                preload: path.join(contentScript, 'all_frames-preload.js')
          afterEach(() =>
            closeWindow(w).then(() => {
          it('applies matching rules in subframes', async () => {
            const detailsPromise = emittedNTimes(w.webContents, 'did-frame-finish-load', 2);
            w.loadURL(`http://127.0.0.1:${port}`);
            const frameEvents = await detailsPromise;
              frameEvents.map(async frameEvent => {
                const [, isMainFrame, frameProcessId, frameRoutingId] = frameEvent;
                const result: any = await executeJavaScriptInFrame(
                  w.webContents,
                  frame!.frameToken,
                  `(() => {
                    const a = document.getElementById('all_frames_enabled')
                    const b = document.getElementById('all_frames_disabled')
                      enabledColor: getComputedStyle(a).backgroundColor,
                      disabledColor: getComputedStyle(b).backgroundColor
                  })()`
                expect(result.enabledColor).to.equal(COLOR_RED);
                expect(result.disabledColor).to.equal(isMainFrame ? COLOR_BLUE : COLOR_TRANSPARENT);
    generateTests(false, false);
    generateTests(false, true);
    generateTests(true, false);
    generateTests(true, true);
  describe('extension ui pages', () => {
    it('loads a ui page of an extension', async () => {
      const { id } = await session.defaultSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'ui-page'));
      await w.loadURL(`chrome-extension://${id}/bare-page.html`);
      const textContent = await w.webContents.executeJavaScript('document.body.textContent');
      expect(textContent).to.equal('ui page loaded ok\n');
    it('can load resources', async () => {
      await w.loadURL(`chrome-extension://${id}/page-script-load.html`);
      expect(textContent).to.equal('script loaded ok\n');
  describe('manifest v3', () => {
    it('registers background service worker', async () => {
      const registrationPromise = new Promise<string>(resolve => {
        customSession.serviceWorkers.once('registration-completed', (event, { scope }) => resolve(scope));
      const extension = await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'mv3-service-worker'));
      const scope = await registrationPromise;
      expect(scope).equals(extension.url);
    it('can run chrome extension APIs', async () => {
      await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'mv3-service-worker'));
      w.webContents.executeJavaScript('window.postMessage(\'fetch-confirmation\', \'*\')');
      const { message } = JSON.parse(responseString);
      expect(message).to.equal('Hello from background.js');
      let w = null as unknown as BrowserWindow;
        await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-i18n', 'v3'));
      it('getAcceptLanguages', async () => {
        const message = { method: 'getAcceptLanguages' };
        expect(response).to.be.an('array').that.is.not.empty('languages array is empty');
      it('getUILanguage', async () => {
        const message = { method: 'getUILanguage' };
        expect(response).to.be.a('string');
      it('getMessage', async () => {
        const message = { method: 'getMessage' };
        expect(response).to.equal('Hola mundo!!');
      it('detectLanguage', async () => {
        const greetings = [
          'Ich liebe dich', // German
          'Mahal kita', // Filipino
          '愛してます', // Japanese
          'دوستت دارم', // Persian
          'Minä rakastan sinua' // Finnish
        const message = { method: 'detectLanguage', args: [greetings] };
        expect(response).to.be.an('array');
        for (const item of response) {
          expect(Object.keys(item)).to.deep.equal(['isReliable', 'languages']);
        const languages = response.map((r: { isReliable: boolean, languages: any[] }) => r.languages[0]);
        expect(languages).to.deep.equal([
          { language: 'de', percentage: 100 },
          { language: 'fil', percentage: 100 },
          { language: 'ja', percentage: 100 },
          { language: 'ps', percentage: 100 },
          { language: 'fi', percentage: 100 }
    // chrome.action is not supported in Electron. These tests only ensure
    // it does not explode.
    describe('chrome.action', () => {
        await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-action-fail'));
      it('isEnabled', async () => {
        const message = { method: 'isEnabled' };
        expect(response).to.equal(false);
      it('setIcon', async () => {
        const message = { method: 'setIcon' };
        expect(response).to.equal(null);
      it('getBadgeText', async () => {
        const message = { method: 'getBadgeText' };
        expect(response).to.equal('');
        await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-tabs', 'api-async'));
      it('getZoom', async () => {
        const message = { method: 'getZoom' };
        expect(response).to.equal(1);
      it('setZoom', async () => {
        const message = { method: 'setZoom', args: [2] };
        expect(response).to.deep.equal(2);
      it('getZoomSettings', async () => {
        const message = { method: 'getZoomSettings' };
        expect(response).to.deep.equal({
          defaultZoomFactor: 1,
          mode: 'automatic',
          scope: 'per-origin'
      it('setZoomSettings', async () => {
        const message = { method: 'setZoomSettings', args: [{ mode: 'disabled' }] };
          mode: 'disabled',
          scope: 'per-tab'
      describe('get', () => {
        it('returns tab properties', async () => {
          const message = { method: 'get' };
          expect(response).to.have.property('url').that.is.a('string');
          expect(response).to.have.property('title').that.is.a('string');
          expect(response).to.have.property('active').that.is.a('boolean');
          expect(response).to.have.property('autoDiscardable').that.is.a('boolean');
          expect(response).to.have.property('discarded').that.is.a('boolean');
          expect(response).to.have.property('groupId').that.is.a('number');
          expect(response).to.have.property('highlighted').that.is.a('boolean');
          expect(response).to.have.property('id').that.is.a('number');
          expect(response).to.have.property('incognito').that.is.a('boolean');
          expect(response).to.have.property('index').that.is.a('number');
          expect(response).to.have.property('pinned').that.is.a('boolean');
          expect(response).to.have.property('selected').that.is.a('boolean');
          expect(response).to.have.property('windowId').that.is.a('number');
        it('does not return privileged properties without tabs permission', async () => {
          const noPrivilegeSes = session.fromPartition(`persist:${uuid.v4()}`);
          await noPrivilegeSes.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-tabs', 'no-privileges'));
          w = new BrowserWindow({ show: false, webPreferences: { session: noPrivilegeSes } });
          w.webContents.executeJavaScript('window.postMessage(\'{}\', \'*\')');
          expect(response).not.to.have.property('url');
          expect(response).not.to.have.property('title');
      it('reload', async () => {
        const message = { method: 'reload' };
        const consoleMessage = once(w.webContents, 'console-message');
        const finish = once(w.webContents, 'did-finish-load');
        await Promise.all([consoleMessage, finish]).then(([[{ message: responseString }]]) => {
          expect(response.status).to.equal('reloaded');
      describe('update', () => {
        it('can update muted status', async () => {
          const message = { method: 'update', args: [{ muted: true }] };
          expect(response).to.have.property('mutedInfo').that.is.a('object');
          const { mutedInfo } = response;
          expect(mutedInfo).to.deep.eq({
            muted: true,
            reason: 'user'
        it('fails when navigating to an invalid url', async () => {
          const message = { method: 'update', args: [{ url: 'chrome://crash' }] };
          const { error } = JSON.parse(responseString);
          expect(error).to.eq('I\'m sorry. I\'m afraid I can\'t do that.');
        it('fails when navigating to prohibited url', async () => {
        it('fails when navigating to a devtools url without permission', async () => {
          const message = { method: 'update', args: [{ url: 'devtools://blah' }] };
          expect(error).to.eq('Cannot navigate to a devtools:// page without either the devtools or debugger permission.');
        it('fails when navigating to a chrome-untrusted url', async () => {
          const message = { method: 'update', args: [{ url: 'chrome-untrusted://blah' }] };
          expect(error).to.eq('Cannot navigate to a chrome-untrusted:// page.');
        it('fails when navigating to a file url withotut file access', async () => {
          const message = { method: 'update', args: [{ url: 'file://blah' }] };
          expect(error).to.eq('Cannot navigate to a file URL without local file access.');
      describe('query', () => {
        it('can query for a tab with specific properties', async () => {
          expect(w.webContents.isAudioMuted()).to.be.false('muted');
          expect(w.webContents.isAudioMuted()).to.be.true('not muted');
          const message = { method: 'query', args: [{ muted: true }] };
          expect(tab.mutedInfo).to.deep.equal({
        it('only returns tabs in the same session', async () => {
          const sameSessionWin = new BrowserWindow({
          sameSessionWin.webContents.setAudioMuted(true);
          const newSession = session.fromPartition(`persist:${uuid.v4()}`);
          const differentSessionWin = new BrowserWindow({
              session: newSession
          differentSessionWin.webContents.setAudioMuted(true);
          expect(response).to.have.lengthOf(2);
          for (const tab of response) {
    describe('chrome.scripting', () => {
        await customSession.extensions.loadExtension(path.join(fixtures, 'extensions', 'chrome-scripting'));
        const message = { method: 'executeScript' };
        const updated = await once(w.webContents, 'page-title-updated');
        expect(updated[1]).to.equal('HEY HEY HEY');
      it('registerContentScripts', async () => {
        const message = { method: 'registerContentScripts' };
        expect(response).to.be.an('array').with.lengthOf(1);
        expect(response[0]).to.deep.equal({
          allFrames: false,
          id: 'session-script',
          js: ['content.js'],
          matchOriginAsFallback: false,
          matches: ['<all_urls>'],
          persistAcrossSessions: false,
          runAt: 'document_start',
          world: 'ISOLATED'
      it('globalParams', async () => {
        const message = { method: 'globalParams' };
        expect(response).to.deep.equal({ changed: true });
      it('insertCSS', async () => {
        const bgBefore = await w.webContents.executeJavaScript('window.getComputedStyle(document.body).backgroundColor');
        expect(bgBefore).to.equal('rgba(0, 0, 0, 0)');
        const message = { method: 'insertCSS' };
        expect(response.success).to.be.true();
        const bgAfter = await w.webContents.executeJavaScript('window.getComputedStyle(document.body).backgroundColor');
        expect(bgAfter).to.equal('rgb(255, 0, 0)');
  describe('custom protocol', () => {
    async function runFixture (name: string) {
      const appProcess = spawn(process.execPath, [(path.join(fixtures, 'extensions', name, 'main.js'))]);
      return output.trim();
    it('loads DevTools extensions on custom protocols with allowExtensions privileges and runs content and background scripts', async () => {
      const output = await runFixture('custom-protocol');
      expect(output).to.equal('Title: MESSAGE RECEIVED');
    it('loads DevTools panels on custom protocols with allowExtensions privileges', async () => {
      const output = await runFixture('custom-protocol-panel');
      expect(output).to.equal('ELECTRON TEST PANEL created');
