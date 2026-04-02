import { app } from 'electron';
import { startRemoteControlApp, ifdescribe, ifit, isTestingBindingAvailable } from './lib/spec-helpers';
// This test depends on functions that are only available when DCHECK_IS_ON.
ifdescribe(isTestingBindingAvailable())('logging', () => {
  it('does not log by default', async () => {
    // ELECTRON_ENABLE_LOGGING might be set in the environment, so remove it
    const { ELECTRON_ENABLE_LOGGING: _, ...envWithoutEnableLogging } = process.env;
    const rc = await startRemoteControlApp([], { env: envWithoutEnableLogging });
      rc.process.stderr!.on('data', function listener (chunk) {
    const [hasLoggingSwitch, hasLoggingVar] = await rc.remotely(() => {
      // Make sure we're actually capturing stderr by logging a known value to
      // stderr.
      console.error('SENTINEL');
      process._linkedBinding('electron_common_testing').log(0, 'TEST_LOG');
      setTimeout(() => { process.exit(0); });
      return [require('electron').app.commandLine.hasSwitch('enable-logging'), !!process.env.ELECTRON_ENABLE_LOGGING];
    expect(hasLoggingSwitch).to.be.false();
    expect(hasLoggingVar).to.be.false();
    // stderr should include the sentinel but not the LOG() message.
    expect(stderr).to.match(/SENTINEL/);
    expect(stderr).not.to.match(/TEST_LOG/);
  it('logs to stderr when --enable-logging is passed', async () => {
    const rc = await startRemoteControlApp(['--enable-logging']);
      setTimeout(() => { require('electron').app.quit(); });
    expect(stderr).to.match(/TEST_LOG/);
  it('logs to stderr when ELECTRON_ENABLE_LOGGING is set', async () => {
    const rc = await startRemoteControlApp([], { env: { ...process.env, ELECTRON_ENABLE_LOGGING: '1' } });
  it('logs to a file in the user data dir when --enable-logging=file is passed', async () => {
    const rc = await startRemoteControlApp(['--enable-logging=file']);
    const userDataDir = await rc.remotely(() => {
      setTimeout(() => { app.quit(); });
      return app.getPath('userData');
    await once(rc.process, 'exit');
    const logFilePath = path.join(userDataDir, 'electron_debug.log');
    const stat = await fs.stat(logFilePath);
    expect(stat.isFile()).to.be.true();
    const contents = await fs.readFile(logFilePath, 'utf8');
    expect(contents).to.match(/TEST_LOG/);
  it('logs to a file in the user data dir when ELECTRON_ENABLE_LOGGING=file is set', async () => {
    const rc = await startRemoteControlApp([], { env: { ...process.env, ELECTRON_ENABLE_LOGGING: 'file' } });
  it('logs to the given file when --log-file is passed', async () => {
    const logFilePath = path.join(app.getPath('temp'), 'test-log-file-' + uuid.v4());
    const rc = await startRemoteControlApp(['--enable-logging', '--log-file=' + logFilePath]);
  ifit(process.platform === 'win32')('child process logs to the given file when --log-file is passed', async () => {
    const preloadPath = path.resolve(__dirname, 'fixtures', 'log-test.js');
    const rc = await startRemoteControlApp(['--enable-logging', `--log-file=${logFilePath}`, `--boot-eval=preloadPath=${JSON.stringify(preloadPath)}`]);
      process._linkedBinding('electron_common_testing').log(0, 'MAIN_PROCESS_TEST_LOG');
      const { app, BrowserWindow } = require('electron');
    expect(contents).to.match(/MAIN_PROCESS_TEST_LOG/);
    expect(contents).to.match(/CHILD_PROCESS_TEST_LOG/);
    expect(contents).to.match(/CHILD_PROCESS_DESTINATION_handle/);
  it('logs to the given file when ELECTRON_LOG_FILE is set', async () => {
    const rc = await startRemoteControlApp([], { env: { ...process.env, ELECTRON_ENABLE_LOGGING: '1', ELECTRON_LOG_FILE: logFilePath } });
  it('does not lose early log messages when logging to a given file with --log-file', async () => {
    const rc = await startRemoteControlApp(['--enable-logging', '--log-file=' + logFilePath, '--boot-eval=process._linkedBinding(\'electron_common_testing\').log(0, \'EARLY_LOG\')']);
      process._linkedBinding('electron_common_testing').log(0, 'LATER_LOG');
    expect(contents).to.match(/EARLY_LOG/);
    expect(contents).to.match(/LATER_LOG/);
  it('enables logging when switch is appended during first tick', async () => {
    const rc = await startRemoteControlApp(['--boot-eval=require(\'electron\').app.commandLine.appendSwitch(\'--enable-logging\')']);
  it('respects --log-level', async () => {
    const rc = await startRemoteControlApp(['--enable-logging', '--log-level=1']);
      process._linkedBinding('electron_common_testing').log(0, 'TEST_INFO_LOG');
      process._linkedBinding('electron_common_testing').log(1, 'TEST_WARNING_LOG');
    expect(stderr).to.match(/TEST_WARNING_LOG/);
    expect(stderr).not.to.match(/TEST_INFO_LOG/);
