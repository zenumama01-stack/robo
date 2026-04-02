    MJGlobal: { Instance: { GetGlobalObjectStore: () => ({}) } },
    BaseEntity: class MockBaseEntity {},
    ValidationResult: class MockValidationResult {
        Errors: Array<{ Source: string; Message: string; Value: unknown; Type: string }> = [];
vi.mock('../generated/entity_subclasses', () => ({
    MJTemplateEntity: class MockTemplateEntity {},
    MJTemplateContentEntity: class MockTemplateContentEntity {},
    MJTemplateParamEntity: class MockTemplateParamEntity {},
import { TemplateEntityExtended } from '../custom/TemplateEntityExtended';
// Helper to create a mock content object with the required properties
function createContent(type: string, priority: number): { Type: string; Priority: number } {
    return { Type: type, Priority: priority };
// Helper to create a mock param object with the required properties
function createParam(
    isRequired: boolean,
    templateContentID: string | null = null
): { Name: string; IsRequired: boolean; TemplateContentID: string | null } {
    return { Name: name, IsRequired: isRequired, TemplateContentID: templateContentID };
describe('TemplateEntityExtended', () => {
    let template: TemplateEntityExtended;
        template = new TemplateEntityExtended();
        template.Content = [];
        template.Params = [];
    // ---------------------------------------------------------------
    // Content and Params getter/setter
    describe('Content getter/setter', () => {
        it('should default to an empty array', () => {
            const fresh = new TemplateEntityExtended();
            expect(fresh.Content).toEqual([]);
        it('should store and retrieve content items', () => {
            const items = [createContent('HTML', 1), createContent('Text', 2)];
            template.Content = items as never[];
            expect(template.Content).toBe(items);
            expect(template.Content).toHaveLength(2);
    describe('Params getter/setter', () => {
            expect(fresh.Params).toEqual([]);
        it('should store and retrieve param items', () => {
            const items = [createParam('subject', true), createParam('body', false)];
            template.Params = items as never[];
            expect(template.Params).toBe(items);
            expect(template.Params).toHaveLength(2);
    // GetContentByType
    describe('GetContentByType', () => {
            template.Content = [
                createContent('HTML', 1),
                createContent('Text', 2),
                createContent('HTML', 3),
        it('should return content matching the given type', () => {
            const results = template.GetContentByType('HTML');
            expect(results.every(c => c.Type === 'HTML')).toBe(true);
        it('should match type case-insensitively', () => {
            expect(template.GetContentByType('html')).toHaveLength(2);
            expect(template.GetContentByType('Html')).toHaveLength(2);
            expect(template.GetContentByType('HTML')).toHaveLength(2);
            expect(template.GetContentByType('tExT')).toHaveLength(1);
        it('should return an empty array when no content matches the type', () => {
            const results = template.GetContentByType('Markdown');
        it('should handle whitespace in the search type', () => {
            const results = template.GetContentByType('  HTML  ');
        it('should handle whitespace in the content type', () => {
            template.Content = [createContent('  HTML  ', 1)] as never[];
        it('should return an empty array when Content is empty', () => {
            expect(template.GetContentByType('HTML')).toEqual([]);
    // GetHighestPriorityContent
    describe('GetHighestPriorityContent', () => {
        it('should return the content with the lowest Priority number (highest priority)', () => {
                createContent('HTML', 5),
            const result = template.GetHighestPriorityContent();
            expect(result.Priority).toBe(1);
        it('should filter by type when type is provided', () => {
                createContent('Text', 1),
            const result = template.GetHighestPriorityContent('HTML');
            expect(result.Priority).toBe(3);
            expect(result.Type).toBe('HTML');
        it('should match type case-insensitively when filtering', () => {
                createContent('HTML', 10),
                createContent('HTML', 2),
            const result = template.GetHighestPriorityContent('html');
            expect(result.Priority).toBe(2);
        it('should handle whitespace in type filter', () => {
                createContent('  HTML  ', 5),
            const result = template.GetHighestPriorityContent('  HTML  ');
            expect(result.Priority).toBe(5);
        it('should return undefined when Content is empty', () => {
        it('should return undefined when no content matches the given type', () => {
            const result = template.GetHighestPriorityContent('Markdown');
        it('should return the first by priority when multiple have same priority', () => {
    // GetParametersForContent
    describe('GetParametersForContent', () => {
        const globalParam1 = createParam('subject', true, null);
        const globalParam2 = createParam('footer', false, null);
        const contentParam1 = createParam('bodyStyle', true, 'content-abc');
        const contentParam2 = createParam('heading', false, 'content-xyz');
            template.Params = [globalParam1, globalParam2, contentParam1, contentParam2] as never[];
        it('should return only global params when no contentId is provided', () => {
            const result = template.GetParametersForContent();
            expect(result).toContain(globalParam1);
            expect(result).toContain(globalParam2);
        it('should return only global params when contentId is an empty string', () => {
            const result = template.GetParametersForContent('');
        it('should return global + content-specific params when contentId matches', () => {
            const result = template.GetParametersForContent('content-abc');
            expect(result).toContain(contentParam1);
        it('should not include params for a different contentId', () => {
            expect(result).not.toContain(contentParam2);
        it('should return only global params when contentId matches no content-specific params', () => {
            const result = template.GetParametersForContent('content-nonexistent');
        it('should return an empty array when there are no params at all', () => {
            expect(template.GetParametersForContent()).toEqual([]);
            expect(template.GetParametersForContent('content-abc')).toEqual([]);
        it('should handle params where TemplateContentID is undefined (treated as global)', () => {
            const undefinedContentParam = { Name: 'extra', IsRequired: false, TemplateContentID: undefined };
            template.Params = [undefinedContentParam] as never[];
            expect(result[0]).toBe(undefinedContentParam);
    // ValidateTemplateInput
    describe('ValidateTemplateInput', () => {
        it('should succeed when all required params are present', () => {
            template.Params = [
                createParam('subject', true),
                createParam('body', true),
            const result = template.ValidateTemplateInput({ subject: 'Hello', body: 'World' });
        it('should succeed when there are no params', () => {
            const result = template.ValidateTemplateInput({});
        it('should succeed when only optional params exist', () => {
                createParam('optionalField', false),
        it('should fail when a required param is missing from data', () => {
            expect(result.Errors[0].Source).toBe('subject');
            expect(result.Errors[0].Type).toBe('Failure');
        it('should fail when a required param is null', () => {
            const result = template.ValidateTemplateInput({ subject: null });
        it('should fail when a required param is undefined', () => {
            const result = template.ValidateTemplateInput({ subject: undefined });
        it('should fail when a required param is an empty string', () => {
            const result = template.ValidateTemplateInput({ subject: '' });
        it('should fail when a required param is a whitespace-only string', () => {
            const result = template.ValidateTemplateInput({ subject: '   ' });
        it('should not fail for optional params that are missing', () => {
            const result = template.ValidateTemplateInput({ subject: 'Hello' });
        it('should not fail for optional params that are null or empty', () => {
                createParam('opt1', false),
                createParam('opt2', false),
            const result = template.ValidateTemplateInput({ subject: 'Hello', opt1: null, opt2: '' });
        it('should report multiple errors when multiple required params are missing', () => {
                createParam('recipient', true),
            expect(result.Errors).toHaveLength(3);
            const sources = result.Errors.map(e => e.Source);
            expect(sources).toContain('subject');
            expect(sources).toContain('body');
            expect(sources).toContain('recipient');
        it('should throw when data is null and there are required params', () => {
            // The source accesses data[p.Name] in the error push block even when data is null,
            // causing a TypeError. This documents the current behavior.
            expect(() => template.ValidateTemplateInput(null)).toThrow(TypeError);
        it('should throw when data is undefined and there are required params', () => {
            // Same as null case -- data[p.Name] throws when data is undefined.
            expect(() => template.ValidateTemplateInput(undefined)).toThrow(TypeError);
        it('should succeed when data is null but there are no required params', () => {
            // No required params means the error push block is never entered,
            // so null data does not cause a TypeError.
            const result = template.ValidateTemplateInput(null);
        it('should include the correct error message format', () => {
                createParam('myParam', true),
            expect(result.Errors[0].Message).toBe('Parameter myParam is required.');
        it('should include the actual value in the error (undefined when missing)', () => {
            expect(result.Errors[0].Value).toBeUndefined();
        it('should include the actual value in the error (null when explicitly null)', () => {
            const result = template.ValidateTemplateInput({ myParam: null });
            expect(result.Errors[0].Value).toBeNull();
        it('should accept non-string truthy values for required params', () => {
                createParam('count', true),
                createParam('active', true),
                createParam('data', true),
            const result = template.ValidateTemplateInput({
                active: false,
                data: { key: 'value' },
            // 0 and false are not null/undefined/empty-string so they pass
            // The code checks: !data => false for object; data[p.Name] === undefined => no; === null => no;
            // typeof 0 === 'number' so string check doesn't apply; same for false (boolean)
        // --- contentId filtering in ValidateTemplateInput ---
        describe('with contentId filtering', () => {
            const globalRequired = createParam('subject', true, null);
            const globalOptional = createParam('footer', false, null);
            const contentRequired = createParam('bodyStyle', true, 'content-abc');
            const contentOptional = createParam('heading', false, 'content-abc');
            const otherContentRequired = createParam('otherField', true, 'content-xyz');
                    globalRequired,
                    globalOptional,
                    contentRequired,
                    contentOptional,
                    otherContentRequired,
            it('should validate against all params when no contentId is provided', () => {
                // All 3 required: subject, bodyStyle, otherField
            it('should validate only against content-specific + global params when contentId is provided', () => {
                // For content-abc: subject (global required) + bodyStyle (content required)
                // otherField (content-xyz) should NOT be validated
                const result = template.ValidateTemplateInput({}, 'content-abc');
                expect(result.Errors).toHaveLength(2);
                expect(sources).toContain('bodyStyle');
                expect(sources).not.toContain('otherField');
            it('should succeed when all required params for a content are satisfied', () => {
                const result = template.ValidateTemplateInput(
                    { subject: 'Hello', bodyStyle: 'bold' },
                    'content-abc'
            it('should validate only global params when contentId matches no content-specific params', () => {
                const result = template.ValidateTemplateInput({}, 'content-nonexistent');
                // Only subject is global required
