import { galata } from '@jupyterlab/galata';
test.use({
  mockSettings: {
    ...galata.DEFAULT_SETTINGS,
    '@jupyter-notebook/application-extension:shell': {
        Debugger: { area: 'left' },
test.describe('Layout Customization', () => {
  test('The Debugger panel should respect the settings and open in the left area', async ({
    const menuPath = 'View>Left Sidebar>Show Debugger';
    await page.menu.clickMenuItem(menuPath);
    const panel = page.locator('#jp-left-stack');
    expect(await panel.isVisible()).toBe(true);
    expect(await panel.screenshot()).toMatchSnapshot('debugger.png');
