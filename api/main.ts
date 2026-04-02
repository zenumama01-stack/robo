import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
 * Main entry point for the Angular Elements demo application.
 * This file bootstraps the Angular application using the AppModule.
 * When the application is built for production with Angular Elements,
 * the output will be bundled into a single JavaScript file that can
 * be used in any HTML page.
 * @see AppModule for the component registration and custom element definitions.
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';
// Bootstrap the Angular application
platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error('Error bootstrapping the application:', err));
async function initAndBootstrap() {
    .then(ref => {
      //LogStatus('Bootstrap success: ' + ref);
    .catch(err => console.error(err));
initAndBootstrap();
import * as electron from 'electron/main';
import * as fs from 'node:fs';
import { Module } from 'node:module';
const { app, dialog } = electron;
type DefaultAppOptions = {
  file: null | string;
  noHelp: boolean;
  version: boolean;
  webdriver: boolean;
  interactive: boolean;
  abi: boolean;
  modules: string[];
// Parse command line options.
const argv = process.argv.slice(1);
const option: DefaultAppOptions = {
  file: null,
  noHelp: Boolean(process.env.ELECTRON_NO_HELP),
  version: false,
  webdriver: false,
  interactive: false,
  abi: false,
  modules: []
let nextArgIsRequire = false;
for (const arg of argv) {
  if (nextArgIsRequire) {
    option.modules.push(arg);
    nextArgIsRequire = false;
  } else if (arg === '--version' || arg === '-v') {
    option.version = true;
  } else if (arg.match(/^--app=/)) {
    option.file = arg.split('=')[1];
  } else if (arg === '--interactive' || arg === '-i' || arg === '-repl') {
    option.interactive = true;
  } else if (arg === '--test-type=webdriver') {
    option.webdriver = true;
  } else if (arg === '--require' || arg === '-r') {
    nextArgIsRequire = true;
  } else if (arg === '--abi' || arg === '-a') {
    option.abi = true;
  } else if (arg === '--no-help') {
    option.noHelp = true;
  } else if (arg[0] === '-') {
    option.file = arg;
  console.error('Invalid Usage: --require [file]\n\n"file" is required');
// Set up preload modules
if (option.modules.length > 0) {
  (Module as any)._preloadModules(option.modules);
async function loadApplicationPackage (packagePath: string) {
  // Add a flag indicating app is started from default app.
  Object.defineProperty(process, 'defaultApp', {
    configurable: false,
    enumerable: true,
    // Override app's package.json data.
    packagePath = path.resolve(packagePath);
    const packageJsonPath = path.join(packagePath, 'package.json');
    let appPath;
      let packageJson;
      const emitWarning = process.emitWarning;
        process.emitWarning = () => {};
        packageJson = (await import(url.pathToFileURL(packageJsonPath).toString(), {
          with: { type: 'json' }
        })).default;
        showErrorMessage(`Unable to parse ${packageJsonPath}\n\n${(e as Error).message}`);
        process.emitWarning = emitWarning;
      if (packageJson.version) {
        app.setVersion(packageJson.version);
      if (packageJson.productName) {
        app.name = packageJson.productName;
      } else if (packageJson.name) {
        app.name = packageJson.name;
      // Set application's desktop name (Linux). These usually match the executable name,
      // so use it as the default to ensure the app gets the correct icon in the taskbar and application switcher.
      const desktopName = packageJson.desktopName || `${path.basename(process.execPath)}.desktop`;
      app.setDesktopName(desktopName);
      // Set v8 flags, deliberately lazy load so that apps that do not use this
      // feature do not pay the price
      if (packageJson.v8Flags) {
        (await import('node:v8')).setFlagsFromString(packageJson.v8Flags);
      appPath = packagePath;
    let filePath: string;
      filePath = (Module as any)._resolveFilename(packagePath, null, true);
      app.setAppPath(appPath || path.dirname(filePath));
      showErrorMessage(`Unable to find Electron app at ${packagePath}\n\n${(e as Error).message}`);
    // Run the app.
    await import(url.pathToFileURL(filePath).toString());
    console.error('App threw an error during load');
    console.error((e as Error).stack || e);
function showErrorMessage (message: string) {
  app.focus();
  dialog.showErrorBox('Error launching app', message);
async function loadApplicationByURL (appUrl: string) {
  const { loadURL } = await import('./default_app.js');
  loadURL(appUrl);
async function loadApplicationByFile (appPath: string) {
  const { loadFile } = await import('./default_app.js');
  loadFile(appPath);
async function startRepl () {
  if (process.platform === 'win32') {
    console.error('Electron REPL not currently supported on Windows');
  // Prevent quitting.
  app.on('window-all-closed', () => {});
  const GREEN = '32';
  const colorize = (color: string, s: string) => `\x1b[${color}m${s}\x1b[0m`;
  const electronVersion = colorize(GREEN, `v${process.versions.electron}`);
  const nodeVersion = colorize(GREEN, `v${process.versions.node}`);
  console.info(`
    Welcome to the Electron.js REPL \\[._.]/
    You can access all Electron.js modules here as well as Node.js modules.
    Using: Node.js ${nodeVersion} and Electron.js ${electronVersion}
  const { start } = await import('node:repl');
  const repl = start({
    prompt: '> '
  }).on('exit', () => {
  function defineBuiltin (context: any, name: string, getter: Function) {
    const setReal = (val: any) => {
      // Deleting the property before re-assigning it disables the
      // getter/setter mechanism.
      delete context[name];
      context[name] = val;
    Object.defineProperty(context, name, {
      get: () => {
        const lib = getter();
          get: () => lib,
          set: setReal,
          configurable: true,
          enumerable: false
        return lib;
  defineBuiltin(repl.context, 'electron', () => electron);
  for (const api of Object.keys(electron) as (keyof typeof electron)[]) {
    defineBuiltin(repl.context, api, () => electron[api]);
  // Copied from node/lib/repl.js. For better DX, we don't want to
  // show e.g 'contentTracing' at a higher priority than 'const', so
  // we only trigger custom tab-completion when no common words are
  // potentially matches.
  const commonWords = [
    'async', 'await', 'break', 'case', 'catch', 'const', 'continue',
    'debugger', 'default', 'delete', 'do', 'else', 'export', 'false',
    'finally', 'for', 'function', 'if', 'import', 'in', 'instanceof', 'let',
    'new', 'null', 'return', 'switch', 'this', 'throw', 'true', 'try',
    'typeof', 'var', 'void', 'while', 'with', 'yield'
  const electronBuiltins = [...Object.keys(electron), 'original-fs', 'electron'];
  const defaultComplete: Function = repl.completer;
  (repl as any).completer = (line: string, callback: Function) => {
    const lastSpace = line.lastIndexOf(' ');
    const currentSymbol = line.substring(lastSpace + 1, repl.cursor);
    const filterFn = (c: string) => c.startsWith(currentSymbol);
    const ignores = commonWords.filter(filterFn);
    const hits = electronBuiltins.filter(filterFn);
    if (!ignores.length && hits.length) {
      callback(null, [hits, currentSymbol]);
      defaultComplete.apply(repl, [line, callback]);
// Start the specified app if there is one specified in command line, otherwise
// start the default app.
if (option.file && !option.webdriver) {
  const file = option.file;
  // eslint-disable-next-line n/no-deprecated-api
  const protocol = URL.canParse(file) ? new URL(file).protocol : null;
  const extension = path.extname(file);
  if (protocol === 'http:' || protocol === 'https:' || protocol === 'file:' || protocol === 'chrome:') {
    await loadApplicationByURL(file);
  } else if (extension === '.html' || extension === '.htm') {
    await loadApplicationByFile(path.resolve(file));
    await loadApplicationPackage(file);
} else if (option.version) {
  console.log('v' + process.versions.electron);
} else if (option.abi) {
  console.log(process.versions.modules);
} else if (option.interactive) {
  await startRepl();
  if (!option.noHelp) {
    const welcomeMessage = `
Electron ${process.versions.electron} - Build cross platform desktop apps with JavaScript, HTML, and CSS
Usage: electron [options] [path]
A path to an Electron app may be specified. It must be one of the following:
  - index.js file.
  - Folder containing a package.json file.
  - Folder containing an index.js file.
  - .html/.htm file.
  - http://, https://, or file:// URL.
Options:
  -i, --interactive     Open a REPL to the main process.
  -r, --require         Module to preload (option can be repeated).
  -v, --version         Print the version.
  -a, --abi             Print the Node ABI version.`;
    console.log(welcomeMessage);
  await loadApplicationByFile('index.html');
/* eslint-disable */
  autoUpdater,
  contentTracing,
  dialog,
  desktopCapturer,
  globalShortcut,
  ipcMain,
  Menu,
  MenuItem,
  net,
  powerMonitor,
  powerSaveBlocker,
  protocol,
  Tray,
  screen,
  systemPreferences,
  webContents,
  TouchBar
} from 'electron/main';
import { clipboard, crashReporter, nativeImage, shell } from 'electron/common';
// Quick start
// https://github.com/electron/electron/blob/main/docs/tutorial/quick-start.md
// be closed automatically when the javascript object is GCed.
let mainWindow: Electron.BrowserWindow = null;
// Check single instance app
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
// This method will be called when Electron has done everything
// initialization and ready for creating browser windows.
  mainWindow = new BrowserWindow({ width: 800, height: 600 });
  mainWindow.loadURL(`file://${__dirname}/index.html`);
  mainWindow.loadURL('file://foo/bar', { userAgent: 'cool-agent', httpReferrer: 'greatReferrer' });
  mainWindow.webContents.loadURL('file://foo/bar', { userAgent: 'cool-agent', httpReferrer: 'greatReferrer' });
  mainWindow.webContents.loadURL('file://foo/bar', { userAgent: 'cool-agent', httpReferrer: 'greatReferrer', postData: [{ type: 'rawData', bytes: Buffer.from([123]) }] });
  mainWindow.webContents.openDevTools();
  mainWindow.webContents.toggleDevTools();
  mainWindow.webContents.openDevTools({ mode: 'detach' });
  mainWindow.webContents.closeDevTools();
  mainWindow.webContents.addWorkSpace('/path/to/workspace');
  mainWindow.webContents.removeWorkSpace('/path/to/workspace');
  const opened = mainWindow.webContents.isDevToolsOpened();
  console.log('isDevToolsOpened', opened);
  const focused = mainWindow.webContents.isDevToolsFocused();
  console.log('isDevToolsFocused', focused);
    mainWindow = null;
  mainWindow.webContents.setVisualZoomLevelLimits(50, 200);
  mainWindow.webContents.print({ silent: true, printBackground: false });
  mainWindow.webContents.print();
  mainWindow.webContents.printToPDF({
      top: 1
    printBackground: true,
    landscape: true,
  }).then((data: Buffer) => console.log(data));
  mainWindow.webContents.printToPDF({}).then(data => console.log(data));
  mainWindow.webContents.executeJavaScript('return true;').then((v: boolean) => console.log(v));
  mainWindow.webContents.executeJavaScript('return true;', true).then((v: boolean) => console.log(v));
  mainWindow.webContents.executeJavaScript('return true;', true);
  mainWindow.webContents.executeJavaScript('return true;', true).then((result: boolean) => console.log(result));
  mainWindow.webContents.insertText('blah, blah, blah');
  mainWindow.webContents.startDrag({ file: '/path/to/img.png', icon: nativeImage.createFromPath('/path/to/icon.png') });
  mainWindow.webContents.findInPage('blah');
  mainWindow.webContents.findInPage('blah', {
    forward: true,
    matchCase: false
  mainWindow.webContents.stopFindInPage('clearSelection');
  mainWindow.webContents.stopFindInPage('keepSelection');
  mainWindow.webContents.stopFindInPage('activateSelection');
  mainWindow.loadURL('https://github.com');
  mainWindow.webContents.on('did-finish-load', function () {
    mainWindow.webContents.savePage('/tmp/test.html', 'HTMLComplete').then(() => {
      console.log('Page saved successfully');
    mainWindow.webContents.debugger.attach('1.1');
    console.log('Debugger attach failed : ', err);
  mainWindow.webContents.debugger.on('detach', function (event, reason) {
    console.log('Debugger detached due to : ', reason);
  mainWindow.webContents.debugger.on('message', function (event, method, params: any) {
    if (method === 'Network.requestWillBeSent') {
      if (params.request.url === 'https://www.github.com') {
        mainWindow.webContents.debugger.detach();
  mainWindow.webContents.debugger.sendCommand('Network.enable');
  mainWindow.webContents.capturePage().then(image => {
    console.log(image.toDataURL());
  mainWindow.webContents.capturePage({ x: 0, y: 0, width: 100, height: 200 }).then(image => {
    console.log(image.toPNG());
app.commandLine.appendSwitch('enable-web-bluetooth');
    const result = (() => {
      for (const device of deviceList) {
        if (device.deviceName === 'test') {
          return device;
      callback(result.deviceId);
// Locale
app.getLocale();
// Desktop environment integration
app.addRecentDocument('/Users/USERNAME/Desktop/work.type');
  <Electron.MenuItemConstructorOptions> {
      console.log('New Window');
    label: 'New Window with Settings',
      <Electron.MenuItemConstructorOptions> { label: 'Basic' },
      <Electron.MenuItemConstructorOptions> { label: 'Pro' }
  <Electron.MenuItemConstructorOptions> { label: 'New Command...' },
        accelerator: 'CmdOrCtrl+Z',
        role: 'undo'
        accelerator: 'Shift+CmdOrCtrl+Z',
        role: 'redo'
        accelerator: 'CmdOrCtrl+X',
        role: 'cut'
        accelerator: 'CmdOrCtrl+C',
        role: 'copy'
        accelerator: 'CmdOrCtrl+V',
        role: 'paste'
app.dock?.setMenu(dockMenu);
app.dock?.setBadge('foo');
const dockid = app.dock?.bounce('informational');
app.dock?.cancelBounce(dockid);
app.dock?.setIcon('/path/to/icon.png');
app.setBadgeCount(app.getBadgeCount() + 1);
app.setUserTasks([
  <Electron.Task> {
    arguments: '--new-window',
    iconPath: process.execPath,
    iconIndex: 0,
    title: 'New Window',
    description: 'Create a new window',
    workingDirectory: path.dirname(process.execPath)
app.setUserTasks([]);
    type: 'custom',
    name: 'Recent Projects',
      { type: 'file', path: 'C:\\Projects\\project1.proj' },
      { type: 'file', path: 'C:\\Projects\\project2.proj' }
  { // has a name so type is assumed to be "custom"
    name: 'Tools',
        title: 'Tool A',
        args: '--run-tool-a',
        description: 'Runs Tool A',
        title: 'Tool B',
        args: '--run-tool-b',
        description: 'Runs Tool B',
    type: 'frequent'
  { // has no name and no type so type is assumed to be "tasks"
if (app.isUnityRunning()) {
  console.log('unity running');
if (app.isAccessibilitySupportEnabled()) {
  console.log('a11y running');
console.log(app.getLoginItemSettings().wasOpenedAtLogin);
  applicationName: 'Test',
// Online/Offline Event Detection
// https://github.com/electron/electron/blob/main/docs/tutorial/online-offline-events.md
let onlineStatusWindow: Electron.BrowserWindow;
  onlineStatusWindow = new BrowserWindow({ width: 0, height: 0, show: false, vibrancy: 'sidebar' });
  onlineStatusWindow.loadURL(`file://${__dirname}/online-status.html`);
app.on('accessibility-support-changed', (_, enabled) => console.log('accessibility: ' + enabled));
ipcMain.on('online-status-changed', (event, status: any) => {
  console.log(status);
  window.loadURL('https://github.com');
// Supported command line switches
// https://github.com/electron/electron/blob/main/docs/api/command-line-switches.md
app.commandLine.appendSwitch('remote-debugging-port', '8315');
app.commandLine.appendSwitch('host-resolver-rules', 'MAP * 127.0.0.1');
app.commandLine.appendSwitch('vmodule', 'console=0');
// systemPreferences
// https://github.com/electron/electron/blob/main/docs/api/system-preferences.md
const browserOptions = {
  systemPreferences.on('color-changed', () => { console.log('color changed'); });
  // @ts-expect-error Removed API
  systemPreferences.on('inverted-color-scheme-changed', (_, inverted) => console.log(inverted ? 'inverted' : 'not inverted'));
  systemPreferences.on('high-contrast-color-scheme-changed', (_, highContrast) => console.log(highContrast ? 'high contrast' : 'not high contrast'));
  console.log('Color for menu is', systemPreferences.getColor('menu'));
  systemPreferences.isAeroGlassEnabled();
  const value = systemPreferences.getUserDefault('Foo', 'string');
  console.log(value);
  const value2 = systemPreferences.getUserDefault('Foo', 'boolean');
  console.log(value2);
  console.log(systemPreferences.getAppLevelAppearance());
  systemPreferences.setAppLevelAppearance('dark');
  console.log(systemPreferences.appLevelAppearance);
  console.log(systemPreferences.getColor('alternate-selected-control-text'));
// Create the window.
const win1 = new BrowserWindow(browserOptions);
// Navigate.
if (browserOptions.transparent) {
  win1.loadURL(`file://${__dirname}/index.html`);
  // No transparency, so we load a fallback that uses basic styles.
  win1.loadURL(`file://${__dirname}/fallback.html`);
// app
// https://github.com/electron/electron/blob/main/docs/api/app.md
app.on('certificate-error', function (event, webContents, url, error, certificate, callback) {
  if (url === 'https://github.com') {
    // Verification logic.
app.on('select-client-certificate', function (event, webContents, url, list, callback) {
  callback(list[0]);
app.on('login', function (event, webContents, request, authInfo, callback) {
  callback('username', 'secret');
const win2 = new BrowserWindow({ show: false });
win2.once('ready-to-show', () => {
  win2.show();
app.relaunch({ args: process.argv.slice(1).concat(['--relaunch']) });
app.configureHostResolver({ secureDnsMode: 'off' });
// @ts-expect-error Invalid type value
app.configureHostResolver({ secureDnsMode: 'foo' });
console.log(app.runningUnderRosettaTranslation);
app.on('gpu-process-crashed', () => {});
app.on('renderer-process-crashed', () => {});
// auto-updater
// https://github.com/electron/electron/blob/main/docs/api/auto-updater.md
  url: 'http://mycompany.com/myapp/latest?version=' + app.getVersion(),
    key: 'value'
  serverType: 'default'
autoUpdater.on('error', (error) => {
autoUpdater.on('update-downloaded', (event, releaseNotes, releaseName, releaseDate, updateURL) => {
  console.log('update-downloaded', releaseNotes, releaseName, releaseDate, updateURL);
// BrowserWindow
// https://github.com/electron/electron/blob/main/docs/api/browser-window.md
let win3 = new BrowserWindow({ width: 800, height: 600, show: false });
win3.on('closed', () => {
  win3 = null;
win3.loadURL('https://github.com');
win3.show();
const toolbarRect = document.getElementById('toolbar').getBoundingClientRect();
win3.setSheetOffset(toolbarRect.height);
let window = new BrowserWindow();
window.setProgressBar(0.5);
window.setRepresentedFilename('/etc/passwd');
window.setDocumentEdited(true);
window.previewFile('/path/to/file');
window.previewFile('/path/to/file', 'Displayed Name');
window.setVibrancy('menu');
window.setVibrancy('titlebar');
window.setVibrancy('selection');
window.setVibrancy('popover');
window.setIcon('/path/to/icon');
// content-tracing
// https://github.com/electron/electron/blob/main/docs/api/content-tracing.md
contentTracing.startRecording(options).then(() => {
  console.log('Tracing started');
    contentTracing.stopRecording('').then(path => {
      console.log(`Tracing data recorded to ${path}`);
// dialog
// https://github.com/electron/electron/blob/main/docs/api/dialog.md
// variant without browserWindow
dialog.showOpenDialogSync({
  title: 'Testing showOpenDialog',
  defaultPath: '/var/log/syslog',
  filters: [{ name: '', extensions: [''] }],
  properties: ['openFile', 'openDirectory', 'multiSelections']
// variant with browserWindow
dialog.showOpenDialog(win3, {
}).then(ret => {
  console.log(ret);
// variants without browserWindow
dialog.showMessageBox({ message: 'test', type: 'warning' });
dialog.showMessageBoxSync({ message: 'test', type: 'error' });
dialog.showMessageBox({ message: 'test', type: 'foo' });
dialog.showMessageBoxSync({ message: 'test', type: 'foo' });
// variants with browserWindow
dialog.showMessageBox(win3, { message: 'test', type: 'question' });
dialog.showMessageBoxSync(win3, { message: 'test', type: 'info' });
dialog.showMessageBox(win3, { message: 'test', type: 'foo' });
dialog.showMessageBoxSync(win3, { message: 'test', type: 'foo' });
// desktopCapturer
// https://github.com/electron/electron/blob/main/docs/api/desktop-capturer.md
ipcMain.handle('get-sources', (event, options) => desktopCapturer.getSources(options));
desktopCapturer.getSources({ types: ['window', 'screen'] });
desktopCapturer.getSources({ types: ['unknown'] });
// global-shortcut
// https://github.com/electron/electron/blob/main/docs/api/global-shortcut.md
// Register a 'ctrl+x' shortcut listener.
const ret = globalShortcut.register('ctrl+x', () => {
  console.log('ctrl+x is pressed');
if (!ret) { console.log('registration fails'); }
// Check whether a shortcut is registered.
console.log(globalShortcut.isRegistered('ctrl+x'));
// Unregister a shortcut.
globalShortcut.unregister('ctrl+x');
// Unregister all shortcuts.
// ipcMain
// https://github.com/electron/electron/blob/main/docs/api/ipc-main.md
ipcMain.handle('ping-pong', (event, arg: any) => {
  console.log(arg); // prints "ping"
  return 'pong';
ipcMain.on('asynchronous-message', (event, arg: any) => {
  event.sender.send('asynchronous-reply', 'pong');
ipcMain.on('synchronous-message', (event, arg: any) => {
  event.returnValue = 'pong';
const winWindows = new BrowserWindow({
  thickFrame: false,
  type: 'toolbar'
console.log(winWindows.id);
// menu-item
// https://github.com/electron/electron/blob/main/docs/api/menu-item.md
const menuItem = new MenuItem({});
menuItem.label = 'Hello World!';
menuItem.click = (passedMenuItem: Electron.MenuItem, browserWindow: Electron.BrowserWindow) => {
  console.log('click', passedMenuItem, browserWindow);
// menu
// https://github.com/electron/electron/blob/main/docs/api/menu.md
let menu = new Menu();
menu.append(new MenuItem({ label: 'MenuItem1', click: () => { console.log('item 1 clicked'); } }));
menu.append(new MenuItem({ type: 'separator' }));
menu.append(new MenuItem({ label: 'MenuItem2', type: 'checkbox', checked: true }));
menu.insert(0, menuItem);
console.log(menu.items);
const pos = screen.getCursorScreenPoint();
menu.popup({ x: pos.x, y: pos.y });
// main.js
const template = <Electron.MenuItemConstructorOptions[]> [
    label: 'Electron',
        label: 'About Electron',
        label: 'Services',
        role: 'services',
        label: 'Hide Electron',
        accelerator: 'Command+H',
        role: 'hide'
        accelerator: 'Command+Shift+H',
        role: 'hideothers'
        label: 'Show All',
        role: 'unhide'
        label: 'Quit',
        accelerator: 'Command+Q',
        click: () => { app.quit(); }
        accelerator: 'Command+Z',
        accelerator: 'Shift+Command+Z',
        accelerator: 'Command+X',
        accelerator: 'Command+C',
        accelerator: 'Command+V',
        accelerator: 'Command+A',
        role: 'selectall'
        accelerator: 'Command+R',
        click: (item, focusedWindow) => {
          if (focusedWindow instanceof BrowserWindow) {
            focusedWindow.webContents.reloadIgnoringCache();
        label: 'Toggle DevTools',
        accelerator: 'Alt+Command+I',
            focusedWindow.webContents.toggleDevTools();
        accelerator: 'CmdOrCtrl+0',
            focusedWindow.webContents.zoomLevel = 0;
        accelerator: 'CmdOrCtrl+Plus',
            const { webContents } = focusedWindow;
        accelerator: 'CmdOrCtrl+-',
        accelerator: 'Command+M',
        label: 'Close',
        accelerator: 'Command+W',
        label: 'Bring All to Front',
        role: 'front'
    label: 'Help',
menu = Menu.buildFromTemplate(template);
Menu.setApplicationMenu(menu); // Must be called within app.whenReady().then(function(){ ... });
  { label: '4', id: '4' },
  { label: '5', id: '5', after: ['4'] },
  { label: '1', id: '1', before: ['4'] },
  { label: '2', id: '2' },
  { label: '3', id: '3' }
  { label: 'a' },
  { label: 'b' },
  { label: 'c' },
// All possible MenuItem roles
  { role: 'togglefullscreen' },
  { role: 'window' },
  { role: 'close' },
  { role: 'help' },
  { role: 'quit' },
  { role: 'stopSpeaking' },
  { role: 'front' },
  { role: 'appMenu' },
  { role: 'recentDocuments' },
  { role: 'clearRecentDocuments' },
  { role: 'toggleTabBar' },
  { role: 'selectNextTab' },
  { role: 'selectPreviousTab' },
  { role: 'showAllTabs' },
  { role: 'mergeAllWindows' },
  { role: 'moveTabToNewWindow' }
// net
// https://github.com/electron/electron/blob/main/docs/api/net.md
  const request = net.request('https://github.com');
  request.setHeader('Some-Custom-Header-Name', 'Some-Custom-Header-Value');
  const header = request.getHeader('Some-Custom-Header-Name');
  console.log('header', header);
  request.removeHeader('Some-Custom-Header-Name');
    console.log(`Status code: ${response.statusCode}`);
    console.log(`Status message: ${response.statusMessage}`);
    console.log(`Headers: ${JSON.stringify(response.headers)}`);
    console.log(`Http version: ${response.httpVersion}`);
    console.log(`Major Http version: ${response.httpVersionMajor}`);
    console.log(`Minor Http version: ${response.httpVersionMinor}`);
      console.log(`BODY: ${chunk}`);
      console.log('No more data in response.');
      console.log('"error" event emitted');
    response.on('aborted', () => {
      console.log('"aborted" event emitted');
  request.on('login', (authInfo, callback) => {
    callback('username', 'password');
  request.on('finish', () => {
    console.log('"finish" event emitted');
  request.on('abort', () => {
    console.log('"abort" event emitted');
  request.on('error', () => {
  request.write('Hello World!', 'utf-8');
  request.end('Hello World!', 'utf-8');
  request.abort();
// power-monitor
// https://github.com/electron/electron/blob/main/docs/api/power-monitor.md
  powerMonitor.on('suspend', () => {
    console.log('The system is going to sleep');
  powerMonitor.on('resume', () => {
    console.log('The system has resumed from sleep');
  powerMonitor.on('on-ac', () => {
    console.log('The system changed to AC power');
  powerMonitor.on('on-battery', () => {
    console.log('The system changed to battery power');
// power-save-blocker
// https://github.com/electron/electron/blob/main/docs/api/power-save-blocker.md
const id = powerSaveBlocker.start('prevent-display-sleep');
console.log(powerSaveBlocker.isStarted(id));
const stopped = powerSaveBlocker.stop(id);
console.log(`The powerSaveBlocker is ${stopped ? 'stopped' : 'not stopped'}`);
// protocol
// https://github.com/electron/electron/blob/main/docs/api/protocol.md
  protocol.registerSchemesAsPrivileged([{ scheme: 'https', privileges: { standard: true, allowServiceWorkers: true } }]);
  protocol.registerFileProtocol('atom', (request, callback) => {
    callback(`${__dirname}/${request.url}`);
  protocol.registerBufferProtocol('atom', (request, callback) => {
    callback({ mimeType: 'text/html', data: Buffer.from('<h5>Response</h5>') });
  protocol.registerStringProtocol('atom', (request, callback) => {
  protocol.registerHttpProtocol('atom', (request, callback) => {
    callback({ url: request.url, method: request.method });
  protocol.unregisterProtocol('atom');
  const registered = protocol.isProtocolRegistered('atom');
  console.log('isProtocolRegistered', registered);
// tray
// https://github.com/electron/electron/blob/main/docs/api/tray.md
let appIcon: Electron.Tray = null;
  appIcon = new Tray('/path/to/my/icon');
    { label: 'Item1', type: 'radio' },
    { label: 'Item2', type: 'radio' },
    { label: 'Item3', type: 'radio', checked: true },
    { label: 'Item4', type: 'radio' }
  appIcon.setTitle('title');
  appIcon.setToolTip('This is my application.');
  appIcon.setImage('/path/to/new/icon');
  appIcon.setPressedImage('/path/to/new/icon');
  appIcon.popUpContextMenu(contextMenu, { x: 100, y: 100 });
  appIcon.setContextMenu(contextMenu);
  appIcon.setIgnoreDoubleClickEvents(true);
  appIcon.on('click', (event, bounds) => {
    console.log('click', event, bounds);
  appIcon.on('balloon-show', () => {
    console.log('balloon-show');
  appIcon.displayBalloon({
    title: 'Hello World!',
    content: 'This is the balloon content.',
    iconType: 'error',
    icon: 'path/to/icon',
    respectQuietTime: true,
    largeIcon: true,
    noSound: true
// clipboard
// https://github.com/electron/electron/blob/main/docs/api/clipboard.md
clipboard.writeText('Example String');
clipboard.writeText('Example String', 'selection');
clipboard.writeBookmark('foo', 'http://example.com');
clipboard.writeBookmark('foo', 'http://example.com', 'selection');
clipboard.writeFindText('foo');
console.log(clipboard.readText('selection'));
console.log(clipboard.readFindText());
console.log(clipboard.availableFormats());
console.log(clipboard.readBookmark().title);
  html: '<html></html>',
  text: 'Hello World!',
  image: clipboard.readImage()
// crash-reporter
// https://github.com/electron/electron/blob/main/docs/api/crash-reporter.md
  productName: 'YourName',
  companyName: 'YourCompany',
  submitURL: 'https://your-domain.com/url-to-submit',
  uploadToServer: true,
    someKey: 'value'
console.log(crashReporter.getLastCrashReport());
console.log(crashReporter.getUploadedReports());
// nativeImage
// https://github.com/electron/electron/blob/main/docs/api/native-image.md
const appIcon2 = new Tray('/Users/somebody/images/icon.png');
appIcon2.destroy();
const window2 = new BrowserWindow({ icon: '/Users/somebody/images/window.png' });
console.log(window2.id);
const image = clipboard.readImage();
console.log(image.getSize());
const appIcon3 = new Tray(image);
appIcon3.destroy();
const appIcon4 = new Tray('/Users/somebody/images/icon.png');
appIcon4.destroy();
const image2 = nativeImage.createFromPath('/Users/somebody/images/icon.png');
console.log(image2.getSize());
image2.resize({ quality: 'best' });
image2.resize({ quality: 'better' });
image2.resize({ quality: 'good' });
image2.resize({ quality: 'bad' });
// process
// https://github.com/electron/electron/blob/main/docs/api/process.md
console.log(process.versions.electron);
console.log(process.versions.chrome);
console.log(process.type);
console.log(process.resourcesPath);
console.log(process.mas);
console.log(process.windowsStore);
process.hang();
process.setFdLimit(8192);
// screen
// https://github.com/electron/electron/blob/main/docs/api/screen.md
  const size = screen.getPrimaryDisplay().workAreaSize;
  mainWindow = new BrowserWindow({ width: size.width, height: size.height });
  let externalDisplay: any = null;
  for (const i in displays) {
    if (displays[i].bounds.x > 0 || displays[i].bounds.y > 0) {
      externalDisplay = displays[i];
  if (externalDisplay) {
      x: externalDisplay.bounds.x + 50,
      y: externalDisplay.bounds.y + 50
  screen.on('display-added', (event, display) => {
    console.log('display-added', display);
  screen.on('display-removed', (event, display) => {
    console.log('display-removed', display);
  screen.on('display-metrics-changed', (event, display, changes) => {
    console.log('display-metrics-changed', display, changes);
// shell
// https://github.com/electron/electron/blob/main/docs/api/shell.md
shell.showItemInFolder('/home/user/Desktop/test.txt');
shell.trashItem('/home/user/Desktop/test.txt').then(() => {});
shell.openPath('/home/user/Desktop/test.txt').then(err => {
  if (err) console.log(err);
shell.openExternal('https://github.com', {
  activate: false
}).then(() => {});
shell.beep();
shell.writeShortcutLink('/home/user/Desktop/shortcut.lnk', 'update', shell.readShortcutLink('/home/user/Desktop/shortcut.lnk'));
// cookies
// https://github.com/electron/electron/blob/main/docs/api/cookies.md
  // Query all cookies.
  session.defaultSession.cookies.get({})
    .then(cookies => {
      console.log(cookies);
    }).catch((error: Error) => {
      console.log(error);
  // Query all cookies associated with a specific url.
  session.defaultSession.cookies.get({ url: 'http://www.github.com' })
  // Set a cookie with the given cookie data;
  // may overwrite equivalent cookies if they exist.
  const cookie = { url: 'http://www.github.com', name: 'dummy_name', value: 'dummy' };
  session.defaultSession.cookies.set(cookie)
      // success
    }, (error: Error) => {
// session
// https://github.com/electron/electron/blob/main/docs/api/session.md
session.defaultSession.clearStorageData({ storages: ['cookies', 'filesystem'] });
session.defaultSession.clearStorageData({ storages: ['localstorage', 'indexdb', 'serviceworkers'] });
session.defaultSession.clearStorageData({ storages: ['shadercache', 'cachestorage'] });
session.defaultSession.clearStorageData({ storages: ['wrong_path'] });
session.defaultSession.on('will-download', (event, item, webContents) => {
  console.log('will-download', webContents.id);
  require('got')(item.getURL()).then((data: any) => {
    require('node:fs').writeFileSync('/somewhere', data);
// In the main process.
  // Set the save path, making Electron not to prompt a save dialog.
  item.setSavePath('/tmp/save.pdf');
  console.log(item.getSavePath());
  console.log(item.getMimeType());
  console.log(item.getFilename());
  console.log(item.getTotalBytes());
  item.on('updated', (_event, state) => {
    if (state === 'interrupted') {
      console.log('Download is interrupted but can be resumed');
    } else if (state === 'progressing') {
      if (item.isPaused()) {
        console.log('Download is paused');
        console.log(`Received bytes: ${item.getReceivedBytes()}`);
    if (state === 'completed') {
      console.log('Download successfully');
      console.log(`Download failed: ${state}`);
// To emulate a GPRS connection with 50kbps throughput and 500 ms latency.
session.defaultSession.enableNetworkEmulation({
// To emulate a network outage.
  offline: true
session.defaultSession.setCertificateVerifyProc((request, callback) => {
  const { hostname } = request;
  if (hostname === 'github.com') {
session.defaultSession.setPermissionRequestHandler(function (webContents, permission, callback) {
  if (webContents.getURL() === 'github.com') {
    if (permission === 'notifications') {
// consider any url ending with `example.com`, `foobar.com`, `baz`
// for integrated authentication.
session.defaultSession.allowNTLMCredentialsForDomains('*example.com, *foobar.com, *baz');
// consider all urls for integrated authentication.
session.defaultSession.allowNTLMCredentialsForDomains('*');
// Modify the user agent for all requests to the following urls.
const filter = {
  urls: ['https://*.github.com/*', '*://electron.github.io']
session.defaultSession.webRequest.onBeforeSendHeaders(filter, function (details: any, callback: any) {
  details.requestHeaders['User-Agent'] = 'MyAgent';
  callback({ cancel: false, requestHeaders: details.requestHeaders });
  protocol.registerFileProtocol('atom', function (request, callback) {
    const url = request.url.substr(7);
    callback(path.normalize(`${__dirname}/${url}`));
// webContents
// https://github.com/electron/electron/blob/main/docs/api/web-contents.md
console.log(webContents.getAllWebContents());
console.log(webContents.getFocusedWebContents());
const win4 = new BrowserWindow({
win4.webContents.on('paint', (event, dirty, _image) => {
  console.log(dirty, _image.toBitmap());
win4.webContents.on('devtools-open-url', (event, url) => {
  console.log(url);
win4.webContents.insertCSS('body {}', { cssOrigin: 'user' });
win4.webContents.insertCSS('body {}', { cssOrigin: 'foo' });
win4.loadURL('http://github.com');
win4.webContents.getPrinters();
win4.webContents.on('scroll-touch-begin', () => {});
win4.webContents.on('scroll-touch-edge', () => {});
win4.webContents.on('scroll-touch-end', () => {});
win4.webContents.on('crashed', () => {});
win4.webContents.on('context-menu', (event, params) => {
  console.log(params.inputFieldType);
// TouchBar
// https://github.com/electron/electron/blob/main/docs/api/touch-bar.md
    new TouchBar.TouchBarButton({ label: '' }),
    new TouchBar.TouchBarLabel({ label: '' })
mainWindow.setTouchBar(touchBar);
import { existsSync, readFileSync } from "fs-extra"
import { AppUpdater } from "./AppUpdater"
import { UpdateInfo } from "builder-util-runtime"
export { BaseUpdater } from "./BaseUpdater"
export { AppUpdater, NoOpLogger } from "./AppUpdater"
export { Provider } from "./providers/Provider"
export { AppImageUpdater } from "./AppImageUpdater"
export { DebUpdater } from "./DebUpdater"
export { PacmanUpdater } from "./PacmanUpdater"
export { RpmUpdater } from "./RpmUpdater"
export { MacUpdater } from "./MacUpdater"
export { NsisUpdater } from "./NsisUpdater"
export * from "./types"
// autoUpdater to mimic electron bundled autoUpdater
let _autoUpdater: any
// required for jsdoc
export declare const autoUpdater: AppUpdater
function doLoadAutoUpdater(): AppUpdater {
  // tslint:disable:prefer-conditional-expression
    _autoUpdater = new (require("./NsisUpdater").NsisUpdater)()
  } else if (process.platform === "darwin") {
    _autoUpdater = new (require("./MacUpdater").MacUpdater)()
    _autoUpdater = new (require("./AppImageUpdater").AppImageUpdater)()
      const identity = path.join(process.resourcesPath, "package-type")
      if (!existsSync(identity)) {
        return _autoUpdater
      const fileType = readFileSync(identity).toString().trim()
      switch (fileType) {
        case "deb":
          _autoUpdater = new (require("./DebUpdater").DebUpdater)()
        case "rpm":
          _autoUpdater = new (require("./RpmUpdater").RpmUpdater)()
        case "pacman":
          _autoUpdater = new (require("./PacmanUpdater").PacmanUpdater)()
        "Unable to detect 'package-type' for autoUpdater (rpm/deb/pacman support). If you'd like to expand support, please consider contributing to electron-builder",
        error.message
Object.defineProperty(exports, "autoUpdater", {
    return _autoUpdater || doLoadAutoUpdater()
 * return null if verify signature succeed
 * return error message if verify signature failed
export type VerifyUpdateCodeSignature = (publisherName: string[], path: string) => Promise<string | null>
export type VerifyUpdateSupport = (updateInfo: UpdateInfo) => boolean | Promise<boolean>
