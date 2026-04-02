import { clipboard, nativeImage } from 'electron/common';
import { Buffer } from 'node:buffer';
// FIXME(zcbenz): Clipboard tests are failing on WOA.
ifdescribe(process.platform !== 'win32' || process.arch !== 'arm64')('clipboard module', () => {
  describe('clipboard.readImage()', () => {
    it('returns NativeImage instance', () => {
      const p = path.join(fixtures, 'assets', 'logo.png');
      const i = nativeImage.createFromPath(p);
      clipboard.writeImage(i);
      const readImage = clipboard.readImage();
      expect(readImage.toDataURL()).to.equal(i.toDataURL());
    it('works for empty image', () => {
      clipboard.writeText('Not an Image');
      expect(clipboard.readImage().isEmpty()).to.be.true();
  describe('clipboard.readText()', () => {
    it('returns unicode string correctly', () => {
      const text = '千江有水千江月，万里无云万里天';
      clipboard.writeText(text);
      expect(clipboard.readText()).to.equal(text);
  describe('clipboard.readHTML()', () => {
    it('returns markup correctly', () => {
      let text = '<string>Hi</string>';
      // CL: https://chromium-review.googlesource.com/c/chromium/src/+/5187335
        text = '<meta charset=\'utf-8\'><string>Hi</string>';
      clipboard.writeHTML('<string>Hi</string>');
      expect(clipboard.readHTML()).to.equal(text);
  describe('clipboard.readRTF', () => {
    it('returns rtf text correctly', () => {
      const rtf = '{\\rtf1\\ansi{\\fonttbl\\f0\\fswiss Helvetica;}\\f0\\pard\nThis is some {\\b bold} text.\\par\n}';
      clipboard.writeRTF(rtf);
      expect(clipboard.readRTF()).to.equal(rtf);
  ifdescribe(process.platform !== 'linux')('clipboard.readBookmark', () => {
    it('returns title and url', () => {
      clipboard.writeBookmark('a title', 'https://electronjs.org/');
      const readBookmark = clipboard.readBookmark();
        expect(readBookmark.title).to.equal('a title');
      expect(clipboard.readBookmark().url).to.equal('https://electronjs.org/');
      clipboard.writeText('no bookmark');
      expect(clipboard.readBookmark()).to.deep.equal({
        url: ''
  describe('clipboard.read()', () => {
    ifit(process.platform !== 'linux')('does not crash when reading various custom clipboard types', () => {
      const type = process.platform === 'darwin' ? 'NSFilenamesPboardType' : 'FileNameW';
        clipboard.read(type);
    it('can read data written with writeBuffer', () => {
      const testText = 'Testing read';
      const buffer = Buffer.from(testText, 'utf8');
      clipboard.writeBuffer('public/utf8-plain-text', buffer);
      expect(clipboard.read('public/utf8-plain-text')).to.equal(testText);
  describe('clipboard.write()', () => {
    it('returns data correctly', () => {
      const text = 'https://electronjs.org/';
      const rtf = '{\\rtf1\\utf8 text}';
      let markup = '<b>Hi</b>';
        markup = '<meta charset=\'utf-8\'><b>Hi</b>';
      const bookmark = { title: 'a title', url: text };
      clipboard.write({
        html: '<b>Hi</b>',
        rtf: '{\\rtf1\\utf8 text}',
        bookmark: 'a title',
        image: i
      expect(clipboard.readHTML()).to.equal(markup);
          expect(clipboard.readBookmark()).to.deep.equal(bookmark);
          expect(clipboard.readBookmark().url).to.equal(bookmark.url);
  ifdescribe(process.platform === 'darwin')('clipboard.read/writeFindText(text)', () => {
    it('reads and write text to the find pasteboard', () => {
      clipboard.writeFindText('find this');
      expect(clipboard.readFindText()).to.equal('find this');
  describe('clipboard.readBuffer(format)', () => {
    it('writes a Buffer for the specified format', function () {
      const buffer = Buffer.from('writeBuffer', 'utf8');
      expect(buffer.equals(clipboard.readBuffer('public/utf8-plain-text'))).to.equal(true);
    it('throws an error when a non-Buffer is specified', () => {
        clipboard.writeBuffer('public/utf8-plain-text', 'hello' as any);
      }).to.throw(/buffer must be a node Buffer/);
    ifit(process.platform !== 'win32')('writes a Buffer using a raw format that is used by native apps', function () {
      const message = 'Hello from Electron!';
      const buffer = Buffer.from(message);
      let rawFormat = 'text/plain';
        rawFormat = 'public.utf8-plain-text';
      clipboard.writeBuffer(rawFormat, buffer);
      expect(clipboard.readText()).to.equal(message);
