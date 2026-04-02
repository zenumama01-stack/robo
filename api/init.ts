 * Init command - Initialize new project
import { Command } from '@oclif/core';
import inquirer from 'inquirer';
export default class Init extends Command {
  static description = 'Initialize a new DBAutoDoc project';
  static examples = ['$ db-auto-doc init'];
    this.log(chalk.blue('DBAutoDoc Initialization\n'));
    // Database configuration
    const dbAnswers = await inquirer.prompt([
        type: 'input',
        name: 'server',
        message: 'SQL Server host:',
        default: 'localhost'
        name: 'database',
        message: 'Database name:',
        validate: (input: string) => input.length > 0 || 'Database name is required'
        name: 'user',
        message: 'Username:',
        default: 'sa'
        type: 'password',
        name: 'password',
        message: 'Password:',
        mask: '*'
        type: 'confirm',
        name: 'encrypt',
        message: 'Use encryption?',
        default: true
        name: 'trustServerCertificate',
        message: 'Trust server certificate?',
    this.log(chalk.yellow('\nTesting database connection...'));
      provider: 'sqlserver' as const,
      host: dbAnswers.server,
      database: dbAnswers.database,
      user: dbAnswers.user,
      password: dbAnswers.password,
      encrypt: dbAnswers.encrypt,
      trustServerCertificate: dbAnswers.trustServerCertificate
      this.error(`Connection failed: ${testResult.message}`);
    this.log(chalk.green('✓ Database connection successful!\n'));
    // AI configuration
    const aiAnswers = await inquirer.prompt([
        type: 'list',
        name: 'provider',
        message: 'AI Provider:',
        choices: ['gemini', 'openai', 'anthropic', 'groq', 'mistral', 'vertex', 'azure', 'cerebras', 'openrouter', 'xai', 'bedrock'],
        default: 'gemini',
        loop: false
        name: 'model',
        message: 'Model name:',
        default: (answers: any) => {
          if (answers.provider === 'gemini') return 'gemini-3-flash-preview';
          if (answers.provider === 'openai') return 'gpt-4-turbo-preview';
          if (answers.provider === 'anthropic') return 'claude-3-opus-20240229';
          if (answers.provider === 'groq') return 'mixtral-8x7b-32768';
          if (answers.provider === 'mistral') return 'mistral-small-latest';
          if (answers.provider === 'vertex') return 'gemini-1.5-flash';
          if (answers.provider === 'azure') return 'gpt-4';
          if (answers.provider === 'cerebras') return 'llama-3.1-8b';
          if (answers.provider === 'openrouter') return 'anthropic/claude-3-opus';
          if (answers.provider === 'xai') return 'grok-2';
          if (answers.provider === 'bedrock') return 'anthropic.claude-3-sonnet-20240229-v1:0';
        name: 'apiKey',
        message: 'API Key:',
        mask: '*',
        validate: (input: string) => input.length > 0 || 'API key is required'
    // Optional seed context
    const contextAnswers = await inquirer.prompt([
        name: 'addContext',
        message: 'Add seed context for better analysis?',
        name: 'overallPurpose',
        message: 'Database overall purpose (e.g., "E-commerce platform"):',
        when: (answers: any) => answers.addContext
        name: 'businessDomains',
        message: 'Business domains (comma-separated, e.g., "Sales, Inventory, Billing"):',
        name: 'industryContext',
        message: 'Industry context (e.g., "Healthcare", "Finance"):',
    // Sample query generation options
    const queryAnswers = await inquirer.prompt([
        name: 'enableSampleQueries',
        message: 'Generate sample queries for each table?',
        name: 'queriesPerTable',
        message: 'Number of queries to generate per table:',
        default: 5,
        when: (answers: any) => answers.enableSampleQueries,
        validate: (input: number) => input > 0 || 'Must be greater than 0'
        name: 'maxTables',
        message: 'Max number of tables to generate queries for (0 = all tables):',
        default: 10,
        validate: (input: number) => input >= 0 || 'Must be 0 or greater'
        name: 'tokenBudget',
        message: 'Token budget for query generation (0 = unlimited):',
        default: 100000,
        name: 'maxExecutionTime',
        message: 'Max execution time for query validation (ms):',
        default: 30000,
        when: (answers: any) => answers.enableSampleQueries
    // Create configuration
    // Update with user inputs
    config.database = {
      server: dbAnswers.server,
      trustServerCertificate: dbAnswers.trustServerCertificate,
    config.ai = {
      provider: aiAnswers.provider as any,
      model: aiAnswers.model,
      apiKey: aiAnswers.apiKey,
    if (contextAnswers.addContext) {
      (config as unknown as Record<string, unknown>)['seedContext'] = {
        overallPurpose: contextAnswers.overallPurpose || undefined,
        businessDomains: contextAnswers.businessDomains
          ? contextAnswers.businessDomains.split(',').map((d: string) => d.trim())
        industryContext: contextAnswers.industryContext || undefined
    // Add sample query generation config if enabled
    if (queryAnswers.enableSampleQueries) {
      config.analysis.sampleQueryGeneration = {
        queriesPerTable: queryAnswers.queriesPerTable || 5,
        maxExecutionTime: queryAnswers.maxExecutionTime || 30000,
        includeMultiQueryPatterns: true,
        validateAlignment: true,
        tokenBudget: queryAnswers.tokenBudget !== undefined ? queryAnswers.tokenBudget : 100000,
        maxRowsInSample: 10,
        maxTables: queryAnswers.maxTables !== undefined ? queryAnswers.maxTables : 10
    // Output directory configuration
    const outputAnswers = await inquirer.prompt([
        name: 'outputDir',
        message: 'Output directory for analysis results:',
        default: './output'
    // Set output directory
    config.output.outputDir = outputAnswers.outputDir;
    // Save configuration
    await ConfigLoader.save(config, './config.json');
    this.log(chalk.green('\n✓ Configuration saved to config.json'));
    this.log(chalk.blue('\nNext steps:'));
    this.log('  1. Run: db-auto-doc analyze');
    this.log('  2. Run: db-auto-doc export --sql --markdown');
export default class DbDocInit extends Command {
  static description = 'Initialize DBAutoDoc project (delegates to db-auto-doc init)';
    const { default: InitCommand } = await import('@memberjunction/db-auto-doc/dist/commands/init');
    // Execute the DBAutoDoc init command
    await InitCommand.run([]);
import { input, select } from '@inquirer/prompts';
  static description = 'Initialize a directory for metadata synchronization';
    const { InitService } = await import('@memberjunction/metadata-sync');
      // Check if already initialized
      const initService = new InitService();
      // Build options from user input
      const options: Parameters<typeof initService.initialize>[0] = {};
      // Check for existing configuration
        // If .mj-sync.json exists, ask about overwrite
        const overwrite = await select({
          message: 'Directory already initialized. Overwrite configuration?',
            { name: 'Yes', value: true },
            { name: 'No', value: false }
        if (!overwrite) {
          this.log('Initialization cancelled');
        options.overwrite = true;
        // File doesn't exist, proceed normally
      // Ask if they want to set up an entity directory
      const setupEntity = await select({
        message: 'Would you like to set up an entity directory now?',
          { name: 'Yes - AI Prompts', value: 'ai-prompts' },
          { name: 'Yes - Other entity', value: 'other' },
          { name: 'No - I\'ll set up later', value: 'no' }
      options.setupEntity = setupEntity as 'ai-prompts' | 'other' | 'no';
      if (setupEntity === 'other') {
        options.entityName = await input({
          message: 'Enter the entity name (e.g., "Templates", "MJ: AI Models"):'
        options.dirName = await input({
          message: 'Enter the directory name:',
          default: options.entityName.toLowerCase().replace(/\s+/g, '-')
      // Initialize with callbacks
      await initService.initialize(options, {
        onProgress: (message) => {
        onSuccess: (message) => {
          spinner.succeed(message);
        onError: (message) => {
          spinner.fail(message);
      this.log('\n✅ Initialization complete!');
      // Show next steps
      const nextSteps = initService.getNextSteps();
      this.log('\nNext steps:');
      nextSteps.forEach((step, index) => {
        this.log(`${index + 1}. ${step.replace('mj-sync', 'mj sync')}`);
      spinner.fail('Initialization failed');
      this.log('\n=== Initialization Error Details ===');
      // Check for error hints
        const hint = new InitService().getErrorHint(error);
        if (hint) {
          this.log(`\nHint: ${hint}`);
import { Hook } from '@oclif/core';
 * Init hook runs once when the CLI starts up.
 * Load environment variables from .env file.
const hook: Hook<'init'> = async function () {
  // Load environment variables from .env file in repo root
  const envPath = path.resolve(process.cwd(), '.env');
export default hook;import type * as defaultMenuModule from '@electron/internal/browser/default-menu';
import type * as url from 'url';
import type * as v8 from 'v8';
const Module = require('module') as NodeJS.ModuleInternal;
// We modified the original process.argv to let node.js load the init.js,
// we need to restore it here.
process.argv.splice(1, 1);
// Import common settings.
require('@electron/internal/common/init');
process._linkedBinding('electron_browser_event_emitter').setEventEmitterPrototype(EventEmitter.prototype);
// Don't quit on fatal error.
process.on('uncaughtException', function (error) {
  // Do nothing if the user has a custom uncaught exception handler.
  if (process.listenerCount('uncaughtException') > 1) {
  // Show error in GUI.
  // We can't import { dialog } at the top of this file as this file is
  // responsible for setting up the require hook for the "electron" module
  // so we import it inside the handler down here
  import('electron')
    .then(({ dialog }) => {
      const stack = error.stack ? error.stack : `${error.name}: ${error.message}`;
      const message = 'Uncaught Exception:\n' + stack;
      dialog.showErrorBox('A JavaScript error occurred in the main process', message);
// Emit 'exit' event on quit.
const { app } = require('electron');
app.on('quit', (_event: any, exitCode: number) => {
  process.emit('exit', exitCode);
  // If we are a Squirrel.Windows-installed app, set app user model ID
  // so that users don't have to do this.
  // Squirrel packages are always of the form:
  // PACKAGE-NAME
  // - Update.exe
  // - app-VERSION
  //   - OUREXE.exe
  // Squirrel itself will always set the shortcut's App User Model ID to the
  // form `com.squirrel.PACKAGE-NAME.OUREXE`. We need to call
  // app.setAppUserModelId with a matching identifier so that renderer processes
  // will inherit this value.
  const updateDotExe = path.join(path.dirname(process.execPath), '..', 'update.exe');
  if (fs.existsSync(updateDotExe)) {
    const packageDir = path.dirname(path.resolve(updateDotExe));
    const packageName = path.basename(packageDir).replaceAll(/\s/g, '');
    const exeName = path.basename(process.execPath).replace(/\.exe$/i, '').replaceAll(/\s/g, '');
    app.setAppUserModelId(`com.squirrel.${packageName}.${exeName}`);
// Map process.exit to app.exit, which quits gracefully.
process.exit = app.exit as () => never;
// Load the RPC server.
require('@electron/internal/browser/rpc-server');
// Load the guest view manager.
require('@electron/internal/browser/guest-view-manager');
// Now we try to load app's package.json.
const v8Util = process._linkedBinding('electron_common_v8_util');
let packagePath = null;
let packageJson = null;
const searchPaths: string[] = v8Util.getHiddenValue(global, 'appSearchPaths');
const searchPathsOnlyLoadASAR: boolean = v8Util.getHiddenValue(global, 'appSearchPathsOnlyLoadASAR');
// Borrow the _getOrCreateArchive asar helper
const getOrCreateArchive = process._getOrCreateArchive;
delete process._getOrCreateArchive;
if (process.resourcesPath) {
  for (packagePath of searchPaths) {
      packagePath = path.join(process.resourcesPath, packagePath);
      if (searchPathsOnlyLoadASAR) {
        if (!getOrCreateArchive?.(packagePath)) {
      packageJson = Module._load(path.join(packagePath, 'package.json'));
if (packageJson == null) {
  process.nextTick(function () {
    return process.exit(1);
  throw new Error('Unable to find a valid app');
// Set application's version.
if (packageJson.version != null) {
// Set application's name.
if (packageJson.productName != null) {
  app.name = `${packageJson.productName}`.trim();
} else if (packageJson.name != null) {
  app.name = `${packageJson.name}`.trim();
if (packageJson.v8Flags != null) {
  (require('v8') as typeof v8).setFlagsFromString(packageJson.v8Flags);
app.setAppPath(packagePath);
// Load the chrome devtools support.
require('@electron/internal/browser/devtools');
// Load protocol module to ensure it is populated on app ready
require('@electron/internal/browser/api/protocol');
// Load service-worker-main module to ensure it is populated on app ready
require('@electron/internal/browser/api/service-worker-main');
// Load web-contents module to ensure it is populated on app ready
require('@electron/internal/browser/api/web-contents');
// Load web-frame-main module to ensure it is populated on app ready
require('@electron/internal/browser/api/web-frame-main');
// Required because `new BrowserWindow` calls some WebContentsView stuff, so
// the inheritance needs to be set up before that happens.
require('@electron/internal/browser/api/web-contents-view');
// Set main startup script of the app.
const mainStartupScript = packageJson.main || 'index.js';
// Quit when all windows are closed and no other one is listening to this.
  if (app.listenerCount('window-all-closed') === 1) {
const { setDefaultApplicationMenu } = require('@electron/internal/browser/default-menu') as typeof defaultMenuModule;
// Create default menu.
// The |will-finish-launching| event is emitted before |ready| event, so default
// menu is set before any user window is created.
app.once('will-finish-launching', setDefaultApplicationMenu);
const { appCodeLoaded } = process;
delete process.appCodeLoaded;
if (packagePath) {
  // Finally load app's main.js and transfer control to C++.
  if ((packageJson.type === 'module' && !mainStartupScript.endsWith('.cjs')) || mainStartupScript.endsWith('.mjs')) {
    const { runEntryPointWithESMLoader } = __non_webpack_require__('internal/modules/run_main') as typeof import('@node/lib/internal/modules/run_main');
    const main = (require('url') as typeof url).pathToFileURL(path.join(packagePath, mainStartupScript));
    runEntryPointWithESMLoader(async (cascadedLoader: any) => {
        await cascadedLoader.import(main.toString(), undefined, Object.create(null));
        appCodeLoaded!();
        process.emit('uncaughtException', err as Error);
    // Call appCodeLoaded before just for safety, it doesn't matter here as _load is synchronous
    Module._load(path.join(packagePath, mainStartupScript), Module, true);
  console.error('Failed to locate a valid package to load (app, app.asar or default_app.asar)');
  console.error('This normally means you\'ve damaged the Electron package somehow');
import timers = require('timers');
import * as util from 'util';
import type * as stream from 'stream';
type AnyFn = (...args: any[]) => any
// setImmediate and process.nextTick makes use of uv_check and uv_prepare to
// run the callbacks, however since we only run uv loop on requests, the
// callbacks wouldn't be called until something else activated the uv loop,
// which would delay the callbacks for arbitrary long time. So we should
// initiatively activate the uv loop once setImmediate and process.nextTick is
// called.
const wrapWithActivateUvLoop = function <T extends AnyFn> (func: T): T {
  return wrap(func, function (func) {
    return function (this: any, ...args: any[]) {
      process.activateUvLoop();
      return func.apply(this, args);
  }) as T;
 * Casts to any below for func are due to Typescript not supporting symbols
 * in index signatures
 * Refs: https://github.com/Microsoft/TypeScript/issues/1863
function wrap <T extends AnyFn> (func: T, wrapper: (fn: AnyFn) => T) {
  const wrapped = wrapper(func);
  if ((func as any)[util.promisify.custom]) {
    (wrapped as any)[util.promisify.custom] = wrapper((func as any)[util.promisify.custom]);
// process.nextTick and setImmediate make use of uv_check and uv_prepare to
// initiatively activate the uv loop once process.nextTick and setImmediate is
process.nextTick = wrapWithActivateUvLoop(process.nextTick);
global.setImmediate = timers.setImmediate = wrapWithActivateUvLoop(timers.setImmediate);
global.clearImmediate = timers.clearImmediate;
// setTimeout needs to update the polling timeout of the event loop, when
// called under Chromium's event loop the node's event loop won't get a chance
// to update the timeout, so we have to force the node's event loop to
// recalculate the timeout in the process.
timers.setTimeout = wrapWithActivateUvLoop(timers.setTimeout);
timers.setInterval = wrapWithActivateUvLoop(timers.setInterval);
// Update the global version of the timer apis to use the above wrapper
// only in the process that runs node event loop alongside chromium
// event loop. We skip renderer with nodeIntegration here because node globals
// are deleted in these processes, see renderer/init.js for reference.
if (process.type === 'browser' ||
    process.type === 'utility') {
  global.setTimeout = timers.setTimeout;
  global.setInterval = timers.setInterval;
  // Always returns EOF for stdin stream.
  const { Readable } = require('stream') as typeof stream;
  const stdin = new Readable();
  stdin.push(null);
  Object.defineProperty(process, 'stdin', {
      return stdin;
// Make a fake Electron module that we will insert into the module cache
const makeElectronModule = (name: string) => {
  const electronModule = new Module('electron', null);
  electronModule.id = 'electron';
  electronModule.loaded = true;
  electronModule.filename = name;
  Object.defineProperty(electronModule, 'exports', {
    get: () => require('electron')
  Module._cache[name] = electronModule;
makeElectronModule('electron');
makeElectronModule('electron/common');
  makeElectronModule('electron/main');
} else if (process.type === 'renderer') {
  makeElectronModule('electron/renderer');
} else if (process.type === 'utility') {
  makeElectronModule('electron/utility');
const originalResolveFilename = Module._resolveFilename;
// 'electron/{common,main,renderer,utility}' are module aliases
// of the 'electron' module for TypeScript purposes, i.e., the types for
// 'electron/main' consist of only main process modules, etc. It is intentional
// that these can be `require()`-ed from both the main process as well as the
// renderer process regardless of the names, they're superficial for TypeScript
const electronModuleNames = new Set([
  'electron', 'electron/main', 'electron/renderer', 'electron/common', 'electron/utility'
Module._resolveFilename = function (request, parent, isMain, options) {
  if (electronModuleNames.has(request)) {
    return 'electron';
    return originalResolveFilename(request, parent, isMain, options);
import type * as webViewElementModule from '@electron/internal/renderer/web-view/web-view-element';
import type { WebViewImplHooks } from '@electron/internal/renderer/web-view/web-view-impl';
declare const isolatedApi: WebViewImplHooks;
if (isolatedApi.guestViewInternal) {
  // Must setup the WebView element in main world.
  const { setupWebView } = require('@electron/internal/renderer/web-view/web-view-element') as typeof webViewElementModule;
  setupWebView(isolatedApi);
/* eslint-disable import/newline-after-import */
/* eslint-disable import/order */
// Initialize ASAR support in fs module.
import { wrapFsWithAsar } from './asar-fs-wrapper';
wrapFsWithAsar(require('fs'));
// See ElectronRendererClient::DidCreateScriptContext.
if ((globalThis as any).blinkfetch) {
  const keys = ['fetch', 'Response', 'FormData', 'Request', 'Headers', 'EventSource'];
    (globalThis as any)[key] = (globalThis as any)[`blink${key}`];
    delete (globalThis as any)[`blink${key}`];
// Hook child_process.fork.
import cp = require('child_process'); // eslint-disable-line import/first
const originalFork = cp.fork;
cp.fork = (modulePath, args?, options?: cp.ForkOptions) => {
  // Parse optional args.
    options = args as cp.ForkOptions;
  // Fallback to original fork to report arg type errors.
  if (typeof modulePath !== 'string' || !Array.isArray(args) ||
      (typeof options !== 'object' && typeof options !== 'undefined')) {
    return originalFork(modulePath, args, options);
  // When forking a child script, we setup a special environment to make
  // the electron binary run like upstream Node.js.
  options = options ?? {};
  options.env = Object.create(options.env || process.env);
  options.env!.ELECTRON_RUN_AS_NODE = '1';
  // On mac the child script runs in helper executable.
  if (!options.execPath && process.platform === 'darwin') {
    options.execPath = process.helperExecPath;
// Prevent Node from adding paths outside this app to search paths.
import path = require('path'); // eslint-disable-line import/first
const resourcesPathWithTrailingSlash = process.resourcesPath + path.sep;
const originalNodeModulePaths = Module._nodeModulePaths;
Module._nodeModulePaths = function (from) {
  const paths: string[] = originalNodeModulePaths(from);
  const fromPath = path.resolve(from) + path.sep;
  // If "from" is outside the app then we do nothing.
  if (fromPath.startsWith(resourcesPathWithTrailingSlash)) {
    return paths.filter(function (candidate) {
      return candidate.startsWith(resourcesPathWithTrailingSlash);
import '@electron/internal/sandboxed_renderer/pre-init';
import type * as ipcRendererUtilsModule from '@electron/internal/renderer/ipc-renderer-internal-utils';
import { createPreloadProcessObject, executeSandboxedPreloadScripts } from '@electron/internal/sandboxed_renderer/preload';
import * as events from 'events';
declare const binding: {
  get: (name: string) => any;
  process: NodeJS.Process;
  createPreloadScript: (src: string) => Function
const ipcRendererUtils = require('@electron/internal/renderer/ipc-renderer-internal-utils') as typeof ipcRendererUtilsModule;
  preloadScripts,
  process: processProps
} = ipcRendererUtils.invokeSync<{
  preloadScripts: ElectronInternal.PreloadScript[];
}>(IPC_MESSAGES.BROWSER_SANDBOX_LOAD);
const electron = require('electron');
const loadedModules = new Map<string, any>([
  ['electron', electron],
  ['electron/common', electron],
  ['events', events],
  ['node:events', events]
const loadableModules = new Map<string, Function>([
  ['url', () => require('url')],
  ['node:url', () => require('url')]
const preloadProcess = createPreloadProcessObject();
Object.assign(preloadProcess, binding.process);
Object.assign(preloadProcess, processProps);
Object.assign(process, processProps);
require('@electron/internal/renderer/ipc-native-setup');
executeSandboxedPreloadScripts({
  loadedModules,
  loadableModules,
  process: preloadProcess,
  createPreloadScript: binding.createPreloadScript,
  exposeGlobals: {
    Buffer,
    // FIXME(samuelmaddock): workaround webpack bug replacing this with just
    // `__webpack_require__.g,` which causes script error
    global: globalThis
}, preloadScripts);
import type * as ipcRendererInternalModule from '@electron/internal/renderer/ipc-renderer-internal';
import { pathToFileURL } from 'url';
// We do not want to allow use of the VM module in the renderer process as
// it conflicts with Blink's V8::Context internal logic.
Module._load = function (request: string) {
  if (request === 'vm') {
    console.warn('The vm module of Node.js is unsupported in Electron\'s renderer process due to incompatibilities with the Blink rendering engine. Crashes are likely and avoiding the module is highly recommended. This module may be removed in a future release.');
  return originalModuleLoad.apply(this, arguments as any);
// Make sure globals like "process" and "global" are always available in preload
// scripts even after they are deleted in "loaded" script.
// Note 1: We rely on a Node patch to actually pass "process" and "global" and
// other arguments to the wrapper.
// Note 2: Node introduced a new code path to use native code to wrap module
// code, which does not work with this hack. However by modifying the
// "Module.wrapper" we can force Node to use the old code path to wrap module
// code with JavaScript.
// Note 3: We provide the equivalent extra variables internally through the
// webpack ProvidePlugin in webpack.config.base.js.  If you add any extra
// variables to this wrapper please ensure to update that plugin as well.
Module.wrapper = [
  '(function (exports, require, module, __filename, __dirname, process, global, Buffer) { ' +
  // By running the code in a new closure, it would be possible for the module
  // code to override "process" and "Buffer" with local variables.
  'return function (exports, require, module, __filename, __dirname) { ',
  '\n}.call(this, exports, require, module, __filename, __dirname); });'
// We modified the original process.argv to let node.js load the
// init.js, we need to restore it here.
const { ipcRendererInternal } = require('@electron/internal/renderer/ipc-renderer-internal') as typeof ipcRendererInternalModule;
process.getProcessMemoryInfo = () => {
  return ipcRendererInternal.invoke<Electron.ProcessMemoryInfo>(IPC_MESSAGES.BROWSER_GET_PROCESS_MEMORY_INFO);
// Process command line arguments.
const { hasSwitch, getSwitchValue } = process._linkedBinding('electron_common_command_line');
const appPath = hasSwitch('app-path') ? getSwitchValue('app-path') : null;
// Common renderer initialization
require('@electron/internal/renderer/common-init');
if (nodeIntegration) {
  // Export node bindings to global.
  const { makeRequireFunction } = __non_webpack_require__('internal/modules/helpers') as typeof import('@node/lib/internal/modules/helpers');
  global.module = new Module('electron/js2c/renderer_init');
  global.require = makeRequireFunction(global.module) as NodeRequire;
  // Set the __filename to the path of html file if it is file: protocol.
  if (window.location.protocol === 'file:') {
    const location = window.location;
    let pathname = location.pathname;
      if (pathname[0] === '/') pathname = pathname.substr(1);
      const isWindowsNetworkSharePath = location.hostname.length > 0 && process.resourcesPath.startsWith('\\');
      if (isWindowsNetworkSharePath) {
        pathname = `//${location.host}/${pathname}`;
    global.__filename = path.normalize(decodeURIComponent(pathname));
    global.__dirname = path.dirname(global.__filename);
    // Set module's filename so relative require can work as expected.
    global.module.filename = global.__filename;
    // Also search for module under the html file.
    global.module.paths = Module._nodeModulePaths(global.__dirname);
    // For backwards compatibility we fake these two paths here
    global.__filename = path.join(process.resourcesPath, 'electron.asar', 'renderer', 'init.js');
    global.__dirname = path.join(process.resourcesPath, 'electron.asar', 'renderer');
      // Search for module under the app directory
      global.module.paths = Module._nodeModulePaths(appPath);
  // Redirect window.onerror to uncaughtException.
  window.onerror = function (_message, _filename, _lineno, _colno, error) {
    if (global.process.listenerCount('uncaughtException') > 0) {
      // We do not want to add `uncaughtException` to our definitions
      // because we don't want anyone else (anywhere) to throw that kind
      // of error.
      global.process.emit('uncaughtException', error as any);
  // Delete Node's symbols after the Environment has been loaded in a
  // non context-isolated environment
  if (!process.contextIsolated) {
    process.once('loaded', function () {
      delete (global as any).process;
      delete (global as any).Buffer;
      delete (global as any).setImmediate;
      delete (global as any).clearImmediate;
      delete (global as any).global;
      delete (global as any).root;
      delete (global as any).GLOBAL;
const { preloadPaths } = ipcRendererUtils.invokeSync<{ preloadPaths: string[] }>(IPC_MESSAGES.BROWSER_NONSANDBOX_LOAD);
const cjsPreloads = preloadPaths.filter(p => path.extname(p) !== '.mjs');
const esmPreloads = preloadPaths.filter(p => path.extname(p) === '.mjs');
if (cjsPreloads.length) {
  // Load the preload scripts.
  for (const preloadScript of cjsPreloads) {
      Module._load(preloadScript);
      console.error(`Unable to load preload script: ${preloadScript}`);
      ipcRendererInternal.send(IPC_MESSAGES.BROWSER_PRELOAD_ERROR, preloadScript, error);
if (esmPreloads.length) {
    for (const preloadScript of esmPreloads) {
      await cascadedLoader.import(pathToFileURL(preloadScript).toString(), undefined, Object.create(null)).catch((err: Error) => {
        ipcRendererInternal.send(IPC_MESSAGES.BROWSER_PRELOAD_ERROR, preloadScript, err);
  }).finally(() => appCodeLoaded!());
import { setImmediate, clearImmediate } from 'timers';
  ['electron/renderer', electron],
  ['timers', () => require('timers')],
  ['node:timers', () => require('timers')],
// InvokeEmitProcessEvent in ElectronSandboxedRendererClient will look for this
v8Util.setHiddenValue(global, 'emit-process-event', (event: string) => {
  (process as events.EventEmitter).emit(event);
  (preloadProcess as events.EventEmitter).emit(event);
    global: globalThis,
    setImmediate,
    clearImmediate
import { ParentPort } from '@electron/internal/utility/parent-port';
const entryScript: string = v8Util.getHiddenValue(process, '_serviceStartupScript');
process.argv.splice(1, 1, entryScript);
const parentPort: ParentPort = new ParentPort();
Object.defineProperty(process, 'parentPort', {
  value: parentPort
// Based on third_party/electron_node/lib/internal/worker/io.js
parentPort.on('newListener', (name: string) => {
  if (name === 'message' && parentPort.listenerCount('message') === 0) {
    parentPort.start();
parentPort.on('removeListener', (name: string) => {
    parentPort.pause();
// Finally load entry script.
const mainEntry = pathToFileURL(entryScript);
    await cascadedLoader.import(mainEntry.toString(), undefined, Object.create(null));
    // @ts-ignore internalBinding is a secret internal global that we shouldn't
    // really be using, so we ignore the type error instead of declaring it in types
    internalBinding('errors').triggerUncaughtException(err);
global.module = new Module('electron/js2c/worker_init');
// See WebWorkerObserver::WorkerScriptReadyForEvaluation.
// NB. 'self' isn't defined in an AudioWorklet.
if (typeof self !== 'undefined' && self.location.protocol === 'file:') {
  const pathname = process.platform === 'win32' && self?.location.pathname[0] === '/' ? self?.location.pathname.substr(1) : self?.location.pathname;
  global.__filename = path.join(process.resourcesPath, 'electron.asar', 'worker', 'init.js');
  global.__dirname = path.join(process.resourcesPath, 'electron.asar', 'worker');
    // Search for module under the app directory.
export default hook;import type * as defaultMenuModule from '@electron/internal/browser/default-menu';
import type * as defaultMenuModule from '@electron/internal/browser/default-menu';
export default hook;