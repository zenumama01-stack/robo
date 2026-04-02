const SUBFOLDER = 'subfolder';
test('Tree', async ({ page }) => {
  const button = await page.$('text="New Notebook"');
  expect(button).toBeDefined();
test('should go to subfolder', async ({ page, tmpPath }) => {
  const dir = `${tmpPath}/${SUBFOLDER}`;
  await page.contents.createDirectory(dir);
  await page.goto(`tree/${dir}`);
    await page.waitForSelector(`.jp-FileBrowser-crumbs >> text=/${SUBFOLDER}/`)
  ).toBeTruthy();
test('should update url when navigating in filebrowser', async ({
  await page.dblclick(`.jp-FileBrowser-listing >> text=${SUBFOLDER}`);
  await page.waitForSelector(`.jp-FileBrowser-crumbs >> text=/${SUBFOLDER}/`);
  const url = new URL(page.url());
  expect(url.pathname).toEqual(`/tree/${tmpPath}/${SUBFOLDER}`);
test('Should activate file browser tab', async ({ page, tmpPath }) => {
    page.locator('#main-panel #jp-running-sessions-tree')
  ).toBeVisible();
  await page.menu.clickMenuItem('View>File Browser');
  await expect(page.locator('#main-panel #filebrowser')).toBeVisible();
