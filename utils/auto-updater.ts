  // windowsStore indicates whether the app is running as a packaged app (MSIX), even outside of the store
  if (process.windowsStore) {
    module.exports = require('./auto-updater/auto-updater-msix');
    module.exports = require('./auto-updater/auto-updater-win');
  module.exports = require('./auto-updater/auto-updater-native');
