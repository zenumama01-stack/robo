import { waitForKernelReady } from './utils';
const MENU_PATHS = [
  'File',
  'File>New',
  'File>Save and Export Notebook As',
  'Edit',
  'View',
  'Run',
  'Kernel',
  'Settings',
  'Settings>Theme',
  'Help',
test.describe('Notebook Menus', () => {
  MENU_PATHS.forEach((menuPath) => {
    test(`Open menu item ${menuPath}`, async ({ page, tmpPath }) => {
      await waitForKernelReady(page);
      await page.menu.open(menuPath);
      expect(await page.menu.isOpen(menuPath)).toBeTruthy();
      const imageName = `opened-menu-${menuPath.replace(/>/g, '-')}.png`;
      const menu = await page.menu.getOpenMenu();
      expect(menu).toBeDefined();
      expect(await menu?.screenshot()).toMatchSnapshot(imageName.toLowerCase());
