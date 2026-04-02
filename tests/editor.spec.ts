const FILE = 'environment.yml';
const processRenameDialog = async (page, prevName: string, newName: string) => {
  // Rename in the input dialog
  await page
    .locator(`text=File Path${prevName}New Name >> input`)
    .fill(newName);
    await page.click('text="Rename"'),
    // wait until the URL is updated
    await page.waitForNavigation(),
test.describe('Editor', () => {
      path.resolve(__dirname, `../../binder/${FILE}`),
      `${tmpPath}/${FILE}`
  test('Renaming the file by clicking on the title', async ({
    const file = `${tmpPath}/${FILE}`;
    await page.goto(`edit/${file}`);
    // Click on the title
    await page.click(`text="${FILE}"`);
    const newName = 'test.yml';
    await processRenameDialog(page, FILE, newName);
    // Check the URL contains the new name
    const url = page.url();
    expect(url).toContain(newName);
  test('Renaming the file via the menu entry', async ({ page, tmpPath }) => {
    await page.menu.clickMenuItem('File>Rename…');
