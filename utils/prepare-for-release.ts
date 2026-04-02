import { prepareRelease } from '../prepare-release';
import { ELECTRON_REPO, isVersionBumpType, NIGHTLY_REPO } from '../types';
const { values: { branch }, positionals } = parseArgs({
    branch: {
  allowPositionals: true
const bumpType = positionals[0];
if (!bumpType || !isVersionBumpType(bumpType)) {
  console.log('Usage: prepare-for-release [stable | minor | beta | alpha | nightly]' +
     ' (--branch=branch)');
prepareRelease({
  isPreRelease: bumpType !== 'stable' && bumpType !== 'minor',
  targetRepo: bumpType === 'nightly' ? NIGHTLY_REPO : ELECTRON_REPO,
  targetBranch: branch,
  bumpType
