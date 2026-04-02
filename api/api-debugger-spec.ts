import { emittedUntil } from './lib/events-helpers';
describe('debugger module', () => {
  describe('debugger.attach', () => {
    it('succeeds when devtools is already open', async () => {
      await w.webContents.loadURL('about:blank');
      expect(w.webContents.debugger.isAttached()).to.be.true();
    it('fails when protocol version is not supported', () => {
      expect(() => w.webContents.debugger.attach('2.0')).to.throw();
      expect(w.webContents.debugger.isAttached()).to.be.false();
    it('attaches when no protocol version is specified', () => {
  describe('debugger.detach', () => {
    it('fires detach event', async () => {
      const detach = once(w.webContents.debugger, 'detach');
      const [, reason] = await detach;
      expect(reason).to.equal('target closed');
    it('doesn\'t disconnect an active devtools session', async () => {
      w.webContents.once('devtools-opened', () => {
      await detach;
      expect(w.devToolsWebContents.isDestroyed()).to.be.false();
  describe('debugger.sendCommand', () => {
      if (server != null) {
    it('returns response', async () => {
      const params = { expression: '4+2' };
      const res = await w.webContents.debugger.sendCommand('Runtime.evaluate', params);
      expect(res.wasThrown).to.be.undefined();
      expect(res.result.value).to.equal(6);
    it('returns response when devtools is opened', async () => {
      const opened = once(w.webContents, 'devtools-opened');
      await opened;
    it('fires message event', async () => {
      const url = process.platform !== 'win32'
        ? `file://${path.join(fixtures, 'pages', 'a.html')}`
        : `file:///${path.join(fixtures, 'pages', 'a.html').replaceAll('\\', '/')}`;
      w.webContents.loadURL(url);
      const message = emittedUntil(w.webContents.debugger, 'message',
        (event: Electron.Event, method: string) => method === 'Console.messageAdded');
      w.webContents.debugger.sendCommand('Console.enable');
      const [,, params] = await message;
      expect(params.message.level).to.equal('log');
      expect(params.message.url).to.equal(url);
      expect(params.message.text).to.equal('a');
    it('returns error message when command fails', async () => {
      const promise = w.webContents.debugger.sendCommand('Test');
      await expect(promise).to.be.eventually.rejectedWith(Error, "'Test' wasn't found");
    it('handles valid unicode characters in message', async () => {
        res.setHeader('Content-Type', 'text/plain; charset=utf-8');
        res.end('\u0024');
      // If we do this synchronously, it's fast enough to attach and enable
      // network capture before the load. If we do it before the loadURL, for
      // some reason network capture doesn't get enabled soon enough and we get
      // an error when calling `Network.getResponseBody`.
      w.webContents.debugger.sendCommand('Network.enable');
      const [,, { requestId }] = await emittedUntil(w.webContents.debugger, 'message', (_event: any, method: string, params: any) =>
        method === 'Network.responseReceived' && params.response.url.startsWith('http://127.0.0.1'));
      await emittedUntil(w.webContents.debugger, 'message', (_event: any, method: string, params: any) =>
        method === 'Network.loadingFinished' && params.requestId === requestId);
      const { body } = await w.webContents.debugger.sendCommand('Network.getResponseBody', { requestId });
      expect(body).to.equal('\u0024');
    it('does not crash for invalid unicode characters in message', async () => {
      const loadingFinished = new Promise<void>(resolve => {
        w.webContents.debugger.on('message', (event, method) => {
          // loadingFinished indicates that page has been loaded and it did not
          // crash because of invalid UTF-8 data
          if (method === 'Network.loadingFinished') {
        res.end('\uFFFF');
      await loadingFinished;
    it('can get and set cookies using the Storage API', async () => {
      await w.webContents.debugger.sendCommand('Storage.clearCookies', {});
      await w.webContents.debugger.sendCommand('Storage.setCookies', {
        cookies: [
            name: 'cookieOne',
            value: 'cookieValueOne',
            url: 'https://cookieone.com'
            name: 'cookieTwo',
            value: 'cookieValueTwo',
            url: 'https://cookietwo.com'
      const { cookies } = await w.webContents.debugger.sendCommand('Storage.getCookies', {});
      expect(cookies).to.have.lengthOf(2);
      const cookieOne = cookies.find((cookie: any) => cookie.name === 'cookieOne');
      expect(cookieOne.domain).to.equal('cookieone.com');
      expect(cookieOne.value).to.equal('cookieValueOne');
      const cookieTwo = cookies.find((cookie: any) => cookie.name === 'cookieTwo');
      expect(cookieTwo.domain).to.equal('cookietwo.com');
      expect(cookieTwo.value).to.equal('cookieValueTwo');
    it('uses empty sessionId by default', async () => {
      const onMessage = once(w.webContents.debugger, 'message');
      await w.webContents.debugger.sendCommand('Target.setDiscoverTargets', { discover: true });
      const [, method, params, sessionId] = await onMessage;
      expect(method).to.equal('Target.targetCreated');
      expect(params.targetInfo.targetId).to.not.be.empty();
      expect(sessionId).to.be.empty();
    it('creates unique session id for each target', (done) => {
      w.webContents.loadFile(path.join(__dirname, 'fixtures', 'sub-frames', 'debug-frames.html'));
      let debuggerSessionId: string;
      w.webContents.debugger.on('message', (event, ...args) => {
        const [method, params, sessionId] = args;
        if (method === 'Target.targetCreated') {
          w.webContents.debugger.sendCommand('Target.attachToTarget', { targetId: params.targetInfo.targetId, flatten: true }).then(result => {
            debuggerSessionId = result.sessionId;
            w.webContents.debugger.sendCommand('Debugger.enable', {}, result.sessionId);
            // Ensure debugger finds a script to pause to possibly reduce flaky
            // tests.
            w.webContents.mainFrame.executeJavaScript('void 0;');
        if (method === 'Debugger.scriptParsed') {
          if (sessionId === debuggerSessionId) {
      w.webContents.debugger.sendCommand('Target.setDiscoverTargets', { discover: true });
