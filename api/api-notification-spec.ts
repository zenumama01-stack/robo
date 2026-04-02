import { Notification } from 'electron/main';
describe('Notification module', () => {
    expect(Notification.prototype.constructor.name).to.equal('Notification');
  it('is supported', () => {
    expect(Notification.isSupported()).to.be.a('boolean');
  ifit(process.platform === 'darwin')('inits and gets id property', () => {
      id: 'my-custom-id',
      body: 'body'
    expect(n.id).to.equal('my-custom-id');
  ifit(process.platform === 'darwin')('id is read-only', () => {
    expect(() => { (n as any).id = 'new-id'; }).to.throw();
  ifit(process.platform === 'darwin')('defaults id to a UUID when not provided', () => {
    expect(n.id).to.be.a('string').and.not.be.empty();
    expect(n.id).to.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/);
  ifit(process.platform === 'darwin')('defaults id to a UUID when empty string is provided', () => {
  ifit(process.platform === 'darwin')('inits and gets groupId property', () => {
      groupId: 'E017VKL2N8H|C07RBMNS9EK'
    expect(n.groupId).to.equal('E017VKL2N8H|C07RBMNS9EK');
  ifit(process.platform === 'darwin')('groupId is read-only', () => {
    expect(() => { (n as any).groupId = 'new-group'; }).to.throw();
  ifit(process.platform === 'darwin')('defaults groupId to empty string when not provided', () => {
    expect(n.groupId).to.equal('');
  it('inits, gets and sets basic string properties correctly', () => {
    expect(n.title).to.equal('title');
    n.title = 'title1';
    expect(n.title).to.equal('title1');
    expect(n.subtitle).equal('subtitle');
    n.subtitle = 'subtitle1';
    expect(n.subtitle).equal('subtitle1');
    expect(n.body).to.equal('body');
    n.body = 'body1';
    expect(n.body).to.equal('body1');
    expect(n.replyPlaceholder).to.equal('replyPlaceholder');
    n.replyPlaceholder = 'replyPlaceholder1';
    expect(n.replyPlaceholder).to.equal('replyPlaceholder1');
    expect(n.sound).to.equal('sound');
    n.sound = 'sound1';
    expect(n.sound).to.equal('sound1');
    expect(n.closeButtonText).to.equal('closeButtonText');
    n.closeButtonText = 'closeButtonText1';
    expect(n.closeButtonText).to.equal('closeButtonText1');
  it('inits, gets and sets basic boolean properties correctly', () => {
      silent: true,
      hasReply: true
    expect(n.silent).to.be.true('silent');
    n.silent = false;
    expect(n.silent).to.be.false('silent');
    expect(n.hasReply).to.be.true('has reply');
    n.hasReply = false;
    expect(n.hasReply).to.be.false('has reply');
  it('inits, gets and sets actions correctly', () => {
          text: '1'
          text: '2'
    expect(n.actions.length).to.equal(2);
    expect(n.actions[0].type).to.equal('button');
    expect(n.actions[0].text).to.equal('1');
    expect(n.actions[1].type).to.equal('button');
    expect(n.actions[1].text).to.equal('2');
    n.actions = [
        text: '3'
        text: '4'
    expect(n.actions[0].text).to.equal('3');
    expect(n.actions[1].text).to.equal('4');
  it('can be shown and closed', () => {
      title: 'test notification',
      body: 'test body',
      silent: true
    n.close();
  ifit(process.platform === 'win32')('inits, gets and sets custom xml', () => {
      toastXml: '<xml/>'
    expect(n.toastXml).to.equal('<xml/>');
  ifit(process.platform === 'darwin')('emits show and close events', async () => {
      const e = once(n, 'show');
      await e;
      const e = once(n, 'close');
  ifit(process.platform === 'darwin')('emits show and close events with custom id', async () => {
      id: 'test-custom-id',
  ifit(process.platform === 'darwin')('emits show and close events with custom id and groupId', async () => {
      id: 'E017VKL2N8H|C07RBMNS9EK|1772656675.039',
      groupId: 'E017VKL2N8H|C07RBMNS9EK',
  ifit(process.platform === 'win32')('emits failed event', async () => {
      toastXml: 'not xml'
      const e = once(n, 'failed');
  // TODO(sethlu): Find way to test init with notification icon?
