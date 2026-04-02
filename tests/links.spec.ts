const NOTEBOOK = 'local_links.ipynb';
const SUBFOLDER = 'test';
test.describe('Local Links', () => {
  test('Open the current directory', async ({ page, tmpPath }) => {
    const [current] = await Promise.all([
      page.getByText('Current Directory').last().click(),
    await current.waitForLoadState();
    await current.waitForSelector('.jp-DirListing-content');
    // Check that the link opened in a new tab
    expect(current.url()).toContain(`tree/${tmpPath}`);
    await current.close();
  test('Open a folder', async ({ page, tmpPath }) => {
    // Create a test folder
    await page.contents.createDirectory(`${tmpPath}/${SUBFOLDER}`);
    const [folder] = await Promise.all([
      page.getByText('Open Test Folder').last().click(),
    await folder.waitForLoadState();
    await folder.waitForSelector('.jp-DirListing-content');
    await folder.close();
    expect(folder.url()).toContain(`tree/${tmpPath}/${SUBFOLDER}`);
