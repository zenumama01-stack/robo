import baseConfig from '@jupyterlab/galata/lib/playwright-config';
  use: {
    appPath: '',
    trace: 'on-first-retry',
    video: 'retain-on-failure',
  retries: 1,
  webServer: [
      command: 'jlpm start',
      port: 8888,
      timeout: 120 * 1000,
      reuseExistingServer: true,
      stdout: 'pipe',
