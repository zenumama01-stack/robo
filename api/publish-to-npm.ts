import * as temp from 'temp';
import { getElectronVersion } from '../../lib/get-version';
import { getCurrentBranch, ELECTRON_DIR } from '../../lib/utils';
import { getAssetContents } from '../get-asset';
import { createGitHubTokenStrategy } from '../github-token';
import { ELECTRON_ORG, ELECTRON_REPO, ElectronReleaseRepo, NIGHTLY_REPO } from '../types';
const rootPackageJson = JSON.parse(fs.readFileSync(path.resolve(__dirname, '../../../package.json'), 'utf-8'));
rootPackageJson.name = 'electron';
if (!process.env.ELECTRON_NPM_OTP) {
  console.error('Please set ELECTRON_NPM_OTP');
let tempDir: string;
temp.track(); // track and cleanup files at exit
const files = [
  'cli.js',
  'index.js',
  'install.js',
  'package.json',
  'README.md',
  'LICENSE'
const jsonFields = [
  'name',
  'repository',
  'license',
  'author',
  'keywords'
let npmTag = '';
const currentElectronVersion = getElectronVersion();
const isNightlyElectronVersion = currentElectronVersion.includes('nightly');
  userAgent: 'electron-npm-publisher',
  return isNightlyElectronVersion ? NIGHTLY_REPO : ELECTRON_REPO;
new Promise<string>((resolve, reject) => {
  temp.mkdir('electron-npm', (err, dirPath) => {
      resolve(dirPath);
  .then((dirPath) => {
    tempDir = dirPath;
    // copy files from `/npm` to temp directory
    for (const name of files) {
      const noThirdSegment = name === 'README.md' || name === 'LICENSE';
        path.join(tempDir, name),
        fs.readFileSync(path.join(ELECTRON_DIR, noThirdSegment ? '' : 'npm', name))
    // copy from root package.json to temp/package.json
    const packageJson = require(path.join(tempDir, 'package.json'));
    for (const fieldName of jsonFields) {
      packageJson[fieldName] = rootPackageJson[fieldName];
    packageJson.version = currentElectronVersion;
      path.join(tempDir, 'package.json'),
      JSON.stringify(packageJson, null, 2)
    return octokit.repos.listReleases({
  .then((releases) => {
  // download electron.d.ts from release
    const release = releases.data.find(
      (release) => release.tag_name === `v${currentElectronVersion}`
    if (!release) {
      throw new Error(`cannot find release with tag v${currentElectronVersion}`);
    return release;
  .then(async (release) => {
    const tsdAsset = release.assets.find((asset) => asset.name === 'electron.d.ts');
    if (!tsdAsset) {
      throw new Error(`cannot find electron.d.ts from v${currentElectronVersion} release assets`);
    const typingsContent = await getAssetContents(
      targetRepo,
      tsdAsset.id
    fs.writeFileSync(path.join(tempDir, 'electron.d.ts'), typingsContent);
    const checksumsAsset = release.assets.find((asset) => asset.name === 'SHASUMS256.txt');
    if (!checksumsAsset) {
      throw new Error(`cannot find SHASUMS256.txt from v${currentElectronVersion} release assets`);
    const checksumsContent = await getAssetContents(
      checksumsAsset.id
    const checksumsObject: Record<string, string> = Object.create(null);
    for (const line of checksumsContent.trim().split('\n')) {
      const [checksum, file] = line.split(' *');
      checksumsObject[file] = checksum;
    fs.writeFileSync(path.join(tempDir, 'checksums.json'), JSON.stringify(checksumsObject, null, 2));
    const gnArgs = await fs.promises.readFile(path.resolve(ELECTRON_DIR, 'build/args/all.gn'), 'utf8');
    const abiVersionLine = gnArgs.split('\n').find((line) => line.startsWith('node_module_version = '));
    const abiVersion = abiVersionLine ? abiVersionLine.split('=')[1].trim() : null;
    if (!abiVersion) {
      throw new Error('Could not find node_module_version in GN args');
    const abiVersionFile = path.join(tempDir, 'abi_version');
    await fs.promises.writeFile(abiVersionFile, abiVersion);
    const currentBranch = await getCurrentBranch();
    if (isNightlyElectronVersion) {
      // Nightlies get published to their own module, so they should be tagged as latest
      npmTag = currentBranch === 'main' ? 'latest' : `nightly-${currentBranch}`;
      const currentJson = JSON.parse(fs.readFileSync(path.join(tempDir, 'package.json'), 'utf8'));
      currentJson.name = 'electron-nightly';
      rootPackageJson.name = 'electron-nightly';
        JSON.stringify(currentJson, null, 2)
      if (currentBranch === 'main') {
        // This should never happen, main releases should be nightly releases
        // this is here just-in-case
        throw new Error('Unreachable release phase, can\'t tag a non-nightly release on the main branch');
      } else if (!release.prerelease) {
        // Tag the release with a `2-0-x` style tag
        npmTag = currentBranch;
      } else if (release.tag_name.indexOf('alpha') > 0) {
        // Tag the release with an `alpha-3-0-x` style tag
        npmTag = `alpha-${currentBranch}`;
        // Tag the release with a `beta-3-0-x` style tag
        npmTag = `beta-${currentBranch}`;
  .then(() => childProcess.execSync('npm pack', { cwd: tempDir }))
  // test that the package can install electron prebuilt from github release
    const tarballPath = path.join(tempDir, `${rootPackageJson.name}-${currentElectronVersion}.tgz`);
      const result = childProcess.spawnSync('npm', ['install', tarballPath, '--force', '--silent'], {
        env: { ...process.env, electron_config_cache: tempDir },
        cwd: tempDir,
        return reject(new Error(`npm install failed with status ${result.status}`));
        const electronPath = require(path.resolve(tempDir, 'node_modules', rootPackageJson.name));
        if (typeof electronPath !== 'string') {
          return reject(new Error(`path to electron binary (${electronPath}) returned by the ${rootPackageJson.name} module is not a string`));
        if (!fs.existsSync(electronPath)) {
          return reject(new Error(`path to electron binary (${electronPath}) returned by the ${rootPackageJson.name} module does not exist on disk`));
        return reject(new Error(`loading the generated ${rootPackageJson.name} module failed with an error`));
      resolve(tarballPath);
  .then((tarballPath) => {
    // TODO: Remove NPX
    const existingVersionJSON = childProcess.execSync(`npx npm@7 view ${rootPackageJson.name}@${currentElectronVersion} --json`).toString('utf-8');
    // It's possible this is a re-run and we already have published the package, if not we just publish like normal
    if (!existingVersionJSON) {
      childProcess.execSync(`npm publish ${tarballPath} --tag ${npmTag} --otp=${process.env.ELECTRON_NPM_OTP}`);
    const currentTags = JSON.parse(childProcess.execSync('npm show electron dist-tags --json').toString());
    const parsedLocalVersion = semver.parse(currentElectronVersion)!;
    if (rootPackageJson.name === 'electron') {
      // We should only customly add dist tags for non-nightly releases where the package name is still
      // "electron"
      if (parsedLocalVersion.prerelease.length === 0 &&
            semver.gt(currentElectronVersion, currentTags.latest)) {
        childProcess.execSync(`npm dist-tag add electron@${currentElectronVersion} latest --otp=${process.env.ELECTRON_NPM_OTP}`);
      if (parsedLocalVersion.prerelease[0] === 'beta' &&
            semver.gt(currentElectronVersion, currentTags.beta)) {
        childProcess.execSync(`npm dist-tag add electron@${currentElectronVersion} beta --otp=${process.env.ELECTRON_NPM_OTP}`);
      if (parsedLocalVersion.prerelease[0] === 'alpha' &&
            semver.gt(currentElectronVersion, currentTags.alpha)) {
        childProcess.execSync(`npm dist-tag add electron@${currentElectronVersion} alpha --otp=${process.env.ELECTRON_NPM_OTP}`);
