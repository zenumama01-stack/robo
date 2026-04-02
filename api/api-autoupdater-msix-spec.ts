  getElectronExecutable,
  getMainJsFixturePath,
  getMsixFixturePath,
  getMsixPackageVersion,
  installMsixCertificate,
  installMsixPackage,
  registerExecutableWithIdentity,
  shouldRunMsixTests,
  spawn,
  uninstallMsixPackage,
  unregisterExecutableWithIdentity
} from './lib/msix-helpers';
import { ifdescribe } from './lib/spec-helpers';
const ELECTRON_MSIX_ALIAS = 'ElectronMSIX.exe';
const MAIN_JS_PATH = getMainJsFixturePath();
const MSIX_V1 = getMsixFixturePath('v1');
const MSIX_V2 = getMsixFixturePath('v2');
// We can only test the MSIX updater on Windows
ifdescribe(shouldRunMsixTests)('autoUpdater MSIX behavior', function () {
  before(async function () {
    await installMsixCertificate();
    const electronExec = getElectronExecutable();
    await registerExecutableWithIdentity(electronExec);
  after(async function () {
    await unregisterExecutableWithIdentity();
  const launchApp = (executablePath: string, args: string[] = []) => {
    return spawn(executablePath, args);
  it('should launch Electron via MSIX alias', async () => {
    const launchResult = await launchApp(ELECTRON_MSIX_ALIAS, ['--version']);
  it('should print package identity information', async () => {
    const launchResult = await launchApp(ELECTRON_MSIX_ALIAS, [MAIN_JS_PATH, '--printPackageId']);
      expect(launchResult.out).to.include('Family Name: Electron.Dev.MSIX_rdjwn13tdj8dy');
      expect(launchResult.out).to.include('Package ID: Electron.Dev.MSIX_1.0.0.0_x64__rdjwn13tdj8dy');
      expect(launchResult.out).to.include('Version: 1.0.0.0');
      await uninstallMsixPackage('com.electron.myapp');
    it('should not update when no update is available', async () => {
      const launchResult = await launchApp(ELECTRON_MSIX_ALIAS, [MAIN_JS_PATH, '--checkUpdate', `http://localhost:${port}/update-check`]);
        expect(requests.length).to.be.greaterThan(0);
        expect(launchResult.out).to.include('Checking for update...');
        expect(launchResult.out).to.include('Update not available');
    it('should hit the update endpoint with custom headers when checkForUpdates is called', async () => {
        expect(req.headers['x-appversion']).to.equal('1.0.0');
        expect(req.headers.authorization).to.equal('Bearer test-token');
      const launchResult = await launchApp(ELECTRON_MSIX_ALIAS, [
        MAIN_JS_PATH,
        '--checkUpdate',
        `http://localhost:${port}/update-check`,
        '--useCustomHeaders'
    it('should update successfully with direct link to MSIX file', async () => {
      await installMsixPackage(MSIX_V1);
      const initialVersion = await getMsixPackageVersion('com.electron.myapp');
      expect(initialVersion).to.equal('1.0.0.0');
      server.get('/update.msix', (req, res) => {
        res.download(MSIX_V2);
        `http://localhost:${port}/update.msix`
        expect(launchResult.out).to.include('Update available');
        expect(launchResult.out).to.include('Update downloaded');
        expect(launchResult.out).to.include('Release Name: N/A');
        expect(launchResult.out).to.include('Release Notes: N/A');
        expect(launchResult.out).to.include(`Update URL: http://localhost:${port}/update.msix`);
      const updatedVersion = await getMsixPackageVersion('com.electron.myapp');
      expect(updatedVersion).to.equal('2.0.0.0');
    it('should update successfully with JSON response', async () => {
      const fixedPubDate = '2011-11-11T11:11:11.000Z';
      const expectedDateStr = new Date(fixedPubDate).toDateString();
          url: `http://localhost:${port}/update.msix`,
          name: '2.0.0',
          notes: 'Test release notes',
          pub_date: fixedPubDate
        expect(launchResult.out).to.include('Release Name: 2.0.0');
        expect(launchResult.out).to.include('Release Notes: Test release notes');
        expect(launchResult.out).to.include(`Release Date: ${expectedDateStr}`);
    it('should update successfully with static JSON releases file', async () => {
                url: `http://localhost:${port}/update-v1.msix`,
                name: '1.0.0',
                notes: 'Initial release',
                pub_date: '2010-10-10T10:10:10.000Z'
                url: `http://localhost:${port}/update-v2.msix`,
                notes: 'Test release notes for static format',
      server.get('/update-v2.msix', (req, res) => {
        expect(launchResult.out).to.include('Release Notes: Test release notes for static format');
        expect(launchResult.out).to.include(`Update URL: http://localhost:${port}/update-v2.msix`);
    it('should not update with update File JSON Format if currentRelease is older than installed version', async () => {
      await installMsixPackage(MSIX_V2);
          currentRelease: '1.0.0',
    it('should downgrade to older version with JSON server format and allowAnyVersion is true', async () => {
      expect(initialVersion).to.equal('2.0.0.0');
      const fixedPubDate = '2010-10-10T10:10:10.000Z';
      server.get('/update-v1.msix', (req, res) => {
        res.download(MSIX_V1);
      const launchResult = await launchApp(ELECTRON_MSIX_ALIAS, [MAIN_JS_PATH, '--checkUpdate', `http://localhost:${port}/update-check`, '--allowAnyVersion']);
        expect(launchResult.out).to.include('Release Name: 1.0.0');
        expect(launchResult.out).to.include('Release Notes: Initial release');
        expect(launchResult.out).to.include(`Update URL: http://localhost:${port}/update-v1.msix`);
      const downgradedVersion = await getMsixPackageVersion('com.electron.myapp');
      expect(downgradedVersion).to.equal('1.0.0.0');
