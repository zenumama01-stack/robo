import { Menu } from 'electron/main';
const bindings = process._linkedBinding('electron_browser_app');
const commandLine = process._linkedBinding('electron_common_command_line');
const { app } = bindings;
Object.setPrototypeOf(app, EventEmitter.prototype);
// Only one app object permitted.
export default app;
let dockMenu: Electron.Menu | null = null;
// Properties.
const nativeASGetter = app.isAccessibilitySupportEnabled;
const nativeASSetter = app.setAccessibilitySupportEnabled;
Object.defineProperty(app, 'accessibilitySupportEnabled', {
  get: () => nativeASGetter.call(app),
  set: (enabled) => nativeASSetter.call(app, enabled)
const nativeBCGetter = app.getBadgeCount;
const nativeBCSetter = app.setBadgeCount;
Object.defineProperty(app, 'badgeCount', {
  get: () => nativeBCGetter.call(app),
  set: (count) => nativeBCSetter.call(app, count)
const nativeNGetter = app.getName;
const nativeNSetter = app.setName;
Object.defineProperty(app, 'name', {
  get: () => nativeNGetter.call(app),
  set: (name) => nativeNSetter.call(app, name)
Object.assign(app, {
  commandLine: {
    hasSwitch: (theSwitch: string) => commandLine.hasSwitch(String(theSwitch)),
    getSwitchValue: (theSwitch: string) => commandLine.getSwitchValue(String(theSwitch)),
    appendSwitch: (theSwitch: string, value?: string) => commandLine.appendSwitch(String(theSwitch), typeof value === 'undefined' ? value : String(value)),
    appendArgument: (arg: string) => commandLine.appendArgument(String(arg)),
    removeSwitch: (theSwitch: string) => commandLine.removeSwitch(String(theSwitch))
  } as Electron.CommandLine
// we define this here because it'd be overly complicated to
// do in native land
Object.defineProperty(app, 'applicationMenu', {
  get () {
    return Menu.getApplicationMenu();
  set (menu: Electron.Menu | null) {
    return Menu.setApplicationMenu(menu);
// The native implementation is not provided on non-windows platforms
app.setAppUserModelId = app.setAppUserModelId || (() => {});
if (process.platform === 'darwin') {
  const setDockMenu = app.dock!.setMenu;
  app.dock!.setMenu = (menu) => {
    dockMenu = menu;
    setDockMenu(menu);
  app.dock!.getMenu = () => dockMenu;
  const patternVmRSS = /^VmRSS:\s*(\d+) kB$/m;
  const patternVmHWM = /^VmHWM:\s*(\d+) kB$/m;
  const getStatus = (pid: number) => {
      return fs.readFileSync(`/proc/${pid}/status`, 'utf8');
  const getEntry = (file: string, pattern: RegExp) => {
    const match = file.match(pattern);
  const getProcessMemoryInfo = (pid: number) => {
    const file = getStatus(pid);
      workingSetSize: getEntry(file, patternVmRSS),
      peakWorkingSetSize: getEntry(file, patternVmHWM)
  const nativeFn = app.getAppMetrics;
  app.getAppMetrics = () => {
    const metrics = nativeFn.call(app);
    for (const metric of metrics) {
      metric.memory = getProcessMemoryInfo(metric.pid);
// Routes the events to webContents.
const events = ['certificate-error', 'select-client-certificate'];
for (const name of events) {
  app.on(name as 'certificate-error', (event, webContents, ...args: any[]) => {
    webContents.emit(name, event, ...args);
app._clientCertRequestPasswordHandler = null;
app.setClientCertRequestPasswordHandler = function (handler: (params: Electron.ClientCertRequestParams) => Promise<string>) {
  app._clientCertRequestPasswordHandler = handler;
app.on('-client-certificate-request-password', async (event: Electron.Event<Electron.ClientCertRequestParams>, callback: (password: string) => void) => {
  const { hostname, tokenName, isRetry } = event;
  if (!app._clientCertRequestPasswordHandler) {
    callback('');
  const password = await app._clientCertRequestPasswordHandler({ hostname, tokenName, isRetry });
  callback(password);
import { Base64ModelFactory } from '@jupyterlab/docregistry';
import { createRendermimePlugins } from '@jupyterlab/application/lib/mimerenderers';
import { LabStatus } from '@jupyterlab/application/lib/status';
import { PageConfig } from '@jupyterlab/coreutils';
import { IRenderMime } from '@jupyterlab/rendermime-interfaces';
import { Throttler } from '@lumino/polling';
import { INotebookShell, NotebookShell } from './shell';
 * App is the main application class. It is instantiated once and shared.
export class NotebookApp extends JupyterFrontEnd<INotebookShell> {
   * Construct a new NotebookApp object.
   * @param options The instantiation options for an application.
  constructor(options: NotebookApp.IOptions = { shell: new NotebookShell() }) {
    super({ ...options, shell: options.shell ?? new NotebookShell() });
    // Add initial model factory.
    this.docRegistry.addModelFactory(new Base64ModelFactory());
    if (options.mimeExtensions) {
      for (const plugin of createRendermimePlugins(options.mimeExtensions)) {
        this.registerPlugin(plugin);
    // Create an IInfo dictionary from the options to override the defaults.
    const info = Object.keys(JupyterLab.defaultInfo).reduce((acc, val) => {
      if (val in options) {
        (acc as any)[val] = JSON.parse(JSON.stringify((options as any)[val]));
    }, {} as Partial<JupyterLab.IInfo>);
    // Populate application info.
    this._info = { ...JupyterLab.defaultInfo, ...info };
    this.restored = this.shell.restored;
    this.restored.then(() => this._formatter.invoke());
   * The name of the application.
  readonly name = 'Jupyter Notebook';
   * A namespace/prefix plugins may use to denote their provenance.
  readonly namespace = this.name;
   * The application busy and dirty status signals and flags.
  readonly status = new LabStatus(this);
   * Promise that resolves when the state is first restored
  override readonly restored: Promise<void>;
   * The version of the application.
  readonly version = PageConfig.getOption('appVersion') ?? 'unknown';
   * The NotebookApp application information dictionary.
  get info(): JupyterLab.IInfo {
    return this._info;
   * The JupyterLab application paths dictionary.
  get paths(): JupyterFrontEnd.IPaths {
      urls: {
        base: PageConfig.getOption('baseUrl'),
        notFound: PageConfig.getOption('notFoundUrl'),
        app: PageConfig.getOption('appUrl'),
        static: PageConfig.getOption('staticUrl'),
        settings: PageConfig.getOption('settingsUrl'),
        themes: PageConfig.getOption('themesUrl'),
        doc: PageConfig.getOption('docUrl'),
        translations: PageConfig.getOption('translationsApiUrl'),
        hubHost: PageConfig.getOption('hubHost') || undefined,
        hubPrefix: PageConfig.getOption('hubPrefix') || undefined,
        hubUser: PageConfig.getOption('hubUser') || undefined,
        hubServerName: PageConfig.getOption('hubServerName') || undefined,
      directories: {
        appSettings: PageConfig.getOption('appSettingsDir'),
        schemas: PageConfig.getOption('schemasDir'),
        static: PageConfig.getOption('staticDir'),
        templates: PageConfig.getOption('templatesDir'),
        themes: PageConfig.getOption('themesDir'),
        userSettings: PageConfig.getOption('userSettingsDir'),
        serverRoot: PageConfig.getOption('serverRoot'),
        workspaces: PageConfig.getOption('workspacesDir'),
   * Handle the DOM events for the application.
   * @param event - The DOM event sent to the application.
  override handleEvent(event: Event): void {
    super.handleEvent(event);
    if (event.type === 'resize') {
      void this._formatter.invoke();
   * Register plugins from a plugin module.
   * @param mod - The plugin module to register.
  registerPluginModule(mod: NotebookApp.IPluginModule): void {
    let data = mod.default;
    // Handle commonjs exports.
    if (!Object.prototype.hasOwnProperty.call(mod, '__esModule')) {
      data = mod as any;
    if (!Array.isArray(data)) {
      data = [data];
    data.forEach((item) => {
        this.registerPlugin(item);
   * Register the plugins from multiple plugin modules.
   * @param mods - The plugin modules to register.
  registerPluginModules(mods: NotebookApp.IPluginModule[]): void {
    mods.forEach((mod) => {
      this.registerPluginModule(mod);
  private _info: JupyterLab.IInfo = JupyterLab.defaultInfo;
  private _formatter = new Throttler(() => {
    Private.setFormat(this);
 * A namespace for App static items.
export namespace NotebookApp {
   * The instantiation options for an App application.
  export interface IOptions
    extends JupyterFrontEnd.IOptions<INotebookShell>,
      Partial<IInfo> {}
   * The information about a Jupyter Notebook application.
  export interface IInfo {
     * The mime renderer extensions.
    readonly mimeExtensions: IRenderMime.IExtensionModule[];
     * The information about available plugins.
    readonly availablePlugins: JupyterLab.IPluginInfo[];
   * The interface for a module that exports a plugin or plugins as
   * the default value.
  export interface IPluginModule {
     * The default export.
    default: JupyterFrontEndPlugin<any> | JupyterFrontEndPlugin<any>[];
 * A namespace for module-private functionality.
namespace Private {
   * Media query for mobile devices.
  const MOBILE_QUERY = 'only screen and (max-width: 760px)';
   * Sets the `format` of a Jupyter front-end application.
   * @param app The front-end application whose format is set.
  export function setFormat(app: NotebookApp): void {
    app.format = window.matchMedia(MOBILE_QUERY).matches ? 'mobile' : 'desktop';
