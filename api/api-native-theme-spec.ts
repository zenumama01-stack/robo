import { nativeTheme, BrowserWindow, ipcMain } from 'electron/main';
describe('nativeTheme module', () => {
  describe('nativeTheme.shouldUseDarkColors', () => {
    it('returns a boolean', () => {
      expect(nativeTheme.shouldUseDarkColors).to.be.a('boolean');
  describe('nativeTheme.themeSource', () => {
      nativeTheme.themeSource = 'system';
      // Wait for any pending events to emit
      await setTimeout(20);
    it('is system by default', () => {
      expect(nativeTheme.themeSource).to.equal('system');
    it('should override the value of shouldUseDarkColors', () => {
      nativeTheme.themeSource = 'dark';
      expect(nativeTheme.shouldUseDarkColors).to.equal(true);
      nativeTheme.themeSource = 'light';
      expect(nativeTheme.shouldUseDarkColors).to.equal(false);
    it('should emit the "updated" event when it is set and the resulting "shouldUseDarkColors" value changes', async () => {
      let updatedEmitted = once(nativeTheme, 'updated');
      await updatedEmitted;
      updatedEmitted = once(nativeTheme, 'updated');
    it('should not emit the "updated" event when it is set and the resulting "shouldUseDarkColors" value is the same', async () => {
      // Wait a few ticks to allow an async events to flush
      nativeTheme.once('updated', () => {
      expect(called).to.equal(false);
    const getPrefersColorSchemeIsDark = async (w: Electron.BrowserWindow) => {
      const isDark: boolean = await w.webContents.executeJavaScript(
        'matchMedia("(prefers-color-scheme: dark)").matches'
      return isDark;
    it('should override the result of prefers-color-scheme CSS media query', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { contextIsolation: false, nodeIntegration: true } });
      await w.loadFile(path.resolve(__dirname, 'fixtures', 'blank.html'));
      await w.webContents.executeJavaScript(`
        window.matchMedia('(prefers-color-scheme: dark)')
          .addEventListener('change', () => require('electron').ipcRenderer.send('theme-change'))
      const originalSystemIsDark = await getPrefersColorSchemeIsDark(w);
      let changePromise = once(ipcMain, 'theme-change');
      if (!originalSystemIsDark) await changePromise;
      expect(await getPrefersColorSchemeIsDark(w)).to.equal(true);
      changePromise = once(ipcMain, 'theme-change');
      await changePromise;
      expect(await getPrefersColorSchemeIsDark(w)).to.equal(false);
      if (originalSystemIsDark) await changePromise;
      expect(await getPrefersColorSchemeIsDark(w)).to.equal(originalSystemIsDark);
  describe('nativeTheme.shouldUseInvertedColorScheme', () => {
      expect(nativeTheme.shouldUseInvertedColorScheme).to.be.a('boolean');
  describe('nativeTheme.shouldUseHighContrastColors', () => {
      expect(nativeTheme.shouldUseHighContrastColors).to.be.a('boolean');
  describe('nativeTheme.shouldUseDarkColorsForSystemIntegratedUI', () => {
      expect(nativeTheme.shouldUseDarkColorsForSystemIntegratedUI).to.be.a('boolean');
  describe('nativeTheme.inForcedColorsMode', () => {
      expect(nativeTheme.inForcedColorsMode).to.be.a('boolean');
  describe('nativeTheme.prefersReducesTransparency', () => {
      expect(nativeTheme.prefersReducedTransparency).to.be.a('boolean');
  ifdescribe(process.platform === 'darwin')('nativeTheme.shouldDifferentiateWithoutColor', () => {
      expect(nativeTheme.shouldDifferentiateWithoutColor).to.be.a('boolean');
