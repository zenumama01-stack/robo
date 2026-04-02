import { ipcMain, session, webContents as webContentsModule, WebContents } from 'electron/main';
import { once, on } from 'node:events';
import { listen, waitUntil } from './lib/spec-helpers';
// Toggle to add extra debug output
const DEBUG = !process.env.CI;
describe('ServiceWorkerMain module', () => {
  const preloadRealmFixtures = path.resolve(fixtures, 'api/preload-realm');
  const webContentsInternal: typeof ElectronInternal.WebContents = webContentsModule as any;
  let ses: Electron.Session;
  let serviceWorkers: Electron.ServiceWorkers;
  let baseUrl: string;
  let wc: WebContents;
    ses = session.fromPartition(`service-worker-main-spec-${crypto.randomUUID()}`);
    serviceWorkers = ses.serviceWorkers;
    if (DEBUG) {
      serviceWorkers.on('console-message', (_e, details) => {
        console.log(details.message);
      serviceWorkers.on('running-status-changed', ({ versionId, runningStatus }) => {
        console.log(`version ${versionId} is ${runningStatus}`);
      const url = new URL(req.url!, `http://${req.headers.host}`);
      // /{uuid}/{file}
      const file = url.pathname!.split('/')[2]!;
      if (file.endsWith('.js')) {
        res.setHeader('Content-Type', 'application/javascript');
      res.end(fs.readFileSync(path.resolve(fixtures, 'api', 'service-workers', file)));
    baseUrl = `http://localhost:${port}/${uuid}`;
    wc = webContentsInternal.create({ session: ses });
      wc.on('console-message', ({ message }) => {
    if (!wc.isDestroyed()) wc.destroy();
    ses.getPreloadScripts().map(({ id }) => ses.unregisterPreloadScript(id));
  function registerPreload (scriptName: string) {
    const id = ses.registerPreloadScript({
      type: 'service-worker',
      filePath: path.resolve(preloadRealmFixtures, scriptName)
    expect(id).to.be.a('string');
  async function loadWorkerScript (scriptUrl?: string) {
    const scriptParams = scriptUrl ? `?scriptUrl=${scriptUrl}` : '';
    return wc.loadURL(`${baseUrl}/index.html${scriptParams}`);
  async function unregisterAllServiceWorkers () {
    await wc.executeJavaScript(`(${async function () {
      const registrations = await navigator.serviceWorker.getRegistrations();
      for (const registration of registrations) {
        registration.unregister();
    }}())`);
  async function waitForServiceWorker (expectedRunningStatus: Electron.ServiceWorkersRunningStatusChangedEventParams['runningStatus'] = 'starting') {
    const serviceWorkerPromise = new Promise<Electron.ServiceWorkerMain>((resolve) => {
      function onRunningStatusChanged ({ versionId, runningStatus }: Electron.ServiceWorkersRunningStatusChangedEventParams) {
        if (runningStatus === expectedRunningStatus) {
          const serviceWorker = serviceWorkers.getWorkerFromVersionID(versionId)!;
          serviceWorkers.off('running-status-changed', onRunningStatusChanged);
          resolve(serviceWorker);
      serviceWorkers.on('running-status-changed', onRunningStatusChanged);
    const serviceWorker = await serviceWorkerPromise;
    expect(serviceWorker).to.not.be.undefined();
    return serviceWorker!;
  /** Runs a test using the framework in preload-tests.js */
  const runTest = async (serviceWorker: Electron.ServiceWorkerMain, rpc: { name: string, args: any[] }) => {
    serviceWorker.send('test', uuid, rpc.name, ...rpc.args);
      serviceWorker.ipc.once(`test-result-${uuid}`, (_event, { error, result }) => {
          reject(result);
  describe('serviceWorkers.getWorkerFromVersionID', () => {
    it('returns undefined for non-live service worker', () => {
      expect(serviceWorkers.getWorkerFromVersionID(-1)).to.be.undefined();
      expect(serviceWorkers._getWorkerFromVersionIDIfExists(-1)).to.be.undefined();
    it('returns instance for live service worker', async () => {
      const runningStatusChanged = once(serviceWorkers, 'running-status-changed');
      loadWorkerScript();
      const [{ versionId }] = await runningStatusChanged;
      const serviceWorker = serviceWorkers.getWorkerFromVersionID(versionId);
      const ifExistsServiceWorker = serviceWorkers._getWorkerFromVersionIDIfExists(versionId);
      expect(ifExistsServiceWorker).to.not.be.undefined();
      expect(serviceWorker).to.equal(ifExistsServiceWorker);
    it('does not crash on script error', async () => {
      wc.loadURL(`${baseUrl}/index.html?scriptUrl=sw-script-error.js`);
      let serviceWorker;
      const actualStatuses = [];
      for await (const [{ versionId, runningStatus }] of on(serviceWorkers, 'running-status-changed')) {
        if (!serviceWorker) {
          serviceWorker = serviceWorkers.getWorkerFromVersionID(versionId);
        actualStatuses.push(runningStatus);
        if (runningStatus === 'stopping') {
      expect(actualStatuses).to.deep.equal(['starting', 'stopping']);
    it('does not find unregistered service worker', async () => {
      const runningServiceWorker = await waitForServiceWorker('running');
      const { versionId } = runningServiceWorker;
      unregisterAllServiceWorkers();
      await waitUntil(() => runningServiceWorker.isDestroyed());
      expect(serviceWorker).to.be.undefined();
  describe('isDestroyed()', () => {
    it('is not destroyed after being created', async () => {
      const serviceWorker = await waitForServiceWorker();
      expect(serviceWorker.isDestroyed()).to.be.false();
    it('is destroyed after being unregistered', async () => {
      await unregisterAllServiceWorkers();
      await waitUntil(() => serviceWorker.isDestroyed());
  describe('"running-status-changed" event', () => {
    it('handles when content::ServiceWorkerVersion has been destroyed', async () => {
      loadWorkerScript('sw-unregister-self.js');
      const serviceWorker = await waitForServiceWorker('running');
  describe('startWorkerForScope()', () => {
    it('resolves with running workers', async () => {
      const startWorkerPromise = serviceWorkers.startWorkerForScope(serviceWorker.scope);
      await expect(startWorkerPromise).to.eventually.be.fulfilled();
      const otherSW = await startWorkerPromise;
      expect(otherSW).to.equal(serviceWorker);
    it('rejects with starting workers', async () => {
      const serviceWorker = await waitForServiceWorker('starting');
      await expect(startWorkerPromise).to.eventually.be.rejected();
    it('starts previously stopped worker', async () => {
      const { scope } = serviceWorker;
      const stoppedPromise = waitForServiceWorker('stopped');
      await serviceWorkers._stopAllWorkers();
      await stoppedPromise;
      const startWorkerPromise = serviceWorkers.startWorkerForScope(scope);
    it('resolves when called twice', async () => {
      const [swA, swB] = await Promise.all([
        serviceWorkers.startWorkerForScope(scope),
        serviceWorkers.startWorkerForScope(scope)
      expect(swA).to.equal(swB);
      expect(swA).to.equal(serviceWorker);
  describe('startTask()', () => {
    it('has no tasks in-flight initially', async () => {
      expect(serviceWorker._countExternalRequests()).to.equal(0);
    it('can start and end a task', async () => {
      // Internally, ServiceWorkerVersion buckets tasks into requests made
      // during and after startup.
      // ServiceWorkerContext::CountExternalRequestsForTest only considers
      // requests made while SW is in running status so we need to wait for that
      // to read an accurate count.
      const task = serviceWorker.startTask();
      expect(task).to.be.an('object');
      expect(task).to.have.property('end').that.is.a('function');
      expect(serviceWorker._countExternalRequests()).to.equal(1);
      task.end();
      // Count will decrement after Promise.finally callback
      await new Promise<void>(queueMicrotask);
    it('can have more than one active task', async () => {
      const taskA = serviceWorker.startTask();
      const taskB = serviceWorker.startTask();
      expect(serviceWorker._countExternalRequests()).to.equal(2);
      taskB.end();
      taskA.end();
    it('throws when starting task after destroyed', async () => {
      expect(() => serviceWorker.startTask()).to.throw();
    it('throws when ending task after destroyed', async () => {
      expect(() => task.end()).to.throw();
  describe("'versionId' property", () => {
    it('matches the expected value', async () => {
      wc.loadURL(`${baseUrl}/index.html`);
      if (!serviceWorker) return;
      expect(serviceWorker).to.have.property('versionId').that.is.a('number');
      expect(serviceWorker.versionId).to.equal(versionId);
  describe("'scope' property", () => {
      expect(serviceWorker).to.have.property('scope').that.is.a('string');
      expect(serviceWorker.scope).to.equal(`${baseUrl}/`);
  describe("'scriptURL' property", () => {
      expect(serviceWorker).to.have.property('scriptURL').that.is.a('string');
      expect(serviceWorker.scriptURL).to.equal(`${baseUrl}/sw.js`);
  describe('ipc', () => {
      registerPreload('preload-tests.js');
    describe('on(channel)', () => {
      it('can receive a message during startup', async () => {
        registerPreload('preload-send-ping.js');
        const pingPromise = once(serviceWorker.ipc, 'ping');
        await pingPromise;
      it('receives a message', async () => {
        runTest(serviceWorker, { name: 'testSend', args: ['ping'] });
      it('does not receive message on ipcMain', async () => {
          let pingReceived = false;
          once(ipcMain, 'ping', { signal: abortController.signal }).then(() => {
            pingReceived = true;
          await once(ses, '-ipc-message');
          expect(pingReceived).to.be.false();
    describe('handle(channel)', () => {
      it('receives and responds to message', async () => {
        serviceWorker.ipc.handle('ping', () => 'pong');
        const result = await runTest(serviceWorker, { name: 'testInvoke', args: ['ping'] });
        expect(result).to.equal('pong');
      it('works after restarting worker', async () => {
        await serviceWorkers.startWorkerForScope(scope);
    it('can evaluate func from preload realm', async () => {
      const result = await runTest(serviceWorker, { name: 'testEvaluate', args: ['evalConstructorName'] });
      expect(result).to.equal('ServiceWorkerGlobalScope');
    it('does not leak prototypes', async () => {
      const result = await runTest(serviceWorker, { name: 'testPrototypeLeak', args: [] });
  describe('extensions', () => {
    const extensionFixtures = path.join(fixtures, 'extensions');
    const testExtensionFixture = path.join(extensionFixtures, 'mv3-service-worker');
      ses = session.fromPartition(`persist:${crypto.randomUUID()}-service-worker-main-spec`);
    it('can observe extension service workers', async () => {
      const serviceWorkerPromise = waitForServiceWorker();
      const extension = await ses.extensions.loadExtension(testExtensionFixture);
      expect(serviceWorker.scope).to.equal(extension.url);
    it('has extension state available when preload runs', async () => {
      registerPreload('preload-send-extension.js');
      const extensionPromise = ses.extensions.loadExtension(testExtensionFixture);
      const result = await new Promise<any>((resolve) => {
        serviceWorker.ipc.handleOnce('preload-extension-result', (_event, result) => {
      const extension = await extensionPromise;
      expect(result).to.be.an('object');
      expect(result.id).to.equal(extension.id);
      expect(result.manifest).to.deep.equal(result.manifest);
