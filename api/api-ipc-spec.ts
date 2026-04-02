import { BrowserWindow, ipcMain, IpcMainInvokeEvent, MessageChannelMain, WebContents } from 'electron/main';
import { EventEmitter, once } from 'node:events';
import { defer, listen } from './lib/spec-helpers';
describe('ipc module', () => {
  describe('invoke', () => {
    async function rendererInvoke (...args: any[]) {
        const result = await ipcRenderer.invoke('test', ...args);
        ipcRenderer.send('result', { result });
        ipcRenderer.send('result', { error: (e as Error).message });
    it('receives a response from a synchronous handler', async () => {
      ipcMain.handleOnce('test', (e: IpcMainInvokeEvent, arg: number) => {
        expect(arg).to.equal(123);
      const done = new Promise<void>(resolve => ipcMain.once('result', (e, arg) => {
        expect(arg).to.deep.equal({ result: 3 });
      await w.webContents.executeJavaScript(`(${rendererInvoke})(123)`);
    it('receives a response from an asynchronous handler', async () => {
      ipcMain.handleOnce('test', async (e: IpcMainInvokeEvent, arg: number) => {
    it('receives an error from a synchronous handler', async () => {
      ipcMain.handleOnce('test', () => {
        throw new Error('some error');
        expect(arg.error).to.match(/some error/);
      await w.webContents.executeJavaScript(`(${rendererInvoke})()`);
    it('receives an error from an asynchronous handler', async () => {
      ipcMain.handleOnce('test', async () => {
    it('throws an error if no handler is registered', async () => {
        expect(arg.error).to.match(/No handler registered/);
    it('throws an error when invoking a handler that was removed', async () => {
      ipcMain.handle('test', () => { });
      ipcMain.removeHandler('test');
    it('forbids multiple handlers', async () => {
        expect(() => { ipcMain.handle('test', () => { }); }).to.throw(/second handler/);
    it('throws an error in the renderer if the reply callback is dropped', async () => {
      ipcMain.handleOnce('test', () => new Promise(() => {
        setTimeout(() => v8Util.requestGarbageCollectionForTesting());
        /* never resolve */
      w.webContents.executeJavaScript(`(${rendererInvoke})()`);
      const [, { error }] = await once(ipcMain, 'result');
      expect(error).to.match(/reply was never sent/);
    it('between send and sendSync is consistent', async () => {
      const received: number[] = [];
      ipcMain.on('test-async', (e, i) => { received.push(i); });
      ipcMain.on('test-sync', (e, i) => { received.push(i); e.returnValue = null; });
      const done = new Promise<void>(resolve => ipcMain.once('done', () => { resolve(); }));
      function rendererStressTest () {
          switch ((Math.random() * 2) | 0) {
              ipcRenderer.send('test-async', i);
              ipcRenderer.sendSync('test-sync', i);
        ipcRenderer.send('done');
        w.webContents.executeJavaScript(`(${rendererStressTest})()`);
        ipcMain.removeAllListeners('test-async');
        ipcMain.removeAllListeners('test-sync');
      expect(received).to.have.lengthOf(1000);
      expect(received).to.deep.equal([...received].sort((a, b) => a - b));
    it('between send, sendSync, and invoke is consistent', async () => {
      ipcMain.handle('test-invoke', (e, i) => { received.push(i); });
          switch ((Math.random() * 3) | 0) {
              ipcRenderer.invoke('test-invoke', i);
        ipcMain.removeHandler('test-invoke');
  describe('MessagePort', () => {
    it('can send a port to the main process', async () => {
      const p = once(ipcMain, 'port');
      await w.webContents.executeJavaScript(`(${function () {
        const channel = new MessageChannel();
        require('electron').ipcRenderer.postMessage('port', 'hi', [channel.port1]);
      const [ev, msg] = await p;
      expect(msg).to.equal('hi');
      expect(ev.ports).to.have.length(1);
      expect(ev.senderFrame.parent).to.be.null();
      expect(ev.senderFrame.frameToken).to.equal(w.webContents.mainFrame.frameToken);
      const [port] = ev.ports;
      expect(port).to.be.an.instanceOf(EventEmitter);
    it('can sent a message without a transfer', async () => {
        require('electron').ipcRenderer.postMessage('port', 'hi');
      expect(ev.ports).to.deep.equal([]);
    it('throws when the transferable is invalid', async () => {
          const buffer = new ArrayBuffer(10);
          require('electron').ipcRenderer.postMessage('port', '', [buffer]);
          require('electron').ipcRenderer.postMessage('port', { error: (e as Error).message });
      const [, msg] = await p;
      expect(msg.error).to.eql('Invalid value for transfer');
    it('can communicate between main and renderer', async () => {
        channel.port2.onmessage = (ev: any) => {
          channel.port2.postMessage(ev.data * 2);
        require('electron').ipcRenderer.postMessage('port', '', [channel.port1]);
      const [ev] = await p;
      port.start();
      port.postMessage(42);
      const [ev2] = await once(port, 'message');
      expect(ev2.data).to.equal(84);
    it('can receive a port from a renderer over a MessagePort connection', async () => {
      function fn () {
        const channel1 = new MessageChannel();
        const channel2 = new MessageChannel();
        channel1.port2.postMessage('', [channel2.port1]);
        channel2.port2.postMessage('matryoshka');
        require('electron').ipcRenderer.postMessage('port', '', [channel1.port1]);
      w.webContents.executeJavaScript(`(${fn})()`);
      const [{ ports: [port1] }] = await once(ipcMain, 'port');
      port1.start();
      const [{ ports: [port2] }] = await once(port1, 'message');
      port2.start();
      const [{ data }] = await once(port2, 'message');
      expect(data).to.equal('matryoshka');
    it('can forward a port from one renderer to another renderer', async () => {
      const w1 = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true, contextIsolation: false } });
      const w2 = new BrowserWindow({ show: false, webPreferences: { nodeIntegration: true, contextIsolation: false } });
      w1.webContents.executeJavaScript(`(${function () {
          require('electron').ipcRenderer.send('message received', ev.data);
      const [{ ports: [port] }] = await once(ipcMain, 'port');
      await w2.webContents.executeJavaScript(`(${function () {
        require('electron').ipcRenderer.on('port', ({ ports: [port] }: any) => {
          port.postMessage('a message');
      w2.webContents.postMessage('port', '', [port]);
      const [, data] = await once(ipcMain, 'message received');
      expect(data).to.equal('a message');
    describe('close event', () => {
      describe('in renderer', () => {
        it('is emitted when the main process closes its end of the port', async () => {
            ipcRenderer.on('port', (e: any) => {
              const [port] = e.ports;
              port.onclose = () => {
                ipcRenderer.send('closed');
          const { port1, port2 } = new MessageChannelMain();
          w.webContents.postMessage('port', null, [port2]);
          port1.close();
          await once(ipcMain, 'closed');
        it('is emitted when the other end of a port is garbage-collected', async () => {
          await w.webContents.executeJavaScript(`(${async function () {
            const { port2 } = new MessageChannel();
              port2.onclose = resolve;
              // @ts-ignore --expose-gc is enabled.
              gc({ type: 'major', execution: 'async' });
        it('is emitted when the other end of a port is sent to nowhere', async () => {
          ipcMain.once('do-a-gc', () => v8Util.requestGarbageCollectionForTesting());
            const { port1, port2 } = new MessageChannel();
              require('electron').ipcRenderer.postMessage('nobody-listening', null, [port1]);
              require('electron').ipcRenderer.send('do-a-gc');
      describe('when context destroyed', () => {
        it('does not crash', async () => {
            switch (req.url) {
              case '/index.html':
                res.setHeader('content-type', 'text/html');
                res.statusCode = 200;
                count = count + 1;
                  <title>Hello${count}</title>
                  var sharedWorker = new SharedWorker('worker.js');
                  sharedWorker.port.addEventListener('close', function(event) {
                     console.log('close event', event.data);
                  </script>`);
              case '/worker.js':
                res.setHeader('content-type', 'application/javascript; charset=UTF-8');
                  self.addEventListener('connect', function(event) {
                    var port = event.ports[0];
                    port.addEventListener('message', function(event) {
                      console.log('Message from main:', event.data);
                      port.postMessage('Hello from SharedWorker!');
                  });`);
                throw new Error(`unsupported endpoint: ${req.url}`);
          await w.loadURL(`http://localhost:${port}/index.html`);
          expect(w.webContents.getTitle()).to.equal('Hello1');
          // Before the fix, it would crash if reloaded, but now it doesn't
          expect(w.webContents.getTitle()).to.equal('Hello2');
          // const crashEvent = emittedOnce(w.webContents, 'render-process-gone');
          // await crashEvent;
    describe('MessageChannelMain', () => {
      it('can be created', () => {
        expect(port1).not.to.be.null();
        expect(port2).not.to.be.null();
      it('should not throw when supported values are passed as message', () => {
        const { port1 } = new MessageChannelMain();
        // @ts-expect-error - this shouldn't crash.
        expect(() => { port1.postMessage(); }).to.not.throw();
        expect(() => { port1.postMessage(undefined); }).to.not.throw();
        expect(() => { port1.postMessage(42); }).to.not.throw();
        expect(() => { port1.postMessage(false); }).to.not.throw();
        expect(() => { port1.postMessage([]); }).to.not.throw();
        expect(() => { port1.postMessage('hello'); }).to.not.throw();
        expect(() => { port1.postMessage({ hello: 'goodbye' }); }).to.not.throw();
      it('throws an error when an invalid parameter is sent to postMessage', () => {
          const buffer = new ArrayBuffer(10) as any;
          port1.postMessage(null, [buffer]);
        }).to.throw(/Port at index 0 is not a valid port/);
          port1.postMessage(null, ['1' as any]);
          port1.postMessage(null, [new Date() as any]);
      it('throws when postMessage transferables contains the source port', () => {
          port1.postMessage(null, [port1]);
        }).to.throw(/Port at index 0 contains the source port./);
      it('can send messages within the process', async () => {
        port2.postMessage('hello');
        const [ev] = await once(port1, 'message');
        expect(ev.data).to.equal('hello');
      it('can pass one end to a WebContents', async () => {
            port.onmessage = () => {
        port1.postMessage('hello');
        await once(ipcMain, 'done');
      it('can be passed over another channel', async () => {
          ipcRenderer.on('port', (e1: any) => {
            e1.ports[0].onmessage = (e2: any) => {
              e2.ports[0].onmessage = (e3: any) => {
                ipcRenderer.send('done', e3.data);
        const { port1: port3, port2: port4 } = new MessageChannelMain();
        port1.postMessage(null, [port4]);
        port3.postMessage('hello');
        const [, message] = await once(ipcMain, 'done');
        expect(message).to.equal('hello');
      it('can send messages to a closed port', () => {
        port2.on('message', () => { throw new Error('unexpected message received'); });
      it('can send messages to a port whose remote end is closed', () => {
        port2.close();
      it('throws when passing null ports', () => {
          port1.postMessage(null, [null] as any);
      it('throws when passing duplicate ports', () => {
        const { port1: port3 } = new MessageChannelMain();
          port1.postMessage(null, [port3, port3]);
        }).to.throw(/duplicate/);
      it('throws when passing ports that have already been neutered', () => {
        port1.postMessage(null, [port3]);
        }).to.throw(/already neutered/);
      it('throws when passing itself', () => {
        }).to.throw(/contains the source port/);
      describe('GC behavior', () => {
        it('is not collected while it could still receive messages', async () => {
          let trigger: Function;
          const promise = new Promise(resolve => { trigger = resolve; });
          const port1 = (() => {
            port2.on('message', (e) => { trigger(e.data); });
            return port1;
          expect(await promise).to.equal('hello');
    const generateTests = (title: string, postMessage: (contents: WebContents) => WebContents['postMessage']) => {
      describe(title, () => {
        it('sends a message', async () => {
            ipcRenderer.on('foo', (_e: Event, msg: string) => {
              ipcRenderer.send('bar', msg);
          postMessage(w.webContents)('foo', { some: 'message' });
          const [, msg] = await once(ipcMain, 'bar');
          expect(msg).to.deep.equal({ some: 'message' });
          it('throws on missing channel', async () => {
              (postMessage(w.webContents) as any)();
          it('throws on invalid channel', async () => {
              postMessage(w.webContents)(null as any, '', []);
          it('throws on missing message', async () => {
              (postMessage(w.webContents) as any)('channel');
          it('throws on non-serializable message', async () => {
              postMessage(w.webContents)('channel', w);
            }).to.throw(/An object could not be cloned/);
          it('throws on invalid transferable list', async () => {
              postMessage(w.webContents)('', '', null as any);
            }).to.throw(/Invalid value for transfer/);
          it('throws on transferring non-transferable', async () => {
              (postMessage(w.webContents) as any)('channel', '', [123]);
          it('throws when passing null ports', async () => {
              postMessage(w.webContents)('foo', null, [null] as any);
          it('throws when passing duplicate ports', async () => {
              postMessage(w.webContents)('foo', null, [port1, port1]);
          it('throws when passing ports that have already been neutered', async () => {
            postMessage(w.webContents)('foo', null, [port1]);
    generateTests('WebContents.postMessage', contents => contents.postMessage.bind(contents));
    generateTests('WebFrameMain.postMessage', contents => contents.mainFrame.postMessage.bind(contents.mainFrame));
  describe('WebContents.ipc', () => {
    it('receives ipc messages sent from the WebContents', async () => {
      w.webContents.executeJavaScript('require(\'electron\').ipcRenderer.send(\'test\', 42)');
      const [, num] = await once(w.webContents.ipc, 'test');
      expect(num).to.equal(42);
    it('receives sync-ipc messages sent from the WebContents', async () => {
      w.webContents.ipc.on('test', (event, arg) => {
        event.returnValue = arg * 2;
      const result = await w.webContents.executeJavaScript('require(\'electron\').ipcRenderer.sendSync(\'test\', 42)');
      expect(result).to.equal(42 * 2);
    it('receives postMessage messages sent from the WebContents, w/ MessagePorts', async () => {
      w.webContents.executeJavaScript('require(\'electron\').ipcRenderer.postMessage(\'test\', null, [(new MessageChannel).port1])');
      const [event] = await once(w.webContents.ipc, 'test');
      expect(event.ports.length).to.equal(1);
    it('handles invoke messages sent from the WebContents', async () => {
      w.webContents.ipc.handle('test', (_event, arg) => arg * 2);
      const result = await w.webContents.executeJavaScript('require(\'electron\').ipcRenderer.invoke(\'test\', 42)');
    it('cascades to ipcMain', async () => {
      let gotFromIpcMain = false;
      const ipcMainReceived = new Promise<void>(resolve => ipcMain.on('test', () => { gotFromIpcMain = true; resolve(); }));
      const ipcReceived = new Promise<boolean>(resolve => w.webContents.ipc.on('test', () => { resolve(gotFromIpcMain); }));
      defer(() => ipcMain.removeAllListeners('test'));
      // assert that they are delivered in the correct order
      expect(await ipcReceived).to.be.false();
      await ipcMainReceived;
    it('overrides ipcMain handlers', async () => {
      ipcMain.handle('test', () => { throw new Error('should not be called'); });
      defer(() => ipcMain.removeHandler('test'));
    it('falls back to ipcMain handlers', async () => {
      ipcMain.handle('test', (_event, arg) => { return arg * 2; });
    it('receives ipcs from child frames', async () => {
      const w = new BrowserWindow({ show: false, webPreferences: { nodeIntegrationInSubFrames: true, preload: path.resolve(fixturesPath, 'preload-expose-ipc.js') } });
      // Preloads don't run in about:blank windows, and file:// urls can't be loaded in iframes, so use a blank http page.
      await w.loadURL(`data:text/html,<iframe src="http://localhost:${port}"></iframe>`);
      w.webContents.mainFrame.frames[0].executeJavaScript('ipc.send(\'test\', 42)');
      const [, arg] = await once(w.webContents.ipc, 'test');
      expect(arg).to.equal(42);
  describe('WebFrameMain.ipc', () => {
    it('responds to ipc messages in the main frame', async () => {
      const [, arg] = await once(w.webContents.mainFrame.ipc, 'test');
    it('responds to sync ipc messages in the main frame', async () => {
      w.webContents.mainFrame.ipc.on('test', (event, arg) => {
      const [event] = await once(w.webContents.mainFrame.ipc, 'test');
      w.webContents.mainFrame.ipc.handle('test', (_event, arg) => arg * 2);
    it('cascades to WebContents and ipcMain', async () => {
      let gotFromWebContents = false;
      const ipcWebContentsReceived = new Promise<boolean>(resolve => w.webContents.ipc.on('test', () => { gotFromWebContents = true; resolve(gotFromIpcMain); }));
      const ipcReceived = new Promise<boolean>(resolve => w.webContents.mainFrame.ipc.on('test', () => { resolve(gotFromWebContents); }));
      expect(await ipcWebContentsReceived).to.be.false();
    it('overrides WebContents handlers', async () => {
      w.webContents.ipc.handle('test', () => { throw new Error('should not be called'); });
    it('falls back to WebContents handlers', async () => {
      w.webContents.ipc.handle('test', (_event, arg) => { return arg * 2; });
      w.webContents.mainFrame.ipc.on('test', () => { throw new Error('should not be called'); });
      const [, arg] = await once(w.webContents.mainFrame.frames[0].ipc, 'test');
    it('receives ipcs from unloading frames in the main frame', async () => {
      await w.loadURL(`http://localhost:${port}`);
      await w.webContents.executeJavaScript('window.onunload = () => require(\'electron\').ipcRenderer.send(\'unload\'); void 0');
      const onUnloadIpc = once(w.webContents.mainFrame.ipc, 'unload');
      w.loadURL(`http://127.0.0.1:${port}`); // cross-origin navigation
      const [{ senderFrame }] = await onUnloadIpc;
      expect(senderFrame.detached).to.be.true();
