import { BrowserWindow, screen } from 'electron/main';
import { hasCapturableScreen } from './lib/screen-helpers';
// Try to load robotjs
let robot: typeof import('@hurdlegroup/robotjs');
  robot = require('@hurdlegroup/robotjs');
  // ignore. tests are skipped below if this is undefined.
const draggablePageURL = pathToFileURL(
  path.join(fixtures, 'pages', 'draggable-page.html')
const iframePageURL = pathToFileURL(
  path.join(fixtures, 'pages', 'iframe.html')
const webviewPageURL = pathToFileURL(
  path.join(fixtures, 'pages', 'webview.html')
const testWindowOpts: Electron.BrowserWindowConstructorOptions = {
  width: Math.round(display.bounds.width / 2),
  height: Math.round(display.bounds.height / 2),
const center = (rect: Electron.Rectangle): Electron.Point => ({
  x: Math.round(rect.x + rect.width / 2),
  y: Math.round(rect.y + rect.height / 2)
const performDrag = async (
  w: BrowserWindow
  start: [number, number];
  end: [number, number];
  const winBounds = w.getBounds();
  const winCenter = center(winBounds);
  const screenCenter = center(display.bounds);
  const start = w.getPosition() as [number, number];
  const moved = once(w, 'move');
  // Extra events based on research from https://github.com/octalmage/robotjs/issues/389
  robot.moveMouse(winCenter.x, winCenter.y);
  robot.mouseToggle('down', 'left');
  robot.moveMouse(winCenter.x + 2, winCenter.y + 2); // extra
  await setTimeout(200); // extra
  robot.dragMouse(screenCenter.x, screenCenter.y);
  robot.mouseToggle('up', 'left');
  await Promise.race([moved, setTimeout(1000)]);
  const end = w.getPosition() as [number, number];
const loadDraggableSubframe = async (w: BrowserWindow): Promise<void> => {
  let selector: string;
  let eventName: string;
  if (w.getURL() === iframePageURL.href) {
    selector = 'iframe';
    eventName = 'load';
  } else if (w.getURL() === webviewPageURL.href) {
    selector = 'webview';
    eventName = 'did-finish-load';
    throw new Error('Unexpected page loaded');
      const frame = document.querySelector('${selector}');
      frame.addEventListener(
        '${eventName}',
        () => resolve(),
      frame.src = '${draggablePageURL.href}';
describe('draggable regions', function () {
    if (!robot || !robot.moveMouse || !hasCapturableScreen()) {
    // The first window may not properly receive events due to UI transitions or
    // focus management. To mitigate this, warm up with a test run.
    const w = new BrowserWindow(testWindowOpts);
    await w.loadURL(draggablePageURL.href);
    await performDrag(w);
  describe('main window', () => {
      w = new BrowserWindow(testWindowOpts);
    it('drags with app-region: drag', async () => {
      const { start, end } = await performDrag(w);
      expect(start).to.not.deep.equal(end);
    it('does not drag when app-region: no-drag overlaps drag region', async () => {
      const noDragURL = new URL(draggablePageURL.href);
      noDragURL.searchParams.set('no-drag', '1');
      await w.loadURL(noDragURL.href);
      expect(start).to.deep.equal(end);
    it('drags after navigation', async () => {
      await w.loadFile(path.join(fixtures, 'pages', 'base-page.html'));
    it('drags after in-page navigation', async () => {
      const didNavigate = once(w.webContents, 'did-navigate-in-page');
        window.history.pushState({}, '', '/new-path');
      await didNavigate;
  describe('child windows (window.open)', () => {
    let childWindow: BrowserWindow;
      await parentWindow.loadFile(
        path.join(fixtures, 'pages', 'base-page.html')
      parentWindow.webContents.setWindowOpenHandler(() => ({
        overrideBrowserWindowOptions: testWindowOpts
      const newBrowserWindow = once(parentWindow.webContents, 'did-create-window');
      await parentWindow.webContents.executeJavaScript(
        `void window.open('${draggablePageURL.href}', '_blank');`
      [childWindow] = await newBrowserWindow;
      await once(childWindow, 'ready-to-show');
      const { start, end } = await performDrag(childWindow);
      await childWindow.loadURL(draggablePageURL.href);
  for (const frameType of ['webview', 'iframe'] as const) {
    // FIXME: this behavior is broken before the tests were added
    // See: https://github.com/electron/electron/issues/49256
    describe.skip(`child frames (${frameType})`, () => {
      const subframePageURL = frameType === 'webview' ? webviewPageURL : iframePageURL;
          ...testWindowOpts,
          webPreferences: frameType === 'webview'
            : {}
      it('drags in subframe with app-region: drag', async () => {
        await w.loadURL(subframePageURL.href);
        await loadDraggableSubframe(w);
      it('drags after subframe navigation', async () => {
      it('does not drag after host page navigation without draggable region', async () => {
      it('drags after host page navigation', async () => {
