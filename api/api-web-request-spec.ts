import { ipcMain, net, protocol, session, WebContents, webContents } from 'electron/main';
import * as WebSocket from 'ws';
import { ReadableStream } from 'node:stream/web';
import { listen, defer, startRemoteControlApp } from './lib/spec-helpers';
describe('webRequest module', () => {
  const ses = session.defaultSession;
      res.setHeader('Location', 'http://' + req.rawHeaders[1]);
    } else if (req.url === '/contentDisposition') {
      res.writeHead(200, [
        'content-disposition',
        Buffer.from('attachment; filename=aa中aa.txt').toString('binary')
      const content = req.url;
      res.end(content);
      res.setHeader('Custom', ['Header']);
      let content = req.url;
      if (req.headers.accept === '*/*;test/header') {
        content += 'header/received';
      if (req.headers.origin === 'http://new-origin') {
        content += 'new/origin';
  let defaultURL: string;
    protocol.registerStringProtocol('cors', (req, cb) => cb(''));
    defaultURL = (await listen(server)).url + '/';
    console.log(http2URL);
    protocol.unregisterProtocol('cors');
    // const w = new BrowserWindow({webPreferences: {sandbox: true}})
    // contents = w.webContents
    await contents.loadFile(path.join(fixturesPath, 'pages', 'fetch.html'));
  describe('webRequest.onBeforeRequest', () => {
      ses.webRequest.onBeforeRequest(null);
    const cancel = (details: Electron.OnBeforeRequestListenerDetails, callback: (response: Electron.CallbackResponse) => void) => {
    it('can cancel the request', async () => {
      ses.webRequest.onBeforeRequest(cancel);
      await expect(ajax(defaultURL)).to.eventually.be.rejected();
    it('matches all requests when no filters are defined', async () => {
      await expect(ajax(`${defaultURL}nofilter/test`)).to.eventually.be.rejected();
      await expect(ajax(`${defaultURL}nofilter2/test`)).to.eventually.be.rejected();
    it('can filter URLs', async () => {
      const filter = { urls: [defaultURL + 'filter/*'] };
      ses.webRequest.onBeforeRequest(filter, cancel);
      const { data } = await ajax(`${defaultURL}nofilter/test`);
      expect(data).to.equal('/nofilter/test');
      await expect(ajax(`${defaultURL}filter/test`)).to.eventually.be.rejected();
    it('can filter all URLs with syntax <all_urls>', async () => {
      const filter = { urls: ['<all_urls>'] };
    it('can filter URLs with overlapping patterns of urls and excludeUrls', async () => {
      // If filter matches both urls and excludeUrls, it should be excluded.
      const filter = { urls: [defaultURL + 'filter/*'], excludeUrls: [defaultURL + 'filter/test'] };
      const { data } = await ajax(`${defaultURL}filter/test`);
      expect(data).to.equal('/filter/test');
    it('can filter URLs with multiple excludeUrls patterns', async () => {
      const filter = { urls: [defaultURL + 'filter/*'], excludeUrls: [defaultURL + 'filter/exclude1/*', defaultURL + 'filter/exclude2/*'] };
      expect((await ajax(`${defaultURL}filter/exclude1/test`)).data).to.equal('/filter/exclude1/test');
      expect((await ajax(`${defaultURL}filter/exclude2/test`)).data).to.equal('/filter/exclude2/test');
      // expect non-excluded URL to pass filter
    it('can filter URLs with empty excludeUrls', async () => {
      const filter = { urls: [defaultURL + 'filter/*'], excludeUrls: [] };
    it('can filter URLs and types', async () => {
      const filter1: Electron.WebRequestFilter = { urls: [defaultURL + 'filter/*'], types: ['xhr'] };
      ses.webRequest.onBeforeRequest(filter1, cancel);
      const filter2: Electron.WebRequestFilter = { urls: [defaultURL + 'filter/*'], types: ['stylesheet'] };
      ses.webRequest.onBeforeRequest(filter2, cancel);
      expect((await ajax(`${defaultURL}nofilter/test`)).data).to.equal('/nofilter/test');
      expect((await ajax(`${defaultURL}filter/test`)).data).to.equal('/filter/test');
    it('can filter URLs, excludeUrls and types', async () => {
      const filter1: Electron.WebRequestFilter = { urls: [defaultURL + 'filter/*'], excludeUrls: [defaultURL + 'exclude/*'], types: ['xhr'] };
      expect((await ajax(`${defaultURL}exclude/test`)).data).to.equal('/exclude/test');
      const filter2: Electron.WebRequestFilter = { urls: [defaultURL + 'filter/*'], excludeUrls: [defaultURL + 'exclude/*'], types: ['stylesheet'] };
    // allowExtensions changes how URLPattern works, so we add extra tests that ensure that filters still work as expected.
    describe('with protocol.registerSchemesAsPrivileged() and allowExtensions', () => {
      it('will filter http URLs properly', async () => {
        const rc = await startRemoteControlApp(['--boot-eval="protocol.registerSchemesAsPrivileged([{ scheme: \'custom\', privileges: { allowExtensions: true } }]);"']);
        const called = await rc.remotely(async (url: string) => {
          const { BrowserWindow, session } = require('electron/main');
          session.defaultSession.webRequest.onBeforeRequest({ urls: ['http://*/*'] }, (_: Electron.OnBeforeRequestListenerDetails, callback: (response: Electron.CallbackResponse) => void) => {
          await w.webContents.executeJavaScript(`fetch("${url}").then(() => true, () => false)`);
          global.setTimeout(() => require('electron').app.quit());
          return called;
        }, defaultURL);
        expect(called).to.be.true();
      it('will not call webRequest.onBeforeRequest for non-custom protocol URLs that do not match the filter', async () => {
          session.defaultSession.webRequest.onBeforeRequest({ urls: ['https://*/*'] }, (_: Electron.OnBeforeRequestListenerDetails, callback: (response: Electron.CallbackResponse) => void) => {
        expect(called).to.be.false();
      it('will call webRequest.onBeforeRequest for custom protocol URLs with <all_urls> filter', async () => {
        const { called, responseText } = await rc.remotely(async () => {
          const { net, protocol, session } = require('electron/main');
          protocol.handle('custom', () => new Response('success'));
          session.defaultSession.webRequest.onBeforeRequest({ urls: ['<all_urls>'] }, (_: Electron.OnBeforeRequestListenerDetails, callback: (response: Electron.CallbackResponse) => void) => {
            callback({ cancel: false });
          const response = await net.fetch('custom://app/test');
          const responseText = await response.text();
          return { called, responseText };
        expect(responseText).to.equal('success');
      it('will not call webRequest.onBeforeRequest for custom protocol URLs that do not match the filter', async () => {
    it('receives details object', async () => {
      ses.webRequest.onBeforeRequest((details, callback) => {
        expect(details.id).to.be.a('number');
        expect(details.timestamp).to.be.a('number');
        expect(details.webContentsId).to.be.a('number');
        expect(details.webContents).to.be.an('object');
        expect(details.webContents!.id).to.equal(details.webContentsId);
        expect(details.frame).to.be.an('object');
        expect(details.url).to.be.a('string').that.is.equal(defaultURL);
        expect(details.method).to.be.a('string').that.is.equal('GET');
        expect(details.resourceType).to.be.a('string').that.is.equal('xhr');
        expect(details.uploadData).to.be.undefined();
      const { data } = await ajax(defaultURL);
      expect(data).to.equal('/');
    it('receives post data in details object', async () => {
        expect(details.url).to.equal(defaultURL);
        expect(details.method).to.equal('POST');
        expect(details.uploadData).to.have.lengthOf(1);
        const data = qs.parse(details.uploadData[0].bytes.toString());
        expect(data).to.deep.equal(postData);
      await expect(ajax(defaultURL, {
        body: qs.stringify(postData)
      })).to.eventually.be.rejected();
    it('can redirect the request', async () => {
        if (details.url === defaultURL) {
          callback({ redirectURL: `${defaultURL}redirect` });
      expect(data).to.equal('/redirect');
    it('does not crash for redirects', async () => {
      await ajax(defaultURL + 'serverRedirect');
    it('works with file:// protocol', async () => {
      const fileURL = url.format({
        pathname: path.join(fixturesPath, 'blank.html').replaceAll('\\', '/'),
      await expect(ajax(fileURL)).to.eventually.be.rejected();
    it('can handle a streaming upload', async () => {
      // Streaming fetch uploads are only supported on HTTP/2, which is only
      const contents = (webContents as typeof ElectronInternal.WebContents).create({ sandbox: true });
      defer(() => contents.close());
      await contents.loadURL(http2URL);
      const result = await contents.executeJavaScript(`
        fetch("${http2URL}", {
          body: stream,
          duplex: 'half',
        }).then(r => r.text())
      expect(result).to.equal('hello world');
    it('can handle a streaming upload if the uploadData is read', async () => {
      function makeStreamFromPipe (pipe: any): ReadableStream {
                controller.enqueue(buf.subarray(0, rv));
      ses.webRequest.onBeforeRequest(async (details, callback) => {
        for await (const chunk of makeStreamFromPipe((details.uploadData[0] as any).body)) { chunks.push(chunk); }
      // NOTE: since the upload stream was consumed by the onBeforeRequest
      // handler, it can't be used again to upload to the actual server.
      // This is a limitation of the WebRequest API.
      expect(result).to.equal('');
  describe('webRequest.onBeforeSendHeaders', () => {
      ses.webRequest.onBeforeSendHeaders(null);
      ses.webRequest.onSendHeaders(null);
      ses.webRequest.onBeforeSendHeaders((details, callback) => {
        expect(details.requestHeaders).to.be.an('object');
        expect(details.requestHeaders['Foo.Bar']).to.equal('baz');
      const { data } = await ajax(defaultURL, { headers: { 'Foo.Bar': 'baz' } });
    it('can change the request headers', async () => {
        const requestHeaders = details.requestHeaders;
        requestHeaders.Accept = '*/*;test/header';
        callback({ requestHeaders });
      expect(data).to.equal('/header/received');
    it('can change the request headers on a custom protocol redirect', async () => {
      protocol.registerStringProtocol('no-cors', (req, callback) => {
        if (req.url === 'no-cors://fake-host/redirect') {
              Location: 'no-cors://fake-host'
          if (req.headers.Accept === '*/*;test/header') {
            content = 'header-received';
          callback(content);
        const { data } = await ajax('no-cors://fake-host/redirect');
        expect(data).to.equal('header-received');
        protocol.unregisterProtocol('no-cors');
    it('can change request origin', async () => {
        requestHeaders.Origin = 'http://new-origin';
      expect(data).to.equal('/new/origin');
    it('can capture CORS requests', async () => {
        callback({ requestHeaders: details.requestHeaders });
      await ajax('cors://host');
    it('resets the whole headers', async () => {
        Test: 'header'
      ses.webRequest.onSendHeaders((details) => {
        expect(details.requestHeaders).to.deep.equal(requestHeaders);
      await ajax(defaultURL);
    it('leaves headers unchanged when no requestHeaders in callback', async () => {
      let originalRequestHeaders: Record<string, string>;
        originalRequestHeaders = details.requestHeaders;
        expect(details.requestHeaders).to.deep.equal(originalRequestHeaders);
      let onSendHeadersCalled = false;
        onSendHeadersCalled = true;
      await ajax(url.format({
      expect(onSendHeadersCalled).to.be.true();
    it('can inject Proxy-Authorization header for net module requests', async () => {
      // Proxy-Authorization is normally rejected by Chromium's network service
      // for security reasons. However, for Electron's trusted net module,
      // webRequest.onBeforeSendHeaders should be able to inject it via the
      // TrustedHeaderClient code path.
      const proxyAuthValue = 'Basic test-credentials';
      let receivedProxyAuth: string | undefined;
        receivedProxyAuth = req.headers['proxy-authorization'];
      const { url: serverUrl } = await listen(server);
          requestHeaders['Proxy-Authorization'] = proxyAuthValue;
        const response = await net.fetch(serverUrl, { bypassCustomProtocolHandlers: true });
        expect(response.ok).to.be.true();
        expect(receivedProxyAuth).to.equal(proxyAuthValue);
  describe('webRequest.onSendHeaders', () => {
  describe('webRequest.onHeadersReceived', () => {
      ses.webRequest.onHeadersReceived(null);
      ses.webRequest.onHeadersReceived((details, callback) => {
        expect(details.statusLine).to.equal('HTTP/1.1 200 OK');
        expect(details.statusCode).to.equal(200);
        expect(details.responseHeaders!.Custom).to.deep.equal(['Header']);
    it('can change the response header', async () => {
        const responseHeaders = details.responseHeaders!;
        responseHeaders.Custom = ['Changed'] as any;
        callback({ responseHeaders });
      const { headers } = await ajax(defaultURL);
      expect(headers).to.to.have.property('custom', 'Changed');
    it('can change response origin', async () => {
        responseHeaders['access-control-allow-origin'] = ['http://new-origin'] as any;
      expect(headers).to.to.have.property('access-control-allow-origin', 'http://new-origin');
    it('can change headers of CORS responses', async () => {
      const { headers } = await ajax('cors://host');
    it('does not change header by default', async () => {
      const { data, headers } = await ajax(defaultURL);
      expect(headers).to.to.have.property('custom', 'Header');
    it('does not change content-disposition header by default', async () => {
        expect(details.responseHeaders!['content-disposition']).to.deep.equal(['attachment; filename=aa中aa.txt']);
      const { data, headers } = await ajax(defaultURL + 'contentDisposition');
      const disposition = Buffer.from('attachment; filename=aa中aa.txt').toString('binary');
      expect(headers).to.to.have.property('content-disposition', disposition);
      expect(data).to.equal('/contentDisposition');
    it('follows server redirect', async () => {
        const responseHeaders = details.responseHeaders;
      const { headers } = await ajax(defaultURL + 'serverRedirect');
    it('can change the header status', async () => {
          responseHeaders,
          statusLine: 'HTTP/1.1 404 Not Found'
  describe('webRequest.onResponseStarted', () => {
      ses.webRequest.onResponseStarted(null);
      ses.webRequest.onResponseStarted((details) => {
        expect(details.fromCache).to.be.a('boolean');
  describe('webRequest.onBeforeRedirect', () => {
      ses.webRequest.onBeforeRedirect(null);
      const redirectURL = defaultURL + 'redirect';
          callback({ redirectURL });
      ses.webRequest.onBeforeRedirect((details) => {
        expect(details.statusLine).to.equal('HTTP/1.1 307 Internal Redirect');
        expect(details.statusCode).to.equal(307);
        expect(details.redirectURL).to.equal(redirectURL);
  describe('webRequest.onCompleted', () => {
      ses.webRequest.onCompleted(null);
      ses.webRequest.onCompleted((details) => {
  describe('webRequest.onErrorOccurred', () => {
      ses.webRequest.onErrorOccurred(null);
      ses.webRequest.onErrorOccurred((details) => {
        expect(details.error).to.equal('net::ERR_BLOCKED_BY_CLIENT');
  describe('WebSocket connections', () => {
    it('can be proxyed', async () => {
      // Setup server.
      const reqHeaders : { [key: string] : any } = {};
      let server = http.createServer((req, res) => {
        reqHeaders[req.url!] = req.headers;
        res.setHeader('foo1', 'bar1');
      let wss = new WebSocket.Server({ noServer: true });
      wss.on('connection', function connection (ws) {
        ws.on('message', function incoming (message) {
          if (message === 'foo') {
            ws.send('bar');
      server.on('upgrade', function upgrade (request, socket, head) {
        const pathname = new URL(request.url!, `http://${request.headers.host}`).pathname;
        if (pathname === '/websocket') {
          reqHeaders[request.url!] = request.headers;
          wss.handleUpgrade(request, socket as Socket, head, function done (ws) {
            wss.emit('connection', ws, request);
      // Start server.
      // Use a separate session for testing.
      const ses = session.fromPartition('WebRequestWebSocket');
      // Setup listeners.
      const receivedHeaders : { [key: string] : any } = {};
        details.requestHeaders.foo = 'bar';
        const pathname = new URL(details.url).pathname;
        receivedHeaders[pathname] = details.responseHeaders;
        if (details.url.startsWith('ws://')) {
          expect(details.responseHeaders!.Connection[0]).be.equal('Upgrade');
        } else if (details.url.startsWith('http')) {
          expect(details.responseHeaders!.foo1[0]).be.equal('bar1');
          expect(details.requestHeaders.foo).be.equal('bar');
          expect(details.requestHeaders.Upgrade).be.equal('websocket');
          expect(details.error).be.equal('net::ERR_WS_UPGRADE');
          expect(details.error).be.equal('net::OK');
      const contents = (webContents as typeof ElectronInternal.WebContents).create({
        session: ses,
        webSecurity: false,
        wss.close();
        wss = null as unknown as WebSocket.Server;
      contents.loadFile(path.join(fixturesPath, 'api', 'webrequest.html'), { query: { port: `${port}` } });
      await once(ipcMain, 'websocket-success');
      expect(receivedHeaders['/websocket'].Upgrade[0]).to.equal('websocket');
      expect(receivedHeaders['/'].foo1[0]).to.equal('bar1');
      expect(reqHeaders['/websocket'].foo).to.equal('bar');
      expect(reqHeaders['/'].foo).to.equal('bar');
    it('authenticates a WebSocket via login event', async () => {
      const authServer = http.createServer();
      const wssAuth = new WebSocket.Server({ noServer: true });
      const expected = 'Basic ' + Buffer.from('user:pass').toString('base64');
      wssAuth.on('connection', ws => {
        ws.send('Authenticated!');
      authServer.on('upgrade', (req, socket, head) => {
        const auth = req.headers.authorization || '';
        if (auth !== expected) {
          socket.write(
            'HTTP/1.1 401 Unauthorized\r\n' +
              'WWW-Authenticate: Basic realm="Test"\r\n' +
              'Content-Length: 0\r\n' +
              '\r\n'
          socket.destroy();
        wssAuth.handleUpgrade(req, socket as Socket, head, ws => {
          wssAuth.emit('connection', ws, req);
      const { port } = await listen(authServer);
      const ses = session.fromPartition(`WebRequestWSAuth-${Date.now()}`);
        authServer.close();
        wssAuth.close();
      ses.webRequest.onBeforeRequest({ urls: ['ws://*/*'] }, (details, callback) => {
      contents.on('login', (event, details: any, _: any, callback: (u: string, p: string) => void) => {
        if (details?.url?.startsWith(`ws://localhost:${port}`)) {
          callback('user', 'pass');
      await contents.loadFile(path.join(fixturesPath, 'blank.html'));
      const message = await contents.executeJavaScript(`new Promise((resolve, reject) => {
        function connect() {
          const ws = new WebSocket('ws://localhost:${port}');
          ws.onmessage = e => resolve(e.data);
          ws.onerror = () => {
            if (attempts < 3) {
              setTimeout(connect, 50);
              reject(new Error('WebSocket auth failed'));
        connect();
        setTimeout(() => reject(new Error('timeout')), 5000);
      expect(message).to.equal('Authenticated!');
