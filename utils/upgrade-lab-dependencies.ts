const PACKAGE_JSON_PATHS: string[] = [
  'app/package.json',
  'buildutils/package.json',
  'packages/application-extension/package.json',
  'packages/application/package.json',
  'packages/console-extension/package.json',
  'packages/docmanager-extension/package.json',
  'packages/documentsearch-extension/package.json',
  'packages/help-extension/package.json',
  'packages/lab-extension/package.json',
  'packages/notebook-extension/package.json',
  'packages/terminal-extension/package.json',
  'packages/tree-extension/package.json',
  'packages/tree/package.json',
  'packages/ui-components/package.json',
  'ui-tests/package.json',
const DEPENDENCY_GROUP = '@jupyterlab';
interface IVersion {
  preRelease?: string;
function parseVersion(version: string): IVersion {
  const match = version.match(/^(\d+)\.(\d+)\.(\d+)(?:(a|b|rc)(\d+))?$/);
    throw new Error(`Invalid version format: ${version}`);
  const [, major, minor, patch, type, preVersion] = match;
  const baseVersion = {
    major: parseInt(major, 10),
    minor: parseInt(minor, 10),
    patch: parseInt(patch, 10),
  if (type && preVersion) {
      ...baseVersion,
      preRelease: `${type}${preVersion}`,
  return baseVersion;
function getVersionRange(version: IVersion): string {
  const baseVersion = `${version.major}.${version.minor}.${version.patch}${
    version.preRelease ?? ''
  return `>=${baseVersion},<${version.major}.${version.minor + 1}`;
function updateVersionInFile(
  pattern: RegExp,
  version: IVersion
  const versionRange = getVersionRange(version);
  const updatedContent = content.replace(pattern, `$1${versionRange}`);
  fs.writeFileSync(filePath, updatedContent);
async function updatePackageJson(newVersion: string): Promise<void> {
  const url = `https://raw.githubusercontent.com/jupyterlab/jupyterlab/v${newVersion}/jupyterlab/staging/package.json`;
    const errorMessage = `Failed to fetch package.json from ${url}. HTTP status code: ${response.status}`;
  // fetch the new galata version
  const galataUrl = `https://raw.githubusercontent.com/jupyterlab/jupyterlab/v${newVersion}/galata/package.json`;
  const galataResponse = await fetch(galataUrl);
  if (!galataResponse.ok) {
    const errorMessage = `Failed to fetch galata/package.json from ${galataUrl}. HTTP status code: ${galataResponse.status}`;
  const newPackageJson = await response.json();
  const galataPackageJson = await galataResponse.json();
  for (const packageJsonPath of PACKAGE_JSON_PATHS) {
    const filePath: string = path.resolve(packageJsonPath);
    const existingPackageJson = JSON.parse(fs.readFileSync(filePath, 'utf-8'));
    const newDependencies = {
      ...newPackageJson.devDependencies,
      ...newPackageJson.resolutions,
      [galataPackageJson.name]: galataPackageJson.version,
    updateDependencyVersion(existingPackageJson, newDependencies);
      JSON.stringify(existingPackageJson, null, 2) + '\n'
function updateDependencyVersion(existingJson: any, newJson: any): void {
  if (!existingJson) {
  const sectionPaths: string[] = [
    'resolutions',
    'dependencies',
    'devDependencies',
  for (const section of sectionPaths) {
    if (!existingJson[section]) {
    const updated = existingJson[section];
    for (const [pkg, version] of Object.entries<string>(
      existingJson[section]
      if (pkg.startsWith(DEPENDENCY_GROUP) && pkg in newJson) {
        if (version[0] === '^' || version[0] === '~') {
          updated[pkg] = version[0] + absoluteVersion(newJson[pkg]);
          updated[pkg] = absoluteVersion(newJson[pkg]);
function absoluteVersion(version: string): string {
  if (version.length > 0 && (version[0] === '^' || version[0] === '~')) {
const versionPattern = /(jupyterlab)(>=[\d.]+(?:[a|b|rc]\d+)?,<[\d.]+)/g;
const FILES_TO_UPDATE = ['pyproject.toml', '.pre-commit-config.yaml'];
async function upgradeLabDependencies(): Promise<void> {
    throw new Error('Please provide the set-version flag and version');
  const version = parseVersion(args[1]);
  await updatePackageJson(args[1]); // Keep original string version for package.json
  for (const file of FILES_TO_UPDATE) {
    updateVersionInFile(path.resolve(file), versionPattern, version);
upgradeLabDependencies();
