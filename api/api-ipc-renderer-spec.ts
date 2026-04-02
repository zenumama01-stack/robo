describe('ipcRenderer module', () => {
    w.webContents.on('console-message', (event, ...args) => console.error(...args));
  describe('send()', () => {
    it('should work when sending an object containing id property', async () => {
      const obj = {
        name: 'ly'
      w.webContents.executeJavaScript(`{
        ipcRenderer.send('message', ${JSON.stringify(obj)})
      const [, received] = await once(ipcMain, 'message');
      expect(received).to.deep.equal(obj);
    it('can send instances of Date as Dates', async () => {
      const isoDate = new Date().toISOString();
        ipcRenderer.send('message', new Date(${JSON.stringify(isoDate)}))
      expect(received.toISOString()).to.equal(isoDate);
    it('can send instances of Buffer', async () => {
      const data = 'hello';
        ipcRenderer.send('message', Buffer.from(${JSON.stringify(data)}))
      expect(received).to.be.an.instanceOf(Uint8Array);
      expect(Buffer.from(data).equals(received)).to.be.true();
    it('throws when sending objects with DOM class prototypes', async () => {
      await expect(w.webContents.executeJavaScript(`{
        ipcRenderer.send('message', document.location)
      }`)).to.eventually.be.rejected();
    it('does not crash when sending external objects', async () => {
        const http = require('node:http')
        const request = http.request({ port: 5000, hostname: '127.0.0.1', method: 'GET', path: '/' })
        const stream = request.agent.sockets['127.0.0.1:5000:'][0]._handle._externalStream
        ipcRenderer.send('message', stream)
    it('can send objects that both reference the same object', async () => {
        const child = { hello: 'world' }
        const foo = { name: 'foo', child: child }
        const bar = { name: 'bar', child: child }
        const array = [foo, bar]
        ipcRenderer.send('message', array, foo, bar, child)
      const child = { hello: 'world' };
      const foo = { name: 'foo', child };
      const bar = { name: 'bar', child };
      const array = [foo, bar];
      const [, arrayValue, fooValue, barValue, childValue] = await once(ipcMain, 'message');
      expect(arrayValue).to.deep.equal(array);
      expect(fooValue).to.deep.equal(foo);
      expect(barValue).to.deep.equal(bar);
      expect(childValue).to.deep.equal(child);
    it('can handle cyclic references', async () => {
        const array = [5]
        array.push(array)
        child.child = child
        ipcRenderer.send('message', array, child)
      const [, arrayValue, childValue] = await once(ipcMain, 'message');
      expect(arrayValue[0]).to.equal(5);
      expect(arrayValue[1]).to.equal(arrayValue);
      expect(childValue.hello).to.equal('world');
      expect(childValue.child).to.equal(childValue);
  describe('sendSync()', () => {
    it('can be replied to by setting event.returnValue', async () => {
      ipcMain.once('echo', (event, msg) => {
        event.returnValue = msg;
      const msg = await w.webContents.executeJavaScript(`new Promise(resolve => {
        resolve(ipcRenderer.sendSync('echo', 'test'))
      expect(msg).to.equal('test');
  describe('ipcRenderer.on', () => {
      const result = await w.webContents.executeJavaScript(`
        require('electron').ipcRenderer.eventNames()
      expect(result).to.deep.equal([]);
  describe('ipcRenderer.removeAllListeners', () => {
    it('removes only the given channel', async () => {
          const { ipcRenderer } = require('electron');
          ipcRenderer.on('channel1', () => {});
          ipcRenderer.on('channel2', () => {});
          ipcRenderer.removeAllListeners('channel1');
          return ipcRenderer.eventNames();
      expect(result).to.deep.equal(['channel2']);
    it('removes all channels if no channel is specified', async () => {
          ipcRenderer.removeAllListeners();
  describe('after context is released', () => {
    it('throws an exception', async () => {
      const error = await w.webContents.executeJavaScript(`(${() => {
        const child = window.open('', 'child', 'show=no,nodeIntegration=yes')! as any;
        const childIpc = child.require('electron').ipcRenderer;
        child.close();
          setInterval(() => {
              childIpc.send('hello');
              resolve(e);
          }, 16);
      }})()`);
      expect(error).to.have.property('message', 'IPC method called after context was released');
