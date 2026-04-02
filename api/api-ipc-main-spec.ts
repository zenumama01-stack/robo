import { ipcMain, BrowserWindow } from 'electron/main';
import { defer } from './lib/spec-helpers';
describe('ipc main module', () => {
  const fixtures = path.join(__dirname, 'fixtures');
  describe('ipc.sendSync', () => {
    afterEach(() => { ipcMain.removeAllListeners('send-sync-message'); });
    it('does not crash when reply is not sent and browser is destroyed', (done) => {
      ipcMain.once('send-sync-message', (event) => {
      w.loadFile(path.join(fixtures, 'api', 'send-sync-message.html'));
    it('does not crash when reply is sent by multiple listeners', (done) => {
      ipcMain.on('send-sync-message', (event) => {
  describe('ipcMain.on', () => {
    it('is not used for internals', async () => {
      const appPath = path.join(fixtures, 'api', 'ipc-main-listeners');
      const appProcess = cp.spawn(electronPath, [appPath]);
      output = JSON.parse(output);
      expect(output).to.deep.equal(['error']);
    it('can be replied to', async () => {
      ipcMain.on('test-echo', (e, arg) => {
        e.reply('test-echo', arg);
        ipcMain.removeAllListeners('test-echo');
      const v = await w.webContents.executeJavaScript(`new Promise((resolve, reject) => {
        const { ipcRenderer } = require('electron')
        ipcRenderer.send('test-echo', 'hello')
        ipcRenderer.on('test-echo', (e, v) => {
          resolve(v)
      })`);
      expect(v).to.equal('hello');
  describe('ipcMain.removeAllListeners', () => {
    beforeEach(() => { ipcMain.removeAllListeners(); });
    it('removes only the given channel', () => {
      ipcMain.on('channel1', () => {});
      ipcMain.on('channel2', () => {});
      ipcMain.removeAllListeners('channel1');
      expect(ipcMain.eventNames()).to.deep.equal(['channel2']);
    it('removes all channels if no channel is specified', () => {
      ipcMain.removeAllListeners();
      expect(ipcMain.eventNames()).to.deep.equal([]);
