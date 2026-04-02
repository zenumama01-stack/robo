describe('autofill', () => {
  it('can be selected via keyboard for a <datalist> with text type', async () => {
    await w.loadFile(path.join(fixturesPath, 'pages', 'datalist-text.html'));
    w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Tab' });
    const inputText = 'clap';
    w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Down' });
    w.webContents.sendInputEvent({ type: 'keyDown', keyCode: 'Enter' });
    const value = await w.webContents.executeJavaScript("document.querySelector('input').value");
    expect(value).to.equal('Eric Clapton');
  it('can be selected via keyboard for a <datalist> with time type', async () => {
    await w.loadFile(path.join(fixturesPath, 'pages', 'datalist-time.html'));
    const inputText = '11P'; // 1:01 PM
      w.webContents.sendInputEvent({ type: 'keyDown', keyCode });
    expect(value).to.equal('13:01');
