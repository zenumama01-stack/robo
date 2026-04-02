import { runAndAdvance } from './utils';
test.describe('Smoke', () => {
  test('Tour', async ({ page, tmpPath }) => {
    // Open the tree page
    await page.locator('.jp-TreePanel >> text="Running"').click();
    await page.locator('.jp-TreePanel >> text="Files"').click();
    // Create a new console
    await page.menu.clickMenuItem('New>Console');
    // Choose the kernel
    const [console] = await Promise.all([
      page.click('text="Select"'),
    await console.waitForLoadState();
    await console.waitForSelector('.jp-CodeConsole');
      // we may have to select the kernel first
      await notebook.click('text="Select"', { timeout: 5000 });
      // The kernel is already selected
    // Enter code in the first cell
    await notebook.locator(
      '.jp-Cell-inputArea >> .cm-editor >> .cm-content[contenteditable="true"]'
    ).type(`import math
math.pi`);
    // Run the cell
    runAndAdvance(notebook);
    // Enter code in the next cell
    await notebook
      .nth(1)
      .type('import this');
    // Save the notebook
    // TODO: re-enable after fixing the name on save dialog?
    // await notebook.click('//span/*[local-name()="svg"]');
    // Click on the Jupyter logo to open the tree page
    const [tree2] = await Promise.all([
      notebook.waitForEvent('popup'),
      notebook.click(
        '//*[local-name()="svg" and normalize-space(.)=\'Jupyter\']'
    // Shut down the kernels
    await tree2.locator('.jp-TreePanel >> text="Running"').click();
    await tree2.click('#main-panel jp-button :text("Shut Down All")');
    await tree2.press('.jp-Dialog', 'Enter');
    // Close the pages
    await tree2.close();
    await console.close();
  test('JupyterLab', async ({ page, tmpPath }) => {
    // Open JupyterLab
    const [lab] = await Promise.all([
      page.menu.clickMenuItem('View>Open JupyterLab'),
    await lab.waitForSelector('.jp-Launcher');
    await lab.close();
