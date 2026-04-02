import { waitForNotebook, runAndAdvance, waitForKernelReady } from './utils';
const NOTEBOOK = 'example.ipynb';
test.describe('Notebook', () => {
      path.resolve(__dirname, `../../binder/${NOTEBOOK}`),
  test('Title should be rendered', async ({ page, tmpPath }) => {
    const href = await page.evaluate(() => {
      return document.querySelector('#jp-NotebookLogo')?.getAttribute('href');
    expect(href).toContain('/tree');
  test('Renaming the notebook should be possible', async ({
    const notebook = `${tmpPath}/${NOTEBOOK}`;
    await page.goto(`notebooks/${notebook}`);
    // Click on the title (with .ipynb extension stripped)
    await page.click('text="example"');
    const newName = 'test.ipynb';
    const newNameStripped = 'test';
      .locator(`text=File Path${NOTEBOOK}New Name >> input`)
    expect(url).toContain(newNameStripped);
  // TODO: rewrite with page.notebook when fixed upstream in Galata
  // and usable in Jupyter Notebook without active tabs
  test('Outputs should be scrolled automatically', async ({
    const notebook = 'autoscroll.ipynb';
    // wait for the checkpoint indicator to be displayed before executing the cells
    await page.waitForSelector('.jp-NotebookCheckpoint');
    await page.click('.jp-Notebook');
    // execute the first cell
    await runAndAdvance(page);
      .locator('.jp-mod-outputsScrolled')
      .nth(0)
      .waitFor({ state: 'visible' });
    // execute the second cell
    // the second cell should not be auto scrolled
    expect(page.locator('.jp-mod-outputsScrolled').nth(1)).toHaveCount(0);
    const checkCell = async (n: number): Promise<boolean> => {
      const scrolled = await page.$eval(`.jp-Notebook-cell >> nth=${n}`, (el) =>
        el.classList.contains('jp-mod-outputsScrolled')
      return scrolled;
    // check the long output area is auto scrolled
    expect(await checkCell(0)).toBe(true);
    // check the short output area is not auto scrolled
    expect(await checkCell(1)).toBe(false);
  test('Open table of content left panel', async ({ page, tmpPath }) => {
    const notebook = 'simple_toc.ipynb';
    const menuPath = 'View>Left Sidebar>Show Table of Contents';
      panel.locator(
        '.jp-SidePanel-content > .jp-TableOfContents-tree > .jp-TableOfContents-content'
    ).toHaveCount(1);
        '.jp-SidePanel-content > .jp-TableOfContents-tree > .jp-TableOfContents-content > .jp-tocItem'
    ).toHaveCount(3);
    const imageName = 'toc-left-panel.png';
    expect(await panel.screenshot()).toMatchSnapshot(imageName);
  test('Open notebook tools right panel', async ({ page, tmpPath }) => {
    const menuPath = 'View>Right Sidebar>Show Notebook Tools';
    const panel = page.locator('#jp-right-stack');
    await page.isVisible('#notebook-tools.jp-NotebookTools');
    await page.isVisible('#notebook-tools.jp-NotebookTools > #add-tag.tag');
    const imageName = 'notebooktools-right-panel.png';
  test('Clicking on "Close and Shut Down Notebook" should close the browser tab', async ({
    const menuPath = 'File>Close and Halt';
    // Press Enter to confirm the dialog
    await page.keyboard.press('Enter');
    expect(page.isClosed());
  test('Toggle the full width of the notebook', async ({
    const menuPath = 'View>Enable Full Width Notebook';
    const notebookPanel = page.locator('.jp-NotebookPanel').first();
    await expect(notebookPanel).toHaveClass(/jp-mod-fullwidth/);
    expect(await page.screenshot()).toMatchSnapshot('notebook-full-width.png');
    // undo the full width
    await expect(notebookPanel).not.toHaveClass(/jp-mod-fullwidth/);
  test('Open the log console widget in the down area', async ({
    const menuPath = 'View>Show Log Console';
    await expect(page.locator('.jp-LogConsole')).toBeVisible();
  test('Toggle cell outputs with the O keyboard shortcut', async ({
    // Wait for the first cell to be active
    const firstCell = page.locator('.jp-Cell').first();
    await expect(firstCell).toHaveClass(/jp-mod-active/);
    // run the two cells
    await page.keyboard.press('Shift+Enter');
    await page.keyboard.press('ControlOrMeta+Enter');
    await page.keyboard.press('Escape');
    await page.keyboard.press('O');
    await page.waitForSelector('.jp-OutputPlaceholder', { state: 'visible' });
    await page.waitForSelector('.jp-OutputPlaceholder', { state: 'hidden' });
