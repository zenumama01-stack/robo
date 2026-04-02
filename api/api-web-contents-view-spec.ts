import { BaseWindow, BrowserWindow, View, WebContentsView, webContents, screen } from 'electron/main';
import { HexColors, ScreenCapture, hasCapturableScreen, nextFrameTime } from './lib/screen-helpers';
import { defer, ifdescribe, waitUntil } from './lib/spec-helpers';
describe('WebContentsView', () => {
    const existingWCS = webContents.getAllWebContents();
    existingWCS.forEach((contents) => contents.close());
    new WebContentsView();
  it('can be instantiated with no webPreferences', () => {
    new WebContentsView({});
  it('accepts existing webContents object', async () => {
    const currentWebContentsCount = webContents.getAllWebContents().length;
    const wc = (webContents as typeof ElectronInternal.WebContents).create({ sandbox: true });
    defer(() => wc.destroy());
    const webContentsView = new WebContentsView({
      webContents: wc
    expect(webContentsView.webContents).to.eq(wc);
    expect(webContents.getAllWebContents().length).to.equal(currentWebContentsCount + 1, 'expected only single webcontents to be created');
  it('should throw error when created with already attached webContents to BrowserWindow', () => {
    const browserWindow = new BrowserWindow();
    defer(() => browserWindow.webContents.destroy());
    const webContentsView = new WebContentsView();
    defer(() => webContentsView.webContents.destroy());
    browserWindow.contentView.addChildView(webContentsView);
    defer(() => browserWindow.contentView.removeChildView(webContentsView));
    expect(() => new WebContentsView({
      webContents: webContentsView.webContents
    })).to.throw('options.webContents is already attached to a window');
  it('should throw an error when adding a destroyed child view to the parent view', async () => {
    const wc = webContentsView.webContents;
    wc.loadURL('about:blank');
    wc.destroy();
    const destroyed = once(wc, 'destroyed');
    expect(() => browserWindow.contentView.addChildView(webContentsView)).to.throw(
      'Can\'t add a destroyed child view to a parent view'
  it('should throw error when created with already attached webContents to other WebContentsView', () => {
    webContentsView.webContents.loadURL('about:blank');
      webContents: browserWindow.webContents
    w.setContentView(new WebContentsView());
  it('can be removed after a close', async () => {
    const wcv = new WebContentsView();
    const wc = wcv.webContents;
    v.addChildView(wcv);
    wc.executeJavaScript('window.close()');
    expect(wc.isDestroyed()).to.be.true();
    v.removeChildView(wcv);
    const wcv1 = new WebContentsView();
    const wcv2 = new WebContentsView();
    const wcv3 = new WebContentsView();
    w.contentView.addChildView(wcv1);
    w.contentView.addChildView(wcv2);
    w.contentView.addChildView(wcv3);
    expect(w.contentView.children).to.deep.equal([wcv1, wcv2, wcv3]);
    expect(w.contentView.children).to.deep.equal([wcv3, wcv1, wcv2]);
  it('handle removal and re-addition of children', () => {
    expect(w.contentView.children).to.deep.equal([]);
    expect(w.contentView.children).to.deep.equal([wcv1, wcv2]);
    w.contentView.removeChildView(wcv1);
    expect(w.contentView.children).to.deep.equal([wcv2]);
    expect(w.contentView.children).to.deep.equal([wcv2, wcv1]);
  function triggerGCByAllocation () {
    const arr = [];
    for (let i = 0; i < 1000000; i++) {
      arr.push([]);
  it('doesn\'t crash when GCed during allocation', (done) => {
      // NB. the crash we're testing for is the lack of a current `v8::Context`
      // when emitting an event in WebContents's destructor. V8 is inconsistent
      // about whether or not there's a current context during garbage
      // collection, and it seems that `v8Util.requestGarbageCollectionForTesting`
      // causes a GC in which there _is_ a current context, so the crash isn't
      // triggered. Thus, we force a GC by other means: namely, by allocating a
      // bunch of stuff.
      triggerGCByAllocation();
  it('does not crash when closed via window.close()', async () => {
    const bw = new BrowserWindow();
    await bw.loadURL('data:text/html,<h1>Main Window</h1>');
    bw.contentView.addChildView(wcv);
    const dto = new Promise<boolean>((resolve) => {
      wc.on('blur', () => {
        const devToolsOpen = !wc.isDestroyed() && wc.isDevToolsOpened();
        resolve(devToolsOpen);
    wc.loadURL('data:text/html,<script>window.close()</script>');
    const open = await dto;
    expect(open).to.be.false();
  it('can be fullscreened', async () => {
    const v = new WebContentsView();
    await v.webContents.loadURL('data:text/html,<div id="div">This is a simple div.</div>');
    await v.webContents.executeJavaScript('document.getElementById("div").requestFullscreen()', true);
    await wcv.webContents.loadURL('data:text/html,<div id="div">This is a simple div.</div>');
    expect(w.contentView.children).to.deep.equal([v]);
    expect(v.children).to.deep.equal([wcv]);
  describe('visibilityState', () => {
    async function haveVisibilityState (view: WebContentsView, state: string) {
      const docVisState = await view.webContents.executeJavaScript('document.visibilityState');
      return docVisState === state;
    it('is initially hidden', async () => {
      await v.webContents.loadURL('data:text/html,<script>initialVisibility = document.visibilityState</script>');
      expect(await v.webContents.executeJavaScript('initialVisibility')).to.equal('hidden');
    it('becomes visible when attached', async () => {
      await v.webContents.loadURL('about:blank');
      expect(await v.webContents.executeJavaScript('document.visibilityState')).to.equal('hidden');
      const p = v.webContents.executeJavaScript('new Promise(resolve => document.addEventListener("visibilitychange", resolve))');
      // Ensure that the above listener has been registered before we add the
      // view to the window, or else the visibilitychange event might be
      // dispatched before the listener is registered.
      // executeJavaScript calls are sequential so if this one's finished then
      // the previous one must also have been finished :)
      await v.webContents.executeJavaScript('undefined');
      expect(await v.webContents.executeJavaScript('document.visibilityState')).to.equal('visible');
    it('is initially visible if load happens after attach', async () => {
      w.contentView = v;
      expect(await v.webContents.executeJavaScript('initialVisibility')).to.equal('visible');
    it('becomes hidden when parent window is hidden', async () => {
      await expect(waitUntil(async () => await haveVisibilityState(v, 'visible'))).to.eventually.be.fulfilled();
      // We have to wait until the listener above is fully registered before hiding the window.
      // On Windows, the executeJavaScript and the visibilitychange can happen out of order
      // without this.
      await v.webContents.executeJavaScript('0');
    it('becomes visible when parent window is shown', async () => {
    it('does not change when view is moved between two visible windows', async () => {
      const p = v.webContents.executeJavaScript('new Promise(resolve => document.addEventListener("visibilitychange", () => resolve(document.visibilityState)))');
      // Ensure the listener has been registered.
      const w2 = new BaseWindow();
      w2.setContentView(v);
      // Wait for the visibility state to settle as "visible".
      // On macOS one visibilitychange event is fired but visibilityState
      // remains "visible". On Win/Linux, two visibilitychange events are
      // fired, a "hidden" and a "visible" one. Reconcile these two models
      // by waiting until at least one event has been fired, and then waiting
      // until the visibility state settles as "visible".
      let visibilityState = await p;
      for (let attempts = 0; visibilityState !== 'visible' && attempts < 10; attempts++) {
        visibilityState = await v.webContents.executeJavaScript('new Promise(resolve => document.visibilityState === "visible" ? resolve("visible") : document.addEventListener("visibilitychange", () => resolve(document.visibilityState)))');
  describe('setBorderRadius', () => {
    ifdescribe(hasCapturableScreen())('capture', () => {
      let w: Electron.BaseWindow;
      let v: Electron.WebContentsView;
      let display: Electron.Display;
      let corners: Electron.Point[];
      const backgroundUrl = `data:text/html,<style>html{background:${encodeURIComponent(HexColors.GREEN)}}</style>`;
        w = new BaseWindow({
          ...display.workArea,
          backgroundColor: HexColors.BLUE,
        v = new WebContentsView();
        v.setBorderRadius(100);
        const readyForCapture = once(v.webContents, 'ready-to-show');
        v.webContents.loadURL(backgroundUrl);
        const inset = 10;
        // Adjust for macOS menu bar height which seems to be about 24px
        // based on the results from accessibility inspector.
        const platformInset = process.platform === 'darwin' ? 15 : 0;
        corners = [
          { x: display.workArea.x + inset, y: display.workArea.y + inset + platformInset }, // top-left
          { x: display.workArea.x + display.workArea.width - inset, y: display.workArea.y + inset + platformInset }, // top-right
          { x: display.workArea.x + display.workArea.width - inset, y: display.workArea.y + display.workArea.height - inset }, // bottom-right
          { x: display.workArea.x + inset, y: display.workArea.y + display.workArea.height - inset } // bottom-left
        await readyForCapture;
        w = v = null!;
      it('should render with cutout corners', async () => {
        for (const corner of corners) {
          await screenCapture.expectColorAtPointOnDisplayMatches(HexColors.BLUE, () => corner);
        // Center should be WebContents page background color
        await screenCapture.expectColorAtCenterMatches(HexColors.GREEN);
      it('should allow resetting corners', async () => {
        const corner = corners[0];
        await nextFrameTime();
        await screenCapture.expectColorAtPointOnDisplayMatches(HexColors.GREEN, () => corner);
      it('should render when set before attached', async () => {
        v.setBorderRadius(100); // must set before
    it('should allow setting when not attached', async () => {
      const devToolsFocused = once(v.webContents, 'devtools-focused');
      v.webContents.openDevTools({ mode: 'right' });
      await devToolsFocused;
      expect(v.webContents.isFocused()).to.be.false();
      await v.webContents.loadURL('data:text/html,<body>test</body>');
      expect(v.webContents.isFocused()).to.be.true();
      const v = new WebContentsView({
