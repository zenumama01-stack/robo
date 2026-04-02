import { nextVersion } from '../script/release/version-bumper';
    [key: string]: string[],
  setBranch (channel: string): void {
    this.branches[channel] = [];
  setVersion (channel: string, latestTag: string): void {
    const tags = [latestTag];
    if (channel === 'alpha') {
      const versionStrs = latestTag.split(`${channel}.`);
      const latest = parseInt(versionStrs[1]);
      for (let i = latest; i >= 1; i--) {
        tags.push(`${versionStrs[0]}${channel}.${latest - i}`);
    this.branches[channel] = tags;
    // handle for promoting from current master HEAD
    let branch = 'stable';
    const v = (args[2] === 'HEAD') ? 'stable' : args[3];
    if (v.includes('nightly')) branch = 'nightly';
    if (v.includes('alpha')) branch = 'alpha';
    if (v.includes('beta')) branch = 'beta';
    if (!this.branches[branch]) this.setBranch(branch);
    stdout = this.branches[branch].join('\n');
describe('version-bumper', () => {
  ifdescribe(!(process.platform === 'linux' && process.arch.indexOf('arm') === 0) && process.platform !== 'darwin')('nextVersion', () => {
    describe('bump versions', () => {
      const nightlyPattern = /[0-9.]*(-nightly.(\d{4})(\d{2})(\d{2}))$/g;
      const betaPattern = /[0-9.]*(-beta[0-9.]*)/g;
      it('bumps to nightly from stable', async () => {
        const version = 'v2.0.0';
        const next = await nextVersion('nightly', version);
        const matches = next.match(nightlyPattern);
      it('bumps to nightly from beta', async () => {
        const version = 'v2.0.0-beta.1';
      it('bumps to nightly from nightly', async () => {
        const version = 'v2.0.0-nightly.19950901';
      it('bumps to a nightly version above our switch from N-0-x to N-x-y branch names', async () => {
        // If it starts with v8 then we didn't bump above the 8-x-y branch
        expect(next.startsWith('v8')).to.equal(false);
      it('throws error when bumping to beta from stable', () => {
        return expect(
          nextVersion('beta', version)
        ).to.be.rejectedWith('Cannot bump to beta from stable.');
      it('bumps to beta from nightly', async () => {
        const next = await nextVersion('beta', version);
        const matches = next.match(betaPattern);
      it('bumps to beta from beta', async () => {
        const version = 'v2.0.0-beta.8';
        expect(next).to.equal('2.0.0-beta.9');
      it('bumps to beta from beta if the previous beta is at least beta.10', async () => {
        const version = 'v6.0.0-beta.15';
        expect(next).to.equal('6.0.0-beta.16');
      it('bumps to stable from beta', async () => {
        const next = await nextVersion('stable', version);
        expect(next).to.equal('2.0.0');
      it('bumps to stable from stable', async () => {
        expect(next).to.equal('2.0.1');
      it('bumps to minor from stable', async () => {
        const next = await nextVersion('minor', version);
        expect(next).to.equal('2.1.0');
      it('bumps to stable from nightly', async () => {
      it('throws on an invalid version', () => {
        const version = 'vI.AM.INVALID';
        ).to.be.rejectedWith(`Invalid current version: ${version}`);
      it('throws on an invalid bump type', () => {
          // @ts-expect-error 'WRONG' is not a valid bump type
          nextVersion('WRONG', version)
        ).to.be.rejectedWith('Invalid bump type.');
  // If we don't plan on continuing to support an alpha channel past Electron 15,
  // these tests will be removed. Otherwise, integrate into the bump versions tests
  describe('bump versions - alpha channel', () => {
    const alphaPattern = /[0-9.]*(-alpha[0-9.]*)/g;
      gitFake.branches = {};
    it('bumps to alpha from nightly', async () => {
      gitFake.setVersion('nightly', version);
      const next = await nextVersion('alpha', version);
      const matches = next.match(alphaPattern);
    it('throws error when bumping to alpha from stable', () => {
        nextVersion('alpha', version)
      ).to.be.rejectedWith('Cannot bump to alpha from stable.');
    it('bumps to alpha from alpha', async () => {
      const version = 'v2.0.0-alpha.8';
      gitFake.setVersion('alpha', version);
      expect(next).to.equal('2.0.0-alpha.9');
    it('bumps to alpha from alpha if the previous alpha is at least alpha.10', async () => {
      const version = 'v6.0.0-alpha.15';
      expect(next).to.equal('6.0.0-alpha.16');
    it('bumps to beta from alpha', async () => {
      expect(next).to.equal('2.0.0-beta.1');
