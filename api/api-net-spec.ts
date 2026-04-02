import { net, session, ClientRequest, ClientRequestConstructorOptions, utilityProcess } from 'electron/main';
import * as http2 from 'node:http2';
import { collectStreamBody, collectStreamBodyBuffer, getResponse, kOneKiloByte, kOneMegaByte, randomBuffer, randomString, respondNTimes, respondOnce } from './lib/net-helpers';
import { listen, defer, ifdescribe, isTestingBindingAvailable } from './lib/spec-helpers';
async function itUtility (name: string, fn?: Function, args?: {[key:string]: any}) {
  it(`${name} in utility process`, async () => {
    if (fn) {
      child.postMessage({ fn: `(${fn})()`, args });
      child.postMessage({ fn: '(() => {})()', args });
async function itIgnoringArgs (name: string, fn?: Mocha.Func|Mocha.AsyncFunc, args?: {[key:string]: any}) {
  it(name, fn);
describe('net module', () => {
  let http2URL: string;
  const h2server = http2.createSecureServer({
    cert: fs.readFileSync(path.join(certPath, 'server.pem'))
  }, async (req, res) => {
      const chunks = [];
      for await (const chunk of req) chunks.push(chunk);
      res.end(Buffer.concat(chunks).toString('utf8'));
    } else if (req.method === 'GET' && req.headers[':path'] === '/get') {
      res.end(JSON.stringify({
        headers: req.headers
      res.end('<html></html>');
    http2URL = (await listen(h2server)).url + '/';
    h2server.close();
  for (const test of [itIgnoringArgs, itUtility]) {
      test('should be able to issue a basic GET request', async () => {
          expect(request.method).to.equal('GET');
        const urlRequest = net.request(serverUrl);
      test('should be able to issue a basic POST request', async () => {
          expect(request.method).to.equal('POST');
          url: serverUrl
      test('should fetch correct data in a GET request', async () => {
        const expectedBodyData = 'Hello World!';
          response.end(expectedBodyData);
        expect(body).to.equal(expectedBodyData);
      test('should post the correct data in a POST request', async () => {
        let postedBodyData: string = '';
          postedBodyData = await collectStreamBody(request);
        urlRequest.write(bodyData);
      test('a 307 redirected POST request preserves the body', async () => {
        let methodAfterRedirect: string | undefined;
        const serverUrl = await respondNTimes.toRoutes({
          '/redirect': (req, res) => {
            res.statusCode = 307;
            res.setHeader('location', serverUrl);
            return res.end();
          '/': async (req, res) => {
            methodAfterRedirect = req.method;
            postedBodyData = await collectStreamBody(req);
          url: serverUrl + '/redirect'
        expect(methodAfterRedirect).to.equal('POST');
      test('a 302 redirected POST request DOES NOT preserve the body', async () => {
        expect(methodAfterRedirect).to.equal('GET');
        expect(postedBodyData).to.equal('');
      test('should support chunked encoding', async () => {
        let receivedRequest: http.IncomingMessage = null as any;
          response.chunkedEncoding = true;
          receivedRequest = request;
          request.on('data', (chunk: Buffer) => {
            response.write(chunk);
          request.on('end', (chunk: Buffer) => {
            response.end(chunk);
        let chunkIndex = 0;
        const chunkCount = 100;
        let sent = Buffer.alloc(0);
        urlRequest.chunkedEncoding = true;
        while (chunkIndex < chunkCount) {
          chunkIndex += 1;
          const chunk = randomBuffer(kOneKiloByte);
          sent = Buffer.concat([sent, chunk]);
          urlRequest.write(chunk);
        expect(receivedRequest.method).to.equal('POST');
        expect(receivedRequest.headers['transfer-encoding']).to.equal('chunked');
        expect(receivedRequest.headers['content-length']).to.equal(undefined);
        const received = await collectStreamBodyBuffer(response);
        expect(sent.equals(received)).to.be.true();
        expect(chunkIndex).to.be.equal(chunkCount);
          test('should emit the login event when 401', async () => {
              response.writeHead(200).end('ok');
            let loginAuthInfo: Electron.AuthInfo;
            request.on('login', (authInfo, cb) => {
              loginAuthInfo = authInfo;
            expect(loginAuthInfo!.realm).to.equal('Foo');
            expect(loginAuthInfo!.scheme).to.equal('basic');
          }, { extraOptions });
          test('should receive 401 response when cancelling authentication', async () => {
                response.writeHead(401, { 'WWW-Authenticate': 'Basic realm="Foo"' });
                response.end('unauthenticated');
              cb();
            expect(body).to.equal('unauthenticated');
          test('should upload body when 401', async () => {
              response.writeHead(200);
              request.on('data', (chunk) => response.write(chunk));
              request.on('end', () => response.end());
            const requestData = randomString(kOneKiloByte);
            request.write(requestData);
            const responseData = await collectStreamBody(response);
            expect(responseData).to.equal(requestData);
        test('should not emit the login event when 401', async () => {
            expect.fail('unexpected login event');
      test('request/response objects should emit expected events', async () => {
        const bodyData = randomString(kOneKiloByte);
          response.end(bodyData);
        // request close event
        const closePromise = once(urlRequest, 'close');
        // request finish event
        const finishPromise = once(urlRequest, 'close');
        // request "response" event
        response.on('error', (error: Error) => {
          expect(error).to.be.an('Error');
        const statusCode = response.statusCode;
        expect(statusCode).to.equal(200);
        // response data event
        // respond end event
        expect(body).to.equal(bodyData);
        urlRequest.on('error', (error) => {
        await Promise.all([closePromise, finishPromise]);
      test('should be able to set a custom HTTP request header before first write', async () => {
        const customHeaderName = 'Some-Custom-Header-Name';
        const customHeaderValue = 'Some-Customer-Header-Value';
          expect(request.headers[customHeaderName.toLowerCase()]).to.equal(customHeaderValue);
        urlRequest.setHeader(customHeaderName, customHeaderValue);
        expect(urlRequest.getHeader(customHeaderName)).to.equal(customHeaderValue);
        expect(urlRequest.getHeader(customHeaderName.toLowerCase())).to.equal(customHeaderValue);
        urlRequest.write('');
      test('should be able to set a non-string object as a header value', async () => {
        const customHeaderName = 'Some-Integer-Value';
        const customHeaderValue = 900;
          expect(request.headers[customHeaderName.toLowerCase()]).to.equal(customHeaderValue.toString());
        urlRequest.setHeader(customHeaderName, customHeaderValue as any);
      test('should not change the case of header name', async () => {
        const customHeaderName = 'X-Header-Name';
        const customHeaderValue = 'value';
          expect(request.rawHeaders.includes(customHeaderName)).to.equal(true);
      test('should not be able to set a custom HTTP request header after first write', async () => {
          expect(request.headers[customHeaderName.toLowerCase()]).to.equal(undefined);
        expect(urlRequest.getHeader(customHeaderName)).to.equal(undefined);
      test('should be able to remove a custom HTTP request header before first write', async () => {
        urlRequest.removeHeader(customHeaderName);
      test('should not be able to remove a custom HTTP request header after first write', async () => {
      test('should keep the order of headers', async () => {
        const customHeaderNameA = 'X-Header-100';
        const customHeaderNameB = 'X-Header-200';
          const headerNames = Array.from(Object.keys(request.headers));
          const headerAIndex = headerNames.indexOf(customHeaderNameA.toLowerCase());
          const headerBIndex = headerNames.indexOf(customHeaderNameB.toLowerCase());
          expect(headerBIndex).to.be.below(headerAIndex);
        urlRequest.setHeader(customHeaderNameB, 'b');
        urlRequest.setHeader(customHeaderNameA, 'a');
      test('should be able to receive cookies', async () => {
        const cookie = ['cookie1', 'cookie2'];
          response.setHeader('set-cookie', cookie);
        expect(response.headers['set-cookie']).to.have.same.members(cookie);
      test('should be able to receive content-type', async () => {
        const contentType = 'mime/test; charset=test';
          response.setHeader('content-type', contentType);
        expect(response.headers['content-type']).to.equal(contentType);
      describe('when {"credentials":"omit"}', () => {
        test('should not send cookies');
        test('should not store cookies');
      test('should set sec-fetch-site to same-origin for request from same origin', async () => {
          expect(request.headers['sec-fetch-site']).to.equal('same-origin');
          origin: serverUrl
      test('should set sec-fetch-site to same-origin for request with the same origin header', async () => {
        urlRequest.setHeader('Origin', serverUrl);
      test('should set sec-fetch-site to cross-site for request from other origin', async () => {
          expect(request.headers['sec-fetch-site']).to.equal('cross-site');
          origin: 'https://not-exists.com'
      test('should not send sec-fetch-user header by default', async () => {
          expect(request.headers).not.to.have.property('sec-fetch-user');
      test('should set sec-fetch-user to ?1 if requested', async () => {
          expect(request.headers['sec-fetch-user']).to.equal('?1');
        urlRequest.setHeader('sec-fetch-user', '?1');
      test('should set sec-fetch-mode to no-cors by default', async () => {
          expect(request.headers['sec-fetch-mode']).to.equal('no-cors');
      for (const mode of ['navigate', 'cors', 'no-cors', 'same-origin']) {
        test(`should set sec-fetch-mode to ${mode} if requested`, async () => {
            expect(request.headers['sec-fetch-mode']).to.equal(mode);
          urlRequest.setHeader('sec-fetch-mode', mode);
        }, { mode });
      test('should set sec-fetch-dest to empty by default', async () => {
          expect(request.headers['sec-fetch-dest']).to.equal('empty');
      for (const dest of [
        'empty', 'audio', 'audioworklet', 'document', 'embed', 'font',
        'frame', 'iframe', 'image', 'manifest', 'object', 'paintworklet',
        'report', 'script', 'serviceworker', 'style', 'track', 'video',
        'worker', 'xslt'
        test(`should set sec-fetch-dest to ${dest} if requested`, async () => {
            expect(request.headers['sec-fetch-dest']).to.equal(dest);
          urlRequest.setHeader('sec-fetch-dest', dest);
        }, { dest });
      test('should be able to abort an HTTP request before first write', async () => {
          expect.fail('Unexpected request event');
        urlRequest.on('response', () => {
          expect.fail('unexpected response event');
        const aborted = once(urlRequest, 'abort');
        urlRequest.abort();
        urlRequest.end();
        await aborted;
      test('it should be able to abort an HTTP request before request end', async () => {
        let requestReceivedByServer = false;
        let urlRequest: ClientRequest | null = null;
        const serverUrl = await respondOnce.toSingleURL(() => {
          requestReceivedByServer = true;
          urlRequest!.abort();
        let requestAbortEventEmitted = false;
        urlRequest = net.request(serverUrl);
          expect.fail('Unexpected response event');
        urlRequest.on('finish', () => {
          expect.fail('Unexpected finish event');
        urlRequest.on('error', () => {
          expect.fail('Unexpected error event');
        urlRequest.on('abort', () => {
          requestAbortEventEmitted = true;
        const p = once(urlRequest, 'close');
        urlRequest.write(randomString(kOneKiloByte));
        expect(requestReceivedByServer).to.equal(true);
        expect(requestAbortEventEmitted).to.equal(true);
      test('it should be able to abort an HTTP request after request end and before response', async () => {
        let requestFinishEventEmitted = false;
          requestFinishEventEmitted = true;
        urlRequest.end(randomString(kOneKiloByte));
        await once(urlRequest, 'abort');
        expect(requestFinishEventEmitted).to.equal(true);
      test('it should be able to abort an HTTP request after response start', async () => {
          response.write(randomString(kOneKiloByte));
        let requestResponseEventEmitted = false;
        let responseCloseEventEmitted = false;
        urlRequest.on('response', (response) => {
          requestResponseEventEmitted = true;
          response.on('data', () => {});
          response.on('end', () => {
            expect.fail('Unexpected end event');
          response.on('error', () => {
          response.on('close' as any, () => {
            responseCloseEventEmitted = true;
        expect(requestFinishEventEmitted).to.be.true('request should emit "finish" event');
        expect(requestReceivedByServer).to.be.true('request should be received by the server');
        expect(requestResponseEventEmitted).to.be.true('"response" event should be emitted');
        expect(responseCloseEventEmitted).to.be.true('response should emit "close" event');
      test('abort event should be emitted at most once', async () => {
        let abortsEmitted = 0;
          abortsEmitted++;
        expect(requestReceivedByServer).to.be.true('request should be received by server');
        expect(abortsEmitted).to.equal(1, 'request should emit exactly 1 "abort" event');
      test('should allow to read response body from non-2xx response', async () => {
          response.statusCode = 404;
        const bodyCheckPromise = getResponse(urlRequest).then(r => {
          expect(r.statusCode).to.equal(404);
        }).then(collectStreamBody).then(receivedBodyData => {
          expect(receivedBodyData.toString()).to.equal(bodyData);
        const eventHandlers = Promise.all([
          bodyCheckPromise,
          once(urlRequest, 'close')
        await eventHandlers;
      test('should throw when calling getHeader without a name', () => {
          (net.request({ url: 'https://test' }).getHeader as any)();
        }).to.throw(/`name` is required for getHeader\(name\)/);
          net.request({ url: 'https://test' }).getHeader(null as any);
      test('should throw when calling removeHeader without a name', () => {
          (net.request({ url: 'https://test' }).removeHeader as any)();
        }).to.throw(/`name` is required for removeHeader\(name\)/);
          net.request({ url: 'https://test' }).removeHeader(null as any);
      test('should follow redirect when no redirect handler is provided', async () => {
        const requestUrl = '/302';
        const serverUrl = await respondOnce.toRoutes({
          '/302': (request, response) => {
          '/200': (request, response) => {
          url: `${serverUrl}${requestUrl}`
      test('should follow redirect chain when no redirect handler is provided', async () => {
          '/redirectChain': (request, response) => {
            response.setHeader('Location', '/302');
          url: `${serverUrl}/redirectChain`
      test('should not follow redirect when request is canceled in redirect handler', async () => {
        urlRequest.on('redirect', () => { urlRequest.abort(); });
        urlRequest.on('error', () => {});
          expect.fail('Unexpected response');
      test('should not follow redirect when mode is error', async () => {
          redirect: 'error'
        await once(urlRequest, 'error');
      test('should follow redirect when handler calls callback', async () => {
        const urlRequest = net.request({ url: `${serverUrl}/redirectChain`, redirect: 'manual' });
        const redirects: string[] = [];
        urlRequest.on('redirect', (status, method, url) => {
          redirects.push(url);
        expect(redirects).to.deep.equal([
          `${serverUrl}/302`,
          `${serverUrl}/200`
      test('should be able to create a request with options', async () => {
        const serverUrlUnparsed = await respondOnce.toURL('/', (request, response) => {
        const serverUrl = new URL(serverUrlUnparsed);
          port: serverUrl.port ? parseInt(serverUrl.port, 10) : undefined,
          hostname: '127.0.0.1',
          headers: { [customHeaderName]: customHeaderValue }
        expect(response.statusCode).to.be.equal(200);
      test('should be able to pipe a readable stream into a net request', async () => {
        const bodyData = randomString(kOneMegaByte);
        let netRequestReceived = false;
        let netRequestEnded = false;
        const [nodeServerUrl, netServerUrl] = await Promise.all([
          respondOnce.toSingleURL((request, response) => response.end(bodyData)),
          respondOnce.toSingleURL((request, response) => {
            netRequestReceived = true;
            let receivedBodyData = '';
            request.on('data', (chunk) => {
              receivedBodyData += chunk.toString();
            request.on('end', (chunk: Buffer | undefined) => {
              netRequestEnded = true;
              expect(receivedBodyData).to.be.equal(bodyData);
        const nodeRequest = http.request(nodeServerUrl);
        const nodeResponse = await getResponse(nodeRequest as any) as any as http.ServerResponse;
        const netRequest = net.request(netServerUrl);
        const responsePromise = once(netRequest, 'response');
        // TODO(@MarshallOfSound) - FIXME with #22730
        nodeResponse.pipe(netRequest as any);
        const [netResponse] = await responsePromise;
        expect(netResponse.statusCode).to.equal(200);
        await collectStreamBody(netResponse);
        expect(netRequestReceived).to.be.true('net request received');
        expect(netRequestEnded).to.be.true('net request ended');
      test('should report upload progress', async () => {
        const netRequest = net.request({ url: serverUrl, method: 'POST' });
        expect(netRequest.getUploadProgress()).to.have.property('active', false);
        netRequest.end(Buffer.from('hello'));
        const [position, total] = await once(netRequest, 'upload-progress');
        expect(netRequest.getUploadProgress()).to.deep.equal({ active: true, started: true, current: position, total });
      test('should emit error event on server socket destroy', async () => {
        const serverUrl = await respondOnce.toSingleURL((request) => {
          request.socket.destroy();
        const [error] = await once(urlRequest, 'error');
        expect(error.message).to.equal('net::ERR_EMPTY_RESPONSE');
      test('should emit error event on server request destroy', async () => {
          request.destroy();
        urlRequest.end(randomBuffer(kOneMegaByte));
        expect(error.message).to.be.oneOf(['net::ERR_FAILED', 'net::ERR_CONNECTION_RESET', 'net::ERR_CONNECTION_ABORTED']);
      test('should not emit any event after close', async () => {
        await once(urlRequest, 'close');
        await new Promise((resolve, reject) => {
          for (const evName of ['finish', 'abort', 'close', 'error']) {
            urlRequest.on(evName as any, () => {
              reject(new Error(`Unexpected ${evName} event`));
          setTimeout(50).then(resolve);
      test('should remove the referer header when no referrer url specified', async () => {
          expect(request.headers.referer).to.equal(undefined);
      test('should set the referer header when a referrer url specified', async () => {
        const referrerURL = 'https://www.electronjs.org/';
          expect(request.headers.referer).to.equal(referrerURL);
        // The referrerPolicy must be unsafe-url because the referrer's origin
        // doesn't match the loaded page. With the default referrer policy
        // (strict-origin-when-cross-origin), the request will be canceled by the
        // network service when the referrer header is invalid.
        // See:
        // - https://source.chromium.org/chromium/chromium/src/+/main:net/url_request/url_request.cc;l=682-683;drc=ae587fa7cd2e5cc308ce69353ee9ce86437e5d41
        // - https://source.chromium.org/chromium/chromium/src/+/main:services/network/public/mojom/network_context.mojom;l=316-318;drc=ae5c7fcf09509843c1145f544cce3a61874b9698
        // - https://w3c.github.io/webappsec-referrer-policy/#determine-requests-referrer
        const urlRequest = net.request({ url: serverUrl, referrerPolicy: 'unsafe-url' });
        urlRequest.setHeader('referer', referrerURL);
    describe('IncomingMessage API', () => {
      test('response object should implement the IncomingMessage API', async () => {
          response.setHeader(customHeaderName, customHeaderValue);
        expect(response.statusMessage).to.equal('OK');
        expect(headers).to.be.an('object');
        const headerValue = headers[customHeaderName.toLowerCase()];
        expect(headerValue).to.equal(customHeaderValue);
        const rawHeaders = response.rawHeaders;
        expect(rawHeaders).to.be.an('array');
        expect(rawHeaders[0]).to.equal(customHeaderName);
        expect(rawHeaders[1]).to.equal(customHeaderValue);
        const httpVersion = response.httpVersion;
        expect(httpVersion).to.be.a('string').and.to.have.lengthOf.at.least(1);
        const httpVersionMajor = response.httpVersionMajor;
        expect(httpVersionMajor).to.be.a('number').and.to.be.at.least(1);
        const httpVersionMinor = response.httpVersionMinor;
        expect(httpVersionMinor).to.be.a('number').and.to.be.at.least(0);
      test('should discard duplicate headers', async () => {
        const includedHeader = 'max-forwards';
        const discardableHeader = 'Max-Forwards';
        const includedHeaderValue = 'max-fwds-val';
        const discardableHeaderValue = 'max-fwds-val-two';
          response.setHeader(discardableHeader, discardableHeaderValue);
          response.setHeader(includedHeader, includedHeaderValue);
        expect(headers).to.have.property(includedHeader);
        expect(headers).to.not.have.property(discardableHeader);
        expect(headers[includedHeader]).to.equal(includedHeaderValue);
      test('should join repeated non-discardable header values with ,', async () => {
          response.setHeader('referrer-policy', ['first-text', 'second-text']);
        expect(headers).to.have.property('referrer-policy');
        expect(headers['referrer-policy']).to.equal('first-text, second-text');
      test('should not join repeated discardable header values with ,', async () => {
          response.setHeader('last-modified', ['yesterday', 'today']);
        expect(headers).to.have.property('last-modified');
        expect(headers['last-modified']).to.equal('yesterday');
      test('should make set-cookie header an array even if single value', async () => {
          response.setHeader('set-cookie', 'chocolate-chip');
        expect(headers).to.have.property('set-cookie');
        expect(headers['set-cookie']).to.be.an('array');
        expect(headers['set-cookie'][0]).to.equal('chocolate-chip');
      test('should keep set-cookie header an array when an array', async () => {
          response.setHeader('set-cookie', ['chocolate-chip', 'oatmeal']);
        expect(headers['set-cookie'][1]).to.equal('oatmeal');
      test('should lowercase header keys', async () => {
          response.setHeader('HEADER-KEY', ['header-value']);
          response.setHeader('SeT-CookiE', ['chocolate-chip', 'oatmeal']);
          response.setHeader('rEFERREr-pOLICy', ['first-text', 'second-text']);
          response.setHeader('LAST-modified', 'yesterday');
        expect(headers).to.have.property('header-key');
      test('should return correct raw headers', async () => {
        const customHeaders: [string, string|string[]][] = [
          ['HEADER-KEY-ONE', 'header-value-one'],
          ['set-cookie', 'chocolate-chip'],
          ['header-key-two', 'header-value-two'],
          ['referrer-policy', ['first-text', 'second-text']],
          ['HEADER-KEY-THREE', 'header-value-three'],
          ['last-modified', ['first-text', 'second-text']],
          ['header-key-four', 'header-value-four']
          for (const headerTuple of customHeaders) {
            response.setHeader(headerTuple[0], headerTuple[1]);
        let rawHeadersIdx = 0;
          const headerKey = headerTuple[0];
          const headerValues = Array.isArray(headerTuple[1]) ? headerTuple[1] : [headerTuple[1]];
          for (const headerValue of headerValues) {
            expect(rawHeaders[rawHeadersIdx]).to.equal(headerKey);
            expect(rawHeaders[rawHeadersIdx + 1]).to.equal(headerValue);
            rawHeadersIdx += 2;
      test('should be able to pipe a net response into a writable stream', async () => {
        let nodeRequestProcessed = false;
        const [netServerUrl, nodeServerUrl] = await Promise.all([
          respondOnce.toSingleURL(async (request, response) => {
            const receivedBodyData = await collectStreamBody(request);
            nodeRequestProcessed = true;
        const netResponse = await getResponse(netRequest);
        const serverUrl = new URL(nodeServerUrl);
        const nodeOptions = {
          path: serverUrl.pathname,
          port: serverUrl.port
        const nodeRequest = http.request(nodeOptions);
        const nodeResponsePromise = once(nodeRequest, 'response');
        (netResponse as any).pipe(nodeRequest);
        const [nodeResponse] = await nodeResponsePromise;
        netRequest.end();
        await collectStreamBody(nodeResponse);
        expect(nodeRequestProcessed).to.equal(true);
      test('should correctly throttle an incoming stream', async () => {
        let numChunksSent = 0;
          const data = randomString(kOneMegaByte);
          const write = () => {
            let ok = true;
              numChunksSent++;
              if (numChunksSent > 30) return;
              ok = response.write(data);
            } while (ok);
            response.once('drain', write);
          write();
        urlRequest.on('response', () => {});
        // TODO(nornagon): I think this ought to max out at 20, but in practice
        // it seems to exceed that sometimes. This is at 25 to avoid test flakes,
        // but we should investigate if there's actually something broken here and
        // if so fix it and reset this to max at 20, and if not then delete this
        // comment.
        expect(numChunksSent).to.be.at.most(25);
    describe('net.isOnline', () => {
      test('getter returns boolean', () => {
        expect(net.isOnline()).to.be.a('boolean');
      test('property returns boolean', () => {
        expect(net.online).to.be.a('boolean');
    describe('Stability and performance', () => {
      test('should free unreferenced, never-started request objects without crash', async () => {
        net.request('https://test');
      test('should collect on-going requests without crash', async () => {
        let finishResponse: (() => void) | null = null;
          finishResponse = () => {
          // Trigger a garbage collection.
          finishResponse!();
      test('should collect unreferenced, ended requests without crash', async () => {
      test('should finish sending data when urlRequest is unreferenced', async () => {
          const received = await collectStreamBodyBuffer(request);
          expect(received.length).to.equal(kOneMegaByte);
        urlRequest.on('close', () => {
        urlRequest.write(randomBuffer(kOneMegaByte));
      test('should finish sending data when urlRequest is unreferenced for chunked encoding', async () => {
      test('should finish sending data when urlRequest is unreferenced before close event for chunked encoding', async () => {
    describe('non-http schemes', () => {
      test('should be rejected by net.request', async () => {
          net.request('file://bar');
        }).to.throw('ClientRequest only supports http: and https: protocols');
      test('should be rejected by net.request when passed in url:', async () => {
          net.request({ url: 'file://bar' });
      // NB. there exist much more comprehensive tests for fetch() in the form of
      // the WPT: https://github.com/web-platform-tests/wpt/tree/master/fetch
      // It's possible to run these tests against net.fetch(), but the test
      // harness to do so is quite complex and hasn't been munged to smoothly run
      // inside the Electron test runner yet.
      // In the meantime, here are some tests for basic functionality and
      // Electron-specific behavior.
      describe('basic', () => {
        test('can fetch http urls', async () => {
            response.end('test');
          const resp = await net.fetch(serverUrl);
          expect(await resp.text()).to.equal('test');
        test('can upload a string body', async () => {
            request.on('data', chunk => response.write(chunk));
          const resp = await net.fetch(serverUrl, {
            body: 'anchovies'
          expect(await resp.text()).to.equal('anchovies');
        test('can read response as an array buffer', async () => {
          expect(new TextDecoder().decode(new Uint8Array(await resp.arrayBuffer()))).to.equal('anchovies');
        test('can read response as form data', async () => {
            response.setHeader('content-type', 'application/x-www-form-urlencoded');
            response.end('foo=bar');
          const result = await resp.formData();
          expect(result.get('foo')).to.equal('bar');
        test('should reject promise on DNS failure', async () => {
          const r = net.fetch('https://i.do.not.exist');
          await expect(r).to.be.rejectedWith(/ERR_NAME_NOT_RESOLVED/);
        test('should reject body promise when stream fails', async () => {
            response.write('first chunk');
            setTimeout().then(() => response.destroy());
          const r = await net.fetch(serverUrl);
          expect(r.status).to.equal(200);
          await expect(r.text()).to.be.rejectedWith(/ERR_INCOMPLETE_CHUNKED_ENCODING/);
    describe('net.resolveHost', () => {
      test('resolves ipv4.localhost2', async () => {
        const { endpoints } = await net.resolveHost('ipv4.localhost2');
        expect(endpoints).to.be.a('array');
        expect(endpoints).to.have.lengthOf(1);
        expect(endpoints[0].family).to.equal('ipv4');
        expect(endpoints[0].address).to.equal('10.0.0.1');
      test('fails to resolve AAAA record for ipv4.localhost2', async () => {
        await expect(net.resolveHost('ipv4.localhost2', {
          queryType: 'AAAA'
          .to.eventually.be.rejectedWith(/net::ERR_NAME_NOT_RESOLVED/);
      test('resolves ipv6.localhost2', async () => {
        const { endpoints } = await net.resolveHost('ipv6.localhost2');
        expect(endpoints[0].family).to.equal('ipv6');
        expect(endpoints[0].address).to.equal('::1');
      test('fails to resolve A record for ipv6.localhost2', async () => {
        await expect(net.resolveHost('notfound.localhost2', {
          queryType: 'A'
      test('fails to resolve notfound.localhost2', async () => {
        await expect(net.resolveHost('notfound.localhost2'))
  for (const test of [itIgnoringArgs]) {
      for (const [priorityName, urgency] of Object.entries({
        throttled: 'u=5',
        idle: 'u=4',
        lowest: '',
        low: 'u=2',
        medium: 'u=1',
        highest: 'u=0'
        for (const priorityIncremental of [true, false]) {
          test(`should set priority to ${priorityName}/${priorityIncremental} if requested`, async () => {
            // Priority header is available on HTTP/2, which is only
            // supported over TLS, so...
            session.defaultSession.setCertificateVerifyProc((req, cb) => cb(0));
              session.defaultSession.setCertificateVerifyProc(null);
              url: `${http2URL}get`,
              priority: priorityName as any,
              priorityIncremental
            const data = JSON.parse(await collectStreamBody(response));
            let expectedPriority = urgency;
            if (priorityIncremental) {
              expectedPriority = expectedPriority ? expectedPriority + ', i' : 'i';
            if (expectedPriority === '') {
              expect(data.headers.priority).to.be.undefined();
              expect(data.headers.priority).to.be.a('string').and.equal(expectedPriority);
          }, { priorityName, urgency, priorityIncremental });
  ifdescribe(isTestingBindingAvailable())('Network Service crash recovery', () => {
    it('should recover net.fetch after Network Service crash (main process)', async () => {
      const binding = process._linkedBinding('electron_common_testing');
        response.end('first');
      const firstResponse = await net.fetch(serverUrl);
      expect(firstResponse.ok).to.be.true();
      expect(await firstResponse.text()).to.equal('first');
      await binding.simulateNetworkServiceCrash();
      // Wait for StoragePartitionImpl's NetworkContext disconnect handler to
      // fire and reinitialize the context in the new Network Service.
      const secondServerUrl = await respondOnce.toSingleURL((request, response) => {
        response.end('second');
      const secondResponse = await net.fetch(secondServerUrl);
      expect(secondResponse.ok).to.be.true();
      expect(await secondResponse.text()).to.equal('second');
    it('should recover net.fetch after Network Service crash (utility process)', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'api', 'utility-process', 'network-restart-test.js'));
      await once(child, 'spawn');
      await once(child, 'message');
      const firstServerUrl = await respondOnce.toSingleURL((request, response) => {
        response.end('utility-first');
      child.postMessage({ type: 'fetch', url: firstServerUrl });
      const [firstResult] = await once(child, 'message');
      expect(firstResult.ok).to.be.true();
      expect(firstResult.body).to.equal('utility-first');
      // Needed for UpdateURLLoaderFactory IPC to propagate to the utility process
      // and for any in-flight requests to settle
        response.end('utility-second');
      child.postMessage({ type: 'fetch', url: secondServerUrl });
      const [secondResult] = await once(child, 'message');
      expect(secondResult.ok).to.be.true();
      expect(secondResult.body).to.equal('utility-second');
      child.kill();
      await once(child, 'exit');
