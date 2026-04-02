// Mock the config module
    dbUsername: 'testuser',
    dbPassword: 'testpass',
    currentUserEmail: 'test@example.com',
    autoRefreshInterval: 3600000
// Mock the db module
        connect: vi.fn().mockResolvedValue(undefined)
// Mock SQL server data provider
    SQLServerProviderConfigData: class SQLServerProviderConfigData {
        constructor(_pool: unknown, _schema: string, _interval: number) {}
    setupSQLServerClient: vi.fn().mockResolvedValue(undefined)
import { timeout, handleServerInit } from '../util';
describe('timeout', () => {
    it('should reject after specified ms', async () => {
        timeout(5000).catch(() => { rejected = true; });
        vi.advanceTimersByTime(2000);
        // Need to flush promises
        await vi.advanceTimersByTimeAsync(0);
        vi.advanceTimersByTime(3000);
describe('handleServerInit', () => {
        // Reset the module-level _serverInitalized flag by resetting modules
        vi.resetModules();
    it('should be a function', () => {
        expect(typeof handleServerInit).toBe('function');
    it('should connect pool and setup client on first call', async () => {
        const db = await import('../db');
        const { setupSQLServerClient } = await import('@memberjunction/sqlserver-dataprovider');
        await handleServerInit(false);
        expect(db.default.connect).toHaveBeenCalled();
        expect(setupSQLServerClient).toHaveBeenCalled();
            existsSync: vi.fn(),
            readFileSync: vi.fn(),
            unlinkSync: vi.fn()
vi.mock('fs-extra', () => ({
        copySync: vi.fn()
import { makeDir, makeDirs, logIf, sortBySequenceAndCreatedAt } from '../Misc/util';
describe('makeDir', () => {
    it('should create directory if it does not exist', () => {
        (fs.existsSync as ReturnType<typeof vi.fn>).mockReturnValue(false);
        makeDir('/tmp/test-dir');
        expect(fs.mkdirSync).toHaveBeenCalledWith('/tmp/test-dir', { recursive: true });
    it('should not create directory if it already exists', () => {
        (fs.existsSync as ReturnType<typeof vi.fn>).mockReturnValue(true);
        makeDir('/tmp/existing-dir');
        expect(fs.mkdirSync).not.toHaveBeenCalled();
    it('should use recursive option', () => {
        makeDir('/tmp/deep/nested/dir');
        expect(fs.mkdirSync).toHaveBeenCalledWith('/tmp/deep/nested/dir', { recursive: true });
describe('makeDirs', () => {
    it('should create all specified directories', () => {
        makeDirs(['/tmp/dir1', '/tmp/dir2', '/tmp/dir3']);
        expect(fs.mkdirSync).toHaveBeenCalledTimes(3);
        makeDirs([]);
    it('should skip existing directories', () => {
        (fs.existsSync as ReturnType<typeof vi.fn>)
            .mockReturnValueOnce(true)
            .mockReturnValueOnce(false);
        makeDirs(['/tmp/exists', '/tmp/new']);
        expect(fs.mkdirSync).toHaveBeenCalledTimes(1);
describe('logIf', () => {
    let consoleSpy: ReturnType<typeof vi.spyOn>;
        consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
    it('should log when shouldLog is true', () => {
        logIf(true, 'test message');
        expect(consoleSpy).toHaveBeenCalledWith('test message');
    it('should not log when shouldLog is false', () => {
        logIf(false, 'hidden message');
        expect(consoleSpy).not.toHaveBeenCalled();
    it('should pass multiple arguments to console.log', () => {
        logIf(true, 'msg', 123, { key: 'value' });
        expect(consoleSpy).toHaveBeenCalledWith('msg', 123, { key: 'value' });
    it('should handle empty call with true', () => {
        logIf(true);
        expect(consoleSpy).toHaveBeenCalled();
describe('sortBySequenceAndCreatedAt', () => {
    it('should sort by Sequence ascending', () => {
        const items = [
            { Sequence: 3, Name: 'C' },
            { Sequence: 1, Name: 'A' },
            { Sequence: 2, Name: 'B' }
        const result = sortBySequenceAndCreatedAt(items);
        expect(result[0].Name).toBe('A');
        expect(result[1].Name).toBe('B');
        expect(result[2].Name).toBe('C');
    it('should use __mj_CreatedAt as secondary sort', () => {
            { Sequence: 1, Name: 'B', __mj_CreatedAt: new Date('2025-02-01') },
            { Sequence: 1, Name: 'A', __mj_CreatedAt: new Date('2025-01-01') }
    it('should not mutate the original array', () => {
            { Sequence: 2, Name: 'B' },
            { Sequence: 1, Name: 'A' }
        const original = [...items];
        sortBySequenceAndCreatedAt(items);
        expect(items[0]).toEqual(original[0]);
        const result = sortBySequenceAndCreatedAt([]);
    it('should handle single element', () => {
        const items = [{ Sequence: 1, Name: 'A' }];
    it('should prioritize items with dates over items without when sequences match', () => {
            { Sequence: 1, Name: 'NoDate' },
            { Sequence: 1, Name: 'HasDate', __mj_CreatedAt: new Date('2025-01-01') }
        expect(result[0].Name).toBe('HasDate');
        expect(result[1].Name).toBe('NoDate');
    it('should maintain order for items with same sequence and no dates', () => {
            { Sequence: 1, Name: 'First' },
            { Sequence: 1, Name: 'Second' }
    it('should sort correctly with mixed sequences', () => {
            { Sequence: 5, Name: 'E' },
            { Sequence: 1, Name: 'A', __mj_CreatedAt: new Date('2025-06-01') },
            { Sequence: 1, Name: 'B', __mj_CreatedAt: new Date('2025-01-01') },
            { Sequence: 2, Name: 'D' }
        expect(result[0].Name).toBe('B'); // Seq 1, earlier date
        expect(result[1].Name).toBe('A'); // Seq 1, later date
        expect(result[2].Name).toBe('D'); // Seq 2
        expect(result[3].Name).toBe('C'); // Seq 3
        expect(result[4].Name).toBe('E'); // Seq 5
    it('should handle negative sequence numbers', () => {
            { Sequence: 0, Name: 'Zero' },
            { Sequence: -1, Name: 'Negative' },
            { Sequence: 1, Name: 'Positive' }
        expect(result[0].Name).toBe('Negative');
        expect(result[1].Name).toBe('Zero');
        expect(result[2].Name).toBe('Positive');
import { BuildComponentCompleteCode, BuildComponentCode } from '../util';
import { ComponentSpec } from '../component-spec';
 * Helper to create a minimal ComponentSpec for testing
function createSpec(overrides: Partial<ComponentSpec> = {}): ComponentSpec {
    name: 'TestComponent',
    description: 'A test component',
    code: 'function TestComponent() { return <div>Hello</div>; }',
    technicalDesign: '',
    exampleUsage: '<TestComponent />',
describe('InteractiveComponents util', () => {
  describe('BuildComponentCompleteCode', () => {
    it('should return empty string when code is null/undefined', () => {
      const spec = createSpec({ code: '' });
      expect(BuildComponentCompleteCode(spec)).toBe('');
    it('should return empty string when code is only whitespace', () => {
      const spec = createSpec({ code: '   ' });
    it('should return code directly when no dependencies', () => {
      const spec = createSpec({ dependencies: [] });
      expect(BuildComponentCompleteCode(spec)).toBe(spec.code);
    it('should return code directly when dependencies is undefined', () => {
      const spec = createSpec();
      delete spec.dependencies;
    it('should append dependency code', () => {
      const dep = createSpec({
        name: 'ChildComponent',
        code: 'function ChildComponent() { return <span>Child</span>; }',
      const spec = createSpec({ dependencies: [dep] });
      const result = BuildComponentCompleteCode(spec);
      expect(result).toContain('TestComponent');
      expect(result).toContain('ChildComponent');
    it('should handle nested dependencies recursively', () => {
      const grandchild = createSpec({
        name: 'GrandchildComp',
        code: 'function GrandchildComp() { return <em>GC</em>; }',
      const child = createSpec({
        name: 'ChildComp',
        code: 'function ChildComp() { return <span>Child</span>; }',
        dependencies: [grandchild],
      const spec = createSpec({ dependencies: [child] });
      expect(result).toContain('GrandchildComp');
      expect(result).toContain('ChildComp');
  describe('BuildComponentCode', () => {
    it('should include comment header with name and description', () => {
      const dep = createSpec({ name: 'HelperComp', description: 'Helps with things' });
      const result = BuildComponentCode(dep, '');
      expect(result).toContain('HelperComp');
      expect(result).toContain('Helps with things');
      expect(result).toContain('/***');
    it('should include path in comment header when provided', () => {
      const dep = createSpec({ name: 'SubComp' });
      const result = BuildComponentCode(dep, 'Root > Parent');
      expect(result).toContain('Root > Parent > SubComp');
    it('should return just the component code when no sub-dependencies', () => {
      const dep = createSpec({ name: 'LeafComp', dependencies: [] });
      expect(result).toContain(dep.code);
vi.mock('@memberjunction/codegen-lib', () => ({
  initializeConfig: vi.fn(),
  RunCodeGenBase: class {},
          setupDataSource: vi.fn().mockResolvedValue(undefined),
import { timeout, ___initialized, handleServerInit } from '../util';
describe('MJCodeGenAPI util', () => {
    it('should reject after the specified time', async () => {
      // Flush all pending microtasks
  describe('___initialized', () => {
    it('should start as false', () => {
      expect(___initialized).toBe(false);
const { mockEmbedTextLocal, mockConfig } = vi.hoisted(() => ({
    mockEmbedTextLocal: vi.fn(),
    mockConfig: vi.fn().mockResolvedValue(undefined),
            Config: mockConfig,
            EmbedTextLocal: mockEmbedTextLocal,
        ContextCurrentUser = { ID: 'test-user' };
    SimpleEmbeddingResult: class {},
import { EmbedTextLocalHelper } from '../custom/util';
describe('EmbedTextLocalHelper', () => {
    it('should call AIEngine.Config with false and contextUser', async () => {
        mockEmbedTextLocal.mockResolvedValue({
            result: { vector: [0.1, 0.2, 0.3] },
            model: { ID: 'model-1' },
        const mockEntity = { ContextCurrentUser: { ID: 'user-123' } };
        await EmbedTextLocalHelper(mockEntity as never, 'test text');
        expect(mockConfig).toHaveBeenCalledWith(false, { ID: 'user-123' });
    it('should call EmbedTextLocal with the provided text', async () => {
            result: { vector: [0.1, 0.2] },
        const mockEntity = { ContextCurrentUser: { ID: 'user-1' } };
        await EmbedTextLocalHelper(mockEntity as never, 'hello world');
        expect(mockEmbedTextLocal).toHaveBeenCalledWith('hello world');
    it('should return vector and modelID on success', async () => {
        const expectedVector = [0.1, 0.2, 0.3, 0.4];
            result: { vector: expectedVector },
            model: { ID: 'embed-model-42' },
        const result = await EmbedTextLocalHelper(mockEntity as never, 'some text');
            vector: expectedVector,
            modelID: 'embed-model-42',
    it('should throw error when no vector returned', async () => {
            result: { vector: null },
        await expect(EmbedTextLocalHelper(mockEntity as never, 'text')).rejects.toThrow(
            'Failed to generate embedding'
    it('should throw error when no model ID returned', async () => {
            result: { vector: [0.1] },
            model: { ID: null },
    it('should throw error when result is undefined', async () => {
        mockEmbedTextLocal.mockResolvedValue(undefined);
        await expect(EmbedTextLocalHelper(mockEntity as never, 'text')).rejects.toThrow();
    it('should throw error when result has no result property', async () => {
  GetGlobalObjectStore,
  CleanJSON,
  SafeJSONParse,
  CleanAndParseJSON,
  CopyScalarsAndArrays,
  convertCamelCaseToHaveSpaces,
  generatePluralName,
  getIrregularPlural,
  stripWhitespace,
  uuidv4,
  stripTrailingChars,
  replaceAllSpaces,
  IsOnlyTimezoneShift,
} from '../util';
describe('GetGlobalObjectStore', () => {
  it('should return a non-null object in Node environment', () => {
    const store = GetGlobalObjectStore();
  it('should return the global object', () => {
    expect(store).toBeDefined();
  it('should allow setting and reading arbitrary keys', () => {
    if (store) {
      const testKey = '__test_key_' + Date.now();
      store[testKey] = 'test-value';
      expect(store[testKey]).toBe('test-value');
      delete store[testKey];
describe('CleanJSON', () => {
    expect(CleanJSON(null)).toBeNull();
    expect(CleanJSON('')).toBeNull();
  it('should return formatted JSON for valid JSON input', () => {
    const input = '{"name":"test","value":123}';
    const result = CleanJSON(input);
    const parsed = JSON.parse(result!);
    expect(parsed.name).toBe('test');
    expect(parsed.value).toBe(123);
  it('should handle already-formatted JSON', () => {
    const input = '{\n  "name": "test"\n}';
  it('should extract JSON from markdown code blocks', () => {
    const input = 'Some text ```json\n{"extracted": true}\n``` more text';
    expect(parsed.extracted).toBe(true);
  it('should handle JSON arrays', () => {
    const input = '[1, 2, 3]';
    expect(parsed).toEqual([1, 2, 3]);
  it('should extract JSON object from mixed content', () => {
    const input = 'Here is the result: {"status": "ok"} and some trailing text';
    expect(parsed.status).toBe('ok');
  it('should handle double-escaped JSON', () => {
    const input = '{\\"name\\": \\"test\\"}';
  it('should throw when input has braces but invalid JSON', () => {
    expect(() => CleanJSON('{not valid json}')).toThrow(
      /Failed to find a path to CleanJSON/
  it('should return processed string when no braces or brackets present', () => {
    const result = CleanJSON('not json at all without braces');
    expect(result).toBe('not json at all without braces');
    const input = '{"outer": {"inner": "value"}}';
    expect(parsed.outer.inner).toBe('value');
  it('should handle JSON with trailing extra brace by removing it', () => {
    const input = '{"name": "test"}}';
describe('SafeJSONParse', () => {
  it('should parse valid JSON', () => {
    const result = SafeJSONParse<{ name: string }>('{"name": "hello"}');
    expect(result!.name).toBe('hello');
    const result = SafeJSONParse('not json');
    expect(SafeJSONParse('')).toBeNull();
    expect(SafeJSONParse(null as unknown as string)).toBeNull();
  it('should parse arrays', () => {
    const result = SafeJSONParse<number[]>('[1, 2, 3]');
  it('should parse primitive types', () => {
    expect(SafeJSONParse<number>('42')).toBe(42);
    expect(SafeJSONParse<boolean>('true')).toBe(true);
    expect(SafeJSONParse<string>('"hello"')).toBe('hello');
  it('should log errors when logErrors is true', () => {
    SafeJSONParse('invalid', true);
    expect(errorSpy).toHaveBeenCalledWith(
      'Error parsing JSON string:',
      expect.objectContaining({ message: expect.stringContaining('') })
  it('should not log errors when logErrors is false', () => {
    SafeJSONParse('invalid', false);
    expect(errorSpy).not.toHaveBeenCalled();
describe('CleanAndParseJSON', () => {
  it('should clean and parse valid JSON', () => {
    const result = CleanAndParseJSON<{ key: string }>('{"key": "value"}');
    expect(result!.key).toBe('value');
  it('should handle markdown-wrapped JSON', () => {
    const input = '```json\n{"id": 123}\n```';
    const result = CleanAndParseJSON<{ id: number }>(input);
    expect(result!.id).toBe(123);
    expect(CleanAndParseJSON(null)).toBeNull();
    expect(CleanAndParseJSON('')).toBeNull();
    const result = CleanAndParseJSON<{ name: string }>(input);
    expect(result!.name).toBe('test');
describe('CopyScalarsAndArrays', () => {
  it('should copy scalar properties', () => {
    const input = { name: 'test', count: 42, flag: true };
    const result = CopyScalarsAndArrays(input);
    expect(result.name).toBe('test');
    expect(result.count).toBe(42);
    expect(result.flag).toBe(true);
  it('should copy array properties', () => {
    const input = { items: [1, 2, 3], tags: ['a', 'b'] };
    expect(result.items).toEqual([1, 2, 3]);
    expect(result.tags).toEqual(['a', 'b']);
    // Verify arrays are copied, not referenced
    expect(result.items).not.toBe(input.items);
  it('should copy function properties as scalars (typeof function !== object)', () => {
    const input = { name: 'test', method: () => 'hello' };
    // Without circular ref mode, functions pass typeof !== 'object' check and are copied
    expect('method' in result).toBe(true);
  it('should copy null values', () => {
    const input = { nullVal: null, name: 'test' };
    expect(result.nullVal).toBeNull();
  it('should recursively copy plain nested objects', () => {
    const input = { outer: { inner: 'deep' } };
    expect(result.outer).toBeDefined();
    expect((result.outer as { inner: string }).inner).toBe('deep');
  it('should skip non-plain object instances', () => {
    class Custom {
      Value = 42;
    const input = { data: new Custom(), name: 'test' };
    // Custom class instances are not plain objects, so skipped
    expect('data' in result).toBe(false);
  describe('with resolveCircularReferences', () => {
    it('should handle circular references', () => {
      const obj: Record<string, unknown> = { name: 'root' };
      obj['self'] = obj;
      const result = CopyScalarsAndArrays(obj, true);
      expect(result.name).toBe('root');
      expect(result.self).toBe('[Circular Reference]');
    it('should handle Date objects', () => {
      const input = { date: new Date('2025-01-01T00:00:00Z') };
      const result = CopyScalarsAndArrays(input, true);
      expect(result.date).toBe('2025-01-01T00:00:00.000Z');
    it('should handle Error objects', () => {
      const input = { error: new Error('test error') };
      const result = CopyScalarsAndArrays(input, true) as { error: { name: string; message: string } };
      expect(result.error.name).toBe('Error');
      expect(result.error.message).toBe('test error');
    it('should replace functions with [Function] marker', () => {
      const input = { fn: () => 'hello' };
      expect(result.fn).toBe('[Function]');
    it('should respect maxDepth', () => {
      const input = { a: { b: { c: { d: 'deep' } } } };
      const result = CopyScalarsAndArrays(input, true, 2) as Record<string, unknown>;
      // At depth 2 we should hit max depth
      expect(result.a).toBeDefined();
describe('convertCamelCaseToHaveSpaces', () => {
  it('should convert simple camelCase', () => {
    expect(convertCamelCaseToHaveSpaces('DatabaseVersion')).toBe('Database Version');
  it('should handle consecutive uppercase letters (acronyms)', () => {
    expect(convertCamelCaseToHaveSpaces('AIAgentLearningCycle')).toBe('AI Agent Learning Cycle');
  it('should return single word unchanged', () => {
    expect(convertCamelCaseToHaveSpaces('Database')).toBe('Database');
  it('should handle all uppercase', () => {
    expect(convertCamelCaseToHaveSpaces('ABC')).toBe('ABC');
  it('should handle single character', () => {
    expect(convertCamelCaseToHaveSpaces('A')).toBe('A');
    expect(convertCamelCaseToHaveSpaces('')).toBe('');
  it('should handle lowercase only', () => {
    expect(convertCamelCaseToHaveSpaces('hello')).toBe('hello');
  it('should handle multiple words', () => {
    expect(convertCamelCaseToHaveSpaces('FirstNameLastName')).toBe('First Name Last Name');
  it('should handle acronym at the end', () => {
    expect(convertCamelCaseToHaveSpaces('GetHTMLParser')).toBe('Get HTML Parser');
  it('should handle acronym at the beginning', () => {
    expect(convertCamelCaseToHaveSpaces('HTMLParser')).toBe('HTML Parser');
describe('generatePluralName', () => {
  it('should handle regular plurals by adding s', () => {
    expect(generatePluralName('dog')).toBe('dogs');
    expect(generatePluralName('cat')).toBe('cats');
    expect(generatePluralName('book')).toBe('books');
  it('should handle irregular plurals', () => {
    expect(generatePluralName('child')).toBe('children');
    expect(generatePluralName('person')).toBe('people');
    expect(generatePluralName('mouse')).toBe('mice');
    expect(generatePluralName('foot')).toBe('feet');
    expect(generatePluralName('tooth')).toBe('teeth');
    expect(generatePluralName('man')).toBe('men');
    expect(generatePluralName('woman')).toBe('women');
  it('should handle words ending in consonant + y', () => {
    expect(generatePluralName('party')).toBe('parties');
    expect(generatePluralName('city')).toBe('cities');
    expect(generatePluralName('baby')).toBe('babies');
  it('should handle words ending in vowel + y by just adding s', () => {
    expect(generatePluralName('day')).toBe('days');
    expect(generatePluralName('boy')).toBe('boys');
    expect(generatePluralName('key')).toBe('keys');
  it('should handle words ending in ch, sh, x, z by adding es', () => {
    expect(generatePluralName('match')).toBe('matches');
    expect(generatePluralName('wish')).toBe('wishes');
    expect(generatePluralName('box')).toBe('boxes');
    expect(generatePluralName('buzz')).toBe('buzzes');
  it('should treat words ending in s as already plural (getSingularForm detects singular)', () => {
    // 'bus' ends in 's', getSingularForm returns 'bu' (different from 'bus'),
    // so generatePluralName considers 'bus' already plural
    expect(generatePluralName('bus')).toBe('bus');
  it('should detect already-plural words and return them unchanged', () => {
    expect(generatePluralName('dogs')).toBe('dogs');
    expect(generatePluralName('customers')).toBe('customers');
  it('should handle capitalizeFirstLetterOnly option', () => {
    expect(generatePluralName('dog', { capitalizeFirstLetterOnly: true })).toBe('Dogs');
  it('should handle capitalizeEntireWord option', () => {
    expect(generatePluralName('dog', { capitalizeEntireWord: true })).toBe('DOGS');
describe('getIrregularPlural', () => {
  it('should return irregular plural for known words', () => {
    expect(getIrregularPlural('child')).toBe('children');
    expect(getIrregularPlural('knife')).toBe('knives');
    expect(getIrregularPlural('leaf')).toBe('leaves');
    expect(getIrregularPlural('Child')).toBe('children');
    expect(getIrregularPlural('MOUSE')).toBe('mice');
  it('should return null for regular words', () => {
    expect(getIrregularPlural('dog')).toBeNull();
    expect(getIrregularPlural('table')).toBeNull();
describe('stripWhitespace', () => {
  it('should remove all spaces', () => {
    expect(stripWhitespace('Hello World')).toBe('HelloWorld');
  it('should remove tabs and newlines', () => {
    expect(stripWhitespace('\tExample\nString ')).toBe('ExampleString');
    expect(stripWhitespace('')).toBe('');
  it('should return null or undefined as-is', () => {
    expect(stripWhitespace(null as unknown as string)).toBeNull();
    expect(stripWhitespace(undefined as unknown as string)).toBeUndefined();
  it('should handle string with only whitespace', () => {
    expect(stripWhitespace('   \t\n  ')).toBe('');
  it('should handle string with no whitespace', () => {
    expect(stripWhitespace('NoSpaces')).toBe('NoSpaces');
  it('should handle multiple consecutive whitespace types', () => {
    expect(stripWhitespace('  a  b  c  ')).toBe('abc');
describe('uuidv4', () => {
  it('should return a string', () => {
    expect(typeof uuidv4()).toBe('string');
  it('should match UUID v4 format', () => {
    const uuidV4Regex = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    expect(uuid).toMatch(uuidV4Regex);
  it('should generate unique values', () => {
    const uuid1 = uuidv4();
    const uuid2 = uuidv4();
    const uuid3 = uuidv4();
    expect(uuid1).not.toBe(uuid2);
    expect(uuid2).not.toBe(uuid3);
    expect(uuid1).not.toBe(uuid3);
  it('should have the correct length', () => {
    expect(uuid.length).toBe(36);
describe('stripTrailingChars', () => {
  it('should strip matching trailing characters', () => {
    expect(stripTrailingChars('example.txt', '.txt', false)).toBe('example');
  it('should not strip when suffix does not match', () => {
    expect(stripTrailingChars('example.csv', '.txt', false)).toBe('example.csv');
  it('should skip stripping on exact match when skipIfExactMatch is true', () => {
    expect(stripTrailingChars('.txt', '.txt', true)).toBe('.txt');
  it('should strip exact match when skipIfExactMatch is false', () => {
    expect(stripTrailingChars('.txt', '.txt', false)).toBe('');
  it('should return input when input is empty', () => {
    expect(stripTrailingChars('', '.txt', false)).toBe('');
  it('should return input when charsToStrip is empty', () => {
    expect(stripTrailingChars('test', '', false)).toBe('test');
describe('replaceAllSpaces', () => {
    expect(replaceAllSpaces('Hello World')).toBe('HelloWorld');
  it('should handle multiple spaces', () => {
    expect(replaceAllSpaces('  Leading spaces')).toBe('Leadingspaces');
  it('should handle string with no spaces', () => {
    expect(replaceAllSpaces('NoSpaces')).toBe('NoSpaces');
    expect(replaceAllSpaces('')).toBe('');
describe('IsOnlyTimezoneShift', () => {
  it('should return true for a 6-hour timezone shift', () => {
    const d1 = new Date('2025-12-25T10:30:45.123Z');
    const d2 = new Date('2025-12-25T16:30:45.123Z');
    expect(IsOnlyTimezoneShift(d1, d2)).toBe(true);
  it('should return false when milliseconds differ', () => {
    const d2 = new Date('2025-12-25T16:30:45.124Z');
    expect(IsOnlyTimezoneShift(d1, d2)).toBe(false);
  it('should return false for identical dates', () => {
    const d2 = new Date('2025-12-25T10:30:45.123Z');
  it('should return true for a 1-hour shift', () => {
    const d1 = new Date('2025-12-25T10:00:00.000Z');
    const d2 = new Date('2025-12-25T11:00:00.000Z');
  it('should return false for a 24-hour shift', () => {
    const d1 = new Date('2025-12-25T00:00:00.000Z');
    const d2 = new Date('2025-12-26T00:00:00.000Z');
 * Unit tests for the MJStorage utility functions.
 * These tests focus on the initializeDriverWithAccountCredentials function
 * which is the core enterprise model for initializing storage drivers.
import { MJGlobal, ClassFactory } from '@memberjunction/global';
// Mock the external dependencies
vi.mock('@memberjunction/global', async () => {
  const actualGlobal = await vi.importActual('@memberjunction/global');
    ...actualGlobal,
vi.mock('@memberjunction/credentials', () => ({
  CredentialEngine: {
      getCredentialById: vi.fn(),
      getCredential: vi.fn(),
  UserInfo: class MockUserInfo {
    ID = 'test-user-id';
    Name = 'Test User';
// Create a concrete implementation of FileStorageBase for testing
class MockFileStorageDriver extends FileStorageBase {
  protected readonly providerName = 'MockProvider';
  public initializeCalledWith: StorageProviderConfig | null = null;
  public async initialize(config: StorageProviderConfig): Promise<void> {
    this.initializeCalledWith = config;
  // Implement all abstract methods with minimal implementations
    return { UploadUrl: `https://mock.upload.url/${objectName}` };
    return `https://mock.download.url/${objectName}`;
      size: 100,
    return Buffer.from('test content');
describe('initializeDriverWithAccountCredentials', () => {
  let mockDriver: MockFileStorageDriver;
  let mockAccountEntity: {
  let mockProviderEntity: {
  let mockContextUser: { ID: string; Name: string };
    // Reset all mocks
    // Create fresh mock instances
    mockDriver = new MockFileStorageDriver();
    // Setup the ClassFactory to return our mock driver
    (MJGlobal.Instance.ClassFactory.CreateInstance as ReturnType<typeof vi.fn>).mockReturnValue(mockDriver);
    mockAccountEntity = {
      ID: 'account-123',
      Name: 'Test Storage Account',
      CredentialID: null,
    mockProviderEntity = {
      Name: 'Test Provider',
      ServerDriverKey: 'TestDriver',
      Configuration: null,
    mockContextUser = {
  describe('driver creation', () => {
    it('should create a driver instance using the provider ServerDriverKey', async () => {
      // Import the function we're testing
      const { initializeDriverWithAccountCredentials } = await import('../util');
      await initializeDriverWithAccountCredentials({
        accountEntity: mockAccountEntity as any,
        providerEntity: mockProviderEntity as any,
        contextUser: mockContextUser as any,
      expect(MJGlobal.Instance.ClassFactory.CreateInstance).toHaveBeenCalledWith(FileStorageBase, 'TestDriver');
    it('should throw an error if driver creation fails', async () => {
      (MJGlobal.Instance.ClassFactory.CreateInstance as ReturnType<typeof vi.fn>).mockReturnValue(null);
        initializeDriverWithAccountCredentials({
      ).rejects.toThrow(/Failed to create storage driver/);
  describe('account information', () => {
    it('should pass accountId from the account entity to the driver', async () => {
      expect(mockDriver.initializeCalledWith).toBeDefined();
      expect(mockDriver.initializeCalledWith!.accountId).toBe('account-123');
    it('should pass accountName from the account entity to the driver', async () => {
      expect(mockDriver.initializeCalledWith!.accountName).toBe('Test Storage Account');
  describe('credential handling', () => {
    it('should use Credential Engine when account has a CredentialID', async () => {
      const { CredentialEngine } = await import('@memberjunction/credentials');
      mockAccountEntity.CredentialID = 'credential-456';
      // Setup credential engine mocks
      (CredentialEngine.Instance.getCredentialById as ReturnType<typeof vi.fn>).mockReturnValue({
        ID: 'credential-456',
        Name: 'Test Credential',
      (CredentialEngine.Instance.getCredential as ReturnType<typeof vi.fn>).mockResolvedValue({
          accessKey: 'test-access-key',
          secretKey: 'test-secret-key',
          bucket: 'test-bucket',
      // Verify Credential Engine was configured
      expect(CredentialEngine.Instance.Config).toHaveBeenCalledWith(false, mockContextUser);
      // Verify credential was looked up by ID
      expect(CredentialEngine.Instance.getCredentialById).toHaveBeenCalledWith('credential-456');
      // Verify getCredential was called with correct parameters
      expect(CredentialEngine.Instance.getCredential).toHaveBeenCalledWith(
        'Test Credential',
          credentialId: 'credential-456',
          contextUser: mockContextUser,
    it('should merge credential values with account info in driver config', async () => {
          accessKey: 'decrypted-access-key',
          secretKey: 'decrypted-secret-key',
      // Verify driver was initialized with merged config
      expect(mockDriver.initializeCalledWith).toMatchObject({
        accountName: 'Test Storage Account',
      // Verify that onTokenRefresh callback was added
      expect(mockDriver.initializeCalledWith.onTokenRefresh).toBeDefined();
      expect(typeof mockDriver.initializeCalledWith.onTokenRefresh).toBe('function');
    it('should throw error if credential lookup fails', async () => {
      // Return null to simulate credential not found
      (CredentialEngine.Instance.getCredentialById as ReturnType<typeof vi.fn>).mockReturnValue(null);
      ).rejects.toThrow(/Credential with ID credential-456 not found/);
  describe('fallback to provider configuration', () => {
    it('should use provider Configuration when no CredentialID', async () => {
      mockAccountEntity.CredentialID = null;
      mockProviderEntity.Configuration = JSON.stringify({
        defaultBucket: 'provider-default-bucket',
        defaultRegion: 'us-east-1',
      // Verify driver was initialized with provider config merged with account info
      expect(mockDriver.initializeCalledWith).toEqual({
    it('should initialize with just account info when no credential or provider config', async () => {
      mockProviderEntity.Configuration = null;
      // Verify driver was initialized with only account info
  describe('return value', () => {
    it('should return the initialized driver', async () => {
      const result = await initializeDriverWithAccountCredentials({
      expect(result).toBe(mockDriver);
      expect(mockDriver.IsConfigured).toBe(true);
    fields: unknown[];
    constructor(init?: Record<string, unknown>) {
      this.name = (init?.name as string) ?? '';
      this.description = (init?.description as string) ?? '';
      this.fields = (init?.fields as unknown[]) ?? [];
    defaultInView?: boolean;
      this.sequence = init?.sequence as number;
      this.type = init?.type as string;
      this.allowsNull = init?.allowsNull as boolean;
      this.isPrimaryKey = init?.isPrimaryKey as boolean;
      this.description = init?.description as string;
      this.defaultInView = init?.defaultInView as boolean;
      this.possibleValues = init?.possibleValues as unknown[];
  MapEntityFieldInfoToSkipEntityFieldInfo,
  MapEntityFieldValueInfoToSkipEntityFieldValueInfo,
  MapEntityRelationshipInfoToSkipEntityRelationshipInfo,
  skipEntityHasField,
  skipEntityGetField,
  skipEntityGetFieldNameSet,
  MapSimpleEntityFieldInfoToSkipEntityFieldInfo,
  MapSkipEntityFieldInfoToSimpleEntityFieldInfo,
import type { SkipEntityInfo, SkipEntityFieldInfo } from '../entity-metadata-types';
describe('SkipTypes util', () => {
  describe('MapEntityFieldInfoToSkipEntityFieldInfo', () => {
    it('should map EntityFieldInfo properties to SkipEntityFieldInfo', () => {
        EntityID: 'ent-1',
        DisplayName: 'First Name',
        Description: 'First name of the user',
        IsUnique: false,
        DefaultValue: '',
        ValueListType: null,
        ExtendedType: null,
        DefaultInView: true,
        DefaultColumnWidth: 150,
        RelatedEntitySchemaName: null,
        RelatedEntityBaseView: null,
        EntityFieldValues: [{ Value: 'Active', Code: 'Active' }],
      const result = MapEntityFieldInfoToSkipEntityFieldInfo(field as never);
      expect(result.name).toBe('FirstName');
      expect(result.displayName).toBe('First Name');
      expect(result.type).toBe('nvarchar');
      expect(result.isNameField).toBe(true);
      expect(result.possibleValues).toHaveLength(1);
  describe('MapEntityFieldValueInfoToSkipEntityFieldValueInfo', () => {
    it('should map value info', () => {
      const pv = { Value: 'Active' };
      const result = MapEntityFieldValueInfoToSkipEntityFieldValueInfo(pv as never);
      expect(result.value).toBe('Active');
      expect(result.displayValue).toBe('Active');
  describe('MapEntityRelationshipInfoToSkipEntityRelationshipInfo', () => {
    it('should map relationship properties', () => {
      const re = {
        Entity: 'MJ: Users',
        EntityBaseView: 'vwUsers',
        EntityKeyField: 'ID',
        RelatedEntityID: 'ent-2',
        RelatedEntityJoinField: 'UserID',
        RelatedEntityBaseView: 'vwOrders',
        RelatedEntity: 'Orders',
        Type: 'OneToMany',
        JoinEntityInverseJoinField: '',
        JoinView: '',
        JoinEntityJoinField: '',
      const result = MapEntityRelationshipInfoToSkipEntityRelationshipInfo(re as never);
      expect(result.entity).toBe('MJ: Users');
      expect(result.relatedEntity).toBe('Orders');
      expect(result.type).toBe('OneToMany');
  describe('skipEntityHasField', () => {
    it('should return true when field exists', () => {
      const entity: SkipEntityInfo = {
        baseView: 'vwTest',
          { name: 'ID', entityID: 'e1', type: 'uniqueidentifier' } as SkipEntityFieldInfo,
        relatedEntities: [],
      expect(skipEntityHasField(entity, 'ID')).toBe(true);
    it('should return false when field does not exist', () => {
      expect(skipEntityHasField(entity, 'Name')).toBe(false);
  describe('skipEntityGetField', () => {
    it('should return the field when it exists', () => {
      const field = { name: 'ID', entityID: 'e1', type: 'uniqueidentifier' } as SkipEntityFieldInfo;
        fields: [field],
      expect(skipEntityGetField(entity, 'ID')).toBe(field);
    it('should return undefined when field does not exist', () => {
      expect(skipEntityGetField(entity, 'Missing')).toBeUndefined();
  describe('skipEntityGetFieldNameSet', () => {
    it('should return a Set of field names', () => {
          { name: 'ID' } as SkipEntityFieldInfo,
          { name: 'Name' } as SkipEntityFieldInfo,
      const set = skipEntityGetFieldNameSet(entity);
      expect(set).toBeInstanceOf(Set);
      expect(set.has('ID')).toBe(true);
      expect(set.has('Name')).toBe(true);
      expect(set.has('Missing')).toBe(false);
  describe('MapSimpleEntityFieldInfoToSkipEntityFieldInfo', () => {
    it('should map simple field info to skip field info', () => {
      const simple = {
        sequence: 3,
        type: 'nvarchar',
        allowsNull: false,
        description: 'Email address',
        defaultInView: true,
        possibleValues: ['a@b.com'],
      const result = MapSimpleEntityFieldInfoToSkipEntityFieldInfo(simple as never);
      expect(result.name).toBe('Email');
      expect(result.sequence).toBe(3);
  describe('MapSkipEntityFieldInfoToSimpleEntityFieldInfo', () => {
    it('should map skip field info back to simple field info', () => {
      const skipField: SkipEntityFieldInfo = {
        entityID: 'e1',
        sequence: 1,
        displayName: 'ID',
        isUnique: true,
        length: 16,
        defaultColumnWidth: 100,
        possibleValues: [{ value: 'val1' }],
      const result = MapSkipEntityFieldInfoToSimpleEntityFieldInfo(skipField);
      expect(result.name).toBe('ID');
      expect(result.isPrimaryKey).toBe(true);
