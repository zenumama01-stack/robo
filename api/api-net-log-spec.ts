import { session, net } from 'electron/main';
import * as ChildProcess from 'node:child_process';
import { Socket } from 'node:net';
const appPath = path.join(__dirname, 'fixtures', 'api', 'net-log');
const dumpFile = path.join(os.tmpdir(), 'net_log.json');
const dumpFileDynamic = path.join(os.tmpdir(), 'net_log_dynamic.json');
const testNetLog = () => session.fromPartition('net-log').netLog;
describe('netLog module', () => {
  const connections: Set<Socket> = new Set();
    server = http.createServer();
    server.on('connection', (connection) => {
      connections.add(connection);
      connection.once('close', () => {
        connections.delete(connection);
    server.on('request', (request, response) => {
    for (const connection of connections) {
      connection.destroy();
    expect(testNetLog().currentlyLogging).to.be.false('currently logging');
      if (fs.existsSync(dumpFile)) {
        fs.unlinkSync(dumpFile);
      if (fs.existsSync(dumpFileDynamic)) {
        fs.unlinkSync(dumpFileDynamic);
      // Ignore error
  it('should begin and end logging to file when .startLogging() and .stopLogging() is called', async () => {
    await testNetLog().startLogging(dumpFileDynamic);
    expect(testNetLog().currentlyLogging).to.be.true('currently logging');
    await testNetLog().stopLogging();
    expect(fs.existsSync(dumpFileDynamic)).to.be.true('currently logging');
  it('should throw an error when .stopLogging() is called without calling .startLogging()', async () => {
    await expect(testNetLog().stopLogging()).to.be.rejectedWith('No net log in progress');
  it('should throw an error when .startLogging() is called with an invalid argument', () => {
    expect(() => testNetLog().startLogging('')).to.throw();
    expect(() => testNetLog().startLogging(null as any)).to.throw();
    expect(() => testNetLog().startLogging([] as any)).to.throw();
    expect(() => testNetLog().startLogging('aoeu', { captureMode: 'aoeu' as any })).to.throw();
    expect(() => testNetLog().startLogging('aoeu', { maxFileSize: null as any })).to.throw();
  it('should include cookies when requested', async () => {
    await testNetLog().startLogging(dumpFileDynamic, { captureMode: 'includeSensitive' });
    const unique = require('uuid').v4();
      const req = net.request(serverUrl);
      req.setHeader('Cookie', `foo=${unique}`);
      req.on('response', (response) => {
        response.on('data', () => {}); // https://github.com/electron/electron/issues/19214
        response.on('end', () => resolve());
    expect(fs.existsSync(dumpFileDynamic)).to.be.true('dump file exists');
    const dump = fs.readFileSync(dumpFileDynamic, 'utf8');
    expect(dump).to.contain(`foo=${unique}`);
  it('should include socket bytes when requested', async () => {
    await testNetLog().startLogging(dumpFileDynamic, { captureMode: 'everything' });
      const req = net.request({ method: 'POST', url: serverUrl });
      req.end(Buffer.from(unique));
    expect(JSON.parse(dump).events.some((x: any) => x.params && x.params.bytes && Buffer.from(x.params.bytes, 'base64').includes(unique))).to.be.true('uuid present in dump');
  ifit(process.platform !== 'linux')('should begin and end logging automatically when --log-net-log is passed', async () => {
    const appProcess = ChildProcess.spawn(process.execPath,
      [appPath], {
          TEST_REQUEST_URL: serverUrl,
          TEST_DUMP_FILE: dumpFile
    expect(fs.existsSync(dumpFile)).to.be.true('dump file exists');
  ifit(process.platform !== 'linux')('should begin and end logging automatically when --log-net-log is passed, and behave correctly when .startLogging() and .stopLogging() is called', async () => {
          TEST_DUMP_FILE: dumpFile,
          TEST_DUMP_FILE_DYNAMIC: dumpFileDynamic,
          TEST_MANUAL_STOP: 'true'
    expect(fs.existsSync(dumpFileDynamic)).to.be.true('dynamic dump file exists');
  ifit(process.platform !== 'linux')('should end logging automatically when only .startLogging() is called', async () => {
          TEST_DUMP_FILE_DYNAMIC: dumpFileDynamic
