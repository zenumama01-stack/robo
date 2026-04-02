import { runReleaseCIJobs } from '../run-release-ci-jobs';
const { values: { ghRelease, job, arch, ci, newVersion }, positionals } = parseArgs({
    ghRelease: {
    job: {
    arch: {
    ci: {
    newVersion: {
const targetBranch = positionals[0];
if (positionals.length < 1) {
  console.log(`Trigger CI to build release builds of electron.
  Usage: ci-release-build.js [--job=CI_JOB_NAME] [--arch=INDIVIDUAL_ARCH] [--ci=GitHubActions]
  [--ghRelease] [--commit=sha] [--newVersion=version_tag] TARGET_BRANCH
if (ci === 'GitHubActions' || !ci) {
  if (!newVersion) {
    console.error('--newVersion is required for GitHubActions');
runReleaseCIJobs(targetBranch, {
  ci: ci as 'GitHubActions',
  ghRelease,
  job: job as any,
  arch,
  newVersion: newVersion!
