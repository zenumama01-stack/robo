import * as fs from 'original-fs';
export async function copyApp (targetDir: string): Promise<string> {
  // On macOS we can just copy the app bundle, easier too because of symlinks
    const newPath = path.resolve(targetDir, 'Electron.app');
  // On windows and linux we should read the zip manifest files and then copy each of those files
  // one by one
  const baseDir = path.dirname(process.execPath);
  const zipManifestPath = path.resolve(__dirname, '..', '..', 'script', 'zip_manifests', `dist_zip.${process.platform === 'win32' ? 'win' : 'linux'}.${process.arch === 'ia32' ? 'x86' : process.arch}.manifest`);
  const filesToCopy = (fs.readFileSync(zipManifestPath, 'utf-8')).split('\n').filter(f => f !== 'LICENSE' && f !== 'LICENSES.chromium.html' && f !== 'version' && f.trim());
    filesToCopy.map(async rel => {
      await fs.promises.mkdir(path.dirname(path.resolve(targetDir, rel)), { recursive: true });
      fs.copyFileSync(path.resolve(baseDir, rel), path.resolve(targetDir, rel));
  return path.resolve(targetDir, path.basename(process.execPath));
export async function withTempDirectory (fn: (dir: string) => Promise<void>, autoCleanUp = true) {
  const dir = await fs.promises.mkdtemp(path.resolve(os.tmpdir(), 'electron-update-spec-'));
    await fn(dir);
    if (autoCleanUp) {
      cp.spawnSync('rm', ['-r', dir]);
