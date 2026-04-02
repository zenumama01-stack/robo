import { dialog, BaseWindow, BrowserWindow } from 'electron/main';
describe('dialog module', () => {
  describe('showOpenDialog', () => {
    ifit(process.platform !== 'win32')('should not throw for valid cases', () => {
        dialog.showOpenDialog({ title: 'i am title' });
        dialog.showOpenDialog(w, { title: 'i am title' });
        const w = new BaseWindow();
    it('throws errors when the options are invalid', () => {
        dialog.showOpenDialog({ properties: false as any });
      }).to.throw(/Properties must be an array/);
        dialog.showOpenDialog({ title: 300 as any });
      }).to.throw(/Title must be a string/);
        dialog.showOpenDialog({ buttonLabel: [] as any });
      }).to.throw(/Button label must be a string/);
        dialog.showOpenDialog({ defaultPath: {} as any });
      }).to.throw(/Default path must be a string/);
        dialog.showOpenDialog({ message: {} as any });
      }).to.throw(/Message must be a string/);
  describe('showSaveDialog', () => {
        dialog.showSaveDialog({ title: 'i am title' });
        dialog.showSaveDialog(w, { title: 'i am title' });
        dialog.showSaveDialog({ title: 300 as any });
        dialog.showSaveDialog({ buttonLabel: [] as any });
        dialog.showSaveDialog({ defaultPath: {} as any });
        dialog.showSaveDialog({ message: {} as any });
        dialog.showSaveDialog({ nameFieldLabel: {} as any });
      }).to.throw(/Name field label must be a string/);
  describe('showMessageBox', () => {
    // parentless message boxes are synchronous on macOS
    // dangling message boxes on windows cause a DCHECK: https://source.chromium.org/chromium/chromium/src/+/main:base/win/message_window.cc;drc=7faa4bf236a866d007dc5672c9ce42660e67a6a6;l=68
    ifit(process.platform !== 'darwin' && process.platform !== 'win32')('should not throw for a parentless message box', () => {
        dialog.showMessageBox({ message: 'i am message' });
        dialog.showMessageBox(w, { message: 'i am message' });
        dialog.showMessageBox(undefined as any, { type: 'not-a-valid-type' as any, message: '' });
      }).to.throw(/Invalid message box type/);
        dialog.showMessageBox(null as any, { buttons: false as any, message: '' });
      }).to.throw(/Buttons must be an array/);
        dialog.showMessageBox({ title: 300 as any, message: '' });
        dialog.showMessageBox({ message: [] as any });
        dialog.showMessageBox({ detail: 3.14 as any, message: '' });
      }).to.throw(/Detail must be a string/);
        dialog.showMessageBox({ checkboxLabel: false as any, message: '' });
      }).to.throw(/checkboxLabel must be a string/);
  describe('showMessageBox with signal', () => {
    it('closes message box immediately', async () => {
      const signal = controller.signal;
      const p = dialog.showMessageBox(w, { signal, message: 'i am message' });
      controller.abort();
      const result = await p;
      expect(result.response).to.equal(0);
    it('closes message box after a while', async () => {
    it('does not crash when there is a defaultId but no buttons', async () => {
      const p = dialog.showMessageBox(w, {
        message: 'i am message',
        title: 'i am title'
    it('cancels message box', async () => {
      expect(result.response).to.equal(1);
    it('cancels message box after a while', async () => {
  describe('showErrorBox', () => {
        (dialog.showErrorBox as any)();
      }).to.throw(/Insufficient number of arguments/);
        dialog.showErrorBox(3 as any, 'four');
      }).to.throw(/Error processing argument at index 0/);
        dialog.showErrorBox('three', 4 as any);
      }).to.throw(/Error processing argument at index 1/);
  describe('showCertificateTrustDialog', () => {
        (dialog.showCertificateTrustDialog as any)();
      }).to.throw(/options must be an object/);
        dialog.showCertificateTrustDialog({} as any);
      }).to.throw(/certificate must be an object/);
        dialog.showCertificateTrustDialog({ certificate: {} as any, message: false as any });
      }).to.throw(/message must be a string/);
  ifdescribe(process.platform === 'darwin' && !process.env.ELECTRON_SKIP_NATIVE_MODULE_TESTS)('end-to-end dialog interaction (macOS)', () => {
    let dialogHelper: any;
      dialogHelper = require('@electron-ci/dialog-helper');
    // Poll for a sheet to appear on the given window.
    async function waitForSheet (w: BrowserWindow): Promise<void> {
      const handle = w.getNativeWindowHandle();
      for (let i = 0; i < 50; i++) {
        const info = dialogHelper.getDialogInfo(handle);
        if (info.type !== 'none') return;
        await setTimeout(100);
      throw new Error('Timed out waiting for dialog sheet to appear');
      it('shows the correct message and buttons', async () => {
          buttons: ['OK', 'Cancel']
        await waitForSheet(w);
        expect(info.type).to.equal('message-box');
        expect(info.message).to.equal('Test message');
        const buttons = JSON.parse(info.buttons);
        expect(buttons).to.include('OK');
        expect(buttons).to.include('Cancel');
        dialogHelper.clickMessageBoxButton(handle, 0);
      it('shows detail text', async () => {
          message: 'Main message',
          detail: 'Extra detail text',
        expect(info.message).to.equal('Main message');
        expect(info.detail).to.equal('Extra detail text');
      it('returns the correct response when a specific button is clicked', async () => {
          message: 'Choose a button',
          buttons: ['First', 'Second', 'Third']
        dialogHelper.clickMessageBoxButton(handle, 1);
      it('returns the correct response when the last button is clicked', async () => {
          buttons: ['Yes', 'No', 'Maybe']
        dialogHelper.clickMessageBoxButton(handle, 2);
        expect(result.response).to.equal(2);
      it('shows a single button when no buttons are specified', async () => {
          message: 'No buttons specified'
        // macOS adds a default "OK" button when none are specified.
        expect(buttons).to.have.lengthOf(1);
      it('renders checkbox with the correct label and initial state', async () => {
          message: 'Checkbox test',
          checkboxLabel: 'Do not show again',
          checkboxChecked: false
        expect(info.checkboxLabel).to.equal('Do not show again');
        expect(info.checkboxChecked).to.be.false();
        expect(result.checkboxChecked).to.be.false();
      it('returns checkboxChecked as true when checkbox is initially checked', async () => {
          message: 'Pre-checked checkbox',
          checkboxLabel: 'Remember my choice',
          checkboxChecked: true
        expect(info.checkboxLabel).to.equal('Remember my choice');
        expect(info.checkboxChecked).to.be.true();
        expect(result.checkboxChecked).to.be.true();
      it('can toggle checkbox and returns updated state', async () => {
          message: 'Toggle test',
          checkboxLabel: 'Toggle me',
        // Verify initially unchecked.
        let info = dialogHelper.getDialogInfo(handle);
        // Click the checkbox to check it.
        dialogHelper.clickCheckbox(handle);
        info = dialogHelper.getDialogInfo(handle);
      it('strips access keys on macOS with normalizeAccessKeys', async () => {
          message: 'Access key test',
          buttons: ['&Save', '&Cancel'],
          normalizeAccessKeys: true
        // On macOS, ampersands are stripped by normalizeAccessKeys.
        expect(buttons).to.include('Save');
        expect(buttons).not.to.include('&Save');
        expect(buttons).not.to.include('&Cancel');
      it('respects defaultId by making it the default button', async () => {
          message: 'Default button test',
          buttons: ['One', 'Two', 'Three'],
          defaultId: 2
        expect(buttons).to.deep.equal(['One', 'Two', 'Three']);
      it('respects cancelId and returns it when cancelled via signal', async () => {
          message: 'Cancel ID test',
          buttons: ['OK', 'Dismiss', 'Abort'],
          cancelId: 2,
      it('works with all message box types', async () => {
        const types: Array<'none' | 'info' | 'warning' | 'error' | 'question'> =
          ['none', 'info', 'warning', 'error', 'question'];
            message: `Type: ${type}`,
          expect(info.message).to.equal(`Type: ${type}`);
          // Allow the event loop to settle between iterations to avoid
          // Chromium DCHECK failures from rapid window lifecycle churn.
      it('can cancel an open dialog', async () => {
        const p = dialog.showOpenDialog(w, {
          title: 'Test Open',
        expect(info.type).to.equal('open-dialog');
        dialogHelper.cancelFileDialog(handle);
        expect(result.canceled).to.be.true();
        expect(result.filePaths).to.have.lengthOf(0);
      it('sets a custom button label', async () => {
          buttonLabel: 'Select This',
        expect(info.prompt).to.equal('Select This');
      it('sets a message on the dialog', async () => {
          message: 'Choose a file to import',
        expect(info.panelMessage).to.equal('Choose a file to import');
      it('defaults to openFile with canChooseFiles enabled', async () => {
        expect(info.canChooseFiles).to.be.true();
        expect(info.canChooseDirectories).to.be.false();
        expect(info.allowsMultipleSelection).to.be.false();
      it('enables directory selection with openDirectory', async () => {
          properties: ['openDirectory']
        expect(info.canChooseDirectories).to.be.true();
        // openFile is not set, so canChooseFiles should be false
        expect(info.canChooseFiles).to.be.false();
      it('enables both file and directory selection together', async () => {
      it('enables multiple selection with multiSelections', async () => {
          properties: ['openFile', 'multiSelections']
        expect(info.allowsMultipleSelection).to.be.true();
      it('shows hidden files with showHiddenFiles', async () => {
          properties: ['openFile', 'showHiddenFiles']
        expect(info.showsHiddenFiles).to.be.true();
      it('does not show hidden files by default', async () => {
        expect(info.showsHiddenFiles).to.be.false();
      it('disables alias resolution with noResolveAliases', async () => {
          properties: ['openFile', 'noResolveAliases']
        expect(info.resolvesAliases).to.be.false();
      it('resolves aliases by default', async () => {
        expect(info.resolvesAliases).to.be.true();
      it('treats packages as directories with treatPackageAsDirectory', async () => {
          properties: ['openFile', 'treatPackageAsDirectory']
        expect(info.treatsPackagesAsDirectories).to.be.true();
      it('enables directory creation with createDirectory', async () => {
          properties: ['openFile', 'createDirectory']
        expect(info.canCreateDirectories).to.be.true();
      it('sets the default path directory', async () => {
        const defaultDir = path.join(__dirname, 'fixtures');
          defaultPath: defaultDir,
        expect(info.directory).to.equal(defaultDir);
      it('applies multiple properties simultaneously', async () => {
          title: 'Multi-Property Test',
          buttonLabel: 'Pick',
          message: 'Select items',
            'openFile',
            'openDirectory',
            'multiSelections',
            'showHiddenFiles',
            'createDirectory',
            'treatPackageAsDirectory',
            'noResolveAliases'
        expect(info.prompt).to.equal('Pick');
        expect(info.panelMessage).to.equal('Select items');
      it('can accept an open dialog and return a file path', async () => {
        const targetDir = path.join(__dirname, 'fixtures');
          defaultPath: targetDir,
        dialogHelper.acceptFileDialog(handle);
        expect(result.canceled).to.be.false();
        expect(result.filePaths).to.have.lengthOf(1);
        expect(result.filePaths[0]).to.equal(targetDir);
      it('can cancel a save dialog', async () => {
        const p = dialog.showSaveDialog(w, {
          title: 'Test Save'
        expect(info.type).to.equal('save-dialog');
        expect(result.filePath).to.equal('');
      it('can accept a save dialog with a filename', async () => {
        const filename = 'test-save-output.txt';
          title: 'Test Save',
          defaultPath: path.join(defaultDir, filename)
        expect(result.filePath).to.equal(path.join(defaultDir, filename));
          buttonLabel: 'Export'
        expect(info.prompt).to.equal('Export');
          message: 'Choose where to save'
        expect(info.panelMessage).to.equal('Choose where to save');
      it('sets a custom name field label', async () => {
          nameFieldLabel: 'Export As:'
        expect(info.nameFieldLabel).to.equal('Export As:');
      it('sets the default filename from defaultPath', async () => {
          defaultPath: path.join(__dirname, 'fixtures', 'my-document.txt')
        expect(info.nameFieldValue).to.equal('my-document.txt');
      it('sets the default directory from defaultPath', async () => {
          defaultPath: path.join(defaultDir, 'some-file.txt')
      it('hides the tag field when showsTagField is false', async () => {
          showsTagField: false
        expect(info.showsTagField).to.be.false();
      it('shows the tag field by default', async () => {
        const p = dialog.showSaveDialog(w, {});
        expect(info.showsTagField).to.be.true();
          properties: ['createDirectory']
          properties: ['showHiddenFiles']
          properties: ['treatPackageAsDirectory']
      it('applies multiple options simultaneously', async () => {
          buttonLabel: 'Save Now',
          message: 'Pick a location',
          nameFieldLabel: 'File Name:',
          defaultPath: path.join(defaultDir, 'output.txt'),
          showsTagField: false,
          properties: ['showHiddenFiles', 'createDirectory']
        expect(info.prompt).to.equal('Save Now');
        expect(info.panelMessage).to.equal('Pick a location');
        expect(info.nameFieldLabel).to.equal('File Name:');
        expect(info.nameFieldValue).to.equal('output.txt');
