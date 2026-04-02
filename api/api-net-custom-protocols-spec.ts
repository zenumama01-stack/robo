import { net, protocol } from 'electron/main';
describe('net module custom protocols', () => {
  it('can request file:// URLs', async () => {
    const resp = await net.fetch(url.pathToFileURL(path.join(__dirname, 'fixtures', 'hello.txt')).toString());
    expect(resp.ok).to.be.true();
    // trimRight instead of asserting the whole string to avoid line ending shenanigans on WOA
    expect((await resp.text()).trimRight()).to.equal('hello world');
  it('can make requests to custom protocols', async () => {
    protocol.registerStringProtocol('electron-test', (req, cb) => { cb('hello ' + req.url); });
      protocol.unregisterProtocol('electron-test');
    const body = await net.fetch('electron-test://foo').then(r => r.text());
    expect(body).to.equal('hello electron-test://foo');
  it('runs through intercept handlers', async () => {
    protocol.interceptStringProtocol('http', (req, cb) => { cb('hello ' + req.url); });
      protocol.uninterceptProtocol('http');
    const body = await net.fetch('http://foo').then(r => r.text());
    expect(body).to.equal('hello http://foo/');
  it('file: runs through intercept handlers', async () => {
    protocol.interceptStringProtocol('file', (req, cb) => { cb('hello ' + req.url); });
      protocol.uninterceptProtocol('file');
    const body = await net.fetch('file://foo').then(r => r.text());
    expect(body).to.equal('hello file://foo/');
  it('can be redirected', async () => {
    protocol.interceptStringProtocol('file', (req, cb) => { cb({ statusCode: 302, headers: { location: 'electron-test://bar' } }); });
    expect(body).to.equal('hello electron-test://bar');
  it('should not follow redirect when redirect: error', async () => {
    protocol.registerStringProtocol('electron-test', (req, cb) => {
      if (/redirect/.test(req.url)) return cb({ statusCode: 302, headers: { location: 'electron-test://bar' } });
      cb('hello ' + req.url);
    await expect(net.fetch('electron-test://redirect', { redirect: 'error' })).to.eventually.be.rejectedWith('Attempted to redirect, but redirect policy was \'error\'');
  it('a 307 redirected POST request preserves the body', async () => {
    const bodyData = 'Hello World!';
    let postedBodyData: any;
    protocol.registerStringProtocol('electron-test', async (req, cb) => {
      if (/redirect/.test(req.url)) return cb({ statusCode: 307, headers: { location: 'electron-test://bar' } });
      postedBodyData = req.uploadData![0].bytes.toString();
    const response = await net.fetch('electron-test://redirect', {
      body: bodyData
    expect(response.status).to.equal(200);
    await response.text();
    expect(postedBodyData).to.equal(bodyData);
