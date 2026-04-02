import { net, session, BrowserWindow, ClientRequestConstructorOptions } from 'electron/main';
import * as dns from 'node:dns';
import { collectStreamBody, getResponse, respondNTimes, respondOnce } from './lib/net-helpers';
// See https://github.com/nodejs/node/issues/40702.
dns.setDefaultResultOrder('ipv4first');
describe('net module (session)', () => {
    respondNTimes.routeFailure = false;
  afterEach(async function () {
    await session.defaultSession.clearCache();
    if (respondNTimes.routeFailure && this.test) {
      if (!this.test.isFailed()) {
        throw new Error('Failing this test due an unhandled error in the respondOnce route handler, check the logs above for the actual error');
  describe('HTTP basics', () => {
    for (const extraOptions of [{}, { credentials: 'include' }, { useSessionCookies: false, credentials: 'include' }] as ClientRequestConstructorOptions[]) {
      describe(`authentication when ${JSON.stringify(extraOptions)}`, () => {
        it('should share credentials with WebContents', async () => {
          const [user, pass] = ['user', 'pass'];
          const serverUrl = await respondNTimes.toSingleURL((request, response) => {
            if (!request.headers.authorization) {
              return response.writeHead(401, { 'WWW-Authenticate': 'Basic realm="Foo"' }).end();
            return response.writeHead(200).end('ok');
          }, 2);
          bw.webContents.on('login', (event, details, authInfo, cb) => {
            cb(user, pass);
          await bw.loadURL(serverUrl);
          bw.close();
          const request = net.request({ method: 'GET', url: serverUrl, ...extraOptions });
          let logInCount = 0;
          request.on('login', () => {
            logInCount++;
          const response = await getResponse(request);
          await collectStreamBody(response);
          expect(logInCount).to.equal(0, 'should not receive a login event, credentials should be cached');
        it('should share proxy credentials with WebContents', async () => {
          const proxyUrl = await respondNTimes((request, response) => {
            if (!request.headers['proxy-authorization']) {
              return response.writeHead(407, { 'Proxy-Authenticate': 'Basic realm="Foo"' }).end();
          const customSession = session.fromPartition(`net-proxy-test-${Math.random()}`);
          await customSession.setProxy({ proxyRules: proxyUrl.replace('http://', ''), proxyBypassRules: '<-loopback>' });
          const bw = new BrowserWindow({ show: false, webPreferences: { session: customSession } });
          await bw.loadURL('http://127.0.0.1:9999');
          const request = net.request({ method: 'GET', url: 'http://127.0.0.1:9999', session: customSession, ...extraOptions });
          const body = await collectStreamBody(response);
          expect(body).to.equal('ok');
    describe('authentication when {"credentials":"omit"}', () => {
      it('should not share credentials with WebContents', async () => {
        const request = net.request({ method: 'GET', url: serverUrl, credentials: 'omit' });
          expect.fail();
        expect(response.statusCode).to.equal(401);
        expect(response.headers['www-authenticate']).to.equal('Basic realm="Foo"');
        const request = net.request({ method: 'GET', url: 'http://127.0.0.1:9999', session: customSession, credentials: 'omit' });
  describe('ClientRequest API', () => {
    it('should be able to set cookie header line', async () => {
      const cookieHeaderName = 'Cookie';
      const cookieHeaderValue = 'test=12345';
      const customSession = session.fromPartition(`test-cookie-header-${Math.random()}`);
      const serverUrl = await respondOnce.toSingleURL((request, response) => {
        expect(request.headers[cookieHeaderName.toLowerCase()]).to.equal(cookieHeaderValue);
        response.statusMessage = 'OK';
      await customSession.cookies.set({
        url: `${serverUrl}`,
        value: '11111',
        expirationDate: 0
      const urlRequest = net.request({
        url: serverUrl,
        session: customSession
      urlRequest.setHeader(cookieHeaderName, cookieHeaderValue);
      expect(urlRequest.getHeader(cookieHeaderName)).to.equal(cookieHeaderValue);
    it('should not use the sessions cookie store by default', async () => {
        response.setHeader('x-cookie', `${request.headers.cookie!}`);
      const sess = session.fromPartition(`cookie-tests-${Math.random()}`);
      const cookieVal = `${Date.now()}`;
      await sess.cookies.set({
        name: 'wild_cookie',
        value: cookieVal
        session: sess
      expect(response.headers['x-cookie']).to.equal('undefined');
    for (const extraOptions of [{ useSessionCookies: true }, { credentials: 'include' }] as ClientRequestConstructorOptions[]) {
      describe(`when ${JSON.stringify(extraOptions)}`, () => {
        it('should be able to use the sessions cookie store', async () => {
            response.setHeader('x-cookie', request.headers.cookie!);
            session: sess,
            ...extraOptions
          expect(response.headers['x-cookie']).to.equal(`wild_cookie=${cookieVal}`);
        it('should be able to use the sessions cookie store with set-cookie', async () => {
            response.setHeader('set-cookie', 'foo=bar');
          let cookies = await sess.cookies.get({});
          expect(cookies).to.have.lengthOf(0);
          await collectStreamBody(await getResponse(urlRequest));
          cookies = await sess.cookies.get({});
          expect(cookies).to.have.lengthOf(1);
          expect(cookies[0]).to.deep.equal({
            name: 'foo',
            value: 'bar',
            domain: '127.0.0.1',
            hostOnly: true,
            secure: false,
            httpOnly: false,
            session: true,
            sameSite: 'unspecified'
        for (const mode of ['Lax', 'Strict']) {
          it(`should be able to use the sessions cookie store with same-site ${mode} cookies`, async () => {
              response.setHeader('set-cookie', `same=site; SameSite=${mode}`);
              response.setHeader('x-cookie', `${request.headers.cookie}`);
              name: 'same',
              value: 'site',
              sameSite: mode.toLowerCase()
            const urlRequest2 = net.request({
            const response2 = await getResponse(urlRequest2);
            expect(response2.headers['x-cookie']).to.equal('same=site');
        it('should be able to use the sessions cookie store safely across redirects', async () => {
          const serverUrl = await respondOnce.toSingleURL(async (request, response) => {
            response.statusCode = 302;
            response.statusMessage = 'Moved';
            const newUrl = await respondOnce.toSingleURL((req, res) => {
              res.statusMessage = 'OK';
              res.setHeader('x-cookie', req.headers.cookie!);
            response.setHeader('location', newUrl.replace('127.0.0.1', 'localhost'));
          const cookie127Val = `${Date.now()}-127`;
          const cookieLocalVal = `${Date.now()}-local`;
          const localhostUrl = serverUrl.replace('127.0.0.1', 'localhost');
          expect(localhostUrl).to.not.equal(serverUrl);
          // cookies with lax or strict same-site settings will not
          // persist after redirects. no_restriction must be used
            sess.cookies.set({
              sameSite: 'no_restriction',
              value: cookie127Val
            }), sess.cookies.set({
              url: localhostUrl,
              value: cookieLocalVal
          urlRequest.on('redirect', (status, method, url, headers) => {
            // The initial redirect response should have received the 127 value here
            expect(headers['x-cookie'][0]).to.equal(`wild_cookie=${cookie127Val}`);
            urlRequest.followRedirect();
          // We expect the server to have received the localhost value here
          // The original request was to a 127.0.0.1 URL
          // That request would have the cookie127Val cookie attached
          // The request is then redirect to a localhost URL (different site)
          // Because we are using the session cookie store it should do the safe / secure thing
          // and attach the cookies for the new target domain
          expect(response.headers['x-cookie']).to.equal(`wild_cookie=${cookieLocalVal}`);
    it('should be able correctly filter out cookies that are secure', async () => {
          url: 'https://electronjs.org',
          domain: 'electronjs.org',
          name: 'cookie1',
          value: '1',
          secure: true
          name: 'cookie2',
          value: '2',
          secure: false
      const secureCookies = await sess.cookies.get({
      expect(secureCookies).to.have.lengthOf(1);
      expect(secureCookies[0].name).to.equal('cookie1');
      const cookies = await sess.cookies.get({
      expect(cookies[0].name).to.equal('cookie2');
    it('throws when an invalid domain is passed', async () => {
      await expect(sess.cookies.set({
        domain: 'wssss.iamabaddomain.fun',
        name: 'cookie1'
      })).to.eventually.be.rejectedWith(/The cookie was set with an invalid Domain attribute/);
    it('should be able correctly filter out cookies that are session', async () => {
          value: '1'
          expirationDate: Math.round(Date.now() / 1000) + 10000
      const sessionCookies = await sess.cookies.get({
        session: true
      expect(sessionCookies).to.have.lengthOf(1);
      expect(sessionCookies[0].name).to.equal('cookie1');
        session: false
    it('should be able correctly filter out cookies that are httpOnly', async () => {
          httpOnly: true
          httpOnly: false
      const httpOnlyCookies = await sess.cookies.get({
      expect(httpOnlyCookies).to.have.lengthOf(1);
      expect(httpOnlyCookies[0].name).to.equal('cookie1');
      it('Should throw when invalid filters are passed', () => {
          session.defaultSession.webRequest.onBeforeRequest(
            { urls: ['*://www.googleapis.com'] },
            (details, callback) => { callback({ cancel: false }); }
        }).to.throw('Invalid url pattern *://www.googleapis.com: Empty path.');
            { urls: ['*://www.googleapis.com/', '*://blahblah.dev'] },
        }).to.throw('Invalid url pattern *://blahblah.dev: Empty path.');
      it('Should not throw when valid filters are passed', () => {
            { urls: ['*://www.googleapis.com/'] },
      it('Requests should be intercepted by webRequest module', async () => {
        const requestUrl = '/requestUrl';
        const redirectUrl = '/redirectUrl';
        let requestIsRedirected = false;
        const serverUrl = await respondOnce.toURL(redirectUrl, (request, response) => {
          requestIsRedirected = true;
        let requestIsIntercepted = false;
          (details, callback) => {
            if (details.url === `${serverUrl}${requestUrl}`) {
              requestIsIntercepted = true;
                redirectURL: `${serverUrl}${redirectUrl}`
        const urlRequest = net.request(`${serverUrl}${requestUrl}`);
        expect(requestIsRedirected).to.be.true('The server should receive a request to the forward URL');
        expect(requestIsIntercepted).to.be.true('The request should be intercepted by the webRequest module');
      it('should to able to create and intercept a request using a custom session object', async () => {
        const customPartitionName = `custom-partition-${Math.random()}`;
        session.defaultSession.webRequest.onBeforeRequest(() => {
          expect.fail('Request should not be intercepted by the default session');
        const customSession = session.fromPartition(customPartitionName, { cache: false });
        customSession.webRequest.onBeforeRequest((details, callback) => {
          url: `${serverUrl}${requestUrl}`,
      it('should to able to create and intercept a request using a custom partition name', async () => {
          partition: customPartitionName
      it('triggers webRequest handlers when bypassCustomProtocolHandlers', async () => {
        let webRequestDetails: Electron.OnBeforeRequestListenerDetails | null = null;
        const serverUrl = await respondOnce.toSingleURL((req, res) => res.end('hi'));
        session.defaultSession.webRequest.onBeforeRequest((details, cb) => {
          webRequestDetails = details;
          cb({});
        const body = await net.fetch(serverUrl, { bypassCustomProtocolHandlers: true }).then(r => r.text());
        expect(body).to.equal('hi');
        expect(webRequestDetails).to.have.property('url', serverUrl);
    it('should throw if given an invalid session option', () => {
        net.request({
          url: 'https://foo',
          session: 1 as any
      }).to.throw('`session` should be an instance of the Session class');
    it('should throw if given an invalid partition option', () => {
          partition: 1 as any
      }).to.throw('`partition` should be a string');
    it('should throw if given a header value that is empty(null/undefined)', () => {
      const emptyHeaderValues = [null, undefined];
      const errorMsg = '`value` required in setHeader("foo", value)';
      for (const emptyValue of emptyHeaderValues) {
            headers: { foo: emptyValue as any }
        }).to.throw(errorMsg);
        const request = net.request({ url: 'https://foo' });
          request.setHeader('foo', emptyValue as any);
  describe('net.fetch', () => {
    it('should be able to use a session cookie store', async () => {
      const response = await sess.fetch(serverUrl, {
        credentials: 'include'
      expect(response.headers.get('x-cookie')).to.equal(`wild_cookie=${cookieVal}`);
        await expect(net.fetch('https://foo')).to.eventually.be.rejectedWith('net::ERR_BLOCKED_BY_CLIENT');
        await expect(net.fetch('file://foo')).to.eventually.be.rejectedWith('net::ERR_BLOCKED_BY_CLIENT');
        await expect(net.fetch('custom-protocol://foo')).to.eventually.be.rejectedWith('net::ERR_BLOCKED_BY_CLIENT');
