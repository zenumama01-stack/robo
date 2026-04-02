import { Menu, Tray } from 'electron/main';
describe('tray module', () => {
  let tray: Tray;
  beforeEach(() => { tray = new Tray(nativeImage.createEmpty()); });
    tray.destroy();
    tray = null as any;
  describe('new Tray', () => {
      expect(Tray.prototype.constructor.name).to.equal('Tray');
    it('throws a descriptive error for a missing file', () => {
        tray = new Tray(badPath);
    ifit(process.platform !== 'linux')('throws a descriptive error if an invalid guid is given', () => {
        tray = new Tray(nativeImage.createEmpty(), 'I am not a guid');
      }).to.throw('Invalid GUID format');
    ifit(process.platform !== 'linux')('accepts a valid guid', () => {
        tray = new Tray(nativeImage.createEmpty(), '0019A433-3526-48BA-A66C-676742C0FEFB');
    it('is an instance of Tray', () => {
      expect(tray).to.be.an.instanceOf(Tray);
  ifdescribe(process.platform === 'darwin')('tray get/set ignoreDoubleClickEvents', () => {
    it('returns false by default', () => {
      const ignored = tray.getIgnoreDoubleClickEvents();
      expect(ignored).to.be.false('ignored');
    it('can be set to true', () => {
      tray.setIgnoreDoubleClickEvents(true);
      expect(ignored).to.be.true('not ignored');
  describe('tray.setContextMenu(menu)', () => {
    it('accepts both null and Menu as parameters', () => {
      expect(() => { tray.setContextMenu(new Menu()); }).to.not.throw();
      expect(() => { tray.setContextMenu(null); }).to.not.throw();
  describe('tray.destroy()', () => {
    it('destroys a tray', () => {
      expect(tray.isDestroyed()).to.be.false('tray should not be destroyed');
      expect(tray.isDestroyed()).to.be.true('tray should be destroyed');
  describe('tray.popUpContextMenu()', () => {
    ifit(process.platform === 'win32')('can be called when menu is showing', async function () {
      tray.setContextMenu(Menu.buildFromTemplate([{ label: 'Test' }]));
      const timeout = setTimeout();
      tray.popUpContextMenu();
      await timeout;
    it('can be called with a menu', () => {
      const menu = Menu.buildFromTemplate([{ label: 'Test' }]);
        tray.popUpContextMenu(menu);
    it('can be called with a position', () => {
        tray.popUpContextMenu({ x: 0, y: 0 } as any);
    it('can be called with a menu and a position', () => {
        tray.popUpContextMenu(menu, { x: 0, y: 0 });
    it('throws an error on invalid arguments', () => {
        tray.popUpContextMenu({} as any);
      }).to.throw(/index 0/);
        tray.popUpContextMenu(menu, {} as any);
      }).to.throw(/index 1/);
  describe('tray.closeContextMenu()', () => {
    ifit(process.platform === 'win32')('does not crash when called more than once', async function () {
      tray.closeContextMenu();
  describe('tray.getBounds()', () => {
    afterEach(() => { tray.destroy(); });
    ifit(process.platform !== 'linux')('returns a bounds object', function () {
      const bounds = tray.getBounds();
      expect(bounds).to.be.an('object').and.to.have.all.keys('x', 'y', 'width', 'height');
  describe('tray.setImage(image)', () => {
        tray.setImage(badPath);
    it('accepts empty image', () => {
      tray.setImage(nativeImage.createEmpty());
  describe('tray.setPressedImage(image)', () => {
        tray.setPressedImage(badPath);
      tray.setPressedImage(nativeImage.createEmpty());
  ifdescribe(process.platform === 'win32')('tray.displayBalloon(image)', () => {
        tray.displayBalloon({
          content: 'wow content',
          icon: badPath
    it('accepts an empty image', () => {
        icon: nativeImage.createEmpty()
  ifdescribe(process.platform === 'darwin')('tray get/set title', () => {
    it('sets/gets non-empty title', () => {
      const title = 'Hello World!';
      tray.setTitle(title);
      const newTitle = tray.getTitle();
      expect(newTitle).to.equal(title);
    it('sets/gets empty title', () => {
      const title = '';
    it('can have an options object passed in', () => {
        tray.setTitle('Hello World!', {});
    it('throws when the options parameter is not an object', () => {
        tray.setTitle('Hello World!', 'test' as any);
      }).to.throw(/setTitle options must be an object/);
    it('can have a font type option set', () => {
        tray.setTitle('Hello World!', { fontType: 'monospaced' });
        tray.setTitle('Hello World!', { fontType: 'monospacedDigit' });
    it('throws when the font type is specified but is not a string', () => {
        tray.setTitle('Hello World!', { fontType: 5.4 as any });
      }).to.throw(/fontType must be one of 'monospaced' or 'monospacedDigit'/);
    it('throws on invalid font types', () => {
        tray.setTitle('Hello World!', { fontType: 'blep' as any });
