import { BrowserWindow, Menu, MenuItem } from 'electron/main';
import { singleModifierCombinations } from './lib/accelerator-helpers';
import { ifit } from './lib/spec-helpers';
import { sortMenuItems } from '../lib/browser/api/menu-utils';
describe('Menu module', function () {
    expect(Menu.prototype.constructor.name).to.equal('Menu');
  describe('Menu.buildFromTemplate', () => {
    it('should be able to attach extra fields', () => {
          extra: 'field'
        } as MenuItem | Record<string, any>
      expect((menu.items[0] as any).extra).to.equal('field');
    it('should be able to accept only MenuItems', () => {
        new MenuItem({ label: 'one' }),
        new MenuItem({ label: 'two' })
      expect(menu.items[0].label).to.equal('one');
      expect(menu.items[1].label).to.equal('two');
    it('should be able to accept only MenuItems in a submenu', () => {
          label: 'one',
            new MenuItem({ label: 'two' }) as any
      expect(menu.items[0].submenu!.items[0].label).to.equal('two');
    it('should be able to accept MenuItems and plain objects', () => {
        { label: 'two' }
    it('does not modify the specified template', () => {
      const template = [{ label: 'text', submenu: [{ label: 'sub' }] }];
      const templateCopy = JSON.parse(JSON.stringify(template));
      Menu.buildFromTemplate(template);
      expect(template).to.deep.equal(templateCopy);
    it('does not throw exceptions for undefined/null values', () => {
        Menu.buildFromTemplate([
            accelerator: undefined
            label: 'text again',
            accelerator: null as any
    it('does throw exceptions for empty objects and null values', () => {
        Menu.buildFromTemplate([{}, null as any]);
      }).to.throw(/Invalid template for MenuItem: must have at least one of label, role or type/);
    it('does throw exception for object without role, label, or type attribute', () => {
        Menu.buildFromTemplate([{ visible: true }]);
    it('does throw exception for undefined', () => {
        Menu.buildFromTemplate([undefined as any]);
    it('throws when an non-array is passed as a template', () => {
        Menu.buildFromTemplate('hello' as any);
      }).to.throw(/Invalid template for Menu: Menu template must be an array/);
    describe('Menu sorting and building', () => {
      describe('sorts groups', () => {
        it('does a simple sort', () => {
          const items: Electron.MenuItemConstructorOptions[] = [
              label: 'two',
              afterGroupContaining: ['1']
              label: 'one'
          const expected = [
          expect(sortMenuItems(items)).to.deep.equal(expected);
        it('does a simple sort with MenuItems', () => {
          const firstItem = new MenuItem({ id: '1', label: 'one' });
          const secondItem = new MenuItem({
          const sep = new MenuItem({ type: 'separator' });
          const items = [secondItem, sep, firstItem];
          const expected = [firstItem, sep, secondItem];
        it('resolves cycles by ignoring things that conflict', () => {
              afterGroupContaining: ['2']
        it('ignores references to commands that do not exist', () => {
              afterGroupContaining: ['does-not-exist']
        it('only respects the first matching [before|after]GroupContaining rule in a given group', () => {
              label: 'three',
              beforeGroupContaining: ['1']
              label: 'four',
              label: 'two'
      describe('moves an item to a different group by merging groups', () => {
        it('can move a group of one item', () => {
              after: ['1']
            { type: 'separator' }
        it("moves all items in the moving item's group", () => {
              label: 'four'
        it("ignores positions relative to commands that don't exist", () => {
              after: ['does-not-exist']
        it('can handle recursive group merging', () => {
              after: ['3']
              before: ['1']
              label: 'three'
        it('can merge multiple groups when given a list of before/after commands', () => {
              after: ['1', '2']
        it('can merge multiple groups based on both before/after commands', () => {
              after: ['1'],
              before: ['2']
      it('should position before existing item', () => {
        expect(menu.items[2].label).to.equal('three');
      it('should position after existing item', () => {
      it('should filter excess menu separators', () => {
        const menuOne = Menu.buildFromTemplate([
            label: 'a'
            label: 'b'
            label: 'c'
        expect(menuOne.items).to.have.length(3);
        expect(menuOne.items[0].label).to.equal('a');
        expect(menuOne.items[1].label).to.equal('b');
        expect(menuOne.items[2].label).to.equal('c');
        const menuTwo = Menu.buildFromTemplate([
        expect(menuTwo.items).to.have.length(3);
        expect(menuTwo.items[0].label).to.equal('a');
        expect(menuTwo.items[1].label).to.equal('b');
        expect(menuTwo.items[2].label).to.equal('c');
      it('should only filter excess menu separators AFTER the re-ordering for before/after is done', () => {
            label: 'Foo',
            id: 'foo'
            label: 'Bar',
            id: 'bar'
            type: 'separator',
            before: ['bar']
        expect(menuOne.items[0].label).to.equal('Foo');
        expect(menuOne.items[1].type).to.equal('separator');
        expect(menuOne.items[2].label).to.equal('Bar');
      it('should continue inserting items at next index when no specifier is present', () => {
            label: 'five'
        expect(menu.items[3].label).to.equal('four');
        expect(menu.items[4].label).to.equal('five');
      it('should continue inserting MenuItems at next index when no specifier is present', () => {
          new MenuItem({
          }), new MenuItem({
  describe('Menu.getMenuItemById', () => {
    it('should return the item with the given id', () => {
              label: 'Enter Fullscreen',
              accelerator: 'ControlCommandF',
              id: 'fullScreen'
      const fsc = menu.getMenuItemById('fullScreen');
      expect(menu.items[0].submenu!.items[0]).to.equal(fsc);
    it('should return the separator with the given id', () => {
          label: 'Item 1',
          id: 'item_1'
          id: 'separator',
          label: 'Item 2',
          id: 'item_2'
      const separator = menu.getMenuItemById('separator');
      expect(separator).to.be.an('object');
      expect(separator).to.equal(menu.items[1]);
  describe('Menu.insert', () => {
    it('should throw when attempting to insert at out-of-range indices', () => {
        { label: '1' },
        { label: '2' },
        { label: '3' }
      const item = new MenuItem({ label: 'badInsert' });
        menu.insert(9999, item);
      }).to.throw(/Position 9999 cannot be greater than the total MenuItem count/);
        menu.insert(-9999, item);
      }).to.throw(/Position -9999 cannot be less than 0/);
    it('should store item in @items by its index', () => {
      const item = new MenuItem({ label: 'inserted' });
      menu.insert(1, item);
      expect(menu.items[0].label).to.equal('1');
      expect(menu.items[1].label).to.equal('inserted');
      expect(menu.items[2].label).to.equal('2');
      expect(menu.items[3].label).to.equal('3');
  describe('Menu.append', () => {
    it('should add the item to the end of the menu', () => {
      expect(menu.items[1].label).to.equal('2');
      expect(menu.items[2].label).to.equal('3');
      expect(menu.items[3].label).to.equal('inserted');
  describe('Menu.popup', () => {
    let menu: Menu;
      w = new BrowserWindow({ show: false, width: 200, height: 200 });
      menu = Menu.buildFromTemplate([
      menu.closePopup();
      menu.closePopup(w);
    it('throws an error if options is not an object', () => {
        menu.popup('this is a string, not an object' as any);
      }).to.throw(/Options must be an object/);
    it('allows for options to be optional', () => {
        menu.popup({});
    it('should emit menu-will-show event', async () => {
      const menuWillShow = once(menu, 'menu-will-show');
      menu.popup({ window: w });
      await menuWillShow;
    it('should emit menu-will-close event', async () => {
      const menuWillClose = once(menu, 'menu-will-close');
      await menuWillClose;
    it('returns immediately', () => {
      const input = { window: w, x: 100, y: 101 };
      const output = menu.popup(input) as unknown as {x: number, y: number, browserWindow: BrowserWindow};
      expect(output.x).to.equal(input.x);
      expect(output.y).to.equal(input.y);
      expect(output.browserWindow).to.equal(input.window);
    it('works without a given BrowserWindow and options', () => {
      const { browserWindow, x, y } = menu.popup({ x: 100, y: 101 }) as unknown as {x: number, y: number, browserWindow: BrowserWindow};
      expect(browserWindow.constructor.name).to.equal('BrowserWindow');
      expect(x).to.equal(100);
      expect(y).to.equal(101);
    it('works with a given BrowserWindow, options and callback', (done) => {
      const { x, y } = menu.popup({
        window: w,
        x: 100,
        y: 101,
        callback: () => done()
      }) as unknown as {x: number, y: number};
    it('works with a given BrowserWindow, no options, and a callback', (done) => {
      menu.popup({ window: w, callback: () => done() });
    it('prevents menu from getting garbage-collected when popuping', async () => {
      const menu = Menu.buildFromTemplate([{ role: 'paste' }]);
      // Keep a weak reference to the menu.
      const wr = new WeakRef(menu);
      // Do garbage collection, since |menu| is not referenced in this closure
      // it would be gone after next call.
      // Try to receive menu from weak reference.
      if (wr.deref()) {
        wr.deref()!.closePopup();
        throw new Error('Menu is garbage-collected while popuping');
    // https://github.com/electron/electron/issues/35724
    // Maximizing window is enough to trigger the bug
    // FIXME(dsanders11): Test always passes on CI, even pre-fix
    ifit(process.platform === 'linux' && !process.env.CI)('does not trigger issue #35724', (done) => {
      const showAndCloseMenu = async () => {
        menu.popup({ window: w, x: 50, y: 50 });
        const closed = once(menu, 'menu-will-close');
      const failOnEvent = () => { done(new Error('Menu closed prematurely')); };
      assert(!w.isVisible());
      w.on('show', async () => {
        assert(!w.isMaximized());
        // Show the menu once, then maximize window
        await showAndCloseMenu();
        // NOTE - 'maximize' event never fires on CI for Linux
        const maximized = once(w, 'maximize');
        await maximized;
        // Bug only seems to trigger programmatically after showing the menu once more
        // Now ensure the menu stays open until we close it
        menu.once('menu-will-close', failOnEvent);
        await setTimeout(1500);
        menu.off('menu-will-close', failOnEvent);
        menu.once('menu-will-close', () => done());
    const chunkSize = 10;
    let chunkCount = 0;
    const totalChunks = Math.ceil(singleModifierCombinations.length / chunkSize);
    for (let i = 0; i < singleModifierCombinations.length; i += chunkSize) {
      const chunk = singleModifierCombinations.slice(i, i + chunkSize);
      it(`does not crash when rendering menu item with single accelerator combinations ${++chunkCount}/${totalChunks}`, async () => {
          ...chunk.map(combination => ({
            label: `Test ${combination}`,
            accelerator: combination
  ifit(process.platform === 'darwin')(
    'emits menu close event even if submenu closes first',
        label: 'parent',
          label: 'child'
      (menu as any)._simulateSubmenuCloseSequenceForTesting();
        menuWillClose,
        setTimeout(1000).then(() => {
          throw new Error('menu-will-close was not emitted');
  describe('Menu.setApplicationMenu', () => {
    it('sets a menu', () => {
        { label: '2' }
      expect(Menu.getApplicationMenu()).to.not.be.null('application menu');
    // DISABLED-FIXME(nornagon): this causes the focus handling tests to fail
    it('unsets a menu with null', () => {
      Menu.setApplicationMenu(null);
      expect(Menu.getApplicationMenu()).to.be.null('application menu');
    ifit(process.platform !== 'darwin')('does not override menu visibility on startup', async () => {
      const appPath = path.join(fixturesPath, 'api', 'test-menu-visibility');
      const appProcess = cp.spawn(process.execPath, [appPath]);
      await new Promise<void>((resolve) => {
        appProcess.stdout.on('data', data => {
          output += data;
          if (data.includes('Window has')) {
      expect(output).to.include('Window has no menu');
    ifit(process.platform !== 'darwin')('does not override null menu on startup', async () => {
      const appPath = path.join(fixturesPath, 'api', 'test-menu-null');
      appProcess.stderr.on('data', data => { output += data; });
      if (!output.includes('Window has no menu')) {
        console.log(code, output);
