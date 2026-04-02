import { BrowserWindow, Session, session } from 'electron/main';
import { ifit, ifdescribe, listen } from './lib/spec-helpers';
ifdescribe(features.isBuiltinSpellCheckerEnabled())('spellchecker', function () {
  this.timeout((process.env.IS_ASAN ? 200 : 20) * 1000);
  async function rightClick () {
    const contextMenuPromise = once(w.webContents, 'context-menu');
      x: 43,
      y: 42
    return (await contextMenuPromise)[1] as Electron.ContextMenuParams;
  // When the page is just loaded, the spellchecker might not be ready yet. Since
  // there is no event to know the state of spellchecker, the only reliable way
  // to detect spellchecker is to keep checking with a busy loop.
  async function rightClickUntil (fn: (params: Electron.ContextMenuParams) => boolean) {
    const timeout = (process.env.IS_ASAN ? 180 : 10) * 1000;
    let contextMenuParams = await rightClick();
    while (!fn(contextMenuParams) && (Date.now() - now < timeout)) {
      contextMenuParams = await rightClick();
    return contextMenuParams;
  // Setup a server to download hunspell dictionary.
  const server = http.createServer(async (req, res) => {
    // The provided is minimal dict for testing only, full list of words can
    // be found at src/third_party/hunspell_dictionaries/xx_XX.dic.
      const data = await fs.readFile(path.join(__dirname, '/../../third_party/hunspell_dictionaries/xx-XX-3-0.bdic'));
      res.end(data);
      console.error('Failed to read dictionary file');
      res.writeHead(404);
      res.end(JSON.stringify(err));
  const preload = path.join(fixtures, 'module', 'preload-electron.js');
            partition: `unique-spell-${Date.now()}`,
            sandbox
        w.webContents.session.setSpellCheckerDictionaryDownloadURL(serverUrl);
        w.webContents.session.setSpellCheckerLanguages(['en-US']);
        await w.loadFile(path.resolve(__dirname, './fixtures/chromium/spellchecker.html'));
      // Context menu test can not run on Windows or Linux (https://github.com/electron/electron/pull/48657 broke linux).
      const shouldRun = process.platform !== 'win32' && process.platform !== 'linux';
      ifit(shouldRun)('should detect correctly spelled words as correct', async () => {
        await w.webContents.executeJavaScript('document.body.querySelector("textarea").value = "typography"');
        await w.webContents.executeJavaScript('document.body.querySelector("textarea").focus()');
        const contextMenuParams = await rightClickUntil((contextMenuParams) => contextMenuParams.selectionText.length > 0);
        expect(contextMenuParams.misspelledWord).to.eq('');
        expect(contextMenuParams.dictionarySuggestions).to.have.lengthOf(0);
      ifit(shouldRun)('should detect incorrectly spelled words as incorrect', async () => {
        await w.webContents.executeJavaScript('document.body.querySelector("textarea").value = "typograpy"');
        const contextMenuParams = await rightClickUntil((contextMenuParams) => contextMenuParams.misspelledWord.length > 0);
        expect(contextMenuParams.misspelledWord).to.eq('typograpy');
        expect(contextMenuParams.dictionarySuggestions).to.have.length.of.at.least(1);
      ifit(shouldRun)('should detect incorrectly spelled words as incorrect after disabling all languages and re-enabling', async () => {
        w.webContents.session.setSpellCheckerLanguages([]);
      ifit(shouldRun)('should expose webFrame spellchecker correctly', async () => {
        await rightClickUntil((contextMenuParams) => contextMenuParams.misspelledWord.length > 0);
        const callWebFrameFn = (expr: string) => w.webContents.executeJavaScript(`electron.webFrame.${expr}`);
        expect(await callWebFrameFn('isWordMisspelled("typography")')).to.equal(false);
        expect(await callWebFrameFn('isWordMisspelled("typograpy")')).to.equal(true);
        expect(await callWebFrameFn('getWordSuggestions("typography")')).to.be.empty();
        expect(await callWebFrameFn('getWordSuggestions("typograpy")')).to.not.be.empty();
      describe('spellCheckerEnabled', () => {
        it('is enabled by default', async () => {
          expect(w.webContents.session.spellCheckerEnabled).to.be.true();
        ifit(shouldRun)('can be dynamically changed', async () => {
          w.webContents.session.spellCheckerEnabled = false;
          expect(w.webContents.session.spellCheckerEnabled).to.be.false();
          // spellCheckerEnabled is sent to renderer asynchronously and there is
          // no event notifying when it is finished, so wait a little while to
          // ensure the setting has been changed in renderer.
          expect(await callWebFrameFn('isWordMisspelled("typograpy")')).to.equal(false);
          w.webContents.session.spellCheckerEnabled = true;
      describe('custom dictionary word list API', () => {
        let ses: Session;
          // ensure a new session runs on each test run
          ses = session.fromPartition(`persist:customdictionary-test-${Date.now()}`);
          if (ses) {
            ses = null as any;
        describe('ses.listWordsFromSpellCheckerDictionary', () => {
          it('should successfully list words in custom dictionary', async () => {
            const words = ['foo', 'bar', 'baz'];
            const results = words.map(word => ses.addWordToSpellCheckerDictionary(word));
            expect(results).to.eql([true, true, true]);
            const wordList = await ses.listWordsInSpellCheckerDictionary();
            expect(wordList).to.have.deep.members(words);
          it('should return an empty array if no words are added', async () => {
            expect(wordList).to.have.length(0);
        describe('ses.addWordToSpellCheckerDictionary', () => {
          it('should successfully add word to custom dictionary', async () => {
            const result = ses.addWordToSpellCheckerDictionary('foobar');
            expect(wordList).to.eql(['foobar']);
          it('should fail for an empty string', async () => {
            const result = ses.addWordToSpellCheckerDictionary('');
            const wordList = await ses.listWordsInSpellCheckerDictionary;
          // remove API will always return false because we can't add words
          it('should fail for non-persistent sessions', async () => {
            const tempSes = session.fromPartition('temporary');
            const result = tempSes.addWordToSpellCheckerDictionary('foobar');
        describe('ses.setSpellCheckerLanguages', () => {
          ifit(isMac)('should be a no-op when setSpellCheckerLanguages is called on macOS', () => {
              w.webContents.session.setSpellCheckerLanguages(['i-am-a-nonexistent-language']);
          ifit(!isMac)('should throw when a bad language is passed', () => {
            }).to.throw(/Invalid language code provided: "i-am-a-nonexistent-language" is not a valid language code/);
          ifit(!isMac)('should not throw when a recognized language is passed', () => {
              w.webContents.session.setSpellCheckerLanguages(['es']);
        describe('SetSpellCheckerDictionaryDownloadURL', () => {
          ifit(isMac)('should be a no-op when a bad url is passed on macOS', () => {
              w.webContents.session.setSpellCheckerDictionaryDownloadURL('i-am-not-a-valid-url');
          ifit(!isMac)('should throw when a bad url is passed', () => {
            }).to.throw(/The URL you provided to setSpellCheckerDictionaryDownloadURL is not a valid URL/);
        describe('ses.removeWordFromSpellCheckerDictionary', () => {
          it('should successfully remove words to custom dictionary', async () => {
            const result1 = ses.addWordToSpellCheckerDictionary('foobar');
            expect(result1).to.equal(true);
            const wordList1 = await ses.listWordsInSpellCheckerDictionary();
            expect(wordList1).to.eql(['foobar']);
            const result2 = ses.removeWordFromSpellCheckerDictionary('foobar');
            const wordList2 = await ses.listWordsInSpellCheckerDictionary();
            expect(wordList2).to.have.length(0);
          it('should fail for words not in custom dictionary', () => {
            expect(result2).to.equal(false);
