import nunjucks from "nunjucks";
 * Action that transforms data using Nunjucks templates
 * Provides powerful templating capabilities for data transformation
 * // Simple object mapping
 *   ActionName: 'Data Mapper',
 *     Name: 'SourceData',
 *     Value: { user: { firstName: 'John', lastName: 'Doe', age: 30 } }
 *     Name: 'MappingTemplate',
 *       fullName: '{{ user.firstName }} {{ user.lastName }}',
 *       displayName: '{{ user.firstName | upper }}',
 *       ageGroup: '{% if user.age < 18 %}minor{% elif user.age < 65 %}adult{% else %}senior{% endif %}'
 * // Array transformation
 *     Value: [{ price: 10, qty: 2 }, { price: 20, qty: 1 }]
 *       total: '{{ price * qty }}',
 *       formattedPrice: '${{ price | number(2) }}'
 *     Name: 'IterateArrays',
 * // String template
 *     Value: { name: 'John', items: ['apple', 'banana'] }
 *     Value: 'Hello {{ name }}, you have {{ items | length }} items: {{ items | join(", ") }}'
 *     Name: 'TemplateType',
 *     Value: 'string'
@RegisterClass(BaseAction, "Data Mapper")
export class DataMapperAction extends BaseAction {
    private nunjucksEnv: nunjucks.Environment;
        // Initialize Nunjucks with custom filters
        this.nunjucksEnv = new nunjucks.Environment();
        this.setupCustomFilters();
     * Setup custom Nunjucks filters
    private setupCustomFilters(): void {
        this.setupCustomFiltersOnEnvironment(this.nunjucksEnv);
     * Transform data using Nunjucks templates
     *   - SourceData: Input object or array to transform
     *   - MappingTemplate: Nunjucks template string or object with templates
     *   - TemplateType: "string" | "object" (default: "object")
     *   - IterateArrays: Boolean - if true, map each array item (default: false)
     *   - CustomFilters: Object with custom Nunjucks filters (optional)
     *   - StrictVariables: Boolean - throw error on undefined variables (default: false)
     * @returns Transformed data based on templates
            let sourceData: any;
            let mappingTemplate: any;
                sourceData = JSONParamHelper.getRequiredJSONParam(params, 'SourceData');
                mappingTemplate = JSONParamHelper.getRequiredJSONParam(params, 'MappingTemplate');
            const templateTypeParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'templatetype');
            const iterateArraysParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'iteratearrays');
            const customFiltersParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'customfilters');
            const strictVariablesParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'strictvariables');
            const templateType = (templateTypeParam?.Value?.toString() || 'object').toLowerCase();
            const iterateArrays = iterateArraysParam?.Value?.toString()?.toLowerCase() === 'true';
            const strictVariables = strictVariablesParam?.Value?.toString()?.toLowerCase() === 'true';
            // Configure Nunjucks environment for strict variables if needed
            let activeEnv = this.nunjucksEnv;
            if (strictVariables) {
                // Create a new environment with strict settings
                activeEnv = new nunjucks.Environment(undefined, { throwOnUndefined: true });
                // Copy filters from the main environment
                this.copyFiltersToEnvironment(activeEnv);
            const customFilters = JSONParamHelper.getJSONParam(params, 'CustomFilters');
            if (customFilters) {
                for (const [name, filterFunc] of Object.entries(customFilters)) {
                    if (typeof filterFunc === 'function') {
                        activeEnv.addFilter(name, filterFunc as (...args: any[]) => any);
            // Perform transformation
            if (iterateArrays && Array.isArray(sourceData)) {
                // Transform each array item
                result = sourceData.map(item => this.transformData(item, mappingTemplate, templateType, activeEnv));
                // Transform single item
                result = this.transformData(sourceData, mappingTemplate, templateType, activeEnv);
                sourceType: Array.isArray(sourceData) ? 'array' : typeof sourceData,
                resultType: Array.isArray(result) ? 'array' : typeof result,
                templateType: templateType,
                itemsProcessed: Array.isArray(sourceData) && iterateArrays ? sourceData.length : 1
                Message: `Failed to map data: ${error instanceof Error ? error.message : String(error)}`,
     * Copy filters from main environment to a new environment
    private copyFiltersToEnvironment(targetEnv: nunjucks.Environment): void {
        // Set up the same custom filters on the target environment
        this.setupCustomFiltersOnEnvironment(targetEnv);
     * Setup custom Nunjucks filters on a specific environment
    private setupCustomFiltersOnEnvironment(env: nunjucks.Environment): void {
        // Number formatting filter
        env.addFilter('number', (value: any, decimals: number = 2) => {
            return isNaN(num) ? value : num.toFixed(decimals);
        // Currency formatting filter
        env.addFilter('currency', (value: any, symbol: string = '$', decimals: number = 2) => {
            return isNaN(num) ? value : `${symbol}${num.toFixed(decimals)}`;
        // Date formatting filter (basic)
        env.addFilter('date', (value: any, format: string = 'YYYY-MM-DD') => {
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            const seconds = String(date.getSeconds()).padStart(2, '0');
            return format
                .replace('YYYY', String(year))
                .replace('MM', month)
                .replace('DD', day)
                .replace('HH', hours)
                .replace('mm', minutes)
                .replace('ss', seconds);
        // JSON stringify filter
        env.addFilter('json', (value: any, indent: number = 0) => {
            return JSON.stringify(value, null, indent);
        // Default value filter
        env.addFilter('default', (value: any, defaultValue: any) => {
            return value === null || value === undefined || value === '' ? defaultValue : value;
        // Truncate filter
        env.addFilter('truncate', (value: string, length: number = 50, suffix: string = '...') => {
            if (!value || value.length <= length) return value;
            return value.substring(0, length - suffix.length) + suffix;
        // Slugify filter
        env.addFilter('slugify', (value: string) => {
            if (!value) return '';
                .toLowerCase()
                .replace(/[^\w\s-]/g, '')
                .replace(/[\s_-]+/g, '-')
                .replace(/^-+|-+$/g, '');
     * Transform a single data item using templates
    private transformData(data: any, template: any, templateType: string, env: nunjucks.Environment = this.nunjucksEnv): any {
            if (templateType === 'string') {
                // Render string template
                return env.renderString(template.toString(), data);
            } else if (templateType === 'object' && typeof template === 'object' && !Array.isArray(template)) {
                // Transform object template
                for (const [key, templateValue] of Object.entries(template)) {
                    if (typeof templateValue === 'string') {
                        // Render template string
                        result[key] = env.renderString(templateValue, data);
                        // Try to parse as JSON if it looks like JSON
                        if (typeof result[key] === 'string' && 
                            (result[key].startsWith('{') || result[key].startsWith('['))) {
                                result[key] = JSON.parse(result[key]);
                                // Keep as string if not valid JSON
                    } else if (typeof templateValue === 'object') {
                        // Recursively transform nested objects
                        result[key] = this.transformData(data, templateValue, 'object', env);
                        // Copy non-string values as-is
                        result[key] = templateValue;
            } else if (Array.isArray(template)) {
                // Transform array template
                return template.map(item => {
                    if (typeof item === 'string') {
                        return env.renderString(item, data);
                    } else if (typeof item === 'object') {
                        return this.transformData(data, item, 'object', env);
                // Return template as-is if not a recognized type
            throw new Error(`Template rendering error: ${error instanceof Error ? error.message : String(error)}`);
