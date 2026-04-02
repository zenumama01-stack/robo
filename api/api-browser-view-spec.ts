import { BrowserView, BrowserWindow, screen, session, webContents } from 'electron/main';
import { ScreenCapture, hasCapturableScreen } from './lib/screen-helpers';
import { defer, ifit, startRemoteControlApp } from './lib/spec-helpers';
import { closeWindow } from './lib/window-helpers';
describe('BrowserView module', () => {
  const fixtures = path.resolve(__dirname, 'fixtures');
  const ses = session.fromPartition(crypto.randomUUID());
  let view: BrowserView;
  const getSessionWebContents = () =>
    webContents.getAllWebContents().filter(wc => wc.session === ses);
    expect(getSessionWebContents().length).to.equal(0, 'expected no webContents to exist');
      width: 400,
        backgroundThrottling: false,
        session: ses
    if (w && !w.isDestroyed()) {
      const p = once(w.webContents, 'destroyed');
      await closeWindow(w);
      w = null as any;
      await p;
    if (view && view.webContents) {
      const p = once(view.webContents, 'destroyed');
      view.webContents.destroy();
      view = null as any;
  it('sets the correct class name on the prototype', () => {
    expect(BrowserView.prototype.constructor.name).to.equal('BrowserView');
  it('can be created with an existing webContents', async () => {
    const wc = (webContents as typeof ElectronInternal.WebContents).create({ session: ses, sandbox: true });
    await wc.loadURL('about:blank');
    view = new BrowserView({ webContents: wc } as any);
    expect(view.webContents === wc).to.be.true('view.webContents === wc');
    expect(view.webContents.getURL()).to.equal('about:blank');
  it('has type browserView', () => {
    view = new BrowserView();
    expect(view.webContents.getType()).to.equal('browserView');
  describe('BrowserView.setBackgroundColor()', () => {
    it('does not throw for valid args', () => {
      view.setBackgroundColor('#000');
    // We now treat invalid args as "no background".
    it('does not throw for invalid args', () => {
        view.setBackgroundColor({} as any);
    ifit(hasCapturableScreen())('sets the background color to transparent if none is set', async () => {
      const display = screen.getPrimaryDisplay();
      const WINDOW_BACKGROUND_COLOR = '#55ccbb';
      w.setBounds(display.bounds);
      w.setBackgroundColor(WINDOW_BACKGROUND_COLOR);
      await w.loadURL('data:text/html,<html></html>');
      view.setBounds(display.bounds);
      w.setBrowserView(view);
      await view.webContents.loadURL('data:text/html,hello there');
      const screenCapture = new ScreenCapture(display);
      await screenCapture.expectColorAtCenterMatches(WINDOW_BACKGROUND_COLOR);
    ifit(hasCapturableScreen())('successfully applies the background color', async () => {
      const VIEW_BACKGROUND_COLOR = '#ff00ff';
      w.setBackgroundColor(VIEW_BACKGROUND_COLOR);
      await screenCapture.expectColorAtCenterMatches(VIEW_BACKGROUND_COLOR);
  describe('BrowserView.setAutoResize()', () => {
      view.setAutoResize({});
      view.setAutoResize({ width: true, height: false });
    it('throws for invalid args', () => {
        view.setAutoResize(null as any);
      }).to.throw(/Invalid auto resize options/);
    it('does not resize when the BrowserView has no AutoResize', () => {
      w.addBrowserView(view);
      view.setBounds({ x: 0, y: 0, width: 400, height: 200 });
      expect(view.getBounds()).to.deep.equal({
      w.setSize(800, 400);
    it('resizes horizontally when the window is resized horizontally', () => {
    it('resizes vertically when the window is resized vertically', () => {
      view.setAutoResize({ width: false, height: true });
      view.setBounds({ x: 0, y: 0, width: 200, height: 400 });
      w.setSize(400, 800);
        height: 800
    it('resizes both vertically and horizontally when the window is resized', () => {
      view.setAutoResize({ width: true, height: true });
      view.setBounds({ x: 0, y: 0, width: 400, height: 400 });
      w.setSize(800, 800);
    it('resizes proportionally', () => {
      view.setBounds({ x: 0, y: 0, width: 200, height: 100 });
        height: 100
    it('does not move x if horizontal: false', () => {
      view.setAutoResize({ width: true });
      view.setBounds({ x: 200, y: 0, width: 200, height: 100 });
        x: 200,
    it('moves x if horizontal: true', () => {
      view.setAutoResize({ horizontal: true });
        x: 400,
    it('moves x if horizontal: true width: true', () => {
      view.setAutoResize({ horizontal: true, width: true });
  describe('BrowserView.setBounds()', () => {
      view.setBounds({ x: 0, y: 0, width: 1, height: 1 });
        view.setBounds(null as any);
      }).to.throw(/conversion failure/);
        view.setBounds({} as any);
    it('can set bounds after view is added to window', () => {
      const bounds = { x: 0, y: 0, width: 50, height: 50 };
      view.setBounds(bounds);
      expect(view.getBounds()).to.deep.equal(bounds);
    it('can set bounds before view is added to window', () => {
    it('can update bounds', () => {
      const bounds1 = { x: 0, y: 0, width: 50, height: 50 };
      view.setBounds(bounds1);
      expect(view.getBounds()).to.deep.equal(bounds1);
      const bounds2 = { x: 0, y: 150, width: 50, height: 50 };
      view.setBounds(bounds2);
      expect(view.getBounds()).to.deep.equal(bounds2);
  describe('BrowserView.getBounds()', () => {
    it('returns the current bounds', () => {
      const bounds = { x: 10, y: 20, width: 30, height: 40 };
    it('does not changer after being added to a window', () => {
  describe('BrowserWindow.setBrowserView()', () => {
    it('does not throw if called multiple times with same view', () => {
  describe('BrowserWindow.getBrowserView()', () => {
    it('returns the set view', () => {
      const view2 = w.getBrowserView();
      expect(view2!.webContents.id).to.equal(view.webContents.id);
    it('returns null if none is set', () => {
      const view = w.getBrowserView();
      expect(view).to.be.null('view');
    it('throws if multiple BrowserViews are attached', () => {
      const view2 = new BrowserView();
      defer(() => view2.webContents.destroy());
      w.addBrowserView(view2);
      defer(() => w.removeBrowserView(view2));
        w.getBrowserView();
      }).to.throw(/has multiple BrowserViews/);
  describe('BrowserWindow.addBrowserView()', () => {
      const view1 = new BrowserView();
      defer(() => view1.webContents.destroy());
      w.addBrowserView(view1);
      defer(() => w.removeBrowserView(view1));
    it('does not crash if the webContents is destroyed after a URL is loaded', () => {
      expect(async () => {
        view.setBounds({ x: 0, y: 0, width: 400, height: 300 });
    it('can handle BrowserView reparenting', async () => {
      expect(view.ownerWindow).to.be.null('ownerWindow');
      view.webContents.loadURL('about:blank');
      await once(view.webContents, 'did-finish-load');
      expect(view.ownerWindow).to.equal(w);
      const w2 = new BrowserWindow({ show: false });
      w2.addBrowserView(view);
      expect(view.ownerWindow).to.equal(w2);
      view.webContents.loadURL(`file://${fixtures}/pages/blank.html`);
      // Clean up - the afterEach hook assumes the webContents on w is still alive.
      w2.close();
      w2.destroy();
    it('allows attaching a BrowserView with a previously-closed webContents', async () => {
      const view = new BrowserView();
      view.webContents.close();
      w2.webContents.loadURL('about:blank');
      await once(w2.webContents, 'did-finish-load');
    it('allows attaching a BrowserView with a previously-destroyed webContents', async () => {
      w.webContents.loadURL('about:blank');
      await once(w.webContents, 'did-finish-load');
    it('document visibilitychange does not change when adding the same BrowserView multiple times', async () => {
      expect(w.isVisible()).to.be.true('w is visible');
      const [width, height] = w.getSize();
      view.setBounds({ x: 0, y: 0, width, height });
      await view.webContents.loadURL(`data:text/html,
            <h1>HELLO BROWSERVIEW</h1>
              document.visibilityChangeCount = 0;
              document.addEventListener('visibilitychange', () => {
                document.visibilityChangeCount++;
      const query = 'document.visibilityChangeCount';
      const countBefore = await view.webContents.executeJavaScript(query);
      expect(countBefore).to.equal(0);
      const countAfter = await view.webContents.executeJavaScript(query);
      expect(countAfter).to.equal(countBefore);
  describe('BrowserWindow.removeBrowserView()', () => {
        w.removeBrowserView(view);
    it('can be called on a BrowserView with a destroyed webContents', async () => {
      const destroyed = once(view.webContents, 'destroyed');
      await destroyed;
  describe('BrowserWindow.getBrowserViews()', () => {
    it('returns same views as was added', () => {
      const views = w.getBrowserViews();
      expect(views).to.have.lengthOf(2);
      expect(views[0].webContents.id).to.equal(view1.webContents.id);
      expect(views[1].webContents.id).to.equal(view2.webContents.id);
    it('persists ordering by z-index', () => {
      w.setTopBrowserView(view1);
      expect(views[0].webContents.id).to.equal(view2.webContents.id);
      expect(views[1].webContents.id).to.equal(view1.webContents.id);
  describe('BrowserWindow.setTopBrowserView()', () => {
    it('should throw an error when a BrowserView is not attached to the window', () => {
        w.setTopBrowserView(view);
      }).to.throw(/is not attached/);
    it('should throw an error when a BrowserView is attached to some other window', () => {
      const win2 = new BrowserWindow();
      view.setBounds({ x: 0, y: 0, width: 100, height: 100 });
      win2.addBrowserView(view);
      win2.close();
      win2.destroy();
    it('should reorder the BrowserView to the top if it is already in the window', () => {
      expect(views.indexOf(view)).to.equal(views.length - 1);
  describe('BrowserView owning window', () => {
    it('points to owning window', () => {
      expect(view.webContents.getOwnerBrowserWindow()).to.be.null('owner browser window');
      expect(view.webContents.getOwnerBrowserWindow()).to.equal(w);
      w.setBrowserView(null);
    it('works correctly when the webContents is destroyed', async () => {
    it('works correctly when owner window is closed', async () => {
      const destroyed = once(w, 'closed');
      expect(view.ownerWindow).to.equal(null);
  describe('shutdown behavior', () => {
    it('emits the destroyed event when the host BrowserWindow is closed', async () => {
            <div id="bv_id">HELLO BROWSERVIEW</div>
      const query = 'document.getElementById("bv_id").textContent';
      const contentBefore = await view.webContents.executeJavaScript(query);
      expect(contentBefore).to.equal('HELLO BROWSERVIEW');
      const closed = once(w, 'closed');
      await Promise.all([destroyed, closed]);
    it('does not destroy its webContents if an owner BrowserWindow close event is prevented', async () => {
      w.once('close', (e) => {
      const contentAfter = await view.webContents.executeJavaScript(query);
      expect(contentAfter).to.equal('HELLO BROWSERVIEW');
    it('does not crash on exit', async () => {
      const rc = await startRemoteControlApp();
      await rc.remotely(() => {
        const { BrowserView, app } = require('electron');
        // eslint-disable-next-line no-new
        new BrowserView({});
      const [code] = await once(rc.process, 'exit');
    it('does not crash on exit if added to a browser window', async () => {
        const { app, BrowserView, BrowserWindow } = require('electron');
        const bv = new BrowserView();
        bv.webContents.loadURL('about:blank');
        const bw = new BrowserWindow({ show: false });
        bw.addBrowserView(bv);
    it('emits the destroyed event when webContents.close() is called', async () => {
      await view.webContents.loadFile(path.join(fixtures, 'pages', 'a.html'));
      await once(view.webContents, 'destroyed');
    it('emits the destroyed event when window.close() is called', async () => {
      view.webContents.executeJavaScript('window.close()');
  describe('window.open()', () => {
    it('works in BrowserView', (done) => {
      view.webContents.setWindowOpenHandler(({ url, frameName }) => {
        expect(url).to.equal('http://host/');
        expect(frameName).to.equal('host');
      view.webContents.loadFile(path.join(fixtures, 'pages', 'window-open.html'));
  describe('BrowserView.capturePage(rect)', () => {
    it('returns a Promise with a Buffer', async () => {
      view = new BrowserView({
          backgroundThrottling: false
      view.setBounds({
        ...w.getBounds(),
        y: 0
      const image = await view.webContents.capturePage({
      expect(image.isEmpty()).to.equal(true);
    xit('resolves after the window is hidden and capturer count is non-zero', async () => {
      const image = await view.webContents.capturePage();
      expect(image.isEmpty()).to.equal(false);
