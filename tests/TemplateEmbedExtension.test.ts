    MJTemplateContentEntity: class {
        TemplateText = '';
        Priority = 0;
        Content: Array<Record<string, unknown>> = [];
        GetContentByType(type: string) {
            return this.Content.filter((c: Record<string, unknown>) => c.Type === type);
        GetHighestPriorityContent() {
            if (this.Content.length === 0) return null;
            return [...this.Content].sort(
                (a, b) => (b.Priority as number) - (a.Priority as number)
            )[0];
// Mock the TemplateEngineServer
const { mockRenderTemplateSimple, mockFindTemplate } = vi.hoisted(() => ({
    mockRenderTemplateSimple: vi.fn(),
    mockFindTemplate: vi.fn(),
vi.mock('../TemplateEngine', () => ({
            FindTemplate: mockFindTemplate,
            RenderTemplateSimple: mockRenderTemplateSimple,
import { TemplateEmbedExtension } from '../extensions/TemplateEmbed.extension';
describe('TemplateEmbedExtension', () => {
    let extension: TemplateEmbedExtension;
        extension = new TemplateEmbedExtension(contextUser as never);
        it('should set tags to ["template"]', () => {
            expect(extension.tags).toEqual(['template']);
        it('should call parser methods and return CallExtensionAsync node', () => {
            const tok = { value: 'template' };
            const lexer = {};
            expect(parser.advanceAfterBlockEnd).toHaveBeenCalledWith('template');
                extension, 'run', parsedParams
        it('should call callback with error when template name is empty', () => {
            const context = { ctx: {} };
            const body = ''; // empty template name
            extension.run(context, body, callback);
            expect(callback).toHaveBeenCalledWith(expect.any(Error));
        it('should call callback with error when template is not found', () => {
            mockFindTemplate.mockReturnValue(null);
            const body = 'NonExistentTemplate';
        it('should detect circular template references', () => {
            const targetTemplate = {
                Name: 'CircularTemplate',
                Content: [{ Type: 'HTML', Priority: 1, TemplateText: 'content' }],
                GetContentByType: vi.fn().mockReturnValue([{ Type: 'HTML', Priority: 1 }]),
                GetHighestPriorityContent: vi.fn().mockReturnValue({ Type: 'HTML', Priority: 1, TemplateText: 'content' }),
            mockFindTemplate.mockReturnValue(targetTemplate);
                ctx: {
                    _mjRenderContext: {
                        templateStack: ['CircularTemplate'], // Already in the stack!
                        currentContentType: 'HTML',
            const body = 'CircularTemplate';
        it('should render embedded template successfully', async () => {
                Name: 'HeaderTemplate',
                Content: [{ Type: 'HTML', Priority: 1, TemplateText: '<h1>Header</h1>' }],
                GetContentByType: vi.fn().mockReturnValue([
                    { Type: 'HTML', Priority: 1, TemplateText: '<h1>Header</h1>' },
                GetHighestPriorityContent: vi.fn().mockReturnValue(
                    { Type: 'HTML', Priority: 1, TemplateText: '<h1>Header</h1>' }
            mockRenderTemplateSimple.mockResolvedValue({
                Output: '<h1>Header</h1>',
            const body = 'HeaderTemplate';
            expect(callback).toHaveBeenCalledWith(null, '<h1>Header</h1>');
        it('should initialize render context if not present', async () => {
                Name: 'TestTemplate',
                Content: [{ Type: 'Text', Priority: 1, TemplateText: 'text' }],
                    { Type: 'Text', Priority: 1, TemplateText: 'text' },
                    { Type: 'Text', Priority: 1, TemplateText: 'text' }
            mockRenderTemplateSimple.mockResolvedValue({ Success: true, Output: 'text' });
            const body = 'TestTemplate';
            // Verify context was initialized
            expect(context.ctx).toHaveProperty('_mjRenderContext');
        it('should pop template from stack after successful render', async () => {
                Name: 'PopTest',
                Content: [{ Type: 'HTML', Priority: 1, TemplateText: 'test' }],
                    { Type: 'HTML', Priority: 1, TemplateText: 'test' },
                    { Type: 'HTML', Priority: 1, TemplateText: 'test' }
            mockRenderTemplateSimple.mockResolvedValue({ Success: true, Output: 'test' });
            const renderContext = {
                templateStack: [],
            const context = { ctx: { _mjRenderContext: renderContext } };
            extension.run(context, 'PopTest', callback);
            expect(renderContext.templateStack).toEqual([]);
        it('should pop template from stack on render failure', async () => {
                Name: 'FailTest',
                Message: 'Render failed',
            extension.run(context, 'FailTest', callback);
        it('should parse template with type parameter', async () => {
                Name: 'TypedTemplate',
                Content: [
                    { Type: 'HTML', Priority: 1, TemplateText: '<b>HTML</b>' },
                    { Type: 'Text', Priority: 1, TemplateText: 'Text version' },
                GetContentByType: vi.fn().mockImplementation((type: string) => {
                    return targetTemplate.Content.filter(c => c.Type === type);
                    { Type: 'HTML', Priority: 1, TemplateText: '<b>HTML</b>' }
            mockRenderTemplateSimple.mockResolvedValue({ Success: true, Output: 'Text version' });
            const body = 'TypedTemplate,type=Text';
            expect(callback).toHaveBeenCalledWith(null, 'Text version');
    describe('resolveContentType (via run)', () => {
        it('should fall back to highest priority content when no type match', async () => {
                Name: 'FallbackTest',
                Content: [{ Type: 'Special', Priority: 1, TemplateText: 'special' }],
                GetContentByType: vi.fn().mockReturnValue([]),
                    { Type: 'Special', Priority: 1, TemplateText: 'special' }
            mockRenderTemplateSimple.mockResolvedValue({ Success: true, Output: 'special' });
            const renderContext = { templateStack: [], currentContentType: 'NonExistent' };
            extension.run(context, 'FallbackTest', callback);
            expect(callback).toHaveBeenCalledWith(null, 'special');
