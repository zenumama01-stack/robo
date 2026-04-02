test.describe('File Browser', () => {
      path.resolve(__dirname, './notebooks/empty.ipynb'),
      `${tmpPath}/empty.ipynb`
    await page.contents.createDirectory(`${tmpPath}/folder1`);
    await page.contents.createDirectory(`${tmpPath}/folder2`);
  test('Select one folder', async ({ page, tmpPath }) => {
    await page.filebrowser.refresh();
    await page.keyboard.down('Control');
    await page.getByText('folder1').last().click();
    const toolbar = page.getByRole('toolbar');
    expect(toolbar.getByText('Rename')).toBeVisible();
    expect(toolbar.getByText('Move to Trash')).toBeVisible();
  test('Select one file', async ({ page, tmpPath }) => {
    await page.getByText('empty.ipynb').last().click();
    ['Rename', 'Open', 'Download', 'Move to Trash'].forEach(async (text) => {
      expect(toolbar.getByText(text)).toBeVisible();
  test('Select files and folders', async ({ page, tmpPath }) => {
    await page.getByText('folder2').last().click();
    expect(toolbar.getByText('Rename')).toBeHidden();
    expect(toolbar.getByText('Open')).toBeHidden();
  test('Select files and open', async ({ page, tmpPath }) => {
    // upload an additional notebook
      path.resolve(__dirname, './notebooks/simple.ipynb'),
      `${tmpPath}/simple.ipynb`
    await page.getByText('simple.ipynb').last().click();
    const [nb1, nb2] = await Promise.all([
      page.waitForEvent('popup'),
      toolbar.getByText('Open').last().click(),
    await nb1.waitForLoadState();
    await nb1.close();
    await nb2.waitForLoadState();
    await nb2.close();
