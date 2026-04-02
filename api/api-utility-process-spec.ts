import { systemPreferences } from 'electron';
import { BrowserWindow, MessageChannelMain, utilityProcess, app } from 'electron/main';
import * as fs from 'node:fs/promises';
import { setImmediate } from 'node:timers/promises';
import { respondOnce, randomString, kOneKiloByte } from './lib/net-helpers';
import { ifit, startRemoteControlApp } from './lib/spec-helpers';
const fixturesPath = path.resolve(__dirname, 'fixtures', 'api', 'utility-process');
const isWindows32Bit = process.platform === 'win32' && process.arch === 'ia32';
describe('utilityProcess module', () => {
  describe('UtilityProcess constructor', () => {
    it('throws when empty script path is provided', async () => {
        utilityProcess.fork('');
    it('throws when options.stdio is not valid', async () => {
        utilityProcess.fork(path.join(fixturesPath, 'empty.js'), [], {
          execArgv: ['--test', '--test2'],
          serviceName: 'test',
          stdio: 'ipc'
      }).to.throw(/stdio must be of the following values: inherit, pipe, ignore/);
          stdio: ['ignore', 'ignore']
      }).to.throw(/configuration missing for stdin, stdout or stderr/);
          stdio: ['pipe', 'inherit', 'inherit']
      }).to.throw(/stdin value other than ignore is not supported/);
  describe('lifecycle events', () => {
    it('emits \'spawn\' when child process successfully launches', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'empty.js'));
    it('emits \'exit\' when child process exits gracefully', (done) => {
      child.on('exit', (code) => {
    it('emits \'exit\' when the child process file does not exist', (done) => {
      const child = utilityProcess.fork('nonexistent');
        expect(code).to.equal(1);
    ifit(!isWindows32Bit)('emits the correct error code when child process exits nonzero', async () => {
      const exit = once(child, 'exit');
      process.kill(child.pid!);
      const [code] = await exit;
      expect(code).to.not.equal(0);
    ifit(!isWindows32Bit)('emits the correct error code when child process is killed', async () => {
    ifit(!isWindows32Bit)('emits \'exit\' when child process crashes', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'crash.js'));
      // SIGSEGV code can differ across pipelines but should never be 0.
    ifit(!isWindows32Bit)('emits \'exit\' corresponding to the child process', async () => {
      const child1 = utilityProcess.fork(path.join(fixturesPath, 'endless.js'));
      await once(child1, 'spawn');
      const child2 = utilityProcess.fork(path.join(fixturesPath, 'crash.js'));
      await once(child2, 'exit');
      expect(child1.kill()).to.be.true();
      await once(child1, 'exit');
    it('emits \'exit\' when there is uncaught exception', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'exception.js'));
    it('emits \'exit\' when there is uncaught exception in ESM', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'exception.mjs'));
    it('emits \'exit\' when process.exit is called', async () => {
      const exitCode = 2;
      const child = utilityProcess.fork(path.join(fixturesPath, 'custom-exit.js'), [`--exitCode=${exitCode}`]);
      expect(code).to.equal(exitCode);
    ifit(process.platform === 'win32')('emits correct exit code when high bit is set on Windows', async () => {
      // NTSTATUS code with high bit set should not be mangled by sign extension.
      const exitCode = 0xC0000005;
    ifit(process.platform !== 'win32')('emits correct exit code when child process crashes on posix', async () => {
      // Crash exit codes should not be sign-extended to large 64-bit values.
      expect(code).to.be.lessThanOrEqual(0xFFFFFFFF);
    it('does not run JS after process.exit is called', async () => {
      const file = path.join(os.tmpdir(), `no-js-after-exit-log-${Math.random()}`);
      const child = utilityProcess.fork(path.join(fixturesPath, 'no-js-after-exit.js'), [`--testPath=${file}`]);
      let handle = null;
      const lines = [];
        handle = await fs.open(file);
        for await (const line of handle.readLines()) {
        await handle?.close();
        await fs.rm(file, { force: true });
      expect(lines.length).to.equal(1);
      expect(lines[0]).to.equal('before exit');
    // 32-bit system will not have V8 Sandbox enabled.
    // WoA testing does not have VS toolchain configured to build native addons.
    ifit(process.arch !== 'ia32' && process.arch !== 'arm' && !isWindowsOnArm)('emits \'error\' when fatal error is triggered from V8', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'external-ab-test.js'));
      const [type, location, report] = await once(child, 'error');
      expect(type).to.equal('FatalError');
      expect(location).to.equal('v8_ArrayBuffer_NewBackingStore');
      const reportJSON = JSON.parse(report);
      expect(reportJSON.header.trigger).to.equal('v8_ArrayBuffer_NewBackingStore');
      const addonPath = path.join(require.resolve('@electron-ci/external-ab'), '..', '..', 'build', 'Release', 'external_ab.node');
      expect(reportJSON.sharedObjects).to.include(path.toNamespacedPath(addonPath));
  describe('app \'child-process-gone\' event', () => {
    const waitForCrash = (name: string) => {
      return new Promise<Electron.Details>((resolve) => {
        app.on('child-process-gone', function onCrash (_event, details) {
          if (details.name === name) {
            app.off('child-process-gone', onCrash);
    ifit(!isWindows32Bit)('with default serviceName', async () => {
      const name = 'Node Utility Process';
      const crashPromise = waitForCrash(name);
      utilityProcess.fork(path.join(fixturesPath, 'crash.js'));
      const details = await crashPromise;
      expect(details.type).to.equal('Utility');
      expect(details.serviceName).to.equal('node.mojom.NodeService');
      expect(details.name).to.equal(name);
    ifit(!isWindows32Bit)('with custom serviceName', async () => {
      const name = crypto.randomUUID();
      utilityProcess.fork(path.join(fixturesPath, 'crash.js'), [], { serviceName: name });
  describe('app.getAppMetrics()', () => {
    it('with default serviceName', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'endless.js'));
      expect(child.pid).to.not.be.null();
      await setImmediate();
      const details = app.getAppMetrics().find(item => item.pid === child.pid)!;
      expect(details).to.be.an('object');
      expect(details.serviceName).to.to.equal('node.mojom.NodeService');
      expect(details.name).to.equal('Node Utility Process');
    it('with custom serviceName', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'endless.js'), [], { serviceName: 'Hello World!' });
      expect(details.name).to.equal('Hello World!');
  describe('kill() API', () => {
    it('terminates the child process gracefully', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'endless.js'), [], {
        serviceName: 'endless'
      expect(child.kill()).to.be.true();
  describe('esm', () => {
    it('is launches an mjs file', async () => {
      const fixtureFile = path.join(fixturesPath, 'esm.mjs');
      const child = utilityProcess.fork(fixtureFile, [], {
        stdio: 'pipe'
      expect(child.stdout).to.not.be.null();
      let log = '';
      child.stdout!.on('data', (chunk) => {
        log += chunk.toString('utf8');
      expect(log).to.equal(pathToFileURL(fixtureFile) + '\n');
    it('import \'electron/lol\' should throw', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'electron-modules', 'import-lol.mjs'), [], {
        stdio: ['ignore', 'ignore', 'pipe']
      child.stderr!.on('data', (data) => { stderr += data.toString('utf8'); });
      expect(stderr).to.match(/Error \[ERR_MODULE_NOT_FOUND\]/);
    it('import \'electron/main\' should not throw', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'electron-modules', 'import-main.mjs'));
    it('import \'electron/renderer\' should not throw', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'electron-modules', 'import-renderer.mjs'));
    it('import \'electron/common\' should not throw', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'electron-modules', 'import-common.mjs'));
    it('import \'electron/utility\' should not throw', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'electron-modules', 'import-utility.mjs'));
  describe('pid property', () => {
    it('is valid when child process launches successfully', async () => {
      expect(child).to.have.property('pid').that.is.a('number');
    it('is undefined when child process fails to launch', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'does-not-exist.js'));
      expect(child.pid).to.be.undefined();
    it('is undefined before the child process is spawned succesfully', async () => {
    it('is undefined when child process is killed', async () => {
  describe('stdout property', () => {
    it('is null when child process launches with default stdio', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'log.js'));
      expect(child.stdout).to.be.null();
      expect(child.stderr).to.be.null();
    it('is null when child process launches with ignore stdio configuration', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'log.js'), [], {
        stdio: 'ignore'
    it('is valid when child process launches with pipe stdio configuration', async () => {
      expect(log).to.equal('hello\n');
  describe('stderr property', () => {
    ifit(!isWindowsOnArm)('is valid when child process launches with pipe stdio configuration', async () => {
        stdio: ['ignore', 'pipe', 'pipe']
      expect(child.stderr).to.not.be.null();
      child.stderr!.on('data', (chunk) => {
      expect(log).to.equal('world');
  describe('postMessage() API', () => {
    it('establishes a default ipc channel with the child process', async () => {
      const result = 'I will be echoed.';
      const child = utilityProcess.fork(path.join(fixturesPath, 'post-message.js'));
      child.postMessage(result);
      expect(data).to.equal(result);
      await exit;
    it('supports queuing messages on the receiving end', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'post-message-queue.js'));
      const p = once(child, 'spawn');
      child.postMessage('This message');
      child.postMessage(' is');
      child.postMessage(' queued');
      expect(data).to.equal('This message is queued');
    it('handles the parent port trying to send an non-clonable object', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'non-cloneable.js'));
      child.postMessage('non-cloneable');
      expect(data).to.equal('caught-non-cloneable');
  describe('behavior', () => {
    it('supports starting the v8 inspector with --inspect-brk', (done) => {
        execArgv: ['--inspect-brk']
        child.stderr!.removeListener('data', listener);
        child.stdout!.removeListener('data', listener);
        child.once('exit', () => { done(); });
      const listener = (data: Buffer) => {
        if (/Debugger listening on ws:/m.test(output)) {
      child.stderr!.on('data', listener);
      child.stdout!.on('data', listener);
    it('supports starting the v8 inspector with --inspect and a provided port', (done) => {
        execArgv: ['--inspect=17364']
          expect(output.trim()).to.contain(':17364', 'should be listening on port 17364');
    it('supports changing dns verbatim with --dns-result-order', (done) => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'dns-result-order.js'), [], {
        execArgv: ['--dns-result-order=ipv4first']
        expect(output.trim()).to.contain('ipv4first', 'default verbatim should be ipv4first');
    ifit(process.platform !== 'win32')('supports redirecting stdout to parent process', async () => {
      const result = 'Output from utility process';
      const appProcess = childProcess.spawn(process.execPath, [path.join(fixturesPath, 'inherit-stdout'), `--payload=${result}`]);
      appProcess.stdout.on('data', (data: Buffer) => { output += data; });
      expect(output).to.equal(result);
    ifit(process.platform !== 'win32')('supports redirecting stderr to parent process', async () => {
      const result = 'Error from utility process';
      const appProcess = childProcess.spawn(process.execPath, [path.join(fixturesPath, 'inherit-stderr'), `--payload=${result}`]);
      appProcess.stderr.on('data', (data: Buffer) => { output += data; });
      expect(output).to.include(result);
    ifit(process.platform !== 'linux')('can access exposed main process modules from the utility process', async () => {
      const message = 'Message from utility process';
      const child = utilityProcess.fork(path.join(fixturesPath, 'expose-main-process-module.js'));
      child.postMessage(message);
      expect(data).to.equal(systemPreferences.getMediaAccessStatus('screen'));
    it('can establish communication channel with sandboxed renderer', async () => {
      const result = 'Message from sandboxed renderer';
          preload: path.join(fixturesPath, 'preload.js')
      await w.loadFile(path.join(__dirname, 'fixtures', 'blank.html'));
      // Create Message port pair for Renderer <-> Utility Process.
      const { port1: rendererPort, port2: childPort1 } = new MessageChannelMain();
      w.webContents.postMessage('port', result, [rendererPort]);
      // Send renderer and main channel port to utility process.
      const child = utilityProcess.fork(path.join(fixturesPath, 'receive-message.js'));
      child.postMessage('', [childPort1]);
    ifit(process.platform === 'linux')('allows executing a setuid binary with child_process', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'suid.js'));
      expect(data).to.not.be.empty();
    it('inherits parent env as default', async () => {
      const appProcess = childProcess.spawn(process.execPath, [path.join(fixturesPath, 'env-app')], {
          FROM: 'parent',
      const result = process.platform === 'win32' ? '\r\nparent' : 'parent';
    // TODO(codebytere): figure out why this is failing in ASAN- builds on Linux.
    ifit(!process.env.IS_ASAN)('does not inherit parent env when custom env is provided', async () => {
      const appProcess = childProcess.spawn(process.execPath, [path.join(fixturesPath, 'env-app'), '--create-custom-env'], {
      const result = process.platform === 'win32' ? '\r\nchild' : 'child';
    it('changes working directory with cwd', async () => {
      const child = utilityProcess.fork('./log.js', [], {
        cwd: fixturesPath,
        stdio: ['ignore', 'pipe', 'ignore']
    it('does not crash when running eval', async () => {
      const child = utilityProcess.fork('./eval.js', [], {
      expect(data).to.equal(42);
    it('should emit the app#login event when 401', async () => {
      const [loginAuthInfo, statusCode] = await remotely(async (serverUrl: string, fixture: string) => {
        const { app, utilityProcess } = require('electron');
        const { once } = require('node:events');
        const child = utilityProcess.fork(fixture, [`--server-url=${serverUrl}`], {
          stdio: 'ignore',
          respondToAuthRequestsFromMainProcess: true
        const [ev,,, authInfo, cb] = await once(app, 'login');
        ev.preventDefault();
        cb('dummy', 'pass');
        const [result] = await once(child, 'message');
        return [authInfo, ...result];
      }, serverUrl, path.join(fixturesPath, 'net.js'));
    it('should receive 401 response when cancelling authentication via app#login event', async () => {
      const [authDetails, responseBody, statusCode] = await remotely(async (serverUrl: string, fixture: string) => {
        const [,, details,, cb] = await once(app, 'login');
        const [response] = await once(child, 'message');
        const [responseBody] = await once(child, 'message');
        return [details, responseBody, ...response];
      expect(authDetails.url).to.equal(serverUrl);
      expect(statusCode).to.equal(401);
      expect(responseBody).to.equal('unauthenticated');
    it('should upload body when 401', async () => {
      const [authDetails, responseBody, statusCode] = await remotely(async (serverUrl: string, requestData: string, fixture: string) => {
        const child = utilityProcess.fork(fixture, [`--server-url=${serverUrl}`, '--request-data'], {
        child.postMessage(requestData);
        cb('user', 'pass');
      }, serverUrl, requestData, path.join(fixturesPath, 'net.js'));
      expect(responseBody).to.equal(requestData);
    it('should not emit the app#login event when 401 with {"credentials":"omit"}', async () => {
      const [statusCode, responseHeaders] = await rc.remotely(async (serverUrl: string, fixture: string) => {
        let gracefulExit = true;
        const child = utilityProcess.fork(fixture, [`--server-url=${serverUrl}`, '--omit-credentials'], {
        app.on('login', () => {
          gracefulExit = false;
          if (gracefulExit) {
      expect(responseHeaders['www-authenticate']).to.equal('Basic realm="Foo"');
    it('should not emit the app#login event with default respondToAuthRequestsFromMainProcess', async () => {
      const [loginAuthInfo, statusCode] = await rc.remotely(async (serverUrl: string, fixture: string) => {
        const child = utilityProcess.fork(fixture, [`--server-url=${serverUrl}`, '--use-net-login-event'], {
        const [authInfo] = await once(child, 'message');
    it('should emit the app#login event when creating requests with fetch API', async () => {
        const child = utilityProcess.fork(fixture, [`--server-url=${serverUrl}`, '--use-fetch-api'], {
        return [authInfo, ...response];
    it('supports generating snapshots via v8.setHeapSnapshotNearHeapLimit', async () => {
      const tmpDir = await fs.mkdtemp(path.resolve(os.tmpdir(), 'electron-spec-utility-oom-'));
      const child = utilityProcess.fork(path.join(fixturesPath, 'oom-grow.js'), [], {
        execArgv: [
          `--diagnostic-dir=${tmpDir}`,
          '--js-flags=--max-old-space-size=50'
          NODE_DEBUG_NATIVE: 'diagnostic'
      const files = (await fs.readdir(tmpDir)).filter((file) => file.endsWith('.heapsnapshot'));
      expect(files.length).to.be.equal(1);
      const stat = await fs.stat(path.join(tmpDir, files[0]));
      expect(stat.size).to.be.greaterThan(0);
      await fs.rm(tmpDir, { recursive: true });
    it('supports --no-experimental-global-navigator flag', async () => {
        const child = utilityProcess.fork(path.join(fixturesPath, 'navigator.js'), [], {
        expect(data).to.be.true();
            '--no-experimental-global-navigator'
        expect(data).to.be.false();
    // Note: This doesn't test that disclaiming works (that requires stubbing / mocking TCC which is
    // just straight up not possible generically). This just tests that utility processes still launch
    // when disclaimed.
    ifit(process.platform === 'darwin')('supports disclaim option on macOS', async () => {
      const child = utilityProcess.fork(path.join(fixturesPath, 'post-message.js'), [], {
        disclaim: true
      expect(child.pid).to.be.a('number');
      // Verify the process can communicate normally
      const testMessage = 'test-disclaim';
      child.postMessage(testMessage);
      expect(data).to.equal(testMessage);
