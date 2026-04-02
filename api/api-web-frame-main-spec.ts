import { BrowserWindow, WebFrameMain, webFrameMain, ipcMain, app, WebContents } from 'electron/main';
describe('webFrameMain module', () => {
  const subframesPath = path.join(fixtures, 'sub-frames');
  const fileUrl = (filename: string) => url.pathToFileURL(path.join(subframesPath, filename)).href;
  type Server = { server: http.Server, url: string, crossOriginUrl: string }
  /** Creates an HTTP server whose handler embeds the given iframe src. */
  const createServer = async (options: {
    headers?: Record<string, string>
  } = {}): Promise<Server> => {
      if (options.headers) {
        for (const [k, v] of Object.entries(options.headers)) {
          res.setHeader(k, v);
      const params = new URLSearchParams(new URL(req.url || '', `http://${req.headers.host}`).search || '');
      if (params.has('frameSrc')) {
        res.end(`<iframe src="${params.get('frameSrc')}"></iframe>`);
    const serverUrl = (await listen(server)).url + '/';
    // HACK: Use 'localhost' instead of '127.0.0.1' so Chromium treats it as
    // a separate origin because differing ports aren't enough 🤔
    const crossOriginUrl = serverUrl.replace('127.0.0.1', 'localhost');
      crossOriginUrl
  describe('WebFrame traversal APIs', () => {
    let webFrame: WebFrameMain;
      await w.loadFile(path.join(subframesPath, 'frame-with-frame-container.html'));
      webFrame = w.webContents.mainFrame;
    it('can access top frame', () => {
      expect(webFrame.top).to.equal(webFrame);
    it('has no parent on top frame', () => {
      expect(webFrame.parent).to.be.null();
    it('can access immediate frame descendents', () => {
      const { frames } = webFrame;
      expect(frames).to.have.lengthOf(1);
      const subframe = frames[0];
      expect(subframe).not.to.equal(webFrame);
      expect(subframe.parent).to.equal(webFrame);
    it('can access deeply nested frames', () => {
      const subframe = webFrame.frames[0];
      const nestedSubframe = subframe.frames[0];
      expect(nestedSubframe).not.to.equal(webFrame);
      expect(nestedSubframe).not.to.equal(subframe);
      expect(nestedSubframe.parent).to.equal(subframe);
    it('can traverse all frames in root', () => {
      const urls = webFrame.framesInSubtree.map(frame => frame.url);
      expect(urls).to.deep.equal([
        fileUrl('frame-with-frame-container.html'),
        fileUrl('frame-with-frame.html'),
        fileUrl('frame.html')
    it('can traverse all frames in subtree', () => {
      const urls = webFrame.frames[0].framesInSubtree.map(frame => frame.url);
    describe('cross-origin', () => {
      let serverA: Server;
      let serverB: Server;
        serverA = await createServer();
        serverB = await createServer();
        serverA.server.close();
        serverB.server.close();
      it('can access cross-origin frames', async () => {
        await w.loadURL(`${serverA.url}?frameSrc=${serverB.url}`);
        expect(webFrame.url.startsWith(serverA.url)).to.be.true();
        expect(webFrame.frames[0].url).to.equal(serverB.url);
  describe('WebFrame.url', () => {
    it('should report correct address for each subframe', async () => {
      const webFrame = w.webContents.mainFrame;
      expect(webFrame.url).to.equal(fileUrl('frame-with-frame-container.html'));
      expect(webFrame.frames[0].url).to.equal(fileUrl('frame-with-frame.html'));
      expect(webFrame.frames[0].frames[0].url).to.equal(fileUrl('frame.html'));
  describe('WebFrame.origin', () => {
    it('should be null for a fresh WebContents', () => {
      expect(w.webContents.mainFrame.origin).to.equal('null');
    it('should be file:// for file frames', async () => {
      await w.loadFile(path.join(fixtures, 'pages', 'blank.html'));
      expect(w.webContents.mainFrame.origin).to.equal('file://');
    it('should be http:// for an http frame', async () => {
      const s = await createServer();
      defer(() => s.server.close());
      await w.loadURL(s.url);
      expect(w.webContents.mainFrame.origin).to.equal(s.url.replace(/\/$/, ''));
    it('should show parent origin when child page is about:blank', async () => {
      await w.webContents.executeJavaScript('window.open("", null, "show=false"), null');
      const [, childWebContents] = await webContentsCreated;
      expect(childWebContents.mainFrame.origin).to.equal('file://');
    it('should show parent frame\'s origin when about:blank child window opened through cross-origin subframe', async () => {
      const serverA = await createServer();
      const serverB = await createServer();
      await w.loadURL(serverA.url + '?frameSrc=' + encodeURIComponent(serverB.url));
      const { mainFrame } = w.webContents;
      expect(mainFrame.origin).to.equal(serverA.url.replace(/\/$/, ''));
      const [childFrame] = mainFrame.frames;
      expect(childFrame.origin).to.equal(serverB.url.replace(/\/$/, ''));
      await childFrame.executeJavaScript('window.open("", null, "show=false"), null');
      expect(childWebContents.mainFrame.origin).to.equal(childFrame.origin);
  describe('WebFrame IDs', () => {
    it('has properties for various identifiers', async () => {
      await w.loadFile(path.join(subframesPath, 'frame.html'));
      expect(webFrame).to.have.property('url').that.is.a('string');
      expect(webFrame).to.have.property('frameTreeNodeId').that.is.a('number');
      expect(webFrame).to.have.property('name').that.is.a('string');
      expect(webFrame).to.have.property('osProcessId').that.is.a('number');
      expect(webFrame).to.have.property('processId').that.is.a('number');
      expect(webFrame).to.have.property('routingId').that.is.a('number');
      expect(webFrame).to.have.property('frameToken').that.is.a('string');
  describe('WebFrame.visibilityState', () => {
    // DISABLED-FIXME(MarshallOfSound): Fix flaky test
    it('should match window state', async () => {
      expect(webFrame.visibilityState).to.equal('visible');
        waitUntil(() => webFrame.visibilityState === 'hidden')
  describe('WebFrame.executeJavaScript', () => {
    it('can inject code into any subframe', async () => {
      const getUrl = (frame: WebFrameMain) => frame.executeJavaScript('location.href');
      expect(await getUrl(webFrame)).to.equal(fileUrl('frame-with-frame-container.html'));
      expect(await getUrl(webFrame.frames[0])).to.equal(fileUrl('frame-with-frame.html'));
      expect(await getUrl(webFrame.frames[0].frames[0])).to.equal(fileUrl('frame.html'));
    it('can resolve promise', async () => {
      const p = () => webFrame.executeJavaScript('new Promise(resolve => setTimeout(resolve(42), 2000));');
      const result = await p();
    it('can reject with error', async () => {
      const p = () => webFrame.executeJavaScript('new Promise((r,e) => setTimeout(e("error!"), 500));');
      await expect(p()).to.be.eventually.rejectedWith('error!');
        await expect(webFrame.executeJavaScript(`Promise.reject(new ${error.name}("Wamp-wamp"))`))
          .to.eventually.be.rejectedWith(/Error/);
    it('can reject when script execution fails', async () => {
      const p = () => webFrame.executeJavaScript('console.log(test)');
      await expect(p()).to.be.eventually.rejectedWith(/ReferenceError/);
  describe('WebFrame.reload', () => {
    it('reloads a frame', async () => {
      await webFrame.executeJavaScript('window.TEMP = 1', false);
      expect(webFrame.reload()).to.be.true();
      expect(await webFrame.executeJavaScript('window.TEMP', false)).to.be.null();
  describe('WebFrame.send', () => {
    it('works', async () => {
          preload: path.join(subframesPath, 'preload.js'),
      webFrame.send('preload-ping');
      expect(frameToken).to.equal(webFrame.frameToken);
  describe('RenderFrame lifespan', () => {
    let server: Awaited<ReturnType<typeof createServer>>;
      server = await createServer();
      server.server.close();
    // TODO(jkleinsc) fix this flaky test on linux
    ifit(process.platform !== 'linux')('throws upon accessing properties when disposed', async () => {
      // Wait for WebContents, and thus RenderFrameHost, to be destroyed.
      expect(() => mainFrame.url).to.throw();
    it('persists through cross-origin navigation', async () => {
      await w.loadURL(server.url);
      expect(mainFrame.url).to.equal(server.url);
      await w.loadURL(server.crossOriginUrl);
      expect(w.webContents.mainFrame).to.equal(mainFrame);
      expect(mainFrame.url).to.equal(server.crossOriginUrl);
    it('recovers from renderer crash on same-origin', async () => {
      // Keep reference to mainFrame alive throughout crash and recovery.
      await w.webContents.loadURL(server.url);
      const crashEvent = once(w.webContents, 'render-process-gone');
      await crashEvent;
      // A short wait seems to be required to reproduce the crash.
      // Log just to keep mainFrame in scope.
      console.log('mainFrame.url', mainFrame.url);
    // Fixed by #34411
    it('recovers from renderer crash on cross-origin', async () => {
      await w.webContents.loadURL(server.crossOriginUrl);
    it('returns null upon accessing senderFrame after cross-origin navigation', async () => {
          preload: path.join(subframesPath, 'preload.js')
      const preloadPromise = once(ipcMain, 'preload-ran');
      const [event] = await preloadPromise;
      // senderFrame now points to a disposed RenderFrameHost. It should
      // be null when attempting to access the lazily evaluated property.
      waitUntil(() => {
        return event.senderFrame === null;
    it('is detached when unload handler sends IPC', async () => {
      const unloadPromise = new Promise<void>((resolve, reject) => {
        ipcMain.once('preload-unload', (event) => {
            const { senderFrame } = event;
            expect(senderFrame).to.not.be.null();
            expect(senderFrame!.detached).to.be.true();
            expect(senderFrame!.processId).to.equal(event.processId);
      await expect(unloadPromise).to.eventually.be.fulfilled();
    it('disposes detached frame after cross-origin navigation', async () => {
      // eslint-disable-next-line prefer-const
      let crossOriginPromise: Promise<void>;
        ipcMain.once('preload-unload', async (event) => {
            await crossOriginPromise;
            expect(() => senderFrame!.url).to.throw(/Render frame was disposed/);
      crossOriginPromise = w.webContents.loadURL(server.crossOriginUrl);
    // Skip test as we don't have an offline repro yet
    it.skip('should not crash due to dangling frames', async () => {
      // Persist frame references so WebFrameMain is initialized for each
      const frames: Electron.WebFrameMain[] = [];
      w.webContents.on('frame-created', (_event, details) => {
        console.log('frame-created');
        frames.push(details.frame!);
        console.log('will-frame-navigate', event);
        frames.push(event.frame!);
      // Load document with several speculative subframes
      await w.webContents.loadURL('https://www.ezcater.com/delivery/pizza-catering');
      // Test that no frame will crash due to a dangling render frame host
      const crashTest = () => {
        for (const frame of frames) {
          expect(frame._lifecycleStateForTesting).to.not.equal('Speculative');
            expect(frame.url).to.be.a('string');
            // Exceptions from non-dangling frames are expected
      crashTest();
      await setTimeout(1);
  describe('webFrameMain.fromId', () => {
    it('returns undefined for unknown IDs', () => {
      expect(webFrameMain.fromId(0, 0)).to.be.undefined();
    it('can find each frame from navigation events', async () => {
      // frame-with-frame-container.html, frame-with-frame.html, frame.html
      const didFrameFinishLoad = emittedNTimes(w.webContents, 'did-frame-finish-load', 3);
      w.loadFile(path.join(subframesPath, 'frame-with-frame-container.html'));
      for (const [, isMainFrame, frameProcessId, frameRoutingId] of await didFrameFinishLoad) {
        expect(frame).not.to.be.null();
        expect(frame?.processId).to.be.equal(frameProcessId);
        expect(frame?.routingId).to.be.equal(frameRoutingId);
        expect(frame?.top === frame).to.be.equal(isMainFrame);
  describe('webFrameMain.fromFrameToken', () => {
    it('returns null for unknown IDs', () => {
      expect(webFrameMain.fromFrameToken(0, '')).to.be.null();
    it('can find existing frame', async () => {
      const frame = webFrameMain.fromFrameToken(mainFrame.processId, mainFrame.frameToken);
      expect(frame).to.equal(mainFrame);
  describe('webFrameMain.collectJavaScriptCallStack', () => {
    let server: Server;
      server = await createServer({
          'Document-Policy': 'include-js-call-stacks-in-crash-reports'
    it('collects call stack during JS execution', async () => {
      const callStackPromise = w.webContents.mainFrame.collectJavaScriptCallStack();
      w.webContents.mainFrame.executeJavaScript('"run a lil js"');
      const callStack = await callStackPromise;
      expect(callStack).to.be.a('string');
  describe('webFrameMain.copyVideoFrameAt', () => {
    const insertVideoInFrame = async (frame: WebFrameMain) => {
      const videoFilePath = url.pathToFileURL(path.join(fixtures, 'cat-spin.mp4')).href;
      await frame.executeJavaScript(`
        const video = document.createElement('video');
        video.src = '${videoFilePath}';
        video.muted = true;
        video.loop = true;
        video.play();
        document.body.appendChild(video);
    const getFramePosition = async (frame: WebFrameMain) => {
      const point = await frame.executeJavaScript(`(${() => {
        if (!iframe) return;
        const rect = iframe.getBoundingClientRect();
        return { x: Math.floor(rect.x), y: Math.floor(rect.y) };
      }})()`) as Electron.Point;
      expect(point).to.be.an('object');
      return point;
    const copyVideoFrameInFrame = async (frame: WebFrameMain) => {
        const video = document.querySelector('video');
        if (!video) return;
        const rect = video.getBoundingClientRect();
          x: Math.floor(rect.x + rect.width / 2),
          y: Math.floor(rect.y + rect.height / 2)
      // Translate coordinate to be relative of main frame
      if (frame.parent) {
        const framePosition = await getFramePosition(frame.parent);
        point.x += framePosition.x;
        point.y += framePosition.y;
      // wait for video to load
      await frame.executeJavaScript(`(${() => {
          if (video.readyState >= 4) resolve(null);
          else video.addEventListener('canplaythrough', resolve, { once: true });
      frame.copyVideoFrameAt(point.x, point.y);
      await waitUntil(() => clipboard.availableFormats().includes('image/png'));
      expect(clipboard.readImage().isEmpty()).to.be.false();
      clipboard.clear();
    // TODO: Re-enable on Windows CI once Chromium fixes the intermittent
    // backwards-time DCHECK hit while copying video frames:
    // DCHECK failed: !delta.is_negative().
    ifit(!(process.platform === 'win32' && process.env.CI))('copies video frame in main frame', async () => {
      await w.webContents.loadFile(path.join(fixtures, 'blank.html'));
      await insertVideoInFrame(w.webContents.mainFrame);
      await copyVideoFrameInFrame(w.webContents.mainFrame);
    ifit(!(process.platform === 'win32' && process.env.CI))('copies video frame in subframe', async () => {
      await w.webContents.loadFile(path.join(subframesPath, 'frame-with-frame.html'));
      expect(subframe).to.exist();
      await insertVideoInFrame(subframe);
      await copyVideoFrameInFrame(subframe);
  describe('"frame-created" event', () => {
    it('emits when the main frame is created', async () => {
      const promise = once(w.webContents, 'frame-created') as Promise<[any, Electron.FrameCreatedDetails]>;
      w.webContents.loadFile(path.join(subframesPath, 'frame.html'));
      const [, details] = await promise;
      expect(details.frame).to.equal(w.webContents.mainFrame);
    it('emits when nested frames are created', async () => {
      const promise = emittedNTimes(w.webContents, 'frame-created', 2) as Promise<[any, Electron.FrameCreatedDetails][]>;
      w.webContents.loadFile(path.join(subframesPath, 'frame-container.html'));
      const [[, mainDetails], [, nestedDetails]] = await promise;
      expect(mainDetails.frame).to.equal(w.webContents.mainFrame);
      expect(nestedDetails.frame).to.equal(w.webContents.mainFrame.frames[0]);
    it('is not emitted upon cross-origin navigation', async () => {
      const server = await createServer();
      let frameCreatedEmitted = false;
      w.webContents.once('frame-created', () => {
        frameCreatedEmitted = true;
      expect(frameCreatedEmitted).to.be.false();
  describe('"dom-ready" event', () => {
    it('emits for top-level frame', async () => {
      const promise = once(w.webContents.mainFrame, 'dom-ready');
    it('emits for sub frame', async () => {
      const promise = new Promise<void>(resolve => {
        w.webContents.on('frame-created', (e, { frame }) => {
          frame!.on('dom-ready', () => {
            if (frame!.name === 'frameA') {
      w.webContents.loadFile(path.join(subframesPath, 'frame-with-frame.html'));
