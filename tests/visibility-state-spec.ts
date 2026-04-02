import { BaseWindow, BrowserWindow, BrowserWindowConstructorOptions, webContents, WebContents, WebContentsView } from 'electron/main';
import { ifdescribe, waitUntil } from './lib/spec-helpers';
// visibilityState specs pass on linux with a real window manager but on CI
// the environment does not let these specs pass
ifdescribe(process.platform !== 'linux')('document.visibilityState', () => {
  let w: BaseWindow & {webContents: WebContents};
    for (const checkWin of BaseWindow.getAllWindows()) {
      console.log('WINDOW EXISTS BEFORE TEST STARTED:', checkWin.title, checkWin.id);
  const load = () => w.webContents.loadFile(path.resolve(__dirname, 'fixtures', 'chromium', 'visibilitystate.html'));
  async function haveVisibilityState (state: string) {
    const docVisState = await w.webContents.executeJavaScript('document.visibilityState');
  const itWithOptions = (name: string, options: BrowserWindowConstructorOptions, fn: Mocha.Func) => {
    it(name, async function (...args) {
        paintWhenInitiallyHidden: false,
          ...(options.webPreferences || {}),
      if (options.show && process.platform === 'darwin') {
      await Promise.resolve(fn.apply(this, args));
    it(name + ' with BaseWindow', async function (...args) {
      const baseWindow = new BaseWindow({
      const wcv = new WebContentsView({ webPreferences: { ...(options.webPreferences ?? {}), nodeIntegration: true, contextIsolation: false } });
      baseWindow.contentView = wcv;
      w = Object.assign(baseWindow, { webContents: wcv.webContents });
  itWithOptions('should be visible when the window is initially shown by default', {}, async () => {
    load();
    await expect(waitUntil(async () => await haveVisibilityState('visible'))).to.eventually.be.fulfilled();
  itWithOptions('should be visible when the window is initially shown', {
  }, async () => {
  itWithOptions('should be hidden when the window is initially hidden', {
    await expect(waitUntil(async () => await haveVisibilityState('hidden'))).to.eventually.be.fulfilled();
  itWithOptions('should be visible when the window is initially hidden but shown before the page is loaded', {
  itWithOptions('should be hidden when the window is initially shown but hidden before the page is loaded', {
  itWithOptions('should be toggle between visible and hidden as the window is hidden and shown', {}, async () => {
  itWithOptions('should become hidden when a window is minimized', {}, async () => {
  itWithOptions('should become visible when a window is restored', {}, async () => {
  ifdescribe(process.platform === 'darwin')('on platforms that support occlusion detection', () => {
    let child: cp.ChildProcess;
    const makeOtherWindow = (opts: { x: number; y: number; width: number; height: number; }) => {
      child = cp.spawn(process.execPath, [path.resolve(__dirname, 'fixtures', 'chromium', 'other-window.js'), `${opts.x}`, `${opts.y}`, `${opts.width}`, `${opts.height}`]);
      return new Promise<void>(resolve => {
          if (chunk.toString().includes('__ready__')) resolve();
      if (child && !child.killed) {
        child.kill('SIGTERM');
    itWithOptions('should be visible when two windows are on screen', {
      await makeOtherWindow({
    itWithOptions('should be visible when two windows are on screen that overlap partially', {
      height: 150
    itWithOptions('should be hidden when a second window completely occludes the current window', {
      height: 50
    }, async function () {
      this.timeout(240000);
      makeOtherWindow({
