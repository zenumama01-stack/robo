import { nativeImage } from 'electron/common';
import { BaseWindow, BrowserWindow, ImageView } from 'electron/main';
describe('ImageView', () => {
  it('can be instantiated with no arguments', () => {
    new ImageView();
  it('can set an empty NativeImage', () => {
    const view = new ImageView();
    const image = nativeImage.createEmpty();
    view.setImage(image);
  it('can set a NativeImage', () => {
    const image = nativeImage.createFromPath(path.join(__dirname, 'fixtures', 'assets', 'logo.png'));
  it('can change its NativeImage', () => {
    const image1 = nativeImage.createFromPath(path.join(__dirname, 'fixtures', 'assets', 'logo.png'));
    const image2 = nativeImage.createFromPath(path.join(__dirname, 'fixtures', 'assets', 'capybara.png'));
    view.setImage(image1);
    view.setImage(image2);
  it('can be embedded in a BaseWindow', () => {
    const w = new BaseWindow({ show: false });
    const image = nativeImage.createFromPath(path.join(__dirname, 'fixtures', 'assets', 'capybara.png'));
    w.setContentView(view);
    w.setContentSize(image.getSize().width, image.getSize().height);
      width: image.getSize().width,
      height: image.getSize().height
  it('can be embedded in a BrowserWindow', () => {
    w.contentView.addChildView(view);
    expect(w.contentView.children).to.include(view);
  it('can be removed from a BrowserWindow', async () => {
    await w.loadFile(path.join(__dirname, 'fixtures', 'api', 'blank.html'));
    w.contentView.removeChildView(view);
    expect(w.contentView.children).to.not.include(view);
