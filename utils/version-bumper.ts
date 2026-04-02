import { valid, coerce, inc } from 'semver';
import { parseArgs } from 'node:util';
import { VersionBumpType } from './types';
  isNightly,
  isAlpha,
  isBeta,
  nextNightly,
  nextAlpha,
  nextBeta,
  isStable
} from './version-utils';
// run the script
  const { values: { bump, dryRun, help } } = parseArgs({
      bump: {
      dryRun: {
        type: 'boolean'
  if (!bump || help) {
    console.log(`
        Bump release version number. Possible arguments:\n
          --bump=patch to increment patch version\n
          --version={version} to set version number directly\n
          --dryRun to print the next version without updating files
        Note that you can use both --bump and --stable  simultaneously.
    if (!bump) process.exit(0);
    else process.exit(1);
  const currentVersion = getElectronVersion();
  const version = await nextVersion(bump as VersionBumpType, currentVersion);
  // print would-be new version and exit early
    console.log(`new version number would be: ${version}\n`);
  console.log(`Bumped to version: ${version}`);
// get next version for release based on [nightly, alpha, beta, stable]
export async function nextVersion (bumpType: VersionBumpType, version: string) {
  if (isNightly(version) || isAlpha(version) || isBeta(version)) {
    switch (bumpType) {
      case 'nightly':
        version = await nextNightly(version);
      case 'alpha':
        version = await nextAlpha(version);
      case 'beta':
        version = await nextBeta(version);
        version = valid(coerce(version))!;
        throw new Error('Invalid bump type.');
  } else if (isStable(version)) {
        throw new Error('Cannot bump to alpha from stable.');
        throw new Error('Cannot bump to beta from stable.');
      case 'minor':
        version = inc(version, 'minor')!;
        version = inc(version, 'patch')!;
    throw new Error(`Invalid current version: ${version}`);
