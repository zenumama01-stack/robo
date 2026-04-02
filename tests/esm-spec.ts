import { stripVTControlCharacters } from 'node:util';
const runFixture = async (appPath: string, args: string[] = []) => {
  const result = cp.spawn(process.execPath, [appPath, ...args], {
    stdout: stripVTControlCharacters(Buffer.concat(stdout).toString().trim()),
    stderr: stripVTControlCharacters(Buffer.concat(stderr).toString().trim())
const fixturePath = path.resolve(__dirname, 'fixtures', 'esm');
    it('should load an esm entrypoint', async () => {
      const result = await runFixture(path.resolve(fixturePath, 'entrypoint.mjs'));
      expect(result.code).to.equal(0);
      expect(result.stdout).to.equal('ESM Launch, ready: false');
    it('should load an esm entrypoint based on type=module', async () => {
      const result = await runFixture(path.resolve(fixturePath, 'package'));
      expect(result.stdout).to.equal('ESM Package Launch, ready: false');
    it('should wait for a top-level await before declaring the app ready', async () => {
      const result = await runFixture(path.resolve(fixturePath, 'top-level-await.mjs'));
      expect(result.stdout).to.equal('Top level await, ready: false');
    it('should allow usage of pre-app-ready apis in top-level await', async () => {
      const result = await runFixture(path.resolve(fixturePath, 'pre-app-ready-apis.mjs'));
    it('should allow use of dynamic import', async () => {
      const result = await runFixture(path.resolve(fixturePath, 'dynamic.mjs'));
      expect(result.stdout).to.equal('Exit with app, ready: false');
      const result = await runFixture(path.resolve(fixturePath, 'electron-modules', 'import-lol.mjs'));
      expect(result.code).to.equal(1);
      expect(result.stderr).to.match(/Error \[ERR_MODULE_NOT_FOUND\]/);
      const result = await runFixture(path.resolve(fixturePath, 'electron-modules', 'import-main.mjs'));
      const result = await runFixture(path.resolve(fixturePath, 'electron-modules', 'import-renderer.mjs'));
      const result = await runFixture(path.resolve(fixturePath, 'electron-modules', 'import-common.mjs'));
      const result = await runFixture(path.resolve(fixturePath, 'electron-modules', 'import-utility.mjs'));
    const tempDirs: string[] = [];
      if (w) w.close();
      w = null;
      while (tempDirs.length) {
        await fs.promises.rm(tempDirs.pop()!, { force: true, recursive: true });
    async function loadWindowWithPreload (preload: string, webPreferences: Electron.WebPreferences) {
      const tmpDir = await fs.promises.mkdtemp(path.resolve(os.tmpdir(), 'e-spec-preload-'));
      tempDirs.push(tmpDir);
      const preloadPath = path.resolve(tmpDir, 'preload.mjs');
      await fs.promises.writeFile(preloadPath, preload);
      let error: Error | null = null;
      w.webContents.on('preload-error', (_, __, err) => {
      await w.loadFile(path.resolve(fixturePath, 'empty.html'));
      return [w.webContents, error] as [Electron.WebContents, Error | null];
    describe('nodeIntegration', () => {
      let badFilePath = '';
        badFilePath = path.resolve(path.resolve(os.tmpdir(), 'bad-file.badjs'));
        await fs.promises.writeFile(badFilePath, 'const foo = "bar";');
        await fs.promises.unlink(badFilePath);
      it('should support an esm entrypoint', async () => {
        const [webContents] = await loadWindowWithPreload('import { resolve } from "path"; window.resolvePath = resolve;', {
        const exposedType = await webContents.executeJavaScript('typeof window.resolvePath');
        expect(exposedType).to.equal('function');
      it('should delay load until the ESM import chain is complete', async () => {
        const [webContents] = await loadWindowWithPreload(`import { resolve } from "path";
        await new Promise(r => setTimeout(r, 500));
        window.resolvePath = resolve;`, {
      it('should support a top-level await fetch blocking the page load', async () => {
        const [webContents] = await loadWindowWithPreload(`
        const r = await fetch("package/package.json");
        window.packageJson = await r.json();`, {
        const packageJson = await webContents.executeJavaScript('window.packageJson');
        expect(packageJson).to.deep.equal(require('./fixtures/esm/package/package.json'));
      const hostsUrl = pathToFileURL(process.platform === 'win32' ? 'C:\\Windows\\System32\\drivers\\etc\\hosts' : '/etc/hosts');
      describe('without context isolation', () => {
        it('should use Blinks dynamic loader in the main world', async () => {
          const [webContents] = await loadWindowWithPreload('', {
            await webContents.executeJavaScript(`import(${JSON.stringify(hostsUrl)})`);
            error = err as Error;
          expect(error).to.not.equal(null);
          // This is a blink specific error message
          expect(error?.message).to.include('Failed to fetch dynamically imported module');
        it('should use Node.js ESM dynamic loader in the preload', async () => {
          const [, preloadError] = await loadWindowWithPreload(`await import(${JSON.stringify((pathToFileURL(badFilePath)))})`, {
          expect(preloadError).to.not.equal(null);
          // This is a node.js specific error message
          expect(preloadError!.toString()).to.include('Unknown file extension');
        it('should use import.meta callback handling from Node.js for Node.js modules', async () => {
          const result = await runFixture(path.resolve(fixturePath, 'import-meta'));
      describe('with context isolation', () => {
        it('should use Node.js ESM dynamic loader in the isolated context', async () => {
    describe('electron modules', () => {
        const [, error] = await loadWindowWithPreload('import { ipcRenderer } from "electron/lol";', {
        expect(error?.message).to.match(/Cannot find package 'electron'/);
        const [, error] = await loadWindowWithPreload('import { ipcRenderer } from "electron/main";', {
        expect(error).to.equal(null);
        const [, error] = await loadWindowWithPreload('import { ipcRenderer } from "electron/renderer";', {
        const [, error] = await loadWindowWithPreload('import { ipcRenderer } from "electron/common";', {
        const [, error] = await loadWindowWithPreload('import { ipcRenderer } from "electron/utility";', {
