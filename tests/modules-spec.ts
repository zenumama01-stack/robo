import { BrowserWindow, utilityProcess } from 'electron/main';
const Module = require('node:module') as NodeJS.ModuleInternal;
const nativeModulesEnabled = !process.env.ELECTRON_SKIP_NATIVE_MODULE_TESTS;
describe('modules support', () => {
  describe('third-party module', () => {
    ifdescribe(nativeModulesEnabled)('echo', () => {
      it('can be required in renderer', async () => {
          w.webContents.executeJavaScript(
            "{ require('@electron-ci/echo'); null }"
        ).to.be.fulfilled();
      it('can be required in node binary', async function () {
        const child = childProcess.fork(path.join(fixtures, 'module', 'echo.js'));
        const [msg] = await once(child, 'message');
        expect(msg).to.equal('ok');
      ifit(process.platform === 'win32')('can be required if electron.exe is renamed', () => {
        const testExecPath = path.join(path.dirname(process.execPath), 'test.exe');
        fs.copyFileSync(process.execPath, testExecPath);
          const fixture = path.join(fixtures, 'module', 'echo-renamed.js');
          expect(fs.existsSync(fixture)).to.be.true();
          const child = childProcess.spawnSync(testExecPath, [fixture]);
          expect(child.status).to.equal(0);
          fs.unlinkSync(testExecPath);
    const enablePlatforms: NodeJS.Platform[] = [
      'linux',
      'darwin',
      'win32'
    ifdescribe(nativeModulesEnabled && enablePlatforms.includes(process.platform))('module that use uv_dlopen', () => {
        await expect(w.webContents.executeJavaScript('{ require(\'@electron-ci/uv-dlopen\'); null }')).to.be.fulfilled();
        const child = childProcess.fork(path.join(fixtures, 'module', 'uv-dlopen.js'));
        const [exitCode] = await once(child, 'exit');
    describe('q', () => {
      describe('Q.when', () => {
        it('emits the fulfil callback', (done) => {
          const Q = require('q');
          Q(true).then((val: boolean) => {
            expect(val).to.be.true();
    describe('require(\'electron/...\')', () => {
      const utilityProcessFixturesPath = path.resolve(__dirname, 'fixtures', 'api', 'utility-process', 'electron-modules');
      it('require(\'electron/lol\') should throw in the main process', () => {
          require('electron/lol');
        }).to.throw(/Cannot find module 'electron\/lol'/);
      it('require(\'electron/lol\') should throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron/lol\'); null }')).to.eventually.be.rejected();
      it('require(\'electron/lol\') should throw in the utility process', async () => {
        const child = utilityProcess.fork(path.join(utilityProcessFixturesPath, 'require-lol.js'), [], {
        expect(stderr).to.match(/Cannot find module 'electron\/lol'/);
      it('require(\'electron\') should not throw in the main process', () => {
      it('require(\'electron\') should not throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron\'); null }')).to.be.fulfilled();
      it('require(\'electron/main\') should not throw in the main process', () => {
          require('electron/main');
      it('require(\'electron/main\') should not throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron/main\'); null }')).to.be.fulfilled();
      it('require(\'electron/main\') should not throw in the utility process', async () => {
        const child = utilityProcess.fork(path.join(utilityProcessFixturesPath, 'require-main.js'));
      it('require(\'electron/renderer\') should not throw in the main process', () => {
          require('electron/renderer');
      it('require(\'electron/renderer\') should not throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron/renderer\'); null }')).to.be.fulfilled();
      it('require(\'electron/renderer\') should not throw in the utility process', async () => {
        const child = utilityProcess.fork(path.join(utilityProcessFixturesPath, 'require-renderer.js'));
      it('require(\'electron/common\') should not throw in the main process', () => {
          require('electron/common');
      it('require(\'electron/common\') should not throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron/common\'); null }')).to.be.fulfilled();
      it('require(\'electron/common\') should not throw in the utility process', async () => {
        const child = utilityProcess.fork(path.join(utilityProcessFixturesPath, 'require-common.js'));
      it('require(\'electron/utility\') should not throw in the main process', () => {
          require('electron/utility');
      it('require(\'electron/utility\') should not throw in the renderer process', async () => {
        await expect(w.webContents.executeJavaScript('{ require(\'electron/utility\'); null }')).to.be.fulfilled();
      it('require(\'electron/utility\') should not throw in the utility process', async () => {
        const child = utilityProcess.fork(path.join(utilityProcessFixturesPath, 'require-utility.js'));
    describe('coffeescript', () => {
      it('can be registered and used to require .coffee files', () => {
          require('coffeescript').register();
        expect(require('./fixtures/module/test.coffee')).to.be.true();
  describe('global variables', () => {
    describe('process', () => {
      it('can be declared in a module', () => {
        expect(require('./fixtures/module/declare-process')).to.equal('declared process');
    describe('global', () => {
        expect(require('./fixtures/module/declare-global')).to.equal('declared global');
    describe('Buffer', () => {
        expect(require('./fixtures/module/declare-buffer')).to.equal('declared Buffer');
  describe('Module._nodeModulePaths', () => {
    describe('when the path is inside the resources path', () => {
      it('does not include paths outside of the resources path', () => {
        let modulePath = process.resourcesPath;
        expect(Module._nodeModulePaths(modulePath)).to.deep.equal([
          path.join(process.resourcesPath, 'node_modules')
        modulePath = process.resourcesPath + '-foo';
        const nodeModulePaths = Module._nodeModulePaths(modulePath);
        expect(nodeModulePaths).to.include(path.join(modulePath, 'node_modules'));
        expect(nodeModulePaths).to.include(path.join(modulePath, '..', 'node_modules'));
        modulePath = path.join(process.resourcesPath, 'foo');
          path.join(process.resourcesPath, 'foo', 'node_modules'),
        modulePath = path.join(process.resourcesPath, 'node_modules', 'foo');
          path.join(process.resourcesPath, 'node_modules', 'foo', 'node_modules'),
        modulePath = path.join(process.resourcesPath, 'node_modules', 'foo', 'bar');
          path.join(process.resourcesPath, 'node_modules', 'foo', 'bar', 'node_modules'),
        modulePath = path.join(process.resourcesPath, 'node_modules', 'foo', 'node_modules', 'bar');
          path.join(process.resourcesPath, 'node_modules', 'foo', 'node_modules', 'bar', 'node_modules'),
    describe('when the path is outside the resources path', () => {
      it('includes paths outside of the resources path', () => {
        const modulePath = path.resolve('/foo');
          path.join(modulePath, 'node_modules'),
          path.resolve('/node_modules')
  describe('require', () => {
    describe('when loaded URL is not file: protocol', () => {
      it('searches for module under app directory', async () => {
        const result = await w.webContents.executeJavaScript('typeof require("q").when');
        expect(result).to.equal('function');
    it('can load the built-in "electron" module via ESM import', async () => {
      await expect(import('electron')).to.eventually.be.ok();
    it('the built-in "electron" module loaded via ESM import has the same exports as the CJS module', async () => {
      const esmElectron = await import('electron');
      const cjsElectron = require('electron');
      expect(Object.keys(esmElectron)).to.deep.equal(Object.keys(cjsElectron));
