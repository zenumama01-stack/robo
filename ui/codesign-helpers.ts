const fixturesPath = path.resolve(__dirname, '..', 'fixtures');
export const shouldRunCodesignTests =
    process.platform === 'darwin' &&
    !process.mas &&
    !features.isComponentBuild();
let identity: string | null;
export function getCodesignIdentity () {
  if (identity === undefined) {
    const result = cp.spawnSync(path.resolve(__dirname, '../../script/codesign/get-trusted-identity.sh'));
    if (result.status !== 0 || result.stdout.toString().trim().length === 0) {
      identity = null;
      identity = result.stdout.toString().trim();
export async function copyMacOSFixtureApp (newDir: string, fixture: string | null = 'initial') {
  const appBundlePath = path.resolve(process.execPath, '../../..');
  const newPath = path.resolve(newDir, 'Electron.app');
  cp.spawnSync('cp', ['-R', appBundlePath, path.dirname(newPath)]);
  if (fixture) {
    const appDir = path.resolve(newPath, 'Contents/Resources/app');
    await fs.promises.mkdir(appDir, { recursive: true });
    await fs.promises.cp(path.resolve(fixturesPath, 'auto-update', fixture), appDir, { recursive: true });
  const plistPath = path.resolve(newPath, 'Contents', 'Info.plist');
    plistPath,
    (await fs.promises.readFile(plistPath, 'utf8')).replace('<key>BuildMachineOSBuild</key>', `<key>NSAppTransportSecurity</key>
    <dict>
        <key>NSAllowsArbitraryLoads</key>
        <true/>
        <key>NSExceptionDomains</key>
            <key>localhost</key>
                <key>NSExceptionAllowsInsecureHTTPLoads</key>
                <key>NSIncludesSubdomains</key>
            </dict>
    </dict><key>BuildMachineOSBuild</key>`)
  return newPath;
export function spawn (cmd: string, args: string[], opts: any = {}) {
  return new Promise<{ code: number, out: string }>((resolve) => {
        code: code!,
export function signApp (appPath: string, identity: string) {
  return spawn('codesign', ['-s', identity, '--deep', '--force', appPath]);
export function unsignApp (appPath: string) {
  return spawn('codesign', ['--remove-signature', '--deep', appPath]);
