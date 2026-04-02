import { Buffer } from 'buffer';
import { Dirent, constants } from 'fs';
import type * as Crypto from 'crypto';
import type * as os from 'os';
const asar = process._linkedBinding('electron_common_asar');
const Promise: PromiseConstructor = global.Promise;
const envNoAsar = process.env.ELECTRON_NO_ASAR &&
    process.type !== 'browser' &&
    process.type !== 'renderer';
const isAsarDisabled = () => process.noAsar || envNoAsar;
const internalBinding = process.internalBinding!;
delete process.internalBinding;
const nextTick = (functionToCall: Function, args: any[] = []) => {
  process.nextTick(() => functionToCall(...args));
const binding = internalBinding('fs');
// Cache asar archive objects.
const cachedArchives = new Map<string, NodeJS.AsarArchive>();
const getOrCreateArchive = (archivePath: string) => {
  const isCached = cachedArchives.has(archivePath);
  if (isCached) {
    return cachedArchives.get(archivePath)!;
    const newArchive = new asar.Archive(archivePath);
    cachedArchives.set(archivePath, newArchive);
    return newArchive;
process._getOrCreateArchive = getOrCreateArchive;
const asarRe = /\.asar/i;
  getValidatedPath,
  getOptions,
  getDirent
} = __non_webpack_require__('internal/fs/utils') as typeof import('@node/lib/internal/fs/utils');
  assignFunctionName
} = __non_webpack_require__('internal/util') as typeof import('@node/lib/internal/util');
  validateBoolean,
  validateFunction
} = __non_webpack_require__('internal/validators') as typeof import('@node/lib/internal/validators');
// In the renderer node internals use the node global URL but we do not set that to be
// the global URL instance.  We need to do instanceof checks against the internal URL impl
const { URL: NodeURL } = __non_webpack_require__('internal/url') as typeof import('@node/lib/internal/url');
// Separate asar package's path from full path.
const splitPath = (archivePathOrBuffer: string | Buffer | URL) => {
  // Shortcut for disabled asar.
  if (isAsarDisabled()) return { isAsar: <const>false };
  // Check for a bad argument type.
  let archivePath = archivePathOrBuffer;
  if (Buffer.isBuffer(archivePathOrBuffer)) {
    archivePath = archivePathOrBuffer.toString();
  if (archivePath instanceof NodeURL) {
    archivePath = getValidatedPath(archivePath);
  if (typeof archivePath !== 'string') return { isAsar: <const>false };
  if (!asarRe.test(archivePath)) return { isAsar: <const>false };
  return asar.splitPath(path.normalize(archivePath));
// Convert asar archive's Stats object to fs's Stats object.
let nextInode = 0;
const uid = process.getuid?.() ?? 0;
const gid = process.getgid?.() ?? 0;
const fakeTime = new Date();
function getDirents (p: string, { 0: names, 1: types }: any[][]): Dirent[] {
  for (let i = 0; i < names.length; i++) {
    let type = types[i];
    const info = splitPath(path.join(p, names[i]));
    if (info.isAsar) {
      const archive = getOrCreateArchive(info.asarPath);
      if (!archive) continue;
      const stats = archive.stat(info.filePath);
      if (!stats) continue;
      type = stats.type;
    names[i] = getDirent(p, names[i], type);
enum AsarFileType {
  kFile = (constants as any).UV_DIRENT_FILE,
  kDirectory = (constants as any).UV_DIRENT_DIR,
  kLink = (constants as any).UV_DIRENT_LINK,
const fileTypeToMode = new Map<AsarFileType, number>([
  [AsarFileType.kFile, constants.S_IFREG],
  [AsarFileType.kDirectory, constants.S_IFDIR],
  [AsarFileType.kLink, constants.S_IFLNK]
const asarStatsToFsStats = function (stats: NodeJS.AsarFileStat) {
  const { Stats } = require('fs');
  const mode = constants.S_IROTH | constants.S_IRGRP | constants.S_IRUSR | constants.S_IWUSR | fileTypeToMode.get(stats.type)!;
  return new Stats(
    1, // dev
    mode, // mode
    1, // nlink
    uid,
    gid,
    0, // rdev
    undefined, // blksize
    ++nextInode, // ino
    stats.size,
    undefined, // blocks,
    fakeTime.getTime(), // atim_msec
    fakeTime.getTime(), // mtim_msec
    fakeTime.getTime(), // ctim_msec
    fakeTime.getTime() // birthtim_msec
const enum AsarError {
  NOT_FOUND = 'NOT_FOUND',
  NOT_DIR = 'NOT_DIR',
  NO_ACCESS = 'NO_ACCESS',
  INVALID_ARCHIVE = 'INVALID_ARCHIVE'
type AsarErrorObject = Error & { code?: string, errno?: number };
const createError = (errorType: AsarError, { asarPath, filePath }: { asarPath?: string, filePath?: string } = {}) => {
  let error: AsarErrorObject;
    case AsarError.NOT_FOUND:
      error = new Error(`ENOENT, ${filePath} not found in ${asarPath}`);
      error.code = 'ENOENT';
      error.errno = -2;
    case AsarError.NOT_DIR:
      error = new Error('ENOTDIR, not a directory');
      error.code = 'ENOTDIR';
      error.errno = -20;
    case AsarError.NO_ACCESS:
      error = new Error(`EACCES: permission denied, access '${filePath}'`);
      error.code = 'EACCES';
      error.errno = -13;
    case AsarError.INVALID_ARCHIVE:
      error = new Error(`Invalid package ${asarPath}`);
      throw new Error(`Invalid error type "${errorType}" passed to createError.`);
const overrideAPISync = function (module: Record<string, any>, name: string, pathArgumentIndex?: number | null, fromAsync: boolean = false) {
  if (pathArgumentIndex == null) pathArgumentIndex = 0;
  const old = module[name];
  const func = function (this: any, ...args: any[]) {
    const pathArgument = args[pathArgumentIndex!];
    const pathInfo = splitPath(pathArgument);
    if (!pathInfo.isAsar) return old.apply(this, args);
    const { asarPath, filePath } = pathInfo;
    const archive = getOrCreateArchive(asarPath);
    if (!archive) throw createError(AsarError.INVALID_ARCHIVE, { asarPath });
    const newPath = archive.copyFileOut(filePath);
    if (!newPath) throw createError(AsarError.NOT_FOUND, { asarPath, filePath });
    args[pathArgumentIndex!] = newPath;
    return old.apply(this, args);
  if (fromAsync) {
    return func;
  module[name] = func;
const overrideAPI = function (module: Record<string, any>, name: string, pathArgumentIndex?: number | null) {
  module[name] = function (this: any, ...args: any[]) {
    const callback = args[args.length - 1];
    if (typeof callback !== 'function') {
      return overrideAPISync(module, name, pathArgumentIndex!, true)!.apply(this, args);
    if (!archive) {
      const error = createError(AsarError.INVALID_ARCHIVE, { asarPath });
      nextTick(callback, [error]);
    if (!newPath) {
      const error = createError(AsarError.NOT_FOUND, { asarPath, filePath });
  if (old[util.promisify.custom]) {
    module[name][util.promisify.custom] = assignFunctionName(
      makePromiseFunction(old[util.promisify.custom], pathArgumentIndex)
  if (module.promises && module.promises[name]) {
    module.promises[name] = makePromiseFunction(module.promises[name], pathArgumentIndex);
let crypto: typeof Crypto;
function validateBufferIntegrity (buffer: Buffer, integrity: NodeJS.AsarFileInfo['integrity']) {
  if (!integrity) return;
  // Delay load crypto to improve app boot performance
  // when integrity protection is not enabled
  crypto = crypto || require('crypto');
  const actual = crypto.createHash(integrity.algorithm).update(buffer).digest('hex');
  if (actual !== integrity.hash) {
    console.error(`ASAR Integrity Violation: got a hash mismatch (${actual} vs ${integrity.hash})`);
const makePromiseFunction = function (orig: Function, pathArgumentIndex: number) {
    const pathArgument = args[pathArgumentIndex];
    if (!pathInfo.isAsar) return orig.apply(this, args);
      return Promise.reject(createError(AsarError.INVALID_ARCHIVE, { asarPath }));
      return Promise.reject(createError(AsarError.NOT_FOUND, { asarPath, filePath }));
    args[pathArgumentIndex] = newPath;
    return orig.apply(this, args);
// Override fs APIs.
export const wrapFsWithAsar = (fs: Record<string, any>) => {
  const logFDs = new Map<string, number>();
  const logASARAccess = (asarPath: string, filePath: string, offset: number) => {
    if (!process.env.ELECTRON_LOG_ASAR_READS) return;
    if (!logFDs.has(asarPath)) {
      const logFilename = `${path.basename(asarPath, '.asar')}-access-log.txt`;
      const logPath = path.join((require('os') as typeof os).tmpdir(), logFilename);
      logFDs.set(asarPath, fs.openSync(logPath, 'a'));
    fs.writeSync(logFDs.get(asarPath), `${offset}: ${filePath}\n`);
  const shouldThrowStatError = (options: any) => {
    if (options && typeof options === 'object' && options.throwIfNoEntry === false) {
  const { lstatSync } = fs;
  fs.lstatSync = (pathArgument: string, options: any) => {
    if (!pathInfo.isAsar) return lstatSync(pathArgument, options);
      if (shouldThrowStatError(options)) {
        throw createError(AsarError.INVALID_ARCHIVE, { asarPath });
    const stats = archive.stat(filePath);
    if (!stats) {
        throw createError(AsarError.NOT_FOUND, { asarPath, filePath });
    return asarStatsToFsStats(stats);
  const { lstat } = fs;
  fs.lstat = (pathArgument: string, options: any, callback: any) => {
    if (typeof options === 'function') {
      callback = options;
    if (!pathInfo.isAsar) return lstat(pathArgument, options, callback);
    const fsStats = asarStatsToFsStats(stats);
    nextTick(callback, [null, fsStats]);
  fs.promises.lstat = util.promisify(fs.lstat);
  const { statSync } = fs;
  fs.statSync = (pathArgument: string, options: any) => {
    const { isAsar } = splitPath(pathArgument);
    if (!isAsar) return statSync(pathArgument, options);
    // Do not distinguish links for now.
    return fs.lstatSync(pathArgument, options);
  const { stat } = fs;
  fs.stat = (pathArgument: string, options: any, callback: any) => {
    if (!isAsar) return stat(pathArgument, options, callback);
    process.nextTick(() => fs.lstat(pathArgument, options, callback));
  fs.promises.stat = util.promisify(fs.stat);
  const wrapRealpathSync = function (realpathSync: Function) {
    return function (this: any, pathArgument: string, options: any) {
      if (!pathInfo.isAsar) return realpathSync.apply(this, arguments);
      const fileRealPath = archive.realpath(filePath);
      if (fileRealPath === false) {
      return path.join(realpathSync(asarPath, options), fileRealPath);
  const { realpathSync } = fs;
  fs.realpathSync = wrapRealpathSync(realpathSync);
  fs.realpathSync.native = wrapRealpathSync(realpathSync.native);
  const wrapRealpath = function (realpath: Function) {
    return function (this: any, pathArgument: string, options: any, callback: any) {
      if (!pathInfo.isAsar) return realpath.apply(this, arguments);
      if (arguments.length < 3) {
      realpath(asarPath, options, (error: Error | null, archiveRealPath: string) => {
        if (error === null) {
          const fullPath = path.join(archiveRealPath, fileRealPath);
          callback(null, fullPath);
  const { realpath } = fs;
  fs.realpath = wrapRealpath(realpath);
  fs.realpath.native = wrapRealpath(realpath.native);
  fs.promises.realpath = util.promisify(fs.realpath.native);
  const { exists: nativeExists } = fs;
  fs.exists = function exists (pathArgument: string, callback: any) {
    let pathInfo: ReturnType<typeof splitPath>;
      pathInfo = splitPath(pathArgument);
      nextTick(callback, [false]);
    if (!pathInfo.isAsar) return nativeExists(pathArgument, callback);
    const pathExists = (archive.stat(filePath) !== false);
    nextTick(callback, [pathExists]);
  fs.exists[util.promisify.custom] = function exists (pathArgument: string) {
    if (!pathInfo.isAsar) return nativeExists[util.promisify.custom](pathArgument);
    return Promise.resolve(archive.stat(filePath) !== false);
  const { existsSync } = fs;
  fs.existsSync = (pathArgument: string) => {
    if (!pathInfo.isAsar) return existsSync(pathArgument);
    if (!archive) return false;
    return archive.stat(filePath) !== false;
  const { access } = fs;
  fs.access = function (pathArgument: string, mode: number, callback: any) {
    if (!pathInfo.isAsar) return access.apply(this, arguments);
    if (typeof mode === 'function') {
      callback = mode;
      mode = fs.constants.F_OK;
    const info = archive.getFileInfo(filePath);
    if (info.unpacked) {
      const realPath = archive.copyFileOut(filePath);
      return fs.access(realPath, mode, callback);
    if (mode & fs.constants.W_OK) {
      const error = createError(AsarError.NO_ACCESS, { asarPath, filePath });
    nextTick(callback);
  const { access: accessPromise } = fs.promises;
  fs.promises.access = function (pathArgument: string, mode: number) {
    if (!pathInfo.isAsar) {
      return accessPromise.apply(this, arguments);
    const p = util.promisify(fs.access);
    return p(pathArgument, mode);
  const { accessSync } = fs;
  fs.accessSync = function (pathArgument: string, mode: any) {
    if (!pathInfo.isAsar) return accessSync.apply(this, arguments);
    if (mode == null) mode = fs.constants.F_OK;
      return fs.accessSync(realPath, mode);
      throw createError(AsarError.NO_ACCESS, { asarPath, filePath });
  function fsReadFileAsar (pathArgument: string, options: any, callback: any) {
    if (pathInfo.isAsar) {
        options = { encoding: null };
        options = { encoding: options };
      } else if (options === null || options === undefined) {
      } else if (typeof options !== 'object') {
        throw new TypeError('Bad arguments');
      const { encoding } = options;
      if (info.size === 0) {
        nextTick(callback, [null, encoding ? '' : Buffer.alloc(0)]);
        return fs.readFile(realPath, options, callback);
      const buffer = Buffer.alloc(info.size);
      const fd = archive.getFdAndValidateIntegrityLater();
      if (!(fd >= 0)) {
      logASARAccess(asarPath, filePath, info.offset);
      fs.read(fd, buffer, 0, info.size, info.offset, (error: Error) => {
        validateBufferIntegrity(buffer, info.integrity);
        callback(error, encoding ? buffer.toString(encoding) : buffer);
  const { readFile } = fs;
  fs.readFile = function (pathArgument: string, options: any, callback: any) {
      return readFile.apply(this, arguments);
    return fsReadFileAsar(pathArgument, options, callback);
  const { readFile: readFilePromise } = fs.promises;
  fs.promises.readFile = function (pathArgument: string, options: any) {
      return readFilePromise.apply(this, arguments);
    const p = util.promisify(fsReadFileAsar);
    return p(pathArgument, options);
  function readFileFromArchiveSync (
    pathInfo: { asarPath: string; filePath: string },
    options: any
  ): ReturnType<typeof readFileSync> {
    if (!info) throw createError(AsarError.NOT_FOUND, { asarPath, filePath });
    if (info.size === 0) return (options) ? '' : Buffer.alloc(0);
      return fs.readFileSync(realPath, options);
    fs.readSync(fd, buffer, 0, info.size, info.offset);
    return (encoding) ? buffer.toString(encoding) : buffer;
  const { readFileSync } = fs;
  fs.readFileSync = function (pathArgument: string, options: any) {
    if (!pathInfo.isAsar) return readFileSync.apply(this, arguments);
    return readFileFromArchiveSync(pathInfo, options);
  type ReaddirOptions = { encoding: BufferEncoding | null; withFileTypes?: false, recursive?: false } | undefined | null;
  type ReaddirCallback = (err: NodeJS.ErrnoException | null, files?: string[]) => void;
  const processReaddirResult = (args: any) => (args.context.withFileTypes ? handleDirents(args) : handleFilePaths(args));
  function handleDirents ({ result, currentPath, context }: { result: any[], currentPath: string, context: any }) {
    const length = result[0].length;
      const resultPath = path.join(currentPath, result[0][i]);
      const info = splitPath(resultPath);
      let type = result[1][i];
        if (!archive) return;
      const dirent = getDirent(currentPath, result[0][i], type);
      const stat = internalBinding('fs').internalModuleStat(resultPath);
      context.readdirResults.push(dirent);
      if (dirent!.isDirectory() || stat === 1) {
        context.pathsQueue.push(path.join(dirent!.parentPath, dirent!.name));
  function handleFilePaths ({ result, currentPath, context }: { result: string[], currentPath: string, context: any }) {
    for (let i = 0; i < result.length; i++) {
      const resultPath = path.join(currentPath, result[i]);
      const relativeResultPath = path.relative(context.basePath, resultPath);
      context.readdirResults.push(relativeResultPath);
      if (stat === 1) {
        context.pathsQueue.push(resultPath);
  function readdirRecursive (basePath: string, options: ReaddirOptions, callback: ReaddirCallback) {
      withFileTypes: Boolean(options!.withFileTypes),
      encoding: options!.encoding,
      readdirResults: [],
      pathsQueue: [basePath]
    function read (pathArg: string) {
      const req = new binding.FSReqCallback();
      req.oncomplete = (err: any, result: string) => {
          callback(null, context.readdirResults);
        processReaddirResult({
          currentPath: pathArg,
        if (i < context.pathsQueue.length) {
          read(context.pathsQueue[i++]);
      const pathInfo = splitPath(pathArg);
        let readdirResult;
        readdirResult = archive.readdir(filePath);
        if (!readdirResult) {
        // If we're in an asar dir, we need to ensure the result is in the same format as the
        // native call to readdir withFileTypes i.e. an array of arrays.
        if (context.withFileTypes) {
          readdirResult = [
            [...readdirResult], readdirResult.map((p: string) => {
              return internalBinding('fs').internalModuleStat(path.join(pathArg, p));
          result: readdirResult,
        binding.readdir(
          pathArg,
          context.encoding,
          context.withFileTypes,
          req
  const { readdir } = fs;
  fs.readdir = function (pathArgument: string, options: ReaddirOptions, callback: ReaddirCallback) {
    callback = typeof options === 'function' ? options : callback;
    validateFunction(callback, 'callback')!;
    options = getOptions(options);
    pathArgument = getValidatedPath(pathArgument);
    if (options?.recursive != null) {
      validateBoolean(options?.recursive, 'options.recursive')!;
    if (options?.recursive) {
      readdirRecursive(pathArgument, options, callback);
    if (!pathInfo.isAsar) return readdir.apply(this, arguments);
      nextTick(callback!, [error]);
    const files = archive.readdir(filePath);
    if (!files) {
    if (options?.withFileTypes) {
      const dirents = [];
        const childPath = path.join(filePath, file);
        const stats = archive.stat(childPath);
          const error = createError(AsarError.NOT_FOUND, { asarPath, filePath: childPath });
        dirents.push(new fs.Dirent(file, stats.type));
      nextTick(callback!, [null, dirents]);
    nextTick(callback!, [null, files]);
  const { readdir: readdirPromise } = fs.promises;
  fs.promises.readdir = async function (pathArgument: string, options: ReaddirOptions) {
      return readdirRecursivePromises(pathArgument, options);
    if (!pathInfo.isAsar) return readdirPromise(pathArgument, options);
          throw createError(AsarError.NOT_FOUND, { asarPath, filePath: childPath });
      return Promise.resolve(dirents);
    return Promise.resolve(files);
  const { readdirSync } = fs;
  fs.readdirSync = function (pathArgument: string, options: ReaddirOptions) {
      return readdirSyncRecursive(pathArgument, options);
    if (!pathInfo.isAsar) return readdirSync.apply(this, arguments);
      return dirents;
  const modBinding = internalBinding('modules');
  modBinding.overrideReadFileSync((jsonPath: string): Buffer | false | undefined => {
    const pathInfo = splitPath(jsonPath);
    // Fallback to Node.js internal implementation
    if (!pathInfo.isAsar) return undefined;
      return readFileFromArchiveSync(pathInfo, undefined);
      // Not found
  const { internalModuleStat } = binding;
  internalBinding('fs').internalModuleStat = (pathArgument: string) => {
    if (!pathInfo.isAsar) return internalModuleStat(pathArgument);
    // -ENOENT
    if (!archive) return -34;
    if (!stats) return -34;
    return (stats.type === AsarFileType.kDirectory) ? 1 : 0;
  const { kUsePromises } = binding;
  async function readdirRecursivePromises (originalPath: string, options: ReaddirOptions) {
    const pathInfo = splitPath(originalPath);
    let queue: [string, string[]][] = [];
    const withFileTypes = Boolean(options?.withFileTypes);
    let initialItem = [];
      const archive = getOrCreateArchive(pathInfo.asarPath);
      if (!archive) return result;
      const files = archive.readdir(pathInfo.filePath);
      if (!files) return result;
      initialItem = files;
      if (withFileTypes) {
        initialItem = [
          [...initialItem], initialItem.map((p: string) => {
            return internalBinding('fs').internalModuleStat(path.join(originalPath, p));
      initialItem = await binding.readdir(
        path.toNamespacedPath(originalPath),
        options!.encoding,
        withFileTypes,
        kUsePromises
    queue = [[originalPath, initialItem]];
        // @ts-expect-error this is a valid array destructure assignment.
        const { 0: pathArg, 1: readDir } = queue.pop();
        for (const dirent of getDirents(pathArg, readDir)) {
          result.push(dirent);
          if (dirent.isDirectory()) {
            const direntPath = path.join(pathArg, dirent.name);
            const info = splitPath(direntPath);
              const files = archive.readdir(info.filePath);
              if (!files) continue;
                [...files], files.map((p: string) => {
                  return internalBinding('fs').internalModuleStat(path.join(direntPath, p));
              readdirResult = await binding.readdir(
                direntPath,
            queue.push([direntPath, readdirResult]);
        for (const ent of readDir) {
          const direntPath = path.join(pathArg, ent);
          const stat = internalBinding('fs').internalModuleStat(direntPath);
          result.push(path.relative(originalPath, direntPath));
            const subPathInfo = splitPath(direntPath);
            let item = [];
            if (subPathInfo.isAsar) {
              const archive = getOrCreateArchive(subPathInfo.asarPath);
              const files = archive.readdir(subPathInfo.filePath);
              item = files;
              item = await binding.readdir(
                path.toNamespacedPath(direntPath),
            queue.push([direntPath, item]);
  function readdirSyncRecursive (basePath: string, options: ReaddirOptions) {
      readdirResults: [] as any,
        if (!readdirResult) return;
        readdirResult = binding.readdir(
          path.toNamespacedPath(pathArg),
          context.withFileTypes
      if (readdirResult === undefined) {
    for (let i = 0; i < context.pathsQueue.length; i++) {
      read(context.pathsQueue[i]);
    return context.readdirResults;
  // Calling mkdir for directory inside asar archive should throw ENOTDIR
  // error, but on Windows it throws ENOENT.
    const { mkdir } = fs;
    fs.mkdir = (pathArgument: string, options: any, callback: any) => {
      if (pathInfo.isAsar && pathInfo.filePath.length > 0) {
        const error = createError(AsarError.NOT_DIR);
      mkdir(pathArgument, options, callback);
    fs.promises.mkdir = util.promisify(fs.mkdir);
    const { mkdirSync } = fs;
    fs.mkdirSync = function (pathArgument: string, options: any) {
      if (pathInfo.isAsar && pathInfo.filePath.length) throw createError(AsarError.NOT_DIR);
      return mkdirSync(pathArgument, options);
  function invokeWithNoAsar (func: Function) {
      const processNoAsarOriginalValue = process.noAsar;
      process.noAsar = true;
        return func.apply(this, arguments);
        process.noAsar = processNoAsarOriginalValue;
  // Strictly implementing the flags of fs.copyFile is hard, just do a simple
  // implementation for now. Doing 2 copies won't spend much time more as OS
  // has filesystem caching.
  overrideAPI(fs, 'copyFile');
  overrideAPISync(fs, 'copyFileSync');
  overrideAPI(fs, 'cp');
  overrideAPISync(fs, 'cpSync');
  overrideAPI(fs, 'open');
  overrideAPISync(process, 'dlopen', 1);
  overrideAPISync(Module._extensions, '.node', 1);
  overrideAPISync(fs, 'openSync');
  const overrideChildProcess = (childProcess: Record<string, any>) => {
    // Executing a command string containing a path to an asar archive
    // confuses `childProcess.execFile`, which is internally called by
    // `childProcess.{exec,execSync}`, causing Electron to consider the full
    // command as a single path to an archive.
    const { exec, execSync } = childProcess;
    childProcess.exec = invokeWithNoAsar(exec);
    childProcess.exec[util.promisify.custom] = assignFunctionName('exec', invokeWithNoAsar(exec[util.promisify.custom]));
    childProcess.execSync = invokeWithNoAsar(execSync);
    overrideAPI(childProcess, 'execFile');
    overrideAPISync(childProcess, 'execFileSync');
  const asarReady = new WeakSet();
  // Lazily override the child_process APIs only when child_process is
  // fetched the first time.  We will eagerly override the child_process APIs
  // when this env var is set so that stack traces generated inside node unit
  // tests will match. This env var will only slow things down in users apps
  // and should not be used.
  if (process.env.ELECTRON_EAGER_ASAR_HOOK_FOR_TESTING) {
    overrideChildProcess(require('child_process'));
    const originalModuleLoad = Module._load;
    Module._load = (request: string, ...args: any[]) => {
      const loadResult = originalModuleLoad(request, ...args);
      if (request === 'child_process' || request === 'node:child_process') {
        if (!asarReady.has(loadResult)) {
          asarReady.add(loadResult);
          // Just to make it obvious what we are dealing with here
          const childProcess = loadResult;
          overrideChildProcess(childProcess);
      return loadResult;
