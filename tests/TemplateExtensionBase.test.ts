import { TemplateExtensionBase, NunjucksCallback } from '../extensions/TemplateExtensionBase';
class TestExtension extends TemplateExtensionBase {
    public parseCalls: unknown[][] = [];
    public runCalls: unknown[][] = [];
    constructor(contextUser: Record<string, unknown>) {
        super(contextUser as never);
        this.tags = ['TestTag'];
    public parse(parser: unknown, nodes: unknown, lexer: unknown) {
        this.parseCalls.push([parser, nodes, lexer]);
        return { type: 'call_extension' };
    public run(context: unknown, ...args: unknown[]) {
        this.runCalls.push([context, ...args]);
describe('TemplateExtensionBase', () => {
    let extension: TestExtension;
    const contextUser = { ID: 'user-1', Name: 'TestUser' };
        extension = new TestExtension(contextUser);
        it('should initialize tags as empty array by default', () => {
            class EmptyExtension extends TemplateExtensionBase {
                constructor(user: Record<string, unknown>) {
                    super(user as never);
                public parse() { return null; }
                public run() {}
            const ext = new EmptyExtension(contextUser);
            expect(ext.tags).toEqual([]);
    describe('tags', () => {
        it('should store tag names', () => {
            expect(extension.tags).toEqual(['TestTag']);
        it('should support multiple tags', () => {
            extension.tags = ['Tag1', 'Tag2', 'Tag3'];
            expect(extension.tags).toHaveLength(3);
    describe('ContextUser', () => {
        it('should return the context user passed to constructor', () => {
            const user = { ID: 'specific-user', Name: 'Specific', Email: 'test@test.com' };
            const ext = new TestExtension(user);
            expect(ext.ContextUser).toBe(user);
        it('should be read-only (via getter)', () => {
            // The _contextUser is private, so we verify the getter works
            const user1 = extension.ContextUser;
            const user2 = extension.ContextUser;
            expect(user1).toBe(user2);
        it('should receive parser, nodes, and lexer arguments', () => {
            const parser = { nextToken: vi.fn() };
            const nodes = { CallExtensionAsync: vi.fn() };
            expect(extension.parseCalls).toHaveLength(1);
            expect(extension.parseCalls[0]).toEqual([parser, nodes, lexer]);
        it('should receive context and additional args', () => {
            const context = { ctx: { _mjRenderContext: null } };
            const body = () => 'body content';
            const callback: NunjucksCallback = (_err, _result) => {};
            expect(extension.runCalls).toHaveLength(1);
            expect(extension.runCalls[0][0]).toBe(context);
describe('NunjucksCallback type', () => {
    it('should accept null error and result', () => {
        const callback: NunjucksCallback = vi.fn();
        callback(null, 'result');
        expect(callback).toHaveBeenCalledWith(null, 'result');
    it('should accept error object', () => {
        expect(callback).toHaveBeenCalledWith(error);
