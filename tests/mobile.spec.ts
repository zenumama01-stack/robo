import { expect, galata } from '@jupyterlab/galata';
import { hideAddCellButton, waitForKernelReady } from './utils';
  autoGoto: false,
  viewport: { width: 524, height: 800 },
  // Set a fixed string as Playwright is preventing the unique test name to be too long
  // and replaces part of the path with a hash
  tmpPath: 'mobile-layout',
test.describe('Mobile', () => {
  // manually create the test directory since tmpPath is set to a fixed value
  test.beforeAll(async ({ request, tmpPath }) => {
    const contents = galata.newContentsHelper(request);
    await contents.createDirectory(tmpPath);
  test.afterAll(async ({ request, tmpPath }) => {
    await contents.deleteDirectory(tmpPath);
  test('The layout should be more compact on the file browser page', async ({
    await page.goto(`tree/${tmpPath}`);
    await page.waitForSelector('#top-panel-wrapper', { state: 'hidden' });
    expect(await page.screenshot()).toMatchSnapshot('tree.png', {
      maxDiffPixels: 300,
  test('The layout should be more compact on the notebook page', async ({
    // Create a new notebook
    const notebookPromise = page.waitForEvent('popup');
    await page.click('text="New"');
        '[data-command="notebook:create-new"] >> text="Python 3 (ipykernel)"'
      .click();
    const notebook = await notebookPromise;
    // wait for the kernel status animations to be finished
    await waitForKernelReady(notebook);
    // force switching back to command mode to avoid capturing the cursor in the screenshot
    await notebook.evaluate(async () => {
      await window.jupyterapp.commands.execute('notebook:enter-command-mode');
    if (browserName === 'firefox') {
      await hideAddCellButton(notebook);
    expect(await notebook.screenshot()).toMatchSnapshot('notebook.png');
    await notebook.close();
