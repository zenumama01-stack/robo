import * as squirrelUpdate from '@electron/internal/browser/api/auto-updater/squirrel-update-win';
  quitAndInstall () {
      return this.emitError(new Error('No update available, can\'t quit and install'));
    squirrelUpdate.processStart();
  getPackageInfo () {
    // Squirrel-based Windows apps don't have MSIX package information
  setFeedURL (options: { url: string } | string) {
    if (!squirrelUpdate.supported()) {
      return this.emitError(new Error('Can not find Squirrel'));
      const update = await squirrelUpdate.checkForUpdate(url);
      if (update == null) {
        return this.emit('update-not-available');
      await squirrelUpdate.update(url);
      const { releaseNotes, version } = update;
      // Date is not available on Windows, so fake it.
      this.emit('update-downloaded', {}, releaseNotes, version, date, this.updateURL, () => {
