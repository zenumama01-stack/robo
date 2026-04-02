import { BrowserWindow, ipcMain } from 'electron/main';
import { contextBridge } from 'electron/renderer';
import { listen } from './lib/spec-helpers';
const fixturesPath = path.resolve(__dirname, 'fixtures', 'api', 'context-bridge');
describe('contextBridge', () => {
      res.setHeader('Content-Type', 'text/html');
    if (server) await new Promise(resolve => server.close(resolve));
    if (dir) await fs.promises.rm(dir, { force: true, recursive: true });
  it('should not be accessible when contextIsolation is disabled', async () => {
        preload: path.resolve(fixturesPath, 'can-bind-preload.js')
    w.loadFile(path.resolve(fixturesPath, 'empty.html'));
    const [, bound] = await once(ipcMain, 'context-bridge-bound');
    expect(bound).to.equal(false);
  it('should be accessible when contextIsolation is enabled', async () => {
    expect(bound).to.equal(true);
  const generateTests = (useSandbox: boolean) => {
    describe(`with sandbox=${useSandbox}`, () => {
      const makeBindingWindow = async (bindingCreator: Function, worldId: number = 0) => {
        const preloadContentForMainWorld = `const renderer_1 = require('electron');
        ${useSandbox
: `require('node:v8').setFlagsFromString('--expose_gc');
        const gc=require('node:vm').runInNewContext('gc');
        renderer_1.contextBridge.exposeInMainWorld('GCRunner', {
          run: () => gc()
        });`}
        (${bindingCreator.toString()})();`;
        const preloadContentForIsolatedWorld = `const renderer_1 = require('electron');
        renderer_1.webFrame.setIsolatedWorldInfo(${worldId}, {
          name: "Isolated World"
        renderer_1.contextBridge.exposeInIsolatedWorld(${worldId}, 'GCRunner', {
        const tmpDir = await fs.promises.mkdtemp(path.resolve(os.tmpdir(), 'electron-spec-preload-'));
        dir = tmpDir;
        await fs.promises.writeFile(path.resolve(tmpDir, 'preload.js'), worldId === 0 ? preloadContentForMainWorld : preloadContentForIsolatedWorld);
            sandbox: useSandbox,
            preload: path.resolve(tmpDir, 'preload.js'),
            additionalArguments: ['--unsafely-expose-electron-internals-for-testing']
        await w.loadURL(serverUrl);
      const callWithBindings = (fn: Function, worldId: number = 0) =>
        worldId === 0 ? w.webContents.executeJavaScript(`(${fn.toString()})(window)`) : w.webContents.executeJavaScriptInIsolatedWorld(worldId, [{ code: `(${fn.toString()})(window)` }]);
      const getGCInfo = async (): Promise<{
        trackedValues: number;
        w.webContents.send('get-gc-info');
        const [, info] = await once(ipcMain, 'gc-info');
      const forceGCOnWindow = async () => {
        w.webContents.debugger.attach();
        await w.webContents.debugger.sendCommand('HeapProfiler.enable');
        await w.webContents.debugger.sendCommand('HeapProfiler.collectGarbage');
        await w.webContents.debugger.sendCommand('HeapProfiler.disable');
        w.webContents.debugger.detach();
      it('should proxy numbers', async () => {
        await makeBindingWindow(() => {
          contextBridge.exposeInMainWorld('example', 123);
        const result = await callWithBindings((root: any) => {
          return root.example;
        expect(result).to.equal(123);
      it('should proxy numbers when exposed in isolated world', async () => {
          contextBridge.exposeInIsolatedWorld(1004, 'example', 123);
        }, 1004);
      it('should make global properties read-only', async () => {
          root.example = 456;
      it('should proxy nested numbers', async () => {
          contextBridge.exposeInMainWorld('example', {
            myNumber: 123
          return root.example.myNumber;
      it('should make properties unwriteable', async () => {
          root.example.myNumber = 456;
      it('should proxy strings', async () => {
          contextBridge.exposeInMainWorld('example', 'my-words');
        expect(result).to.equal('my-words');
      it('should proxy nested strings', async () => {
            myString: 'my-words'
          return root.example.myString;
      it('should proxy nested strings when exposed in isolated world', async () => {
          contextBridge.exposeInIsolatedWorld(1004, 'example', {
      it('should proxy arrays', async () => {
          contextBridge.exposeInMainWorld('example', [123, 'my-words']);
          return [root.example, Array.isArray(root.example)];
        expect(result).to.deep.equal([[123, 'my-words'], true]);
      it('should proxy nested arrays', async () => {
            myArr: [123, 'my-words']
          return root.example.myArr;
        expect(result).to.deep.equal([123, 'my-words']);
      it('should make arrays immutable', async () => {
        const immutable = await callWithBindings((root: any) => {
            root.example.push(456);
        expect(immutable).to.equal(true);
      it('should make nested arrays immutable', async () => {
            root.example.myArr.push(456);
      it('should proxy booleans', async () => {
          contextBridge.exposeInMainWorld('example', true);
      it('should proxy nested booleans', async () => {
            myBool: true
          return root.example.myBool;
      it('should proxy promises and resolve with the correct value', async () => {
          contextBridge.exposeInMainWorld('example',
            Promise.resolve('i-resolved')
        expect(result).to.equal('i-resolved');
      it('should proxy nested promises and resolve with the correct value', async () => {
            myPromise: Promise.resolve('i-resolved')
          return root.example.myPromise;
      it('should proxy promises and reject with the correct value', async () => {
          contextBridge.exposeInMainWorld('example', Promise.reject(new Error('i-rejected')));
        const result = await callWithBindings(async (root: any) => {
            await root.example;
        expect(result).to.be.an.instanceOf(Error).with.property('message', 'i-rejected');
      it('should proxy nested promises and reject with the correct value', async () => {
            myPromise: Promise.reject(new Error('i-rejected'))
            await root.example.myPromise;
      it('should proxy promises and resolve with the correct value if it resolves later', async () => {
            myPromise: () => new Promise(resolve => setTimeout(() => resolve('delayed'), 20))
          return root.example.myPromise();
        expect(result).to.equal('delayed');
      it('should proxy nested promises correctly', async () => {
            myPromise: () => new Promise(resolve => setTimeout(() => resolve(Promise.resolve(123)), 20))
      it('should proxy methods', async () => {
            getNumber: () => 123,
            getString: () => 'help',
            getBoolean: () => false,
            getPromise: async () => 'promise'
          return [root.example.getNumber(), root.example.getString(), root.example.getBoolean(), await root.example.getPromise()];
        expect(result).to.deep.equal([123, 'help', false, 'promise']);
      it('should proxy functions', async () => {
          contextBridge.exposeInMainWorld('example', () => 'return-value');
          return root.example();
        expect(result).equal('return-value');
      it('should not double-proxy functions when they are returned to their origin side of the bridge', async () => {
          contextBridge.exposeInMainWorld('example', (fn: any) => fn);
          const fn = () => null;
          return root.example(fn) === fn;
        expect(result).equal(true);
      it('should proxy function arguments only once', async () => {
          contextBridge.exposeInMainWorld('example', (a: any, b: any) => a === b);
          const obj = { foo: 1 };
          return root.example(obj, obj);
        expect(result).to.be.true();
      it('should properly handle errors thrown in proxied functions', async () => {
          contextBridge.exposeInMainWorld('example', () => { throw new Error('oh no'); });
            root.example();
            return (e as Error).message;
        expect(result).equal('oh no');
      it('should proxy methods that are callable multiple times', async () => {
            doThing: () => 123
          return [root.example.doThing(), root.example.doThing(), root.example.doThing()];
        expect(result).to.deep.equal([123, 123, 123]);
      it('should proxy methods in the reverse direction', async () => {
            callWithNumber: (fn: any) => fn(123)
          return root.example.callWithNumber((n: number) => n + 1);
        expect(result).to.equal(124);
      it('should proxy promises in the reverse direction', async () => {
            getPromiseValue: (p: Promise<any>) => p
          return root.example.getPromiseValue(Promise.resolve('my-proxied-value'));
        expect(result).to.equal('my-proxied-value');
      it('should proxy objects with number keys', async () => {
            1: 123,
            2: 456,
            3: 789
          return [root.example[1], root.example[2], root.example[3], Array.isArray(root.example)];
        expect(result).to.deep.equal([123, 456, 789, false]);
      it('it should proxy null', async () => {
          contextBridge.exposeInMainWorld('example', null);
          // Convert to strings as although the context bridge keeps the right value
          // IPC does not
          return `${root.example}`;
        expect(result).to.deep.equal('null');
      it('it should proxy undefined', async () => {
          contextBridge.exposeInMainWorld('example', undefined);
        expect(result).to.deep.equal('undefined');
      it('it should proxy nested null and undefined correctly', async () => {
            values: [null, undefined]
          return root.example.values.map((val: any) => `${val}`);
        expect(result).to.deep.equal(['null', 'undefined']);
      it('should proxy symbols', async () => {
          const mySymbol = Symbol('unique');
          const isSymbol = (s: Symbol) => s === mySymbol;
          contextBridge.exposeInMainWorld('symbol', mySymbol);
          contextBridge.exposeInMainWorld('isSymbol', isSymbol);
          return root.isSymbol(root.symbol);
        expect(result).to.equal(true, 'symbols should be equal across contexts');
      it('should proxy symbols such that symbol equality works', async () => {
            getSymbol: () => mySymbol,
            isSymbol: (s: Symbol) => s === mySymbol
          return root.example.isSymbol(root.example.getSymbol());
      it('should proxy symbols such that symbol key lookup works', async () => {
            getObject: () => ({ [mySymbol]: 123 })
          return root.example.getObject()[root.example.getSymbol()];
        expect(result).to.equal(123, 'symbols key lookup should work across contexts');
      it('should proxy typed arrays', async () => {
          contextBridge.exposeInMainWorld('example', new Uint8Array(100));
          return Object.getPrototypeOf(root.example) === Uint8Array.prototype;
      it('should proxy regexps', async () => {
          contextBridge.exposeInMainWorld('example', /a/g);
          return Object.getPrototypeOf(root.example) === RegExp.prototype;
      it('should proxy typed arrays and regexps through the serializer', async () => {
            arr: new Uint8Array(100),
            regexp: /a/g
            Object.getPrototypeOf(root.example.arr) === Uint8Array.prototype,
            Object.getPrototypeOf(root.example.regexp) === RegExp.prototype
        expect(result).to.deep.equal([true, true]);
      it('should handle recursive objects', async () => {
          const o: any = { value: 135 };
          o.o = o;
            o
          return [root.example.o.value, root.example.o.o.value, root.example.o.o.o.value];
        expect(result).to.deep.equal([135, 135, 135]);
      it('should handle DOM elements', async () => {
            getElem: () => document.body
          return [root.example.getElem().tagName, root.example.getElem().constructor.name, typeof root.example.getElem().querySelector];
        expect(result).to.deep.equal(['BODY', 'HTMLBodyElement', 'function']);
      it('should handle DOM elements going backwards over the bridge', async () => {
            getElemInfo: (fn: Function) => {
              const elem = fn();
              return [elem.tagName, elem.constructor.name, typeof elem.querySelector];
          return root.example.getElemInfo(() => document.body);
      it('should handle Blobs', async () => {
            getBlob: () => new Blob(['ab', 'cd'])
          return [await root.example.getBlob().text()];
        expect(result).to.deep.equal(['abcd']);
      it('should handle Blobs going backwards over the bridge', async () => {
            getBlobText: async (fn: Function) => {
              const blob = fn();
              return [await blob.text()];
          return root.example.getBlobText(() => new Blob(['12', '45']));
        expect(result).to.deep.equal(['1245']);
      it('should handle VideoFrames', async () => {
            getVideoFrame: () => {
              const canvas = new OffscreenCanvas(16, 16);
              canvas.getContext('2d')!.fillRect(0, 0, 16, 16);
              return new VideoFrame(canvas, { timestamp: 0 });
          const frame = root.example.getVideoFrame();
          const info = [frame.constructor.name, frame.codedWidth, frame.codedHeight, frame.timestamp];
          frame.close();
        expect(result).to.deep.equal(['VideoFrame', 16, 16, 0]);
      it('should handle VideoFrames going backwards over the bridge', async () => {
            getVideoFrameInfo: (fn: Function) => {
              const frame = fn();
          return root.example.getVideoFrameInfo(() => {
            const canvas = new OffscreenCanvas(32, 32);
            canvas.getContext('2d')!.fillRect(0, 0, 32, 32);
            return new VideoFrame(canvas, { timestamp: 100 });
        expect(result).to.deep.equal(['VideoFrame', 32, 32, 100]);
      // Can only run tests which use the GCRunner in non-sandboxed environments
      if (!useSandbox) {
        it('should release the global hold on methods sent across contexts', async () => {
            const trackedValues: WeakRef<object>[] = [];
            require('electron').ipcRenderer.on('get-gc-info', (e: any) => e.sender.send('gc-info', { trackedValues: trackedValues.filter(value => value.deref()).length }));
              getFunction: () => () => 123,
              track: (value: object) => { trackedValues.push(new WeakRef(value)); }
          await callWithBindings(async (root: any) => {
            root.GCRunner.run();
          expect((await getGCInfo()).trackedValues).to.equal(0);
            const fn = root.example.getFunction();
            root.example.track(fn);
            root.x = [fn];
          expect((await getGCInfo()).trackedValues).to.equal(1);
            root.x = [];
      if (useSandbox) {
        it('should not leak the global hold on methods sent across contexts when reloading a sandboxed renderer', async () => {
            require('electron').ipcRenderer.send('window-ready-for-tasking');
          const loadPromise = once(ipcMain, 'window-ready-for-tasking');
          await callWithBindings((root: any) => {
            root.example.track(root.example.getFunction());
            root.location.reload();
          await forceGCOnWindow();
          // If this is ever "2" it means we leaked the exposed function and
          // therefore the entire context after a reload
      it('it should not let you overwrite existing exposed things', async () => {
          let threw = false;
            attempt: 1,
            getThrew: () => threw
              attempt: 2,
            threw = true;
          return [root.example.attempt, root.example.getThrew()];
        expect(result).to.deep.equal([1, true]);
      it('should work with complex nested methods and promises', async () => {
            first: (second: Function) => second((fourth: Function) => {
              return fourth();
          return root.example.first((third: Function) => {
            return third(() => Promise.resolve('final value'));
        expect(result).to.equal('final value');
      it('should work with complex nested methods and promises attached directly to the global', async () => {
            (second: Function) => second((fourth: Function) => {
          return root.example((third: Function) => {
      it('should throw an error when recursion depth is exceeded', async () => {
            doThing: (a: any) => console.log(a)
        let threw = await callWithBindings((root: any) => {
            let a: any = [];
            for (let i = 0; i < 999; i++) {
              a = [a];
            root.example.doThing(a);
        expect(threw).to.equal(false);
        threw = await callWithBindings((root: any) => {
            for (let i = 0; i < 1000; i++) {
        expect(threw).to.equal(true);
      it('should copy thrown errors into the other context', async () => {
            throwNormal: () => {
              throw new Error('whoops');
            throwWeird: () => {
              throw 'this is no error...'; // eslint-disable-line no-throw-literal
            throwNotClonable: () => {
              return Object(Symbol('foo'));
            throwNotClonableNestedArray: () => {
              return [Object(Symbol('foo'))];
            throwNotClonableNestedObject: () => {
                bad: Object(Symbol('foo'))
            throwDynamic: () => {
                get bad () {
                  throw new Error('damm');
            argumentConvert: () => {},
            rejectNotClonable: async () => {
              throw Object(Symbol('foo'));
            resolveNotClonable: async () => Object(Symbol('foo'))
          const getError = (fn: Function) => {
          const getAsyncError = async (fn: Function) => {
              await fn();
          const normalIsError = Object.getPrototypeOf(getError(root.example.throwNormal)) === Error.prototype;
          const weirdIsError = Object.getPrototypeOf(getError(root.example.throwWeird)) === Error.prototype;
          const notClonableIsError = Object.getPrototypeOf(getError(root.example.throwNotClonable)) === Error.prototype;
          const notClonableNestedArrayIsError = Object.getPrototypeOf(getError(root.example.throwNotClonableNestedArray)) === Error.prototype;
          const notClonableNestedObjectIsError = Object.getPrototypeOf(getError(root.example.throwNotClonableNestedObject)) === Error.prototype;
          const dynamicIsError = Object.getPrototypeOf(getError(root.example.throwDynamic)) === Error.prototype;
          const argumentConvertIsError = Object.getPrototypeOf(getError(() => root.example.argumentConvert(Object(Symbol('test'))))) === Error.prototype;
          const rejectNotClonableIsError = Object.getPrototypeOf(await getAsyncError(root.example.rejectNotClonable)) === Error.prototype;
          const resolveNotClonableIsError = Object.getPrototypeOf(await getAsyncError(root.example.resolveNotClonable)) === Error.prototype;
          return [normalIsError, weirdIsError, notClonableIsError, notClonableNestedArrayIsError, notClonableNestedObjectIsError, dynamicIsError, argumentConvertIsError, rejectNotClonableIsError, resolveNotClonableIsError];
        expect(result).to.deep.equal([true, true, true, true, true, true, true, true, true], 'should all be errors in the current context');
      it('should not leak prototypes', async () => {
            number: 123,
            string: 'string',
            boolean: true,
            arr: [123, 'string', true, ['foo']],
            symbol: Symbol('foo'),
            bigInt: 10n,
            getObject: () => ({ thing: 123 }),
            getString: () => 'string',
            getBoolean: () => true,
            getArr: () => [123, 'string', true, ['foo']],
            getPromise: async () => ({ number: 123, string: 'string', boolean: true, fn: () => 'string', arr: [123, 'string', true, ['foo']] }),
            getFunctionFromFunction: async () => () => null,
            object: {
              getPromise: async () => ({ number: 123, string: 'string', boolean: true, fn: () => 'string', arr: [123, 'string', true, ['foo']] })
            receiveArguments: (fn: any) => fn({ key: 'value' }),
            symbolKeyed: {
              [Symbol('foo')]: 123
            getBody: () => document.body,
            getBlob: () => new Blob(['ab', 'cd']),
          const { example } = root;
          let arg: any;
          example.receiveArguments((o: any) => { arg = o; });
          const protoChecks = [
            ...Object.keys(example).map(key => [key, String]),
            ...Object.getOwnPropertySymbols(example.symbolKeyed).map(key => [key, Symbol]),
            [example, Object],
            [example.number, Number],
            [example.string, String],
            [example.boolean, Boolean],
            [example.arr, Array],
            [example.arr[0], Number],
            [example.arr[1], String],
            [example.arr[2], Boolean],
            [example.arr[3], Array],
            [example.arr[3][0], String],
            [example.symbol, Symbol],
            [example.bigInt, BigInt],
            [example.getNumber, Function],
            [example.getNumber(), Number],
            [example.getObject(), Object],
            [example.getString(), String],
            [example.getBoolean(), Boolean],
            [example.getArr(), Array],
            [example.getArr()[0], Number],
            [example.getArr()[1], String],
            [example.getArr()[2], Boolean],
            [example.getArr()[3], Array],
            [example.getArr()[3][0], String],
            [example.getFunctionFromFunction, Function],
            [example.getFunctionFromFunction(), Promise],
            [await example.getFunctionFromFunction(), Function],
            [example.getPromise(), Promise],
            [await example.getPromise(), Object],
            [(await example.getPromise()).number, Number],
            [(await example.getPromise()).string, String],
            [(await example.getPromise()).boolean, Boolean],
            [(await example.getPromise()).fn, Function],
            [(await example.getPromise()).fn(), String],
            [(await example.getPromise()).arr, Array],
            [(await example.getPromise()).arr[0], Number],
            [(await example.getPromise()).arr[1], String],
            [(await example.getPromise()).arr[2], Boolean],
            [(await example.getPromise()).arr[3], Array],
            [(await example.getPromise()).arr[3][0], String],
            [example.object, Object],
            [example.object.number, Number],
            [example.object.string, String],
            [example.object.boolean, Boolean],
            [example.object.arr, Array],
            [example.object.arr[0], Number],
            [example.object.arr[1], String],
            [example.object.arr[2], Boolean],
            [example.object.arr[3], Array],
            [example.object.arr[3][0], String],
            [await example.object.getPromise(), Object],
            [(await example.object.getPromise()).number, Number],
            [(await example.object.getPromise()).string, String],
            [(await example.object.getPromise()).boolean, Boolean],
            [(await example.object.getPromise()).fn, Function],
            [(await example.object.getPromise()).fn(), String],
            [(await example.object.getPromise()).arr, Array],
            [(await example.object.getPromise()).arr[0], Number],
            [(await example.object.getPromise()).arr[1], String],
            [(await example.object.getPromise()).arr[2], Boolean],
            [(await example.object.getPromise()).arr[3], Array],
            [(await example.object.getPromise()).arr[3][0], String],
            [arg, Object],
            [arg.key, String],
            [example.getBody(), HTMLBodyElement],
            [example.getBlob(), Blob],
            [example.getVideoFrame(), VideoFrame]
            protoMatches: protoChecks.map(([a, Constructor]) => Object.getPrototypeOf(a) === Constructor.prototype)
        // Every protomatch should be true
        expect(result.protoMatches).to.deep.equal(result.protoMatches.map(() => true));
      it('should not leak prototypes when attaching directly to the global', async () => {
          const toExpose = {
            getError: () => new Error('foo'),
            getWeirdError: () => {
              const e = new Error('foo');
              e.message = { garbage: true } as any;
          for (const [key, value] of Object.entries(toExpose)) {
            keys.push(key);
            contextBridge.exposeInMainWorld(key, value);
          contextBridge.exposeInMainWorld('keys', keys);
          const { keys } = root;
          const cleanedRoot: any = {};
          for (const [key, value] of Object.entries(root)) {
            if (keys.includes(key)) {
              cleanedRoot[key] = value;
          cleanedRoot.receiveArguments((o: any) => { arg = o; });
            ...Object.keys(cleanedRoot).map(key => [key, String]),
            ...Object.getOwnPropertySymbols(cleanedRoot.symbolKeyed).map(key => [key, Symbol]),
            [cleanedRoot, Object],
            [cleanedRoot.number, Number],
            [cleanedRoot.string, String],
            [cleanedRoot.boolean, Boolean],
            [cleanedRoot.arr, Array],
            [cleanedRoot.arr[0], Number],
            [cleanedRoot.arr[1], String],
            [cleanedRoot.arr[2], Boolean],
            [cleanedRoot.arr[3], Array],
            [cleanedRoot.arr[3][0], String],
            [cleanedRoot.symbol, Symbol],
            [cleanedRoot.bigInt, BigInt],
            [cleanedRoot.getNumber, Function],
            [cleanedRoot.getNumber(), Number],
            [cleanedRoot.getObject(), Object],
            [cleanedRoot.getString(), String],
            [cleanedRoot.getBoolean(), Boolean],
            [cleanedRoot.getArr(), Array],
            [cleanedRoot.getArr()[0], Number],
            [cleanedRoot.getArr()[1], String],
            [cleanedRoot.getArr()[2], Boolean],
            [cleanedRoot.getArr()[3], Array],
            [cleanedRoot.getArr()[3][0], String],
            [cleanedRoot.getFunctionFromFunction, Function],
            [cleanedRoot.getFunctionFromFunction(), Promise],
            [await cleanedRoot.getFunctionFromFunction(), Function],
            [cleanedRoot.getError(), Error],
            [cleanedRoot.getError().message, String],
            [cleanedRoot.getWeirdError(), Error],
            [cleanedRoot.getWeirdError().message, String],
            [cleanedRoot.getPromise(), Promise],
            [await cleanedRoot.getPromise(), Object],
            [(await cleanedRoot.getPromise()).number, Number],
            [(await cleanedRoot.getPromise()).string, String],
            [(await cleanedRoot.getPromise()).boolean, Boolean],
            [(await cleanedRoot.getPromise()).fn, Function],
            [(await cleanedRoot.getPromise()).fn(), String],
            [(await cleanedRoot.getPromise()).arr, Array],
            [(await cleanedRoot.getPromise()).arr[0], Number],
            [(await cleanedRoot.getPromise()).arr[1], String],
            [(await cleanedRoot.getPromise()).arr[2], Boolean],
            [(await cleanedRoot.getPromise()).arr[3], Array],
            [(await cleanedRoot.getPromise()).arr[3][0], String],
            [cleanedRoot.object, Object],
            [cleanedRoot.object.number, Number],
            [cleanedRoot.object.string, String],
            [cleanedRoot.object.boolean, Boolean],
            [cleanedRoot.object.arr, Array],
            [cleanedRoot.object.arr[0], Number],
            [cleanedRoot.object.arr[1], String],
            [cleanedRoot.object.arr[2], Boolean],
            [cleanedRoot.object.arr[3], Array],
            [cleanedRoot.object.arr[3][0], String],
            [await cleanedRoot.object.getPromise(), Object],
            [(await cleanedRoot.object.getPromise()).number, Number],
            [(await cleanedRoot.object.getPromise()).string, String],
            [(await cleanedRoot.object.getPromise()).boolean, Boolean],
            [(await cleanedRoot.object.getPromise()).fn, Function],
            [(await cleanedRoot.object.getPromise()).fn(), String],
            [(await cleanedRoot.object.getPromise()).arr, Array],
            [(await cleanedRoot.object.getPromise()).arr[0], Number],
            [(await cleanedRoot.object.getPromise()).arr[1], String],
            [(await cleanedRoot.object.getPromise()).arr[2], Boolean],
            [(await cleanedRoot.object.getPromise()).arr[3], Array],
            [(await cleanedRoot.object.getPromise()).arr[3][0], String],
            [arg.key, String]
      describe('internalContextBridge', () => {
        describe('overrideGlobalValueFromIsolatedWorld', () => {
          it('should override top level properties', async () => {
              contextBridge.internalContextBridge!.overrideGlobalValueFromIsolatedWorld(['open'], () => ({ you: 'are a wizard' }));
              return root.open();
            expect(result).to.deep.equal({ you: 'are a wizard' });
          it('should override deep properties', async () => {
              contextBridge.internalContextBridge!.overrideGlobalValueFromIsolatedWorld(['document', 'foo'], () => 'I am foo');
              return root.document.foo();
            expect(result).to.equal('I am foo');
        describe('overrideGlobalPropertyFromIsolatedWorld', () => {
          it('should call the getter correctly', async () => {
              let callCount = 0;
              const getter = () => {
                callCount++;
              contextBridge.internalContextBridge!.overrideGlobalPropertyFromIsolatedWorld(['isFun'], getter);
              contextBridge.exposeInMainWorld('foo', {
                callCount: () => callCount
              return [root.isFun, root.foo.callCount()];
            expect(result[0]).to.equal(true);
            expect(result[1]).to.equal(1);
          it('should not make a setter if none is provided', async () => {
              contextBridge.internalContextBridge!.overrideGlobalPropertyFromIsolatedWorld(['isFun'], () => true);
              root.isFun = 123;
              return root.isFun;
          it('should call the setter correctly', async () => {
              const callArgs: any[] = [];
              const setter = (...args: any[]) => {
                callArgs.push(args);
              contextBridge.internalContextBridge!.overrideGlobalPropertyFromIsolatedWorld(['isFun'], () => true, setter);
                callArgs: () => callArgs
              return root.foo.callArgs();
            expect(result).to.have.lengthOf(1);
            expect(result[0]).to.have.lengthOf(1);
            expect(result[0][0]).to.equal(123);
        describe('overrideGlobalValueWithDynamicPropsFromIsolatedWorld', () => {
          it('should not affect normal values', async () => {
              contextBridge.internalContextBridge!.overrideGlobalValueWithDynamicPropsFromIsolatedWorld(['thing'], {
                a: 123,
                b: () => 2,
                c: () => ({ d: 3 })
              return [root.thing.a, root.thing.b(), root.thing.c()];
            expect(result).to.deep.equal([123, 2, { d: 3 }]);
          it('should work with getters', async () => {
                get foo () {
                  return 'hi there';
              return root.thing.foo;
            expect(result).to.equal('hi there');
          it('should work with nested getters', async () => {
                    get bar () {
              return root.thing.foo.bar;
          it('should work with setters', async () => {
              let a: any = null;
                set foo (arg: any) {
                  a = arg + 1;
              root.thing.foo = 123;
          it('should work with nested getter / setter combos', async () => {
                get thingy () {
              root.thing.thingy.foo = 123;
              return root.thing.thingy.foo;
          it('should work with deep properties', async () => {
                a: () => ({
                    return 'still here';
              return root.thing.a().foo;
            expect(result).to.equal('still here');
      describe('executeInMainWorld', () => {
        it('serializes function and proxies args', async () => {
          await makeBindingWindow(async () => {
              123,
              'string',
              [123, 'string', true, ['foo']],
              () => 'string',
              Symbol('foo')
            function appendArg (arg: any) {
              // @ts-ignore
              globalThis.args = globalThis.args || [];
              globalThis.args.push(arg);
                await contextBridge.executeInMainWorld({
                  func: appendArg,
                  args: [value]
                contextBridge.executeInMainWorld({
                  args: ['FAIL']
          const result = await callWithBindings(() => {
            return globalThis.args.map(arg => {
              // Map unserializable IPC types to their type string
              if (['function', 'symbol'].includes(typeof arg)) {
                return typeof arg;
          expect(result).to.deep.equal([
            'function',
            'symbol'
        it('allows function args to be invoked', async () => {
          const donePromise = once(ipcMain, 'done');
          makeBindingWindow(() => {
            const uuid = crypto.randomUUID();
            const done = (receivedUuid: string) => {
              if (receivedUuid === uuid) {
                require('electron').ipcRenderer.send('done');
              func: (callback, innerUuid) => {
                callback(innerUuid);
              args: [done, uuid]
          await donePromise;
        it('proxies arguments only once', async () => {
            const obj = {};
            globalThis.result = contextBridge.executeInMainWorld({
              func: (a, b) => a === b,
              args: [obj, obj]
            return globalThis.result;
          }, 999);
        it('safely clones returned objects', async () => {
            const obj = contextBridge.executeInMainWorld({
              func: () => ({})
            globalThis.safe = obj.constructor === Object;
            return globalThis.safe;
        it('uses internal Function.prototype.toString', async () => {
            const funcHack = () => {
              globalThis.hacked = 'nope';
            funcHack.toString = () => '() => { globalThis.hacked = \'gotem\'; }';
              func: funcHack
            return globalThis.hacked;
          expect(result).to.equal('nope');
  generateTests(true);
  generateTests(false);
describe('ContextBridgeMutability', () => {
  it('should not make properties unwriteable and read-only if ContextBridgeMutability is on', async () => {
    const appPath = path.join(fixturesPath, 'context-bridge-mutability');
    const appProcess = cp.spawn(process.execPath, ['--enable-logging', '--enable-features=ContextBridgeMutability', appPath]);
    expect(output).to.include('some-modified-text');
    expect(output).to.include('obj-modified-prop');
    expect(output).to.include('1,2,5,3,4');
  it('should make properties unwriteable and read-only if ContextBridgeMutability is off', async () => {
    const appProcess = cp.spawn(process.execPath, ['--enable-logging', appPath]);
    expect(output).to.include('some-text');
    expect(output).to.include('obj-prop');
    expect(output).to.include('1,2,3,4');
