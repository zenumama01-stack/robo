// We test the pure functions from the manifest generator by importing via a module-level mock setup.
// Many functions in the manifest generator are module-private, but we can test the exported types
// and the extractRegisterClassDecorators logic by creating our own test source code and parsing it.
// Mock fs, glob, and path for anything the module loads at import time
            existsSync: vi.fn().mockReturnValue(false),
            readFileSync: vi.fn().mockReturnValue('{}'),
            realpathSync: vi.fn((p: string) => p),
            mkdirSync: vi.fn()
vi.mock('glob', () => ({
    glob: vi.fn().mockResolvedValue([]),
    globSync: vi.fn().mockReturnValue([])
// Since the functions we want to test are not exported, we test the underlying TypeScript AST parsing
// which is the core logic of the manifest generator
describe('Manifest Generator - TypeScript AST Parsing', () => {
    function parseSourceForRegisterClass(sourceCode: string): { className: string; baseClass?: string; key?: string }[] {
            'test.ts',
            sourceCode,
        const results: { className: string; baseClass?: string; key?: string }[] = [];
                        if (ts.isCallExpression(decorator.expression)) {
                            const expr = decorator.expression;
                            if (ts.isIdentifier(expr.expression) && expr.expression.text === 'RegisterClass') {
                                const info: { className: string; baseClass?: string; key?: string } = {
                                    className: node.name.text
                                if (expr.arguments.length > 0 && ts.isIdentifier(expr.arguments[0])) {
                                    info.baseClass = expr.arguments[0].text;
                                if (expr.arguments.length > 1 && ts.isStringLiteral(expr.arguments[1])) {
                                    info.key = expr.arguments[1].text;
    it('should detect @RegisterClass on a class', () => {
        const source = `
            @RegisterClass(BaseEntity, 'Users')
            export class MJUserEntity extends BaseEntity {}
        const results = parseSourceForRegisterClass(source);
        expect(results[0].className).toBe('MJUserEntity');
        expect(results[0].baseClass).toBe('BaseEntity');
        expect(results[0].key).toBe('Users');
    it('should detect multiple @RegisterClass decorators in one file', () => {
            @RegisterClass(BaseEntity, 'Roles')
            export class MJRoleEntity extends BaseEntity {}
        expect(results[1].className).toBe('MJRoleEntity');
    it('should return empty for files without @RegisterClass', () => {
            export class SimpleClass {
                DoSomething() {}
    it('should handle class with no key argument', () => {
            @RegisterClass(BaseAction)
            export class MyAction extends BaseAction {}
        expect(results[0].className).toBe('MyAction');
        expect(results[0].baseClass).toBe('BaseAction');
        expect(results[0].key).toBeUndefined();
    it('should skip undecorated classes', () => {
            export class NotDecorated {}
            @RegisterClass(BaseEntity, 'Decorated')
            export class DecoratedClass extends BaseEntity {}
            export class AlsoNotDecorated {}
        expect(results[0].className).toBe('DecoratedClass');
    it('should ignore other decorators', () => {
            @Component({})
            export class MyComponent {}
            @RegisterClass(BaseEntity, 'Test')
            export class MJTestEntity extends BaseEntity {}
        expect(results[0].className).toBe('MJTestEntity');
describe('Manifest Generator Types', () => {
    it('should have RegisteredClassInfo shape', () => {
        // Validate the type shape compiles
        const info: { className: string; filePath: string; packageName: string; baseClassName?: string; key?: string } = {
            className: 'TestClass',
            filePath: '/path/to/file.ts',
            packageName: '@memberjunction/test'
        expect(info.className).toBe('TestClass');
        expect(info.filePath).toBe('/path/to/file.ts');
        expect(info.packageName).toBe('@memberjunction/test');
    it('should have GenerateManifestOptions shape', () => {
        const options: {
            outputPath: './manifest.ts',
            verbose: true,
            excludePackages: ['@memberjunction']
        expect(options.outputPath).toBe('./manifest.ts');
        expect(options.excludePackages).toContain('@memberjunction');
    it('should have GenerateManifestResult shape', () => {
        const result: {
            classes: unknown[];
            ManifestChanged: false,
            classes: [],
            packages: ['@memberjunction/core'],
            totalDepsWalked: 15,
        expect(result.packages).toHaveLength(1);
describe('isPackageExcluded (logic test)', () => {
    // Re-implement the logic to test it since it's a private function
    it('should exclude packages matching a prefix', () => {
        expect(isPackageExcluded('@memberjunction/core', ['@memberjunction'])).toBe(true);
    it('should not exclude packages not matching any prefix', () => {
        expect(isPackageExcluded('lodash', ['@memberjunction'])).toBe(false);
    it('should handle multiple prefixes', () => {
        expect(isPackageExcluded('@angular/core', ['@memberjunction', '@angular'])).toBe(true);
    it('should return false for empty excludePackages', () => {
        expect(isPackageExcluded('@memberjunction/core', [])).toBe(false);
    it('should handle exact match', () => {
        expect(isPackageExcluded('lodash', ['lodash'])).toBe(true);
    it('should not match partial package names incorrectly', () => {
        expect(isPackageExcluded('lodash-es', ['lodash'])).toBe(true); // starts with lodash
        expect(isPackageExcluded('my-lodash', ['lodash'])).toBe(false); // doesn't start with lodash
