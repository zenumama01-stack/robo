import { BrowserWindow, WebPreferences } from 'electron/main';
const messageContainsSecurityWarning = (event: Event, level: number, message: string) => {
  return message.includes('Electron Security Warning');
const isLoaded = (event: Event, level: number, message: string) => {
  return (message === 'loaded');
describe('security warnings', () => {
  let useCsp = true;
    // Create HTTP Server
    server = http.createServer(async (request, response) => {
      const uri = new URL(request.url!, `http://${request.headers.host}`).pathname!;
      let filename = path.join(__dirname, 'fixtures', 'pages', uri);
        const stats = await fs.stat(filename);
          filename += '/index.html';
        const file = await fs.readFile(filename, 'binary');
        const cspHeaders = [
          ...(useCsp ? ['script-src \'self\' \'unsafe-inline\''] : [])
        response.writeHead(200, { 'Content-Security-Policy': cspHeaders });
        response.write(file, 'binary');
        response.writeHead(404, { 'Content-Type': 'text/plain' });
    serverUrl = `http://localhost2:${(await listen(server)).port}`;
    server = null as unknown as any;
    useCsp = true;
    w = null as unknown as any;
  it('should warn about Node.js integration with remote content', async () => {
    w.loadURL(`${serverUrl}/base-page-security.html`);
    const [{ message }] = await emittedUntil(w.webContents, 'console-message', messageContainsSecurityWarning);
    expect(message).to.include('Node.js Integration with Remote Content');
  it('should not warn about Node.js integration with remote content from localhost', async () => {
    w.loadURL(`${serverUrl}/base-page-security-onload-message.html`);
    const [{ message }] = await emittedUntil(w.webContents, 'console-message', isLoaded);
    expect(message).to.not.include('Node.js Integration with Remote Content');
  const generateSpecs = (description: string, webPreferences: WebPreferences) => {
      it('should warn about disabled webSecurity', async () => {
            ...webPreferences
        expect(message).to.include('Disabled webSecurity');
      it('should warn about insecure Content-Security-Policy', async () => {
        useCsp = false;
        expect(message).to.include('Insecure Content-Security-Policy');
      it('should not warn about secure Content-Security-Policy', async () => {
        let didNotWarn = true;
        w.webContents.on('console-message', () => {
          didNotWarn = false;
        expect(didNotWarn).to.equal(true);
      it('should warn about allowRunningInsecureContent', async () => {
            allowRunningInsecureContent: true,
        expect(message).to.include('allowRunningInsecureContent');
      it('should warn about experimentalFeatures', async () => {
            experimentalFeatures: true,
        expect(message).to.include('experimentalFeatures');
      it('should warn about enableBlinkFeatures', async () => {
            enableBlinkFeatures: 'my-cool-feature',
        expect(message).to.include('enableBlinkFeatures');
      it('should warn about allowpopups', async () => {
        w.loadURL(`${serverUrl}/webview-allowpopups.html`);
        expect(message).to.include('allowpopups');
      it('should warn about insecure resources', async () => {
        w.loadURL(`${serverUrl}/insecure-resources.html`);
        expect(message).to.include('Insecure Resources');
      it('should not warn about loading insecure-resources.html from localhost', async () => {
        expect(message).to.not.include('insecure-resources.html');
  generateSpecs('without sandbox', { contextIsolation: false });
  generateSpecs('with sandbox', { sandbox: true, contextIsolation: false });
