import { app, session, BrowserWindow, net, ipcMain, Session, webFrameMain, WebFrameMain } from 'electron/main';
import * as auth from 'basic-auth';
import * as send from 'send';
import { defer, ifit, listen, waitUntil } from './lib/spec-helpers';
describe('session module', () => {
  describe('session.defaultSession', () => {
    it('returns the default session', () => {
      expect(session.defaultSession).to.equal(session.fromPartition(''));
  describe('session.fromPartition(partition, options)', () => {
    it('returns existing session with same partition', () => {
      expect(session.fromPartition('test')).to.equal(session.fromPartition('test'));
  describe('session.fromPath(path)', () => {
    it('returns storage path of a session which was created with an absolute path', () => {
      const tmppath = require('electron').app.getPath('temp');
      const ses = session.fromPath(tmppath);
      expect(ses.storagePath).to.equal(tmppath);
  describe('ses.cookies', () => {
    const name = '0';
    const value = '0';
    // Clear cookie of defaultSession after each test.
      const { cookies } = session.defaultSession;
      const cs = await cookies.get({ url });
      for (const c of cs) {
        await cookies.remove(url, c.name);
    it('should get cookies', async () => {
        res.setHeader('Set-Cookie', [`${name}=${value}`]);
        res.end('finished');
      await w.loadURL(`${url}:${port}`);
      const list = await w.webContents.session.cookies.get({ url });
      const cookie = list.find(cookie => cookie.name === name);
      expect(cookie).to.exist.and.to.have.property('value', value);
    it('sets cookies', async () => {
      const name = '1';
      const value = '1';
      await cookies.set({ url, name, value, expirationDate: (Date.now()) / 1000 + 120 });
      const c = (await cookies.get({ url }))[0];
      expect(c.name).to.equal(name);
      expect(c.value).to.equal(value);
      expect(c.session).to.equal(false);
    it('sets session cookies', async () => {
      const name = '2';
      await cookies.set({ url, name, value });
      expect(c.session).to.equal(true);
    it('sets cookies without name', async () => {
      const value = '3';
      await cookies.set({ url, value });
      expect(c.name).to.be.empty();
    for (const sameSite of <const>['unspecified', 'no_restriction', 'lax', 'strict']) {
      it(`sets cookies with samesite=${sameSite}`, async () => {
        const value = 'hithere';
        await cookies.set({ url, value, sameSite });
        expect(c.sameSite).to.equal(sameSite);
    it('fails to set cookies with samesite=garbage', async () => {
      await expect(cookies.set({ url, value, sameSite: 'garbage' as any })).to.eventually.be.rejectedWith('Failed to convert \'garbage\' to an appropriate cookie same site value');
    it('gets cookies without url', async () => {
      const cs = await cookies.get({ domain: '127.0.0.1' });
      expect(cs.some(c => c.name === name && c.value === value)).to.equal(true);
    it('rejects when setting a cookie with missing required fields', async () => {
        cookies.set({ url: '', name, value })
      ).to.eventually.be.rejectedWith('Failed to set cookie - The cookie was set with an invalid Domain attribute.');
    it('rejects when setting a cookie with an invalid URL', async () => {
        cookies.set({ url: 'asdf', name, value })
    it('rejects when setting a cookie with an invalid ASCII control character', async () => {
      const name = 'BadCookie';
      const value = 'test;test';
        cookies.set({ url, name, value })
      ).to.eventually.be.rejectedWith('Failed to set cookie - The cookie contains ASCII control characters');
    it('should overwrite previous cookies', async () => {
      const name = 'DidOverwrite';
      for (const value of ['No', 'Yes']) {
        const list = await cookies.get({ url });
        expect(list.some(cookie => cookie.name === name && cookie.value === value)).to.equal(true);
    it('should remove cookies', async () => {
      const value = '2';
      await cookies.remove(url, name);
      expect(list.some(cookie => cookie.name === name && cookie.value === value)).to.equal(false);
    // DISABLED-FIXME
    it('should set cookie for standard scheme', async () => {
      const domain = 'fake-host';
      const url = `${standardScheme}://${domain}`;
      const name = 'custom';
      expect(list).to.have.lengthOf(1);
      expect(list[0]).to.have.property('name', name);
      expect(list[0]).to.have.property('value', value);
      expect(list[0]).to.have.property('domain', domain);
    it('emits a changed event when a cookie is added or removed', async () => {
      const { cookies } = session.fromPartition('cookies-changed');
      const name = 'foo';
      const value = 'bar';
      const a = once(cookies, 'changed');
      const [, setEventCookie, setEventCause, setEventRemoved] = await a;
      const b = once(cookies, 'changed');
      const [, removeEventCookie, removeEventCause, removeEventRemoved] = await b;
      expect(setEventCookie.name).to.equal(name);
      expect(setEventCookie.value).to.equal(value);
      expect(setEventCause).to.equal('inserted');
      expect(setEventRemoved).to.equal(false);
      expect(removeEventCookie.name).to.equal(name);
      expect(removeEventCookie.value).to.equal(value);
      expect(removeEventCause).to.equal('explicit');
      expect(removeEventRemoved).to.equal(true);
    describe('ses.cookies.flushStore()', async () => {
      it('flushes the cookies to disk', async () => {
        await cookies.flushStore();
    it('should survive an app restart for persistent partition', async function () {
      const appPath = path.join(fixtures, 'api', 'cookie-app');
      const runAppWithPhase = (phase: string) => {
          const appProcess = ChildProcess.spawn(
            process.execPath,
            [appPath],
            { env: { PHASE: phase, ...process.env } }
          appProcess.on('exit', () => {
            resolve(output.replaceAll(/(\r\n|\n|\r)/gm, ''));
      expect(await runAppWithPhase('one')).to.equal('011');
      expect(await runAppWithPhase('two')).to.equal('110');
  describe('domain matching', () => {
    let testSession: Electron.Session;
      testSession = session.fromPartition(`cookies-domain-test-${Date.now()}`);
      // Clear cookies after each test
      await testSession.clearStorageData({ storages: ['cookies'] });
    // Helper to set a cookie and then test if it's retrieved with a domain filter
    async function testDomainMatching (setCookieOpts: Electron.CookiesSetDetails,
      expectMatch: boolean) {
      await testSession.cookies.set(setCookieOpts);
      const cookies = await testSession.cookies.get({ domain });
      if (expectMatch) {
        expect(cookies[0].name).to.equal(setCookieOpts.name);
        expect(cookies[0].value).to.equal(setCookieOpts.value);
    it('should match exact domain', async () => {
      await testDomainMatching({
        url: 'http://example.com',
        name: 'exactMatch',
        value: 'value1',
        domain: 'example.com'
      }, 'example.com', true);
    it('should match subdomain when filter has leading dot', async () => {
        url: 'http://sub.example.com',
        name: 'subdomainMatch',
        value: 'value2',
        domain: '.example.com'
      }, 'sub.example.com', true);
    it('should match subdomain when filter has no leading dot (host-only normalization)', async () => {
        name: 'hostOnlyNormalization',
        value: 'value3',
    it('should not match unrelated domain', async () => {
        name: 'noMatch',
        value: 'value4',
      }, 'other.com', false);
    it('should match domain with a leading dot in both cookie and filter', async () => {
        name: 'leadingDotBoth',
        value: 'value5',
      }, '.example.com', true);
    it('should handle case insensitivity in domain', async () => {
        name: 'caseInsensitive',
        value: 'value7',
        domain: 'Example.com'
    it('should handle IP address matching', async () => {
        url: 'http://127.0.0.1',
        name: 'ipExactMatch',
        value: 'value8',
        domain: '127.0.0.1'
      }, '127.0.0.1', true);
    it('should not match different IP addresses', async () => {
        name: 'ipMismatch',
        value: 'value9',
      }, '127.0.0.2', false);
    it('should handle complex subdomain matching properly', async () => {
      // Set a cookie with domain .example.com
      await testSession.cookies.set({
        url: 'http://a.b.example.com',
        name: 'complexSubdomain',
        value: 'value11',
      // This should match the cookie
      const cookies1 = await testSession.cookies.get({ domain: 'a.b.example.com' });
      expect(cookies1).to.have.lengthOf(1);
      expect(cookies1[0].name).to.equal('complexSubdomain');
      // This should also match
      const cookies2 = await testSession.cookies.get({ domain: 'b.example.com' });
      expect(cookies2).to.have.lengthOf(1);
      const cookies3 = await testSession.cookies.get({ domain: 'example.com' });
      expect(cookies3).to.have.lengthOf(1);
      // This should not match
      const cookies4 = await testSession.cookies.get({ domain: 'otherexample.com' });
      expect(cookies4).to.have.lengthOf(0);
    it('should handle multiple cookies with different domains', async () => {
      // Set two cookies with different domains
        value: 'domain1',
        url: 'http://other.com',
        value: 'domain2',
        domain: 'other.com'
      // Filter for the first domain
      const cookies1 = await testSession.cookies.get({ domain: 'example.com' });
      expect(cookies1[0].name).to.equal('cookie1');
      // Filter for the second domain
      const cookies2 = await testSession.cookies.get({ domain: 'other.com' });
      expect(cookies2[0].name).to.equal('cookie2');
      // Get all cookies
      const allCookies = await testSession.cookies.get({});
      expect(allCookies).to.have.lengthOf(2);
  describe('ses.clearStorageData(options)', () => {
    it('clears localstorage data', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true } });
      await w.loadFile(path.join(fixtures, 'api', 'localstorage.html'));
      await w.webContents.session.clearStorageData({
        origin: 'file://',
        storages: ['localstorage']
      while (await w.webContents.executeJavaScript('localStorage.length') !== 0) {
        // The storage clear isn't instantly visible to the renderer, so keep
        // trying until it is.
  describe('shared dictionary APIs', () => {
    // Shared dictionaries can only be created from real https websites, which we
    // lack the APIs to fake in CI. If you're working on this code, you can run
    // the real-internet tests below by uncommenting the `skip` below.
    // In CI, we'll run simple tests here that ensure that the code in question doesn't
    // crash, even if we expect it to not return any real dictionaries.
    it('can get shared dictionary usage info', async () => {
      expect(await session.defaultSession.getSharedDictionaryUsageInfo()).to.deep.equal([]);
    it('can get shared dictionary info', async () => {
      expect(await session.defaultSession.getSharedDictionaryInfo({
        frameOrigin: 'https://compression-dictionary-transport-threejs-demo.glitch.me',
        topFrameSite: 'https://compression-dictionary-transport-threejs-demo.glitch.me'
      })).to.deep.equal([]);
    it('can clear shared dictionary cache', async () => {
      await session.defaultSession.clearSharedDictionaryCache();
    it('can clear shared dictionary cache for isolation key', async () => {
      await session.defaultSession.clearSharedDictionaryCacheForIsolationKey({
  describe.skip('shared dictionary APIs (using a real website with real dictionaries)', () => {
    const appPath = path.join(fixtures, 'api', 'shared-dictionary');
    const runApp = (command: 'getSharedDictionaryInfo' | 'getSharedDictionaryUsageInfo' | 'clearSharedDictionaryCache' | 'clearSharedDictionaryCacheForIsolationKey') => {
          [appPath, command]
          const trimmedOutput = output.replaceAll(/(\r\n|\n|\r)/gm, '');
            resolve(JSON.parse(trimmedOutput));
            console.error(`Error trying to deserialize ${trimmedOutput}`);
      fs.rmSync(path.join(fixtures, 'api', 'shared-dictionary', 'user-data-dir'), { recursive: true });
      // In our fixture, this calls session.defaultSession.getSharedDictionaryUsageInfo()
      expect(await runApp('getSharedDictionaryUsageInfo')).to.deep.equal([{
        topFrameSite: 'https://compression-dictionary-transport-threejs-demo.glitch.me',
        totalSizeBytes: 1198641
      // In our fixture, this calls session.defaultSession.getSharedDictionaryInfo({
      //   frameOrigin: 'https://compression-dictionary-transport-threejs-demo.glitch.me',
      //   topFrameSite: 'https://compression-dictionary-transport-threejs-demo.glitch.me'
      const sharedDictionaryInfo = await runApp('getSharedDictionaryInfo') as Electron.SharedDictionaryInfo[];
      expect(sharedDictionaryInfo).to.have.lengthOf(1);
      expect(sharedDictionaryInfo[0].match).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].hash).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].lastFetchTime).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].responseTime).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].expirationDuration).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].lastUsedTime).to.not.be.undefined();
      expect(sharedDictionaryInfo[0].size).to.not.be.undefined();
      // In our fixture, this calls session.defaultSession.clearSharedDictionaryCache()
      // followed by session.defaultSession.getSharedDictionaryUsageInfo()
      expect(await runApp('clearSharedDictionaryCache')).to.deep.equal([]);
      // In our fixture, this calls session.defaultSession.clearSharedDictionaryCacheForIsolationKey({
      expect(await runApp('clearSharedDictionaryCacheForIsolationKey')).to.deep.equal([]);
  describe('will-download event', () => {
    it('can cancel default download behavior', async () => {
      const mockFile = Buffer.alloc(1024);
      const contentDisposition = 'inline; filename="mockFile.txt"';
      const downloadServer = http.createServer((req, res) => {
          'Content-Length': mockFile.length,
          'Content-Type': 'application/plain',
          'Content-Disposition': contentDisposition
        res.end(mockFile);
        downloadServer.close();
      const url = (await listen(downloadServer)).url;
      const downloadPrevented: Promise<{itemUrl: string, itemFilename: string, item: Electron.DownloadItem}> = new Promise(resolve => {
        w.webContents.session.once('will-download', function (e, item) {
          resolve({ itemUrl: item.getURL(), itemFilename: item.getFilename(), item });
      const { item, itemUrl, itemFilename } = await downloadPrevented;
      expect(itemUrl).to.equal(url + '/');
      expect(itemFilename).to.equal('mockFile.txt');
      // Delay till the next tick.
      expect(() => item.getURL()).to.throw('DownloadItem used after being destroyed');
  describe('ses.protocol', () => {
    const partitionName = 'temp';
    const protocolName = 'sp';
    let customSession: Session;
    const protocol = session.defaultSession.protocol;
    const handler = (ignoredError: any, callback: Function) => {
      callback({ data: '<script>require(\'electron\').ipcRenderer.send(\'hello\')</script>', mimeType: 'text/html' });
      customSession = session.fromPartition(partitionName);
      await customSession.protocol.registerStringProtocol(protocolName, handler);
      await customSession.protocol.unregisterProtocol(protocolName);
      customSession = null as any;
    it('does not affect defaultSession', () => {
      const result1 = protocol.isProtocolRegistered(protocolName);
      expect(result1).to.equal(false);
      const result2 = customSession.protocol.isProtocolRegistered(protocolName);
      expect(result2).to.equal(true);
    it('handles requests from partition', async () => {
          partition: partitionName,
      w.loadURL(`${protocolName}://fake-host`);
      await once(ipcMain, 'hello');
  describe('ses.setProxy(options)', () => {
    let customSession: Electron.Session;
      customSession = session.fromPartition('proxyconfig');
      if (!created) {
        // Work around for https://github.com/electron/electron/issues/26166 to
        // reduce flake
      await customSession.setProxy(config);
      const proxy = await customSession.resolveProxy('http://example.com/');
      const proxy = await customSession.resolveProxy('http://localhost');
        const proxy = await customSession.resolveProxy('https://google.com');
      const proxy = await customSession.resolveProxy('http://example/');
      await expect(customSession.setProxy(config)).to.eventually.be.rejectedWith(/Invalid mode/);
    it('reload proxy configuration', async () => {
      let proxyPort = 8132;
            return "PROXY myproxy:${proxyPort}";
      const config = { mode: 'pac_script' as const, pacScript: url };
        expect(proxy).to.equal(`PROXY myproxy:${proxyPort}`);
        proxyPort = 8133;
        await customSession.forceReloadProxyConfig();
  describe('ses.resolveHost(host)', () => {
      customSession = session.fromPartition('resolvehost');
    it('resolves ipv4.localhost2', async () => {
      const { endpoints } = await customSession.resolveHost('ipv4.localhost2');
    it('fails to resolve AAAA record for ipv4.localhost2', async () => {
      await expect(customSession.resolveHost('ipv4.localhost2', {
    it('resolves ipv6.localhost2', async () => {
      const { endpoints } = await customSession.resolveHost('ipv6.localhost2');
    it('fails to resolve A record for ipv6.localhost2', async () => {
      await expect(customSession.resolveHost('notfound.localhost2', {
    it('fails to resolve notfound.localhost2', async () => {
      await expect(customSession.resolveHost('notfound.localhost2'))
  describe('ses.getBlobData()', () => {
    const scheme = 'cors-blob';
    const url = `${scheme}://host`;
      await protocol.unregisterProtocol(scheme);
    it('returns blob data for uuid', (done) => {
      const postData = JSON.stringify({
        value: 'hello'
      const content = `<html>
                       let fd = new FormData();
                       fd.append('file', new Blob(['${postData}'], {type:'application/json'}));
                       fetch('${url}', {method:'POST', body: fd });
      protocol.registerStringProtocol(scheme, (request, callback) => {
          if (request.method === 'GET') {
            callback({ data: content, mimeType: 'text/html' });
          } else if (request.method === 'POST') {
            const uuid = request.uploadData![1].blobUUID;
            expect(uuid).to.be.a('string');
            session.defaultSession.getBlobData(uuid!).then(result => {
                expect(result.toString()).to.equal(postData);
  describe('ses.getBlobData() (gc)', () => {
    const waitForBlobDataRejection = (uuid: string) => waitUntil(async () => {
      const attempt = session.defaultSession.getBlobData(uuid)
        .then(() => false)
        .catch(error => String(error).includes('Could not get blob data handle'));
      const deadline = setTimeout(1000).then(() => false);
      const rejected = await Promise.race([attempt, deadline]);
      return rejected;
    const waitForGarbageCollection = (weak: WeakRef<object>) => waitUntil(() => {
      v8Util.runUntilIdle();
      return weak.deref() === undefined;
    const makeContent = (url: string, postData: string) => `<html>
    const registerPostHandler = (
      scheme: string,
      onDataPipe: (dataPipe: unknown) => void
    ) => new Promise<{ uuid: string }>((resolve, reject) => {
            const uploadData = request.uploadData as any;
            const uuid: string = uploadData[1].blobUUID;
            const dataPipe = uploadData[1].dataPipe;
            expect(dataPipe).to.be.ok();
            onDataPipe(dataPipe);
            resolve({ uuid });
    it('keeps blob data alive while wrapper is referenced', async () => {
      const url = `${scheme}://gc-alive-${Date.now()}`;
      const postData = 'payload';
      const content = makeContent(url, postData);
      let heldDataPipe: unknown = null;
      const postInfo = registerPostHandler(scheme, content, dataPipe => {
        heldDataPipe = dataPipe;
        const { uuid } = await postInfo;
        const result = await session.defaultSession.getBlobData(uuid);
        expect(heldDataPipe).to.be.ok();
    it('rejects after wrapper is collected', async () => {
      const url = `${scheme}://gc-released-${Date.now()}`;
        const weak = new WeakRef(heldDataPipe as object);
        heldDataPipe = null;
        await waitForGarbageCollection(weak);
        await waitForBlobDataRejection(uuid);
  describe('ses.getBlobData2()', () => {
                       fd.append("data", new Blob(new Array(65_537).fill('a')));
                const data = new Array(65_537).fill('a');
                expect(result.toString()).to.equal(data.join(''));
  describe('ses.setCertificateVerifyProc(callback)', () => {
      const certPath = path.join(fixtures, 'certificates');
        res.end('<title>hello</title>');
      server.close(done);
    it('accepts the request when the callback is called with 0', async () => {
      const ses = session.fromPartition(`${Math.random()}`);
      let validate: () => void;
      ses.setCertificateVerifyProc(({ hostname, verificationResult, errorCode }, callback) => {
        if (hostname !== '127.0.0.1') return callback(-3);
        validate = () => {
          expect(verificationResult).to.be.oneOf(['net::ERR_CERT_AUTHORITY_INVALID', 'net::ERR_CERT_COMMON_NAME_INVALID']);
          expect(errorCode).to.be.oneOf([-202, -200]);
        callback(0);
      expect(w.webContents.getTitle()).to.equal('hello');
      expect(validate!).not.to.be.undefined();
      validate!();
    it('rejects the request when the callback is called with -2', async () => {
      ses.setCertificateVerifyProc(({ hostname, certificate, verificationResult, isIssuedByKnownRoot }, callback) => {
          expect(certificate.issuerName).to.equal('Intermediate CA');
          expect(certificate.subjectName).to.equal('localhost');
          expect(certificate.issuer.commonName).to.equal('Intermediate CA');
          expect(certificate.subject.commonName).to.equal('localhost');
          expect(certificate.issuerCert.issuer.commonName).to.equal('Root CA');
          expect(certificate.issuerCert.subject.commonName).to.equal('Intermediate CA');
          expect(certificate.issuerCert.issuerCert.issuer.commonName).to.equal('Root CA');
          expect(certificate.issuerCert.issuerCert.subject.commonName).to.equal('Root CA');
          expect(certificate.issuerCert.issuerCert.issuerCert).to.equal(undefined);
          expect(isIssuedByKnownRoot).to.be.false();
        callback(-2);
      await expect(w.loadURL(serverUrl)).to.eventually.be.rejectedWith(/ERR_FAILED/);
    it('saves cached results', async () => {
      let numVerificationRequests = 0;
      ses.setCertificateVerifyProc((e, callback) => {
        if (e.hostname !== '127.0.0.1') return callback(-3);
        numVerificationRequests++;
      await expect(w.loadURL(serverUrl), 'first load').to.eventually.be.rejectedWith(/ERR_FAILED/);
      await once(w.webContents, 'did-stop-loading');
      await expect(w.loadURL(serverUrl + '/test'), 'second load').to.eventually.be.rejectedWith(/ERR_FAILED/);
      expect(numVerificationRequests).to.equal(1);
    it('does not cancel requests in other sessions', async () => {
      const ses1 = session.fromPartition(`${Math.random()}`);
      ses1.setCertificateVerifyProc((opts, cb) => cb(0));
      const ses2 = session.fromPartition(`${Math.random()}`);
      const req = net.request({ url: serverUrl, session: ses1, credentials: 'include' });
        ses2.setCertificateVerifyProc((opts, callback) => callback(0));
      await expect(new Promise<void>((resolve, reject) => {
        req.on('error', (err) => {
        req.on('response', () => {
  describe('ses.clearAuthCache()', () => {
    it('can clear http auth info from cache', async () => {
      const ses = session.fromPartition('auth-cache');
        const credentials = auth(req);
        if (!credentials || credentials.name !== 'test' || credentials.pass !== 'test') {
          res.statusCode = 401;
          res.setHeader('WWW-Authenticate', 'Basic realm="Restricted"');
          res.end('authenticated');
      const fetch = (url: string) => new Promise((resolve, reject) => {
        const request = net.request({ url, session: ses });
        request.on('response', (response) => {
          let data: string | null = null;
          response.on('data', (chunk) => {
              data = '';
            data += chunk;
              reject(new Error('Empty response'));
              resolve(data);
          response.on('error', (error: any) => { reject(new Error(error)); });
        request.on('error', (error: any) => { reject(new Error(error)); });
        request.end();
      // the first time should throw due to unauthenticated
      await expect(fetch(`http://127.0.0.1:${port}`)).to.eventually.be.rejected();
      // passing the password should let us in
      expect(await fetch(`http://test:test@127.0.0.1:${port}`)).to.equal('authenticated');
      // subsequently, the credentials are cached
      expect(await fetch(`http://127.0.0.1:${port}`)).to.equal('authenticated');
      await ses.clearAuthCache();
      // once the cache is cleared, we should get an error again
  describe('DownloadItem', () => {
    const mockPDF = Buffer.alloc(1024 * 1024 * 5);
    const downloadFilePath = path.join(__dirname, '..', 'fixtures', 'mock.pdf');
    const protocolName = 'custom-dl';
    const contentDisposition = 'inline; filename="mock.pdf"';
    let port: number;
    let downloadServer: http.Server;
      downloadServer = http.createServer((req, res) => {
          'Content-Length': mockPDF.length,
          'Content-Type': 'application/pdf',
          'Content-Disposition': req.url === '/?testFilename' ? 'inline' : contentDisposition
        res.end(mockPDF);
      port = (await listen(downloadServer)).port;
      await new Promise(resolve => downloadServer.close(resolve));
    const isPathEqual = (path1: string, path2: string) => {
      return path.relative(path1, path2) === '';
    const assertDownload = (state: string, item: Electron.DownloadItem, isCustom = false) => {
      expect(state).to.equal('completed');
      expect(item.getFilename()).to.equal('mock.pdf');
      expect(path.isAbsolute(item.savePath)).to.equal(true);
      expect(isPathEqual(item.savePath, downloadFilePath)).to.equal(true);
      if (isCustom) {
        expect(item.getURL()).to.equal(`${protocolName}://item`);
        expect(item.getURL()).to.be.equal(`${url}:${port}/`);
      expect(item.getMimeType()).to.equal('application/pdf');
      expect(item.getReceivedBytes()).to.equal(mockPDF.length);
      expect(item.getTotalBytes()).to.equal(mockPDF.length);
      expect(item.getContentDisposition()).to.equal(contentDisposition);
      expect(fs.existsSync(downloadFilePath)).to.equal(true);
      fs.unlinkSync(downloadFilePath);
    describe('session.downloadURL', () => {
      it('can perform a download', async () => {
        const willDownload = once(session.defaultSession, 'will-download');
        session.defaultSession.downloadURL(`${url}:${port}`);
        const [, item] = await willDownload;
        item.savePath = downloadFilePath;
        const [, state] = await once(item, 'done');
        assertDownload(state, item);
      it('can perform a download with a valid auth header', async () => {
          const { authorization } = req.headers;
          if (!authorization || authorization !== 'Basic i-am-an-auth-header') {
        const downloadDone: Promise<Electron.DownloadItem> = new Promise((resolve) => {
          session.defaultSession.once('will-download', (e, item) => {
            item.on('done', () => {
                resolve(item);
        session.defaultSession.downloadURL(`${url}:${port}`, {
            Authorization: 'Basic i-am-an-auth-header'
        const today = Math.floor(Date.now() / 1000);
        const item = await downloadDone;
        expect(item.getState()).to.equal('completed');
        expect(item.getPercentComplete()).to.equal(100);
        expect(item.getCurrentBytesPerSecond()).to.equal(0);
        const start = item.getStartTime();
        const end = item.getEndTime();
        expect(start).to.be.greaterThan(today);
        expect(end).to.be.greaterThan(start);
      it('throws when called with invalid headers', () => {
            // @ts-ignore this line is intentionally incorrect
            headers: 'i-am-a-bad-header'
        }).to.throw(/Invalid value for headers - must be an object/);
      it('correctly handles a download with an invalid auth header', async () => {
        const downloadFailed: Promise<Electron.DownloadItem> = new Promise((resolve) => {
          session.defaultSession.once('will-download', (_, item) => {
            item.on('done', (e, state) => {
              console.log(state);
            Authorization: 'wtf-is-this'
        const item = await downloadFailed;
        expect(item.getState()).to.equal('interrupted');
        expect(item.getReceivedBytes()).to.equal(0);
        expect(item.getTotalBytes()).to.equal(0);
    describe('webContents.downloadURL', () => {
        const willDownload = once(w.webContents.session, 'will-download');
        w.webContents.downloadURL(`${url}:${port}`);
          w.webContents.session.once('will-download', (e, item) => {
        w.webContents.downloadURL(`${url}:${port}`, {
      it('can perform a download with referer header', async () => {
          const { referer } = req.headers;
          if (!referer || !referer.startsWith('http://www.electronjs.org')) {
            res.statusCode = 403;
            // Setting a Referer header with HTTPS scheme while the download URL's
            // scheme is HTTP might lead to download failure.
            referer: 'http://www.electronjs.org'
      it('correctly handles a download and an invalid auth header', async () => {
          w.webContents.session.once('will-download', (_, item) => {
      it('can download from custom protocols', async () => {
          callback({ url: `${url}:${port}` });
        protocol.registerHttpProtocol(protocolName, handler);
        w.webContents.downloadURL(`${protocolName}://item`);
        assertDownload(state, item, true);
      it('can cancel download', async () => {
        w.webContents.downloadURL(`${url}:${port}/`);
        const itemDone = once(item, 'done');
        item.cancel();
        const [, state] = await itemDone;
        expect(state).to.equal('cancelled');
      ifit(process.platform !== 'win32')('can generate a default filename', async () => {
        w.webContents.downloadURL(`${url}:${port}/?testFilename`);
        await itemDone;
        expect(item.getFilename()).to.equal('download.pdf');
      it('can set options for the save dialog', async () => {
        const filePath = path.join(__dirname, 'fixtures', 'mock.pdf');
          message: 'message',
          buttonLabel: 'buttonLabel',
          nameFieldLabel: 'nameFieldLabel',
          defaultPath: '/',
            name: '1', extensions: ['.1', '.2']
            name: '2', extensions: ['.3', '.4', '.5']
          showsTagField: true,
          securityScopedBookmarks: true
        item.setSavePath(filePath);
        item.setSaveDialogOptions(options);
        expect(item.getSaveDialogOptions()).to.deep.equal(options);
      describe('when a save path is specified and the URL is unavailable', () => {
        it('does not display a save dialog and reports the done state as interrupted', async () => {
          w.webContents.downloadURL(`file://${path.join(__dirname, 'does-not-exist.txt')}`);
          if (item.getState() === 'interrupted') {
            item.resume();
          expect(state).to.equal('interrupted');
    describe('WebView.downloadURL', () => {
        function webviewDownload ({ fixtures, url, port }: { fixtures: string, url: string, port: string }) {
          const webview = new (window as any).WebView();
          webview.addEventListener('did-finish-load', () => {
            webview.downloadURL(`${url}:${port}/`);
          webview.src = `file://${fixtures}/api/blank.html`;
          document.body.appendChild(webview);
        const done: Promise<[string, Electron.DownloadItem]> = new Promise(resolve => {
            item.on('done', function (e, state) {
              resolve([state, item]);
        await w.webContents.executeJavaScript(`(${webviewDownload})(${JSON.stringify({ fixtures, url, port })})`);
        const [state, item] = await done;
  describe('ses.createInterruptedDownload(options)', () => {
    it('can create an interrupted download item', async () => {
        path: downloadFilePath,
        urlChain: ['http://127.0.0.1/'],
        mimeType: 'application/pdf',
        length: 5242880
      const p = once(w.webContents.session, 'will-download');
      w.webContents.session.createInterruptedDownload(options);
      const [, item] = await p;
      expect(item.getURLChain()).to.deep.equal(options.urlChain);
      expect(item.getMimeType()).to.equal(options.mimeType);
      expect(item.getReceivedBytes()).to.equal(options.offset);
      expect(item.getTotalBytes()).to.equal(options.length);
      expect(item.savePath).to.equal(downloadFilePath);
    it('can be resumed', async () => {
      const downloadFilePath = path.join(fixtures, 'logo.png');
      const rangeServer = http.createServer((req, res) => {
        const options = { root: fixtures };
        send(req, req.url!, options)
          .on('error', (error: any) => { throw error; }).pipe(res);
        rangeServer.close();
        const { url } = await listen(rangeServer);
        const downloadCancelled: Promise<Electron.DownloadItem> = new Promise((resolve) => {
            item.setSavePath(downloadFilePath);
            item.on('done', function () {
        const downloadUrl = `${url}/assets/logo.png`;
        w.webContents.downloadURL(downloadUrl);
        const item = await downloadCancelled;
        expect(item.getState()).to.equal('cancelled');
          path: item.savePath,
          urlChain: item.getURLChain(),
          mimeType: item.getMimeType(),
          offset: item.getReceivedBytes(),
          length: item.getTotalBytes(),
          lastModified: item.getLastModifiedTime(),
          eTag: item.getETag()
        const downloadResumed: Promise<Electron.DownloadItem> = new Promise((resolve) => {
        const completedItem = await downloadResumed;
        expect(completedItem.getState()).to.equal('completed');
        expect(completedItem.getFilename()).to.equal('logo.png');
        expect(completedItem.savePath).to.equal(downloadFilePath);
        expect(completedItem.getURL()).to.equal(downloadUrl);
        expect(completedItem.getMimeType()).to.equal('image/png');
        expect(completedItem.getReceivedBytes()).to.equal(14022);
        expect(completedItem.getTotalBytes()).to.equal(14022);
  describe('ses.setPermissionRequestHandler(handler)', () => {
    it('cancels any pending requests when cleared', async () => {
          partition: 'very-temp-permission-handler',
      const ses = w.webContents.session;
      ses.setPermissionRequestHandler(() => {
        ses.setPermissionRequestHandler(null);
      ses.protocol.interceptStringProtocol('https', (req, cb) => {
        cb(`<html><script>(${remote})()</script></html>`);
      const result = once(require('electron').ipcMain, 'message');
      function remote () {
        (navigator as any).requestMIDIAccess({ sysex: true }).then(() => {}, (err: any) => {
          require('electron').ipcRenderer.send('message', err.name);
      await w.loadURL('https://myfakesite');
      const [, name] = await result;
      expect(name).to.deep.equal('NotAllowedError');
    it('successfully resolves when calling legacy getUserMedia', async () => {
      ses.setPermissionRequestHandler(
        (_webContents, _permission, callback) => {
          callback(true);
    it('successfully rejects when calling legacy getUserMedia', async () => {
      await expect(w.webContents.executeJavaScript(`
      `)).to.eventually.be.rejectedWith('Permission denied');
  describe('ses.setPermissionCheckHandler(handler)', () => {
    it('details provides requestingURL for mainFrame', async () => {
          partition: 'very-temp-permission-handler'
      const loadUrl = 'https://myfakesite/';
      let handlerDetails : Electron.PermissionCheckHandlerHandlerDetails;
        cb('<html><script>console.log(\'test\');</script></html>');
      ses.setPermissionCheckHandler((wc, permission, requestingOrigin, details) => {
        if (permission === 'clipboard-read') {
          handlerDetails = details;
      const readClipboardPermission: any = () => {
        return w.webContents.executeJavaScript(`
          navigator.permissions.query({name: 'clipboard-read'})
              .then(permission => permission.state).catch(err => err.message);
      await w.loadURL(loadUrl);
      const state = await readClipboardPermission();
      expect(state).to.equal('granted');
      expect(handlerDetails!.requestingUrl).to.equal(loadUrl);
    it('details provides requestingURL for cross origin subFrame', async () => {
      const readClipboardPermission: any = (frame: WebFrameMain) => {
        return frame.executeJavaScript(`
        var iframe = document.createElement('iframe');
        iframe.src = '${loadUrl}';
        iframe.allow = 'clipboard-read';
        document.body.appendChild(iframe);
      const [,, frameProcessId, frameRoutingId] = await once(w.webContents, 'did-frame-finish-load');
      const state = await readClipboardPermission(webFrameMain.fromId(frameProcessId, frameRoutingId));
      expect(handlerDetails!.isMainFrame).to.be.false();
      expect(handlerDetails!.embeddingOrigin).to.equal('file:///');
  describe('ses.isPersistent()', () => {
    it('returns default session as persistent', () => {
      expect(ses.isPersistent()).to.be.true();
    it('returns persist: session as persistent', () => {
      const ses = session.fromPartition(`persist:${Math.random()}`);
    it('returns temporary session as not persistent', () => {
      expect(ses.isPersistent()).to.be.false();
  describe('ses.setUserAgent()', () => {
    it('can be retrieved with getUserAgent()', () => {
      const userAgent = 'test-agent';
      ses.setUserAgent(userAgent);
      expect(ses.getUserAgent()).to.equal(userAgent);
    it('sets the User-Agent header for web requests made from renderers', async () => {
      ses.setUserAgent(userAgent, 'en-US,fr,de');
      let headers: http.IncomingHttpHeaders | null = null;
        headers = req.headers;
      expect(headers!['user-agent']).to.equal(userAgent);
      expect(headers!['accept-language']).to.equal('en-US,fr;q=0.9,de;q=0.8');
  describe('session-created event', () => {
    it('is emitted when a session is created', async () => {
      const sessionCreated = once(app, 'session-created') as Promise<[any, Session]>;
      const session1 = session.fromPartition('' + Math.random());
      const [session2] = await sessionCreated;
      expect(session1).to.equal(session2);
  describe('session.storagePage', () => {
    it('returns a string', () => {
      expect(session.defaultSession.storagePath).to.be.a('string');
    it('returns null for in memory sessions', () => {
      expect(session.fromPartition('in-memory').storagePath).to.equal(null);
    it('returns different paths for partitions and the default session', () => {
      expect(session.defaultSession.storagePath).to.not.equal(session.fromPartition('persist:two').storagePath);
    it('returns different paths for different partitions', () => {
      expect(session.fromPartition('persist:one').storagePath).to.not.equal(session.fromPartition('persist:two').storagePath);
  describe('session.setCodeCachePath()', () => {
    it('throws when relative or empty path is provided', () => {
        session.defaultSession.setCodeCachePath('../fixtures');
      }).to.throw('Absolute path must be provided to store code cache.');
        session.defaultSession.setCodeCachePath('');
        session.defaultSession.setCodeCachePath(path.join(app.getPath('userData'), 'electron-test-code-cache'));
  describe('ses.setSSLConfig()', () => {
    it('can disable cipher suites', async () => {
      const server = https.createServer({
        minVersion: 'TLSv1.2',
        maxVersion: 'TLSv1.2',
        ciphers: 'AES128-GCM-SHA256'
      }, (req, res) => {
        res.end('hi');
      function request () {
          const r = net.request({
            url: `https://127.0.0.1:${port}`,
          r.on('response', (res) => {
            let data = '';
            res.on('data', (chunk) => {
              data += chunk.toString('utf8');
            res.on('end', () => {
          r.end();
      await expect(request()).to.be.rejectedWith(/ERR_CERT_AUTHORITY_INVALID/);
      ses.setSSLConfig({
        disabledCipherSuites: [0x009C]
      await expect(request()).to.be.rejectedWith(/ERR_SSL_VERSION_OR_CIPHER_MISMATCH/);
  describe('ses.clearData()', () => {
    // NOTE: This API clears more than localStorage, but localStorage is a
    // convenient test target for this API
    it('clears all data when no options supplied', async () => {
      expect(await w.webContents.executeJavaScript('localStorage.length')).to.be.greaterThan(0);
      await w.webContents.session.clearData();
      expect(await w.webContents.executeJavaScript('localStorage.length')).to.equal(0);
    it('clears all data when no options supplied, called twice in parallel', async () => {
      // This first call is not awaited immediately
      const clearDataPromise = w.webContents.session.clearData();
      // Await the first promise so it doesn't creep into another test
      await clearDataPromise;
    it('only clears specified data categories', async () => {
        webPreferences: { nodeIntegration: true, contextIsolation: false }
      await w.loadFile(
        path.join(fixtures, 'api', 'localstorage-and-indexeddb.html')
      const { webContents } = w;
      const { session } = webContents;
      await once(ipcMain, 'indexeddb-ready');
      async function queryData (channel: string): Promise<string> {
        const event = once(ipcMain, `result-${channel}`);
        webContents.send(`get-${channel}`);
        return (await event)[1];
      // Data is in localStorage
      await expect(queryData('localstorage')).to.eventually.equal('hello localstorage');
      // Data is in indexedDB
      await expect(queryData('indexeddb')).to.eventually.equal('hello indexeddb');
      // Clear only indexedDB, not localStorage
      await session.clearData({ dataTypes: ['indexedDB'] });
      // The localStorage data should still be there
      // The indexedDB data should be gone
      await expect(queryData('indexeddb')).to.eventually.be.undefined();
    it('only clears the specified origins', async () => {
      const { session } = w.webContents;
      const { cookies } = session;
        cookies.set({
          url: 'https://example.com/',
          name: 'testdotcom',
          value: 'testdotcom'
          url: 'https://example.org/',
          name: 'testdotorg',
          value: 'testdotorg'
      await session.clearData({ origins: ['https://example.com'] });
      expect((await cookies.get({ url: 'https://example.com/', name: 'testdotcom' })).length).to.equal(0);
      expect((await cookies.get({ url: 'https://example.org/', name: 'testdotorg' })).length).to.be.greaterThan(0);
    it('clears all except the specified origins', async () => {
      await session.clearData({ excludeOrigins: ['https://example.com'] });
      expect((await cookies.get({ url: 'https://example.com/', name: 'testdotcom' })).length).to.be.greaterThan(0);
      expect((await cookies.get({ url: 'https://example.org/', name: 'testdotorg' })).length).to.equal(0);
