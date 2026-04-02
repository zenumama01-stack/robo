        protected ContextUser = { ID: 'test-user', Name: 'Test' };
    ValidationErrorInfo: class { Message = ''; },
        TemplateID = '';
        ValidateTemplateInput(_data: unknown, _contentId?: string) {
            return { Success: true, Errors: [] };
        GetParametersForContent(_contentId: string) {
            return this.Params;
        GetHighestPriorityContent(_type?: string) {
            return this.Content.length > 0 ? this.Content[0] : null;
    MJTemplateParamEntity: class {
        Type = 'Scalar';
const { mockGetAllRegistrations } = vi.hoisted(() => ({
    mockGetAllRegistrations: vi.fn().mockReturnValue([]),
                GetAllRegistrations: mockGetAllRegistrations,
    TemplateRenderResult: class { Success = false; Output: string | null = null; Message?: string = undefined; },
    TemplateEngineBase: class {
            TemplateParams: [],
            TemplateContentTypes: [],
            TemplateCategories: [],
        get Templates() { return this._Metadata.Templates; }
        get TemplateContents() { return this._Metadata.TemplateContents; }
        get TemplateParams() { return this._Metadata.TemplateParams; }
        get TemplateContentTypes() { return this._Metadata.TemplateContentTypes; }
        get TemplateCategories() { return this._Metadata.TemplateCategories; }
        FindTemplate(name: string) {
            return this._Metadata.Templates.find(
                (t: Record<string, unknown>) => (t.Name as string).trim().toLowerCase() === name.trim().toLowerCase()
import { TemplateEngineServer, TemplateEntityLoader } from '../TemplateEngine';
describe('TemplateEntityLoader', () => {
    let loader: TemplateEntityLoader;
        loader = new TemplateEntityLoader();
    it('should have async flag set to true', () => {
        expect(loader.async).toBe(true);
    describe('AddTemplate', () => {
        it('should store template by ID', () => {
            const template = { ID: 'tmpl-1', Name: 'Test', Get: 'template content' };
            loader.AddTemplate('tmpl-1', template as never);
            // Verify by calling getSource
            loader.getSource('tmpl-1', callback);
            // The template uses Number(name) which would be NaN for 'tmpl-1',
            // so let's test with numeric IDs
        it('should retrieve template via getSource with numeric ID', () => {
            const template = { ID: '42', Name: 'NumericTest', Get: 'content here' };
            loader.AddTemplate('42', template as never);
            loader.getSource('42', callback);
            expect(callback).toHaveBeenCalledWith(
                    src: 'content here',
                    noCache: true,
        it('should not call callback if template is not found', () => {
            loader.getSource('999', callback);
            expect(callback).not.toHaveBeenCalled();
describe('TemplateEngineServer', () => {
    let engine: TemplateEngineServer;
        engine = new TemplateEngineServer();
        // Call SetupNunjucks to initialize the nunjucks environment
        engine.SetupNunjucks();
        it('should return an instance', () => {
            const instance = TemplateEngineServer.Instance;
    describe('SetupNunjucks', () => {
        it('should initialize nunjucks environment and loader', () => {
            // Verify the engine can render simple templates after setup
    describe('ClearTemplateCache', () => {
        it('should clear the template cache', () => {
            engine.ClearTemplateCache();
            // No error thrown means success; verify it works after clearing
            expect(() => engine.ClearTemplateCache()).not.toThrow();
    describe('RenderTemplateSimple', () => {
        it('should render a simple template with variables', async () => {
            const result = await engine.RenderTemplateSimple(
                'Hello {{ name }}!',
                { name: 'World' }
            expect(result.Output).toBe('Hello World!');
        it('should render template with no variables', async () => {
                'Static content',
                {}
            expect(result.Output).toBe('Static content');
        it('should handle template syntax errors gracefully', async () => {
                '{% invalid syntax %}',
            expect(result.Output).toBeNull();
            expect(result.Message).toBeDefined();
        it('should render template with conditionals', async () => {
            const template = '{% if show %}Visible{% else %}Hidden{% endif %}';
            const resultShow = await engine.RenderTemplateSimple(template, { show: true });
            expect(resultShow.Output).toBe('Visible');
            const resultHide = await engine.RenderTemplateSimple(template, { show: false });
            expect(resultHide.Output).toBe('Hidden');
        it('should render template with loops', async () => {
            const template = '{% for item in items %}{{ item }},{% endfor %}';
            const result = await engine.RenderTemplateSimple(template, { items: ['a', 'b', 'c'] });
            expect(result.Output).toBe('a,b,c,');
        it('should handle nested object access', async () => {
            const template = '{{ user.name }} - {{ user.address.city }}';
            const result = await engine.RenderTemplateSimple(template, {
                user: { name: 'Alice', address: { city: 'NYC' } },
            expect(result.Output).toBe('Alice - NYC');
        it('should handle undefined variables gracefully', async () => {
                'Value: {{ undefinedVar }}',
            expect(result.Output).toBe('Value: ');
        it('should auto-escape HTML by default', async () => {
                '{{ content }}',
                { content: '<script>alert("xss")</script>' }
            expect(result.Output).not.toContain('<script>');
            expect(result.Output).toContain('&lt;script&gt;');
        it('should support safe filter to bypass escaping', async () => {
                '{{ content | safe }}',
                { content: '<b>Bold</b>' }
            expect(result.Output).toBe('<b>Bold</b>');
        it('should support built-in filters', async () => {
                '{{ name | upper }}',
                { name: 'hello' }
            expect(result.Output).toBe('HELLO');
    describe('Custom Filters', () => {
        it('should provide json filter for object serialization', async () => {
                '{{ data | json | safe }}',
                { data: { key: 'value' } }
            // json filter with indent=2
            expect(result.Output).toContain('"key"');
            expect(result.Output).toContain('"value"');
        it('should provide jsoninline filter for compact output', async () => {
                '{{ data | jsoninline | safe }}',
                { data: { a: 1, b: 2 } }
            expect(result.Output).toContain('{"a":1,"b":2}');
        it('should provide jsonparse filter to parse JSON strings', async () => {
            const template = '{% set obj = jsonStr | jsonparse %}{{ obj.name }}';
                { jsonStr: '{"name":"Alice"}' }
            expect(result.Output).toBe('Alice');
        it('should handle jsonparse with invalid JSON gracefully', async () => {
            const template = '{{ badJson | jsonparse }}';
                { badJson: 'not-json' }
            expect(result.Output).toBe('not-json'); // Returns original string
        it('should handle json filter with circular reference', async () => {
            // The json filter catches errors and returns an error message
            const circular: Record<string, unknown> = {};
            circular.self = circular;
                '{{ data | json }}',
                { data: circular }
            expect(result.Output).toContain('[Error serializing to JSON');
    describe('RenderTemplate', () => {
        it('should return failure when templateContent is null', async () => {
            const templateEntity = {
                ID: 't-1',
                ValidateTemplateInput: vi.fn().mockReturnValue({ Success: true, Errors: [] }),
                GetParametersForContent: vi.fn().mockReturnValue([]),
            const result = await engine.RenderTemplate(templateEntity as never, null as never, {});
            expect(result.Message).toContain('templateContent variable is required');
        it('should return failure when TemplateText is empty', async () => {
            const templateContent = { ID: 'tc-1', TemplateText: '' };
            const result = await engine.RenderTemplate(
                templateEntity as never,
                templateContent as never,
            expect(result.Message).toContain('TemplateText variable is required');
        it('should return failure when validation fails', async () => {
                ValidateTemplateInput: vi.fn().mockReturnValue({
                        { Message: 'Missing required field: name' },
                        { Message: 'Invalid type for age' },
            const templateContent = { ID: 'tc-1', TemplateText: 'Hello {{ name }}' };
            expect(result.Message).toContain('Missing required field: name');
            expect(result.Message).toContain('Invalid type for age');
        it('should skip validation when SkipValidation is true', async () => {
                ValidateTemplateInput: vi.fn(),
                { name: 'World' },
                true // SkipValidation
            expect(templateEntity.ValidateTemplateInput).not.toHaveBeenCalled();
        it('should render template with provided data', async () => {
            const templateContent = { ID: 'tc-1', TemplateText: 'Dear {{ name }}, welcome!' };
                { name: 'Alice' }
            expect(result.Output).toBe('Dear Alice, welcome!');
        it('should catch rendering errors', async () => {
            const templateContent = {
                ID: 'tc-err',
                TemplateText: '{% if %}missing condition{% endif %}',
        it('should use template caching', async () => {
            const templateContent = { ID: 'cached-tc', TemplateText: '{{ x }}' };
            // First render
            const result1 = await engine.RenderTemplate(
                { x: 'first' }
            expect(result1.Output).toBe('first');
            // Second render with same content ID should use cache
            const result2 = await engine.RenderTemplate(
                { x: 'second' }
            expect(result2.Output).toBe('second');
    describe('mergeDefaultValues', () => {
        it('should apply default values for missing parameters', async () => {
                GetParametersForContent: vi.fn().mockReturnValue([
                    { Name: 'greeting', Type: 'Scalar', DefaultValue: 'Hello' },
                    { Name: 'title', Type: 'Scalar', DefaultValue: 'Mr.' },
                ID: 'tc-defaults',
                TemplateText: '{{ greeting }} {{ title }} {{ name }}',
                { name: 'Smith' }
            expect(result.Output).toBe('Hello Mr. Smith');
        it('should not override provided values with defaults', async () => {
                ID: 'tc-no-override',
                TemplateText: '{{ greeting }}',
                { greeting: 'Hi' }
            expect(result.Output).toBe('Hi');
        it('should parse JSON default values for complex types', async () => {
                    { Name: 'config', Type: 'Object', DefaultValue: '{"color":"red"}' },
                ID: 'tc-json-default',
                TemplateText: '{{ config.color }}',
            expect(result.Output).toBe('red');
        it('should handle non-JSON default values for complex types', async () => {
                    { Name: 'data', Type: 'Array', DefaultValue: 'not-json' },
                ID: 'tc-nonjson',
                TemplateText: '{{ data }}',
            expect(result.Output).toBe('not-json');
        it('should skip defaults when value is null or undefined', async () => {
                    { Name: 'nullDefault', Type: 'Scalar', DefaultValue: null },
                    { Name: 'undefDefault', Type: 'Scalar', DefaultValue: undefined },
                ID: 'tc-nulls',
                TemplateText: '[{{ nullDefault }}][{{ undefDefault }}]',
            expect(result.Output).toBe('[][]');
        it('should handle content-specific params overriding global params', async () => {
            const contentId = 'tc-precedence';
                    { Name: 'val', Type: 'Scalar', DefaultValue: 'global' }, // global param
                    { Name: 'val', Type: 'Scalar', DefaultValue: 'content-specific', TemplateContentID: contentId }, // content-specific
                ID: contentId,
                TemplateText: '{{ val }}',
            expect(result.Output).toBe('content-specific');
    describe('getNunjucksTemplate (caching behavior)', () => {
        it('should cache templates by contentId', async () => {
            // Render same template twice with same content ID
            const tc = { ID: 'cache-id', TemplateText: '{{ x }}' };
            await engine.RenderTemplate(templateEntity as never, tc as never, { x: 'a' });
            await engine.RenderTemplate(templateEntity as never, tc as never, { x: 'b' });
            // Both should succeed (cache is being used)
            const result = await engine.RenderTemplate(templateEntity as never, tc as never, { x: 'c' });
            expect(result.Output).toBe('c');
        it('should delegate to the template loader', () => {
            const templateEntity = { ID: 'tmpl-1', Name: 'Test' };
            expect(() => engine.AddTemplate(templateEntity as never)).not.toThrow();
