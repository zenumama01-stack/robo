import { IPC_MESSAGES } from '@electron/internal//common/ipc-messages';
import { ipcMainInternal } from '@electron/internal/browser/ipc-main-internal';
import * as ipcMainUtils from '@electron/internal/browser/ipc-main-internal-utils';
import { dialog, Menu } from 'electron/main';
const convertToMenuTemplate = function (items: ContextMenuItem[], handler: (id: number) => void) {
  return items.map(function (item) {
    const transformed: Electron.MenuItemConstructorOptions = item.type === 'subMenu'
          type: 'submenu',
          label: item.label,
          enabled: item.enabled,
          submenu: convertToMenuTemplate(item.subItems, handler)
      : item.type === 'separator'
            type: 'separator'
        : item.type === 'checkbox'
              checked: item.checked
          : {
              type: 'normal',
              enabled: item.enabled
    if (item.id != null) {
      transformed.click = () => handler(item.id);
const getEditMenuItems = function (): Electron.MenuItemConstructorOptions[] {
    { role: 'undo' },
    { role: 'redo' },
    { type: 'separator' },
    { role: 'pasteAndMatchStyle' },
    { role: 'delete' },
    { role: 'selectAll' }
const isChromeDevTools = function (pageURL: string) {
  const { protocol } = new URL(pageURL);
  return protocol === 'devtools:';
const assertChromeDevTools = function (contents: Electron.WebContents, api: string) {
  const pageURL = contents.getURL();
  if (!isChromeDevTools(pageURL)) {
    console.error(`Blocked ${pageURL} from calling ${api}`);
    throw new Error(`Blocked ${api}`);
ipcMainInternal.handle(IPC_MESSAGES.INSPECTOR_CONTEXT_MENU, function (event, items: ContextMenuItem[], isEditMenu: boolean) {
  return new Promise<number | void>(resolve => {
    if (event.type !== 'frame') return;
    assertChromeDevTools(event.sender, 'window.InspectorFrontendHost.showContextMenuAtPoint()');
    const template = isEditMenu ? getEditMenuItems() : convertToMenuTemplate(items, resolve);
    const window = event.sender.getOwnerBrowserWindow()!;
    menu.popup({ window, callback: () => resolve() });
ipcMainInternal.handle(IPC_MESSAGES.INSPECTOR_SELECT_FILE, async function (event) {
  if (event.type !== 'frame') return [];
  assertChromeDevTools(event.sender, 'window.UI.createFileSelectorElement()');
  const result = await dialog.showOpenDialog({});
  if (result.canceled) return [];
  const path = result.filePaths[0];
  const data = await fs.promises.readFile(path);
  return [path, data];
ipcMainUtils.handleSync(IPC_MESSAGES.INSPECTOR_CONFIRM, async function (event, message: string = '', title: string = '') {
  assertChromeDevTools(event.sender, 'window.confirm()');
    title: String(title),
  const { response } = await dialog.showMessageBox(window, options);
  return response === 0;
