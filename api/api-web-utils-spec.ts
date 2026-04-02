// import { once } from 'node:events';
describe('webUtils module', () => {
  describe('getPathForFile', () => {
    it('returns nothing for a Blob', async () => {
      await w.loadFile(path.resolve(fixtures, 'pages', 'file-input.html'));
      const pathFromWebUtils = await w.webContents.executeJavaScript('require("electron").webUtils.getPathForFile(new Blob([1, 2, 3]))');
      expect(pathFromWebUtils).to.equal('');
    it('reports the correct path for a File object', async () => {
      const { debugger: debug } = w.webContents;
          files: [__filename],
        const pathFromWebUtils = await w.webContents.executeJavaScript('require("electron").webUtils.getPathForFile(document.querySelector("input").files[0])');
        expect(pathFromWebUtils).to.equal(__filename);
